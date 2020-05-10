using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Grapher : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject xAxisTitlePrefab;
    [SerializeField] GameObject yAxisTitlePrefab, xAxisDividerPrefab, yAxisDividerPrefab;

    [Header("Sprite")]
    [SerializeField] Sprite pointSprite;

    [Header("Smallest value to display")]
    [SerializeField] public int minGraphValue;

    [SerializeField]
    int yMax, xMax, xSteps, ySteps, yInterval, xInterval;
    bool overMainArea;

    [Header("Set gap between dividers")]
    [SerializeField] public float minGap;

    [Header("Compression Settings")]
    [SerializeField] public int maxAmountofPointsToShow;
    [SerializeField] public int compressionFactor = 1;

    [Header("Graph navigation")]
    [SerializeField] public float zoomFactor = 1;
    float prevZoomFactor = 1;
    [SerializeField] public float zoomSensitivity;
    [SerializeField] public float zoomOffset = 0;
    [SerializeField] public float zoomOffsetTest = 0;

    //Divider and title data
    float xGap, yGap, xTitleGap, yTitleGap;
    Vector2 prevMousePosition;

    [Header("Set gap sizes")]
    [SerializeField] int[] increments;

    [Header("Current Graph Data")]
    [SerializeField] public GraphData currentGraph;

    [Header("Type of graph")]
    public Statistics.Stat representing = Statistics.Stat.notSelected;

    //Lists of gameobjects
    List<GameObject> points = new List<GameObject>(), joiners = new List<GameObject>(), xTitles = new List<GameObject>(), yTitles = new List<GameObject>();
    List<GameObject> xAxisDividers = new List<GameObject>(), yAxisDividers = new List<GameObject>();

    //Parents
    Transform pointsParent, joinerParent, xTextParents, yTextParents, xAxisDividerParent, yAxisDividerParent;

    //The rect transform of the inner graph area 
    RectTransform rectTransform;

    void Start()
    {
        //Get Instances
        rectTransform = GetComponent<RectTransform>();

        //Set parents of each of the object types
        #region Parents
        pointsParent = transform.Find("Points");
        joinerParent = transform.Find("Lines");
        xTextParents = transform.parent.Find("X Axis Text");
        yTextParents = transform.parent.Find("Y Axis Text");
        xAxisDividerParent = transform.Find("Dividers").Find("X Axis Divider");
        yAxisDividerParent = transform.Find("Dividers").Find("Y Axis Divider");
        #endregion

        //See method below
        AddDefaultObjects();

        //Assign deafult values
        currentGraph.grapher = this;
        representing = Statistics.main.typeToBecome;
        Statistics.main.graphs[(int)representing] = this;

        //Set title of graph
        currentGraph.title = Statistics.titles[representing];

        

        //Set data from the statisics object
        currentGraph.ReplaceDataStart(Statistics.main.stats[(int)representing]);

        SetUpGraph();
    }

    //Fixes an error where points didn't line up with dividers since the indexes were off
    public void AddDefaultObjects()
    {
        xAxisDividers.Add(null);
        yAxisDividers.Add(null);
        joiners.Add(null);
    }

    private void Update()
    {
        if(overMainArea)
        {
            zoomFactor += Input.GetAxis("MouseScrollWheel") * zoomSensitivity;
            if(zoomFactor < 1)
                zoomFactor = 1;

            if(Input.GetMouseButtonDown(0))
            {
                prevMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                zoomOffset += prevMousePosition.x - Input.mousePosition.x;
                if (zoomOffset < 0)
                    zoomOffset = 0;
                prevMousePosition = Input.mousePosition;
            }
            AdjustGraph();
        }
    }

    public void OverMainArea()
    {
        overMainArea = true;
        LoadCamera.main.allowZoom = false;
    }

    public void notOverMainArea()
    {
        overMainArea = false;
        LoadCamera.main.allowZoom = true;
    }


    /// <summary>
    /// Work out gaps and increments between points
    /// </summary>
    void CalculateIncrements(float length, int points, bool isX)
    {
        int index = 0;
        int totalPoints = 0;


        //Checks each increment to see if it satifies the minGap requirement adding a buffer to the points to leave at least a divider in place
        for (int i = 0; i < increments.Length; i++)
        {
            index = i;
            totalPoints = Mathf.CeilToInt(points / (float)increments[i]) * increments[i] + increments[i];
            if ((length / (totalPoints / increments[i])) > minGap)
            {
                break;
            }

        }

        //If there is going to be less than space for the minAmount of points set the number of point to that value
        if (totalPoints < minGraphValue)
            totalPoints = minGraphValue;

        if (isX)
        {
            //Max is the highest value the titles will display the bounds of the graph
            xMax = totalPoints;
            //Is the number of possible points between dividers
            xInterval = increments[index];
            //Amount of dividers plus the 2 on either side
            xSteps = totalPoints / increments[index];
            //Gap between points
            xGap = length / totalPoints * zoomFactor;
            //Gap between titles and dividers > minGap
            xTitleGap = length / xSteps * zoomFactor;

            //Calculate Compression Factor

            int pointsOnScreen = Mathf.CeilToInt((rectTransform.rect.width / xGap) / compressionFactor);
            if (pointsOnScreen > currentGraph.data[currentGraph.dataToDisplay].Count)
                pointsOnScreen = currentGraph.data[currentGraph.dataToDisplay].Count;

            compressionFactor = Mathf.CeilToInt((float)pointsOnScreen/ (float)maxAmountofPointsToShow);
            if (compressionFactor < 1)
                compressionFactor = 1;
        }
        else
        {
            //Same as before
            yMax = totalPoints;
            yInterval = increments[index];
            ySteps = totalPoints / increments[index];
            yGap = length / totalPoints;
            yTitleGap = length / ySteps;
        }
    }

    /// <summary>
    /// Adjusts the graph after a change has been made to the data or size
    /// </summary>
    public void AdjustGraph()
    {
        //Previous steps
        int prevXSteps = xSteps, prevYSteps = ySteps;

        //Calculate data for displaying the graph
        CalculateIncrements(rectTransform.rect.width, currentGraph.data[(int)GraphData.dataType.pure].Count, true);
        CalculateIncrements(rectTransform.rect.height, currentGraph.maxYValue, false);

        currentGraph.CompressData(compressionFactor);


        #region Text and Dividers

        //X Axis Titles and Dividers
        //If there is less titles/dividers than before
        if (prevXSteps > xSteps)
        {
            for (int i = 0; i < prevXSteps; i++)
            {
                //If over the new ammount of titles 
                if (i >= xSteps)
                {
                    //Delete titles
                    GameObject g = xTitles[xSteps];
                    xTitles.RemoveAt(xSteps);
                    Destroy(g);

                    //Delete dividers
                    g = xAxisDividers[xSteps];
                    xAxisDividers.RemoveAt(xSteps);
                    Destroy(g);

                }
                //Otherwise just move title and dividers to new position
                else
                {
                    ModifyXAxisTitle(i);
                }

            }
        }
        // Else there is now more steps then there used to be or the same amount
        else
        {
            
            for (int i = 0; i <= xSteps; i++)
            {
                //If more create
                if (i > prevXSteps)
                {
                    CreateXAxisTitle(i);
                }
                //Else move to new postion
                else
                {
                    ModifyXAxisTitle(i);
                }
            }
        }
        //Y Axis Titles and Dividers
        //If there is less titles/dividers han before
        if (prevYSteps > ySteps)
        {
            for (int i = 0; i < prevYSteps; i++)
            {
                //If over the new ammount of titles 
                if (i >= ySteps)
                {
                    //Delete titles
                    GameObject g = yTitles[ySteps];
                    yTitles.RemoveAt(ySteps);
                    Destroy(g);

                    //Delete Dividers
                    g = yAxisDividers[ySteps];
                    yAxisDividers.RemoveAt(ySteps);
                    Destroy(g);

                }
                //Otherwise just move title and dividers to new position
                else
                {
                    ModifyYAxisTitle(i);
                }

            }
        }
        // Else there is now more steps then there used to be or the same amount
        else
        {

            for (int i = 0; i <= ySteps; i++)
            {
                //If more create
                if (i > prevYSteps)
                {
                    CreateYAxisTitle(i);
                }
                //Else move to new position
                else
                {
                    ModifyYAxisTitle(i);
                }
            }
        }
        #endregion

        int pointsOnScreen = Mathf.CeilToInt((rectTransform.rect.width / xGap) / compressionFactor);
        if (pointsOnScreen > currentGraph.data[currentGraph.dataToDisplay].Count)
            pointsOnScreen = currentGraph.data[currentGraph.dataToDisplay].Count;
        //Debug.Log("1. Points on screen = " + pointsOnScreen);
        //Debug.Log("2. rectTransform.rect.width / xGap = " + (rectTransform.rect.width / xGap));
        //Debug.Log("3. total points = " + currentGraph.data[currentGraph.dataToDisplay].Count);
        //Same structure as before just for points and lines
        //More than last time or same amount
        if (pointsOnScreen >= points.Count)
        {
            for (int i = 0; i < pointsOnScreen; i++)
            {
                //if(xGap * i * compressionFactor > rectTransform.rect.width)
                //{
                //    GameObject g = points[currentGraph.data[currentGraph.dataToDisplay].Count];
                //    points.RemoveAt(currentGraph.data[currentGraph.dataToDisplay].Count);
                //    Destroy(g);

                //    g = joiners[currentGraph.data[currentGraph.dataToDisplay].Count];
                //    joiners.RemoveAt(currentGraph.data[currentGraph.dataToDisplay].Count);
                //    Destroy(g);
                //    continue;
                //}
                //Create 
                if (i >= points.Count)
                {
                    CreatePoint(new Vector2(xGap * i * compressionFactor, yGap * currentGraph.data[currentGraph.dataToDisplay][i]));
                    CreateJoiner(i, false);
                }
                //Move
                else
                {
                    ModifyPoint(i);
                    ModifyJoiner(i, false);
                }
            }
        }
        //Less than last time
        else
        {
            for (int i = 0; i < points.Count; i++)
            {
                //Remove Points
                if (i >= pointsOnScreen)
                {
                    GameObject g = points[pointsOnScreen];
                    points.RemoveAt(pointsOnScreen);
                    Destroy(g);

                    g = joiners[pointsOnScreen];
                    joiners.RemoveAt(pointsOnScreen);
                    Destroy(g);
                }
                //Move
                else
                {
                    ModifyPoint(i);
                    ModifyJoiner(i, false);
                }
            }
        }
        if (pointsOnScreen < currentGraph.data[currentGraph.dataToDisplay].Count && currentGraph.data[currentGraph.dataToDisplay].Count > 5) 
        {
            CreateHiddenPoint(new Vector2(xGap * pointsOnScreen * compressionFactor, yGap * currentGraph.data[currentGraph.dataToDisplay][pointsOnScreen]));
            CreateJoiner(pointsOnScreen, true);
        }
    }


    /// <summary>
    /// Takes care of the initial construction of the graph
    /// </summary>
    void SetUpGraph()
    {
        CalculateIncrements(rectTransform.rect.width, currentGraph.data[currentGraph.dataToDisplay].Count, true);
        CalculateIncrements(rectTransform.rect.height, currentGraph.maxYValue, false);

        currentGraph.CompressData(compressionFactor);

        //Create Titles
        for (int i = 0; i <= xSteps; i++)
        {
            CreateXAxisTitle(i);
        }
        for (int i = 0; i <= ySteps; i++) 
        {
            CreateYAxisTitle(i);
        }

        //Create points and joiners 
        for (int i = 0; i < currentGraph.data[currentGraph.dataToDisplay].Count; i++)
        {
            CreatePoint(new Vector2(xGap * i * compressionFactor, yGap * currentGraph.data[currentGraph.dataToDisplay][i]));
            CreateJoiner(i, false);
        }
        AdjustGraph();

    }

    /// <summary>
    /// Creates a point at given coordinate
    /// </summary>
    GameObject CreatePoint(Vector2 anchoredPosition)
    {
        //Create the point using the point image and set parent to the pointsParent 
        GameObject gameObject = new GameObject("Circle", typeof(Image));
        gameObject.transform.SetParent(pointsParent, false);
        gameObject.GetComponent<Image>().sprite = pointSprite;

        //Set position and size to correct values
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(15, 15);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchoredPosition = anchoredPosition;
        //Add to list of gameObejcts 
        points.Add(gameObject);

        return gameObject;
    }

    GameObject CreateHiddenPoint(Vector2 anchoredPosition)
    {
        //Create the point using the point image and set parent to the pointsParent 
        GameObject gameObject = new GameObject("Circle", typeof(Image));
        gameObject.transform.SetParent(pointsParent, false);
        gameObject.GetComponent<Image>().sprite = pointSprite;
        gameObject.SetActive(false);

        //Set position and size to correct values
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(15, 15);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchoredPosition = anchoredPosition;
        //Add to list of gameObejcts 
        points.Add(gameObject);

        return gameObject;
    }

    /// <summary>
    /// Gets point from index and moves it to the correct location
    /// </summary>
    GameObject ModifyPoint(int index)
    {
        //Retrive from index 
        GameObject gameObject = points[index];
        gameObject.SetActive(true);
        //Move to new postion
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(xGap * index * compressionFactor, yGap * currentGraph.data[currentGraph.dataToDisplay][index]);

        return gameObject;
    }

    /// <summary>
    /// Create a joiner based off index
    /// </summary>
    GameObject CreateJoiner(int index, bool cutRightSide)
    {
        //Don't create a Joiner infront of the first point
        if (index == 0)
            return null;

        //Create the line (Which is a rectangle tilted)
        GameObject gameObject = new GameObject("Line", typeof(Image));
        gameObject.transform.SetParent(joinerParent, false);

        //Set the colour to a greyish colour
        gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        //Get point on either side
        Vector2 v1 = new Vector2(xGap * index * compressionFactor, yGap * currentGraph.data[currentGraph.dataToDisplay][index]);
        Vector2 v2 = new Vector2(xGap * (index - 1) * compressionFactor, yGap * currentGraph.data[currentGraph.dataToDisplay][index - 1]);


        if(cutRightSide)
        {
            float m = (v2.y - v1.y) / (v2.x - v1.x);
            //Equation: y = mx - m*v1.x + v1.y
            v1 = new Vector2(this.rectTransform.rect.width, (m * this.rectTransform.rect.width) - (m * v2.x) + (v2.y));
        }
        //Find the distance between the points which is the length of the line
        float distance = Vector2.Distance(v1, v2);

        //Set size and position
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = (v1 + v2) / 2;
        rectTransform.sizeDelta = new Vector2(distance, 6f);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        //Set rotation based off the two points
        rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(v1.y - v2.y,v1.x - v2.x));

        //Add the the list of joiners 
        joiners.Add(gameObject);

        return gameObject;
    }

    /// <summary>
    /// Modify the position of an already created Line/Joiner
    /// </summary>
    GameObject ModifyJoiner(int index, bool cutRightSide)
    {
        //Don't do the first one
        if (index == 0)
            return null;

        //Retrieve from list
        GameObject gameObject = joiners[index];

        //Get points on either side
        Vector2 v1 = new Vector2(xGap * index * compressionFactor, yGap * currentGraph.data[currentGraph.dataToDisplay][index]);
        Vector2 v2 = new Vector2(xGap * (index - 1) * compressionFactor, yGap * currentGraph.data[currentGraph.dataToDisplay][index - 1]);

        if (cutRightSide)
        {
            float m = (v1.y - v2.y) / (v1.x - v2.x);
            //Equation: y = mx - m*v1.x + v1.y
            v1 = new Vector2(this.rectTransform.rect.width, (m * this.rectTransform.rect.width) - (m * v1.x) + (v1.y));
        }
        //Get distance between points which is length of the points
        float distance = Vector2.Distance(v1, v2);

        //Set size and rotation
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        if (distance < 7.5f)
        {
            //If it's short enough position it off screen to increase proformance
            rectTransform.anchoredPosition = new Vector2(-1000, -1000);
        }
        else
        {
            //Set size and rotation
            rectTransform.anchoredPosition = (v1 + v2) / 2;
            rectTransform.sizeDelta = new Vector2(distance, 6f);
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(v1.y - v2.y, v1.x - v2.x));
        }

        return gameObject;
    }

    /// <summary>
    /// Create a X Axis Title 
    /// </summary>
    GameObject CreateXAxisTitle(int index)
    {
        //Instantiate the prefab parented to the x axis text parent
        GameObject gameObject = Instantiate(xAxisTitlePrefab, xTextParents);

        //Set position
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(index * xTitleGap - zoomOffset, 0);

        //Set the text to the correct number
        gameObject.GetComponent<TextMeshProUGUI>().text = (index * xInterval).ToString();

        gameObject.SetActive(true);

        if (index * xTitleGap - zoomOffset > rectTransform.rect.width + 1 || index * xTitleGap - zoomOffset < -1)
        {
            gameObject.SetActive(false);
        }

        //Add to list of x axis titles
        xTitles.Add(gameObject);

        //Create the corresponding divider
        CreateXAxisDivider(index);

        return gameObject;
    }

    /// <summary>
    /// Modify x axis title based off index
    /// </summary>
    GameObject ModifyXAxisTitle(int index)
    {
        //Get the title from the list
        GameObject gameObject = xTitles[index];

        //Set position and number
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(index * xTitleGap - zoomOffset, 0);
        gameObject.GetComponent<TextMeshProUGUI>().text = (index * xInterval).ToString();
        gameObject.SetActive(true);

        if(index * xTitleGap - zoomOffset > rectTransform.rect.width + 1 || index * xTitleGap - zoomOffset < -1)
        {
            gameObject.SetActive(false);
        }

        //Modify the corresponding divider
        ModifyXAxisDivider(index);

        return gameObject;
    }

    /// <summary>
    /// Create an X axis divider based off index
    /// </summary>
    GameObject CreateXAxisDivider(int index)
    {
        //Do not create first an last divider
        if (index == 0 || index == xSteps)
            return null;

        //Instantiate, parent then adjust transform
        GameObject gameObject = Instantiate(xAxisDividerPrefab, xAxisDividerParent);
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(index * xTitleGap + 1.5f, 0);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.rect.height, 3);

        //Add to list of dividers 
        xAxisDividers.Add(gameObject);
       
        return gameObject;
    }

    /// <summary>
    /// Modify a previously created x axis divider
    /// </summary>
    GameObject ModifyXAxisDivider(int index)
    {
        //Do not move first or last null divider
        if (index == 0 || index >= xSteps)
            return null;

        //If there is not a divider created then create a new one
        if(index >= xAxisDividers.Count)
        {
            return CreateXAxisDivider(index);
        }

        //Retreive from list
        GameObject gameObject = xAxisDividers[index];

        //Adjust transform
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(index * xTitleGap + 1.5f, 0);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.rect.height, 3);

        return gameObject;
    }

    /// <summary>
    /// Create a y axis title based off an index
    /// </summary>
    GameObject CreateYAxisTitle(int index)
    {
        //Create from prefab
        GameObject gameObject = Instantiate(yAxisTitlePrefab, yTextParents);

        //Ajust tranform
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, index * yTitleGap);

        //Set text
        gameObject.GetComponent<TextMeshProUGUI>().text = (index * yInterval).ToString();

        //Add to list of y axis titles
        yTitles.Add(gameObject);

        //Create corrseponding divider
        CreateYAxisDivider(index);

        return gameObject;
    }

    /// <summary>
    /// Modify Y axis title based off index
    /// </summary>
    GameObject ModifyYAxisTitle(int index)
    {
        //Retrieve from list
        GameObject gameObject = yTitles[index];

        //Adjust position 
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, index * yTitleGap);

        //Adjust text of title
        gameObject.GetComponent<TextMeshProUGUI>().text = (index * yInterval).ToString();

        //Modify corrseponding divider
        ModifyYAxisDivider(index);
        return gameObject;
    }

    /// <summary>
    /// Create Y axis divider based off index 
    /// </summary>
    GameObject CreateYAxisDivider(int index)
    {
        //Do not create if first or last
        if (index == 0 || index == ySteps)
            return null;

        //Instantiate object from prefab
        GameObject gameObject = Instantiate(yAxisDividerPrefab, yAxisDividerParent);

        //Adjust transform
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, index * yTitleGap);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.rect.width, 3);

        //Add to list of Y axis dividers
        yAxisDividers.Add(gameObject);

        return gameObject;
    }

    /// <summary>
    /// Modify previously created y axis dividers
    /// </summary>
    GameObject ModifyYAxisDivider(int index)
    {
        //Do not adjust if first or last
        if (index == 0 || index >= ySteps)
            return null;

        //If does not exist currently 
        if (index >= yAxisDividers.Count)
        {
            return CreateYAxisDivider(index);
        }

        //Retreive from list
        GameObject gameObject = yAxisDividers[index];

        //Adjust transform
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, index * yTitleGap + 1.5f);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.rect.width, 3);

        return gameObject;
    }

    /// <summary>
    /// Reset all lists to default values, then recreate graph
    /// </summary>
    public void ResetGraph()
    {
        foreach(GameObject g in points) { Destroy(g); }
        points = new List<GameObject>();
        foreach (GameObject g in joiners) { Destroy(g); }
        joiners = new List<GameObject>();
        foreach (GameObject g in xTitles) { Destroy(g); }
        xTitles = new List<GameObject>();
        foreach (GameObject g in yTitles) { Destroy(g); }
        yTitles = new List<GameObject>();
        foreach (GameObject g in xAxisDividers) {Destroy(g);}
        xAxisDividers = new List<GameObject>();
        foreach (GameObject g in yAxisDividers) { Destroy(g); }
        yAxisDividers = new List<GameObject>();

        AddDefaultObjects();
        SetUpGraph();

    }

}




/// <summary>
/// This class contains all data needed to create a graph
/// </summary>
[System.Serializable]
public class GraphData
{
    //The grapher resposible for rendering this graph
    public Grapher grapher;

    //The title to display at the top of the graph
    public string title;

    public enum dataType
    {
        pure,
        compressed
    }

    public int dataToDisplay;

    //The data
    public List<int>[] data = new List<int>[2];

    //Maximum y value to display
    public int maxYValue;

    //Compression
    int prevCompressionFactor = 1;
    int uncompressedValues = 0; 
   
    //Constructor
    public GraphData(string title, List<int> data, Grapher grapher)
    {
        this.data = new List<int>[2];
        this.title = title;
        this.data[(int)dataType.pure] = data;
        dataToDisplay = (int)dataType.pure;
        this.grapher = grapher;
        

        data.Sort();
        this.maxYValue = data[data.Count - 1];

    }

    //Add a new point to an already created graph data which calculates the max value
    public void AddDataPoint(int point)
    {
        if (point > maxYValue)
            maxYValue = point;
        grapher.AdjustGraph();
    }

    //Replace the data to display
    public void ReplaceData(List<int> data)
    {
        this.data = new List<int>[2];
        this.data[(int)dataType.pure] = data;

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

    //Replace the data before it has been displayed
    public void ReplaceDataStart(List<int> data)
    {
        this.data = new List<int>[2];
        this.data[(int)dataType.pure] = data;
        dataToDisplay = (int)dataType.pure;

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

    public void CompressData(int compressionFactor)
    {
        data[(int)dataType.compressed] = new List<int>();

        if(compressionFactor <= 1)
        {
            dataToDisplay = (int)dataType.pure;
            return;
        }

        int lastIndex = 0;
        for(int index = 0; index < data[(int)dataType.pure].Count; index += compressionFactor)
        {
            lastIndex = index;
            int maxValue = data[(int)dataType.pure][index];
            for(int i = 1; i < compressionFactor; i++)
            {
                if (data[(int)dataType.pure].Count <= index + i)
                {
                    break;
                }
                if(maxValue < data[(int)dataType.pure][index + i])
                {
                    maxValue = data[(int)dataType.pure][index + i];
                }
            }
            data[(int)dataType.compressed].Add(maxValue);
        }
        uncompressedValues = data[(int)dataType.pure].Count - lastIndex - 1;
        dataToDisplay = (int)dataType.compressed;
    }


}
