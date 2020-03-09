using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Controller : MonoBehaviour
{
    //Instances
    public static Controller main;
    Statistics statistics;

    [Header("Constants")]
    [SerializeField]
    public Vector2Int size;
    [SerializeField]
    public bool recordStats;
    [SerializeField]
    float holdTime, gapHoldLength;

    [Header("Prefabs")]
    [SerializeField]
    GameObject cell;

    //Relates to the grid arrays
    public bool[,] grid {get; private set;}
    bool[,] newGrid;
    public SpriteRenderer[,] spriteRenderers;

    [Header("States")]
    [SerializeField]
    public bool start;
    public bool running;

    //History
    public bool[][,] history = new bool[100][,];
    [Header("History")]
    public int historyIndex = 0;
    public int historyLimit = 0;
    bool arrowHold;
    float holdStartTime;
    float holdRunTime;
    
    //Time Values
    float timeOfCompute;

    private void Awake()
    {
        //Start Singleton
        main = this;
    }

    void Start()
    {
        //Assign Statistics main instance
        statistics = Statistics.main;

        //Set size of multidimentional array
        grid = new bool[size.x + 2, size.y + 2];
        spriteRenderers = new SpriteRenderer[size.x + 2, size.y + 2];

        //Create Parent of cells
        GameObject parent = new GameObject("Grid");

        //
        for (int y = 1; y <= size.y; y++)
        {
            for (int x = 1; x <= size.x; x++)
            {
                GameObject obj = Instantiate(cell);
                obj.name = "Cell: (" + x + ", " + y + ")";
                obj.transform.position = new Vector2(x / 2f - 0.25f - size.x/4f, y / 2f - 0.25f - size.y/4f);
                spriteRenderers[x, y] = obj.GetComponent<SpriteRenderer>();
                obj.transform.parent = parent.transform;
                obj.GetComponent<CellContainer>().position = new Vector2Int(x, y);
            }
        }
        for (int x = 0; x <= size.x; x++)
        {
            grid[x, 0] = false;
        }
        for (int x = 0; x <= size.x; x++)
        {
            grid[x, size.y + 1] = false;
        }
        for (int y = 0; y <= size.y; y++)
        {
            grid[0, y] = false;
        }
        for (int y = 0; y <= size.y; y++)
        {
            grid[size.x + 1, y] = false;
        }
        newGrid = grid;

        //for(int y = 1; y <= 100; y++)
        //{
        //    for (int x = 1; x <= 100; x++)
        //    {
        //        int num = y * 99 + x;
        //        if(num%2 == 0)
        //            newGrid[x, y] = true;
        //    }
        //}

        RenderGrid();

        for (int i = 0; i < 100; i++)
        {
            history[i] = new bool[size.x + 2, size.y + 2];
        }


       

    }

    // Update is called once per frame
    void Update()
    {
        if(start)
        {
            if (!running)
            {
                StartCoroutine(compute());
            }
        }
        else
        {
            if(Input.GetKeyUp(KeyCode.RightArrow) ||  Input.GetKeyUp(KeyCode.LeftArrow))
            {
                arrowHold = false;
            }

            if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                arrowHold = true;
                holdStartTime = Time.time;
                RightMove();
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                arrowHold = true;
                holdStartTime = Time.time;
                LeftMove();
            }

            if (arrowHold && ((holdStartTime + holdTime) < Time.time) && ((holdRunTime + gapHoldLength) < Time.time))
            {
                if(Input.GetKey(KeyCode.RightArrow) && !running)
                {
                    RightMove();
                } 
                else if(Input.GetKey(KeyCode.LeftArrow))
                {
                    LeftMove();
                }
                holdRunTime = Time.time;
            }

            //if ((Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKeyDown(KeyCode.Z))
            //{
            //    arrowHold = true;
            //    holdStartTime = Time.time;
            //    CtrlZ();
            //}


        }
    }

    IEnumerator compute()
    {
        Statistics.main.framesPerSecond = 1f / (Time.time - timeOfCompute);
        timeOfCompute = Time.time;
        running = true;
        if (Statistics.main.stats[0].Count == 0)
        {
            CalculateInitialValues();
        }
        if (!recordStats)
            CalculateNewGrid();
        else
            CalculateNewGridWithStats();
        RenderGrid();
        yield return null;
        running = false;
    }

    public void CalculateInitialValues()
    {
        Statistics.main.stats[(int)Statistics.Stat.deaths].Add(0);
        Statistics.main.stats[(int)Statistics.Stat.births].Add(0);

        int alive = 0;
        for (int y = 1; y <= size.y; y++)
        {
            for (int x = 1; x <= size.x; x++)
            {
                if (grid[x, y])
                    alive++;
            }
        }
        Statistics.main.stats[(int)Statistics.Stat.aliveCells].Add(alive);
    }
    void CalculateNewGrid()
    {
        newGrid = new bool[size.x + 2, size.y + 2];
        for (int y = 1; y <= size.y; y++)
        {
            for (int x = 1; x <= size.x; x++)
            {
                switch (checkSquares(x, y))
                {
                    case 0:
                        newGrid[x, y] = false;
                        break;
                    case 1:
                        newGrid[x, y] = false;
                        break;
                    case 2:
                        if (grid[x, y])
                            newGrid[x, y] = true;
                        else
                            newGrid[x, y] = false;
                        break;
                    case 3:
                        newGrid[x, y] = true;
                        break;
                    case 4:
                        newGrid[x, y] = false;
                        break;

                }
            }
        }

        AddToHistoryIndex();

        history[historyIndex] = newGrid;

    }

    void CalculateNewGridWithStats()
    {
        int deaths = 0, births = 0 , alive = 0;
        newGrid = new bool[size.x + 2, size.y + 2];
        for (int y = 1; y <= size.y; y++)
        {
            for (int x = 1; x <= size.x; x++)
            {
                switch (checkSquares(x, y))
                {
                    case 0:
                        newGrid[x, y] = false;
                        if (grid[x, y])
                            deaths++;
                        break;
                    case 1:
                        newGrid[x, y] = false;
                        if (grid[x, y])
                            deaths++;
                        break;
                    case 2:
                        if (grid[x, y])
                        {
                            newGrid[x, y] = true;
                            alive++;
                        }
                        else
                            newGrid[x, y] = false;
                        break;
                    case 3:
                        newGrid[x, y] = true;
                        alive++;
                        if (!grid[x,y])
                        {
                            births++;
                        }
                        break;
                    case 4:
                        newGrid[x, y] = false;
                        if (grid[x, y])
                            deaths++;
                        break;

                }
            }
        }

        statistics.AddValue(deaths, Statistics.Stat.deaths);
        statistics.AddValue(births, Statistics.Stat.births);
        statistics.AddValue(alive, Statistics.Stat.aliveCells);


        AddToHistoryIndex();

        history[historyIndex] = newGrid;

    }

    public void AddToHistoryIndex()
    {
        bool adjustLimit = false;
        if (historyLimit == historyIndex)
            adjustLimit = true;

        historyIndex++;
        if (historyIndex > 99)
            historyIndex = 0;

        if (adjustLimit)
            historyLimit = historyIndex;
    }

    void RenderGrid()
    {
        grid = newGrid;
        for (int y = 1; y <= size.y; y++)
        {
            for (int x = 1; x <= size.x; x++)
            {
                if (grid[x, y])
                {
                    spriteRenderers[x, y].color = Color.black;
                }
                else
                {
                    spriteRenderers[x, y].color = Color.white;
                }
                  
            }
            
        }
    }

    int checkSquares(int x, int y)
    {
        int sum = 0;
        if (grid[x - 1, y + 1])
            sum++;
        if (grid[x - 1, y])
            sum++;
        if (grid[x - 1, y - 1])
            sum++;
        if (grid[x, y + 1])
            sum++;
        if (grid[x, y - 1])
            sum++;
        if (grid[x + 1, y + 1])
            sum++;
        if (grid[x + 1, y])
            sum++;
        if (grid[x + 1, y - 1])
            sum++;
        if(sum>4)
            sum = 4;
        return sum;
    }

    public void Modify(int x, int y, bool b)
    {
        grid[x, y] = b;
        if (b)
            spriteRenderers[x, y].color = Color.black;
        else
            spriteRenderers[x, y].color = Color.white;
    }
    public void Modify(Vector2Int pos, bool b)
    {
        grid[pos.x, pos.y] = b;
        if (b)
            spriteRenderers[pos.x, pos.y].color = Color.black;
        else
            spriteRenderers[pos.x, pos.y].color = Color.white;
    }

    public void ModifyView(Vector2Int pos, bool b)
    {
        if (b)
            spriteRenderers[pos.x, pos.y].color = Color.black;
        else
            spriteRenderers[pos.x, pos.y].color = Color.white;
    }


    public IEnumerator RandomCellsCoroutine()
    {
        if (start || running)
        {
            start = false;
            yield return null; ;
        }

        if (!start)
        {
            grid = new bool[size.x + 2, size.y + 2];
            newGrid = grid;
            for (int y = 1; y <= size.y; y++)
            {
                for (int x = 1; x <= size.x; x++)
                {
                    int rand = Random.Range(1, 3);
                    if (rand == 1)
                        newGrid[x, y] = true;
                }
            }
            RenderGrid();
        }
        yield return null;
    }

    public void Clear()
    {
        newGrid = new bool[size.x + 2, size.y + 2];
        RenderGrid();
        OnEdit();
    }

    public void RightMove()
    {
        history[historyIndex] = grid;
        StartCoroutine(compute());

        //if (historyIndex == historyLimit)
        //{
        //  StartCoroutine(compute());
        //  return;
        //}
        //else
        //{
        //    historyIndex++;
        //    if (historyIndex > 99)
        //        historyIndex = 0;
        //    newGrid = history[historyIndex];
        //    Debug.Log(historyIndex);
        //    RenderGrid();
        //}

    }

    public void LeftMove()
    {
        int tempHistoryIndex = historyIndex - 1;

        if(tempHistoryIndex < 0)
        {
            tempHistoryIndex = 99;
        }

        if (tempHistoryIndex == historyLimit)
        {
            return;
        }

        newGrid = history[tempHistoryIndex];

        historyIndex = tempHistoryIndex;
        Statistics.main.DeleteLast();
        RenderGrid();

        
    }

    //void CtrlZ()
    //{
    //    if(editNum > 0)
    //    {
    //        LeftMove();
    //    }
    //}

    public void OnEdit()
    {


        AddToHistoryIndex();

        //bool[,] tempGrid = new bool[size.x + 1, size.y];

        //for (int y = 1; y < size.y; y++)
        //{
        //    for (int x = 1; x < size.x; x++)
        //    {
        //        tempGrid[x, y] = grid[x, y];
        //    }
        //}

        object tempGrid = grid.Clone();

        history[historyIndex] = (bool[,])tempGrid;

    }
}
