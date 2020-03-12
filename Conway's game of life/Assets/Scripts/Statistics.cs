using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Statistics : MonoBehaviour
{
    public static Statistics main;

    public List<int>[] stats = new List<int>[3];
    public bool[] graphsOn = new bool[3];
    public Grapher[] graphs = new Grapher[3];
    [SerializeField] List<int> deaths;
    public Stat typeToBecome = Stat.notSelected;
    public float framesPerSecond;
    public Stack<Vector3Int> redoData = new Stack<Vector3Int>();


    public void AddValue(int value, Stat dataSet)
    {
        stats[(int)dataSet].Add(value);
        if (graphsOn[(int)dataSet])
        {
            print("Add Value");
            if(graphs[(int)dataSet] != null)
                graphs[(int)dataSet].currentGraph.AddDataPoint(value);
        }
    }
    public static Dictionary<Stat, string> titles = new Dictionary<Stat, string>()
    {
        {Stat.deaths, "Deaths Per Generation" },
        {Stat.aliveCells, "Alive Cells Each Generation" },
        {Stat.births, "Birthed Cells Per Generation" }

    };

    public void ClearGraphs()
    {
        for (int i = 0; i< stats.Length; i++)
        {
            stats[i] = new List<int>();
        }

        foreach(Grapher g in graphs)
        {
            if (g != null)
            {
                g.currentGraph.ReplaceDataStart(stats[(int)g.representing]);
                g.ResetGraph();

            }
        }


    }
    private void Update()
    {
        deaths = stats[0];
    }
    public enum Stat
    {
        deaths = 0,
        births,
        aliveCells,
        notSelected
    }

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        for(int i = 0; i < stats.Length; i++)
        {
            stats[i] = new List<int>();
        }
    }

    public void OpenGraph(int graphType)
    {
        if (graphsOn[graphType])
        {
            graphs[graphType].transform.parent.parent.GetComponent<BackgroundController>().Close();
        }
        else
        {
            typeToBecome = (Stat)graphType;
            graphsOn[graphType] = true;
            SceneManager.LoadScene("Graphs", LoadSceneMode.Additive);
            //grapher.currentGraph.title = titles[(Stat)graphType];
            //grapher.currentGraph.ReplaceData(stats[graphType]);
            //graphs[graphType] = grapher;
        }
    }

    public void DeleteLast()
    {
        if(stats[0].Count != 0)
            redoData.Push(new Vector3Int(stats[0][stats[0].Count - 1], stats[1][stats[1].Count - 1], stats[2][stats[2].Count - 1]));

        for (int i = 0; i < stats.Length; i++)
        {
            if(stats[i].Count != 0)
                stats[i].RemoveAt(stats[i].Count - 1);
        }
        foreach(Grapher g in graphs)
        {
            if(g != null)
                g.AdjustGraph();
        }
    }
}
