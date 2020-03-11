using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Grapher : MonoBehaviour
{

    [SerializeField]
    GameObject xAxisTitlePrefab, yAxisTitlePrefab, xAxisDividerPrefab, yAxisDividerPrefab;
    [SerializeField]
    Sprite pointSprite;

    public int totalGenerations;

    Transform pointsParent, joinerParent, xTextParents, yTextParents, xAxisDividerParent, yAxisDividerParent;

    [SerializeField] public int minGraphValue;
    int yMax, xMax, xSteps, ySteps, yInterval, xInterval;

    [SerializeField] List<GameObject> points = new List<GameObject>(), joiners = new List<GameObject>(), xTitles = new List<GameObject>(), yTitles = new List<GameObject>();
    [SerializeField] List<GameObject> xAxisDividers = new List<GameObject>(), yAxisDividers = new List<GameObject>();

    [SerializeField]
    float minGap;


    [SerializeField]
    float gapMutliplier, xGap, yGap, xTitleGap, yTitleGap;

    [SerializeField]
    int[] increments;

    [SerializeField]
    public GraphData currentGraph;

    BackgroundController backgroundController;

    RectTransform rectTransform;

    public Statistics.Stat representing = Statistics.Stat.notSelected;
    private void Awake()
    {
    }

    void Start()
    {
        backgroundController = BackgroundController.main;
        rectTransform = GetComponent<RectTransform>();

        #region Parents
        pointsParent = transform.Find("Points");
        joinerParent = transform.Find("Lines");
        xTextParents = transform.parent.Find("X Axis Text");
        yTextParents = transform.parent.Find("Y Axis Text");
        xAxisDividerParent = transform.parent.Find("X Axis Divider");
        yAxisDividerParent = transform.parent.Find("Y Axis Divider");
        #endregion

        AddDefaultObjects();

        currentGraph.grapher = this;

        representing = Statistics.main.typeToBecome;
        currentGraph.title = Statistics.titles[representing];
        Statistics.main.graphs[(int)representing] = this;
        currentGraph.ReplaceDataStart(Statistics.main.stats[(int)representing]);

        SetUpGraph();


        //StartCoroutine(AddValues());
    }

    public void AddDefaultObjects()
    {
        xAxisDividers.Add(null);
        yAxisDividers.Add(null);
        joiners.Add(null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CalculateIncrements(float length, int points, bool isX)
    {
        //print("Length: " + length);
        int index = 0;
        int totalPoints = 0;
        for (int i = 0; i < increments.Length; i++)
        {
            index = i;
            totalPoints = Mathf.CeilToInt(points / (float)increments[i]) * increments[i] + increments[i];
            if ((length / (totalPoints / increments[i])) > minGap)
            {
                break;
            }

        }

        if (totalPoints < 10)
            totalPoints = minGraphValue;

        if (isX)
        {
            xMax = totalPoints;
            xInterval = increments[index];
            xSteps = totalPoints / increments[index];
            xGap = length / totalPoints;
            xTitleGap = length / xSteps;
        }
        else
        {
            yMax = totalPoints;
            yInterval = increments[index];
            ySteps = totalPoints / increments[index];
            yGap = length / totalPoints;
            yTitleGap = length / ySteps;
        }
    }

    public void AdjustGraph()
    {
        Debug.Log("Adjust");
        int prevXSteps = xSteps, prevYSteps = ySteps;
        CalculateIncrements(rectTransform.rect.width, currentGraph.data.Count, true);
        CalculateIncrements(rectTransform.rect.height, currentGraph.maxYValue, false);

        #region Text and Dividers
        //X Axis and Dividers 
        if (prevXSteps > xSteps)
        {
            for (int i = 0; i < prevXSteps; i++)
            {
                if (i >= xSteps)
                {
                    GameObject g = xTitles[xSteps];
                    xTitles.RemoveAt(xSteps);
                    Destroy(g);

                    g = xAxisDividers[xSteps];
                    xAxisDividers.RemoveAt(xSteps);
                    Destroy(g);

                }
                else
                {
                    ModifyXAxisTitle(i);
                }

            }
        }
        else
        {
            
            for (int i = 0; i <= xSteps; i++)
            {
                if (i > prevXSteps)
                {
                    CreateXAxisTitle(i);
                }
                else
                {
                    ModifyXAxisTitle(i);
                }
            }
        }
        //Y Axis Text and Dividers
        if (prevYSteps > ySteps)
        {
            for (int i = 0; i < prevYSteps; i++)
            {
                if (i >= ySteps)
                {
                    GameObject g = yTitles[ySteps];
                    yTitles.RemoveAt(ySteps);
                    Destroy(g);

                    g = yAxisDividers[ySteps];
                    yAxisDividers.RemoveAt(ySteps);
                    Destroy(g);

                }
                else
                {
                    ModifyYAxisTitle(i);
                }

            }
        }
        else
        {

            for (int i = 0; i <= ySteps; i++)
            {
                if (i > prevYSteps)
                {
                    CreateYAxisTitle(i);
                }
                else
                {
                    ModifyYAxisTitle(i);
                }
            }
        }
        #endregion

        //Points
        if (currentGraph.data.Count >= points.Count)
        {
            for (int i = 0; i < currentGraph.data.Count; i++)
            {
                if (i >= points.Count)
                {
                    CreatePoint(new Vector2(xGap * i, yGap * currentGraph.data[i]));
                    CreateJoiner(i);
                }
                else
                {
                    ModifyPoint(i);
                    ModifyJoiner(i);
                }
            }
        }
        else
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (i < currentGraph.data.Count)
                {
                    GameObject g = points[currentGraph.data.Count];
                    points.RemoveAt(currentGraph.data.Count);
                    Destroy(g);

                    g = joiners[currentGraph.data.Count];
                    joiners.RemoveAt(currentGraph.data.Count);
                    Destroy(g);
                }
                else
                {
                    ModifyPoint(i);
                    ModifyJoiner(i);
                }
            }
        }
    }

    void SetUpGraph()
    {
        CalculateIncrements(rectTransform.rect.width, currentGraph.data.Count, true);
        CalculateIncrements(rectTransform.rect.height, currentGraph.maxYValue, false);

        for (int i = 0; i <= xSteps; i++)
        {
            CreateXAxisTitle(i);
        }

        for (int i = 0; i <= ySteps; i++) 
        {
            CreateYAxisTitle(i);
        }

        for (int i = 0; i < currentGraph.data.Count; i++)
        {
            CreatePoint(new Vector2(xGap * i, yGap * currentGraph.data[i]));
            CreateJoiner(i);
        }


    }


    GameObject CreatePoint(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("Circle", typeof(Image));
        gameObject.transform.SetParent(pointsParent, false);
        gameObject.GetComponent<Image>().sprite = pointSprite;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(15, 15);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        points.Add(gameObject);
        return gameObject;
    }

    GameObject ModifyPoint(int index)
    {
        GameObject gameObject = points[index];

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(xGap * index, yGap * currentGraph.data[index]);

        return gameObject;
    }

    GameObject CreateJoiner(int index)
    {
        if (index == 0)
            return null;

        GameObject gameObject = new GameObject("Line", typeof(Image));
        gameObject.transform.SetParent(joinerParent, false);

        gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        Vector2 v1 = new Vector2(xGap * index, yGap * currentGraph.data[index]);
        Vector2 v2 = new Vector2(xGap * (index - 1), yGap * currentGraph.data[index - 1]);
        float distance = Vector2.Distance(v1, v2);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = (v1 + v2) / 2;
        rectTransform.sizeDelta = new Vector2(distance, 6f);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(v1.y - v2.y,v1.x - v2.x));

        

        joiners.Add(gameObject);
        return gameObject;
    }

    GameObject ModifyJoiner(int index)
    {
        if (index == 0)
            return null;

        print("Joiner Index" + index);
        //GameObject gameObject = new GameObject("Line", typeof(Image));
        //gameObject.transform.SetParent(joinerParent, false);

        //gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        GameObject gameObject = joiners[index];

        Vector2 v1 = new Vector2(xGap * index, yGap * currentGraph.data[index]);
        Vector2 v2 = new Vector2(xGap * (index - 1), yGap * currentGraph.data[index - 1]);
        float distance = Vector2.Distance(v1, v2);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

        if (distance < 7.5f)
        {
            rectTransform.anchoredPosition = new Vector2(-1000, -1000);
        }
        else
        {

            rectTransform.anchoredPosition = (v1 + v2) / 2;
            rectTransform.sizeDelta = new Vector2(distance, 6f);
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(v1.y - v2.y, v1.x - v2.x));
        }
        return gameObject;
    }

    GameObject CreateXAxisTitle(int index)
    {
        GameObject gameObject = Instantiate(xAxisTitlePrefab, xTextParents);

        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(index * xTitleGap, 0);
        gameObject.GetComponent<TextMeshProUGUI>().text = (index * xInterval).ToString();
        xTitles.Add(gameObject);
        CreateXAxisDivider(index);
        return gameObject;
    }

    GameObject ModifyXAxisTitle(int index)
    {
        GameObject gameObject = xTitles[index];

        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(index * xTitleGap, 0);
        gameObject.GetComponent<TextMeshProUGUI>().text = (index * xInterval).ToString();

        ModifyXAxisDivider(index);
        return gameObject;
    }

    GameObject CreateXAxisDivider(int index)
    {
        if (index == 0 || index == xSteps)
            return null;
        GameObject gameObject = Instantiate(xAxisDividerPrefab, xAxisDividerParent);
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(index * xTitleGap + 1.5f, 0);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.rect.height, 3);
        xAxisDividers.Add(gameObject);
        return gameObject;
    }

    GameObject ModifyXAxisDivider(int index)
    {
        if (index == 0 || index >= xSteps)
            return null;

        if(index >= xAxisDividers.Count)
        {
            return CreateXAxisDivider(index);
        }
        GameObject gameObject = xAxisDividers[index];
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(index * xTitleGap + 1.5f, 0);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.rect.height, 3);
        return gameObject;
    }

    GameObject CreateYAxisTitle(int index)
    {
        GameObject gameObject = Instantiate(yAxisTitlePrefab, yTextParents);

        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, index * yTitleGap);
        gameObject.GetComponent<TextMeshProUGUI>().text = (index * yInterval).ToString();
        yTitles.Add(gameObject);
        CreateYAxisDivider(index);
        return gameObject;
    }

    GameObject ModifyYAxisTitle(int index)
    {
        GameObject gameObject = yTitles[index];

        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, index * yTitleGap);
        gameObject.GetComponent<TextMeshProUGUI>().text = (index * yInterval).ToString();

        ModifyYAxisDivider(index);
        return gameObject;
    }

    GameObject CreateYAxisDivider(int index)
    {
        if (index == 0 || index == ySteps)
            return null;
        GameObject gameObject = Instantiate(yAxisDividerPrefab, yAxisDividerParent);
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, index * yTitleGap);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.rect.width, 3);
        yAxisDividers.Add(gameObject);
        return gameObject;
    }

    GameObject ModifyYAxisDivider(int index)
    {
        if (index == 0 || index >= ySteps)
            return null;

        if (index >= yAxisDividers.Count)
        {
            return CreateYAxisDivider(index);
        }
        GameObject gameObject = yAxisDividers[index];
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, index * yTitleGap + 1.5f);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.rect.width, 3);
        return gameObject;
    }

    public void ResetGraph()
    {
        foreach(GameObject g in points)
        {
            Destroy(g);
        }
        points = new List<GameObject>();
        foreach (GameObject g in joiners)
        {
            Destroy(g);
        }
        joiners = new List<GameObject>();
        foreach (GameObject g in xTitles)
        {
            Destroy(g);
        }
        xTitles = new List<GameObject>();
        foreach (GameObject g in yTitles)
        {
            Destroy(g);
        }
        yTitles = new List<GameObject>();
        foreach (GameObject g in xAxisDividers)
        {
            Destroy(g);
        }
        xAxisDividers = new List<GameObject>();
        foreach (GameObject g in yAxisDividers)
        {
            Destroy(g);
        }
        yAxisDividers = new List<GameObject>();

        AddDefaultObjects();
        SetUpGraph();

    }

    IEnumerator AddValues()
    {
        while (true)
        {
            currentGraph.AddDataPoint(Random.Range(0, 100));
            yield return null;
        }
    }

}





[System.Serializable]
public class GraphData
{
    public Grapher grapher;
    public string title;
    public List<int> data;
    List<int> sortedData;
    public int maxYValue;

    public GraphData(string title, List<int> data, Grapher grapher)
    {
        this.title = title;
        this.data = data;
        this.grapher = grapher;

        data.Sort();
        this.maxYValue = data[data.Count - 1];

    }
    public void AddDataPoint(int point)
    {
        Debug.Log("Add Data Point");
        //data.Add(point);
        if (point > maxYValue)
            maxYValue = point;
        grapher.AdjustGraph();
    }

    public void ReplaceData(List<int> data)
    {

        Debug.Log("Replace Data");
        this.data = data;

        maxYValue = 0;
        foreach(int i in data)
        {
            if (i > maxYValue)
                maxYValue = i;
        }

        if (maxYValue < grapher.minGraphValue)
            maxYValue = grapher.minGraphValue;
        grapher.AdjustGraph();

        grapher.transform.parent.Find("Title").GetComponent<TextMeshProUGUI>().text = title;
    }
    public void ReplaceDataStart(List<int> data)
    {

        Debug.Log("Replace Data");
        this.data = data;

        maxYValue = 0;
        foreach (int i in data)
        {
            if (i > maxYValue)
                maxYValue = i;
        }

        if (maxYValue < grapher.minGraphValue)
            maxYValue = grapher.minGraphValue;

        grapher.transform.parent.Find("Title").GetComponent<TextMeshProUGUI>().text = title;
    }

}
