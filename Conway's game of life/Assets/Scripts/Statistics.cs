using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Statistics : MonoBehaviour
{
    //Singleton instance
    public static Statistics main;

    //Type of graph
    public enum Stat
    {
        deaths = 0,
        births,
        aliveCells,
        notSelected
    }

    [Header("Data Sets")]
    [HideInInspector] public List<int>[] stats = new List<int>[3];

    //Whether graphs are on or not
    [HideInInspector] public bool[] graphsOn = new bool[3];

    //Graphers corresponding with data
    [HideInInspector] public Grapher[] graphs = new Grapher[3];

    //Type the most recently created grapher becomes
    [HideInInspector] public Stat typeToBecome = Stat.notSelected;

    [Header("Generations Per Second")]
    //Data for stats section to display
    public float generationsPerSecond;

    //Data to place back in graphs
    public Stack<Vector3Int> redoData = new Stack<Vector3Int>();

    

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        for (int i = 0; i < stats.Length; i++)
        {
            stats[i] = new List<int>();
        }
    }

    //Add to a value to a data set
    public void AddValue(int value, Stat dataSet)
    {
        //Add to the data
        stats[(int)dataSet].Add(value);

        //If it is currently being displayed in graph send to graph as well
        if (graphsOn[(int)dataSet])
        {
            print("Add Value");
            if(graphs[(int)dataSet] != null)
                graphs[(int)dataSet].currentGraph.AddDataPoint(value);
        }
    }

    //Titles for graphs
    public static Dictionary<Stat, string> titles = new Dictionary<Stat, string>()
    {
        {Stat.deaths, "Deaths Per Generation" },
        {Stat.aliveCells, "Alive Cells Each Generation" },
        {Stat.births, "Birthed Cells Per Generation" }

    };

    //Clear and delete all graph data
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

    //Open or close graph
    public void OpenGraph(int graphType)
    {
        //If graph is already on then close
        if (graphsOn[graphType])
        {
            graphs[graphType].transform.parent.parent.GetComponent<BackgroundController>().Close();
        }
        //Open new grapher/graph
        else
        {
            typeToBecome = (Stat)graphType;
            graphsOn[graphType] = true;
            SceneManager.LoadScene("Graphs", LoadSceneMode.Additive);
        }
    }

    //List the most recent data entry and save in the redoData stack
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
