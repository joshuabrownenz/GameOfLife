using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Grapher : MonoBehaviour
{
    public static Grapher main;

    [SerializeField]
    GameObject xAxisTitlePrefab, yAxisTitlePrefab, xAxisDividerPrefab, yAxisDividerPrefab;
    [SerializeField]
    Sprite pointSprite;

    public int totalGenerations;

    Transform pointsParent, joinerParent, xTextParents, yTextParents, xAxisDividerParent, yAxisDividerParent;

    [SerializeField]
    int yMax, xMax, xSteps, ySteps, yInterval, xInterval;

    List<GameObject> points = new List<GameObject>(), joiners = new List<GameObject>(), xTitles = new List<GameObject>(), yTitles = new List<GameObject>();
    [SerializeField] List<GameObject> xAxisDividers = new List<GameObject>(), yAxisDividers = new List<GameObject>();

    [SerializeField]
    float minGap;


    [SerializeField]
    float gapMutliplier, xGap, yGap, xTitleGap, yTitleGap;

    [SerializeField]
    int[] increments;

    [SerializeField]
    GraphData currentGraph;

    BackgroundController backgroundController;

    RectTransform rectTransform;
    void Start()
    {
        main = this;
        backgroundController = BackgroundController.main;
        rectTransform = GetComponent<RectTransform>();

        pointsParent = transform.Find("Points");
        joinerParent = transform.Find("Lines");
        xTextParents = transform.Find("X Axis Text");
        yTextParents = transform.Find("Y Axis Text");
        xAxisDividerParent = transform.Find("X Axis Divider");
        yAxisDividerParent = transform.Find("Y Axis Divider");

        xAxisDividers.Add(null);
        yAxisDividers.Add(null);

        SetUpGraph();
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
            print(totalPoints);
            if ((length / (totalPoints / increments[i])) > minGap)
            {
                print("break");
                break;
            }

        }

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

    GameObject ModifyPoint(Vector2 anchoredPosition)
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

        print("Index: " + index);
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

        print("Index: " + index);
        if (index >= yAxisDividers.Count)
        {
            return CreateYAxisDivider(index);
        }
        GameObject gameObject = yAxisDividers[index];
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, index * yTitleGap + 1.5f);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.rect.width, 3);
        return gameObject;
    }
}





[System.Serializable]
public class GraphData
{
    public string title;
    public List<int> data;
    public int maxYValue;

    public GraphData(string title, List<int> data)
    {
        this.title = title;
        this.data = data;

        data.Sort();
        this.maxYValue = data[data.Count - 1];

    }
    public void AddDataPoint(int point)
    {
        data.Add(point);
        if (point > maxYValue)
            maxYValue = point;
    }

    public void ReplaceData(List<int> data)
    {
        this.data = data;

        data.Sort();
        this.maxYValue = data[data.Count - 1];
    }

}
