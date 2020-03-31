using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //Instances
    public static Controller main;
    Statistics statistics;

    [Header("Constants")]
    [SerializeField] public Vector2Int size;
    [SerializeField] public bool recordStats;
    [SerializeField] float holdTime, gapHoldLength;
    [SerializeField] Slider slider;

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

    //History varibles
    MaxStack historyStack = new MaxStack(100);
    MaxStack redoStack = new MaxStack(100);
    //public bool[][,] history = new bool[100][,];
    [Header("History")]
    //public int historyIndex = 0;
    //public int historyLimit = 0;
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

        //Create Every Cell
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

        //Assign outside cells as false
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

        //Set all the cells to the correct colour
        RenderGrid();

        ////Assign array of Multidimentional array as the right size
        //for (int i = 0; i < 100; i++)
        //{
        //    history[i] = new bool[size.x + 2, size.y + 2];
        //}
    }

    void Update()
    {
        if(start)
        {
            if (!running)
            {
                //If it is meant to run and it is not already running run main compute coroutine
                StartCoroutine(compute());
            }
        }
        else
        {
            //Detect Key presses as well as hold
            #region Detect Key presses
            if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.Z)) 
            {
                arrowHold = false;
            }

            if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                arrowHold = true;
                holdStartTime = Time.time;
                RightMove();
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Z))
            {
                arrowHold = true;
                holdStartTime = Time.time;
                LeftMove();
            }
            if(Input.GetKeyDown(KeyCode.X))
            {
                arrowHold = true;
                holdStartTime = Time.time;
                Redo();
            }

            if (arrowHold && ((holdStartTime + holdTime) < Time.time) && ((holdRunTime + gapHoldLength) < Time.time))
            {
                if(Input.GetKey(KeyCode.RightArrow) && !running)
                {
                    RightMove();
                } 
                else if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Z))
                {
                    LeftMove();
                }
                else if(Input.GetKey(KeyCode.X))
                {
                    Redo();
                }
                holdRunTime = Time.time;
            }
            #endregion
        }
    }

    IEnumerator compute()
    {
        running = true;
        //Calculate frame rate 
        Statistics.main.framesPerSecond = 1f / (Time.time - timeOfCompute);
        timeOfCompute = Time.time;

        //Clear redo data
        redoStack.Clear();
        Statistics.main.redoData.Clear();

        //If stats has no entry then calculate intial values
        if (Statistics.main.stats[0].Count == 0)
        {
            CalculateInitialValues();
        }

        //Work out the new grid
        if (!recordStats)
            CalculateNewGrid();
        else
            CalculateNewGridWithStats();

        //Display grid
        RenderGrid();

        //Wait for the amount of time the slider dictates
        for (int i = 0; i < slider.value; i++)
        {
            yield return null;
        }

        running = false;
    }

    public void CalculateInitialValues()
    {
        //Asign deaths and births value to 0
        Statistics.main.stats[(int)Statistics.Stat.deaths].Add(0);
        Statistics.main.stats[(int)Statistics.Stat.births].Add(0);

        //Find alive cells
        int alive = 0;
        for (int y = 1; y <= size.y; y++)
        {
            for (int x = 1; x <= size.x; x++)
            {
                if (grid[x, y])
                    alive++;
            }
        }
        //Assign alive cells
        Statistics.main.stats[(int)Statistics.Stat.aliveCells].Add(alive);
    }

    void CalculateNewGrid()
    {
        //Create clear new grid to hold all changes
        newGrid = new bool[size.x + 2, size.y + 2];

        //Go through every cell but the outside ones 
        for (int y = 1; y <= size.y; y++)
        {
            for (int x = 1; x <= size.x; x++)
            {
                //Check every cells neighbours and apply rules
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

        //Adjust value of history index
        //AddToHistoryIndex();

        //Add grid to history
        historyStack.Push((bool[,])grid.Clone());
        print("Count is: " + historyStack.Count);

    }

    //Same as before just recording statistics as well
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


        //AddToHistoryIndex();


        historyStack.Push((bool[,])grid.Clone());
        print("Count is: " + historyStack.Count);
        //history[historyIndex] = (bool[,])tempGrid;

        //historyStack.Push((bool[,])grid.Clone());

    }

    //Obslete
    //public void AddToHistoryIndex()
    //{
    //    bool adjustLimit = false;
    //    if (historyLimit == historyIndex)
    //        adjustLimit = true;

    //    historyIndex++;
    //    if (historyIndex > 99)
    //        historyIndex = 0;

    //    if (adjustLimit)
    //        historyLimit = historyIndex;
    //}

    void RenderGrid()
    {
        //Display the grid based off its state in the grid[,]
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

    //Reutrns the number neighbours of a specfic cell
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

    //Change the state of a cell based off xy coords - used by editor
    public void Modify(int x, int y, bool b)
    {
        grid[x, y] = b;
        if (b)
            spriteRenderers[x, y].color = Color.black;
        else
            spriteRenderers[x, y].color = Color.white;
    }

    //Change the state of a cell based off Vector2Int - used by editor
    public void Modify(Vector2Int pos, bool b)
    {
        grid[pos.x, pos.y] = b;
        if (b)
            spriteRenderers[pos.x, pos.y].color = Color.black;
        else
            spriteRenderers[pos.x, pos.y].color = Color.white;
    }

    //Change display of a cell without changing its state - used by display tasks in editor
    public void ModifyView(Vector2Int pos, bool b)
    {
        if (b)
            spriteRenderers[pos.x, pos.y].color = Color.black;
        else
            spriteRenderers[pos.x, pos.y].color = Color.white;
    }

    //Randomizes grid
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
                    int rand = UnityEngine.Random.Range(1, 3);
                    if (rand == 1)
                        newGrid[x, y] = true;
                }
            }
            RenderGrid();
        }
        yield return null;
    }

    //Clear the grid
    public void Clear()
    {
        OnEdit();
        newGrid = new bool[size.x + 2, size.y + 2];
        RenderGrid();
    }

    //On a right key press do new generations
    public void RightMove()
    {
        historyStack.Push((bool[,])grid.Clone());
        //object tempGrid = grid.Clone();

        //history[historyIndex] = (bool[,])tempGrid;
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

    //Recall previous undone movement
    public void Redo()
    {
        if(redoStack.Count != 0)
        {
            //Retrive from history stack
            historyStack.Push((bool[,])grid.Clone());
            newGrid = redoStack.Pop();
            RenderGrid();

            //Add data if it exists
            if (Statistics.main.redoData.Count != 0)
            {
                Vector3Int data = Statistics.main.redoData.Pop();

                statistics.AddValue(data.x, Statistics.Stat.deaths);
                statistics.AddValue(data.y, Statistics.Stat.births);
                statistics.AddValue(data.z, Statistics.Stat.aliveCells);
            }
        }
    }

    //Go back one generation
    public void LeftMove()
    {
        //int tempHistoryIndex = historyIndex - 1;

        //if(tempHistoryIndex < 0)
        //{
        //    tempHistoryIndex = 99;
        //}

        //if (tempHistoryIndex == historyLimit)
        //{
        //    return;
        //}




        //historyIndex = tempHistoryIndex;

        redoStack.Push((bool[,])grid.Clone());
        newGrid = historyStack.Pop();
        

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

    //Saves a scene when it is edited
    public void OnEdit()
    {
        redoStack.Clear();
        Statistics.main.redoData.Clear();

        historyStack.Push((bool[,])grid.Clone());
        //AddToHistoryIndex();

        //object tempGrid = grid.Clone();

        //history[historyIndex] = (bool[,])tempGrid;

        //bool[,] tempGrid = new bool[size.x + 1, size.y];

        //for (int y = 1; y < size.y; y++)
        //{
        //    for (int x = 1; x < size.x; x++)
        //    {
        //        tempGrid[x, y] = grid[x, y];
        //    }
        //}


    }


    //!!!!This is not my code it was borrowed from https://ntsblog.homedev.com.au/index.php/2010/05/06/c-stack-with-maximum-limit/!!!

    /// <summary>
    /// Generic stack implementation with a maximum limit
    /// When something is pushed on the last item is removed from the list
    /// </summary>
    [Serializable]
    public class MaxStack
    {
        #region Fields

        private int _limit;
        private LinkedList<bool[,]> _list;

        #endregion

        #region Constructors

        public MaxStack(int maxSize)
        {
            _limit = maxSize;
            _list = new LinkedList<bool[,]>();

        }

        #endregion

        #region Public Stack Implementation

        public void Push(bool[,] value)
        {
            if (_list.Count == _limit)
            {
                _list.RemoveLast();
            }
            _list.AddFirst(value);
        }

        public bool[,] Pop()
        {
            if (_list.Count > 0)
            {
                bool[,] value = _list.First.Value;
                _list.RemoveFirst();
                return value;
            }
            else
            {

                return new bool[Controller.main.size.x + 2, Controller.main.size.y + 2];
            }


        }

        public bool[,] Peek()
        {
            if (_list.Count > 0)
            {
                bool[,] value = _list.First.Value;
                return value;
            }
            else
            {
                throw new InvalidOperationException("The Stack is empty");
            }

        }

        public void Clear()
        {
            _list.Clear();

        }

        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// Checks if the top object on the stack matches the value passed in
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsTop(bool[,] value)
        {
            bool result = false;
            if (this.Count > 0)
            {
                result = Peek().Equals(value);
            }
            return result;
        }

        public bool Contains(bool[,] value)
        {
            bool result = false;
            if (this.Count > 0)
            {
                result = _list.Contains(value);
            }
            return result;
        }

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

    }
}
