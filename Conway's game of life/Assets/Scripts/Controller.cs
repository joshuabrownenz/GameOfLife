using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Controller : MonoBehaviour
{
    public static Controller controller;

    [SerializeField]
    public Vector2Int size;
    [SerializeField]
    bool[,] grid;
    bool[,] newGrid;
    SpriteRenderer[,] spriteRenderers;
    public bool start;
    private void Awake()
    {
        controller = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        grid = new bool[size.x + 2, size.y + 2];
        spriteRenderers = new SpriteRenderer[size.x + 2, size.y + 2];

        GameObject parent = new GameObject("Grid");
        for (int y = 1; y <= size.y; y++)
        {
            for (int x = 1; x <= size.x; x++)
            {
                GameObject obj = Instantiate(Resources.Load("cell") as GameObject);
                obj.name = "Cell: (" + x + ", " + y + ")";
                obj.transform.position = new Vector2(x / 2f - 0.25f, y / 2f - 0.25f);
                spriteRenderers[x, y] = obj.GetComponent<SpriteRenderer>();
                obj.transform.parent = parent.transform;
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

        newGrid[5, 5] = true;
        newGrid[6, 5] = true;
        newGrid[4, 5] = true;
        RenderGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if(start)
        {
            CalculateNewGrid();
            RenderGrid();
        }
        
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
    }

    void RenderGrid()
    {
        grid = newGrid;
        for (int y = 1; y < size.y; y++)
        {
            for (int x = 1; x < size.x; x++)
            {
                Debug.Log("(" + x + ", " + y + ")");
                if (grid[x, y])
                {
                    spriteRenderers[x, y].color = Color.black;
                    if (x == 2 && y == 1)
                        Debug.Log("Yes");
                }
                else
                {
                    spriteRenderers[x, y].color = Color.white;
                    if (x == 2 && y == 1)
                        Debug.Log("No");
                }
                  
            }
            Debug.Log("Y");
            
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

    public void modify(int x, int y, bool b)
    {
        grid[x, y] = b;
        if (b)
            spriteRenderers[x, y].color = Color.black;
        else
            spriteRenderers[x, y].color = Color.white;
    }
}
