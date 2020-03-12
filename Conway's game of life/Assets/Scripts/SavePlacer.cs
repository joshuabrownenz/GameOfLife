using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePlacer : MonoBehaviour
{

    public static SavePlacer main;
    Controller controller;
    public bool placing, justPlaced, isNewGrid;
    bool[,] grid;
    Vector2Int prevCoords, prevSize;
   
    // Start is called before the first frame update
    void Start()
    {

        main = this;
        controller = Controller.main;
        prevCoords = new Vector2Int(1, 1);

        
    }

    // Update is called once per frame
    void Update()
    {
        justPlaced = false;
        Vector2Int coords = GetCoords();
        if (coords == new Vector2Int(-1, -1))
            coords = prevCoords;

        if (placing)
        {
            if(Input.GetMouseButtonDown(0))
            {
                PlaceGrid(coords);
                placing = false;
                justPlaced = true;
                return;
            }

            if(coords != prevCoords)
            {
                ClearGrid();
            }

            DrawGrid(coords);
        }

        prevCoords = coords;
    }

    void PlaceGrid(Vector2Int coords)
    {
        Controller.main.OnEdit();
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                int xCoord = coords.x + x;
                int yCoord = coords.y - (grid.GetLength(1) - y) + 1;

                
                if (xCoord > controller.size.x || yCoord < 1)
                    continue;

                Color color = Color.white;
                if (grid[x, y])
                    color = Color.black;

                controller.spriteRenderers[xCoord, yCoord].color = color;
                controller.grid[xCoord, yCoord] = grid[x, y];
            }
        }
    }

    void ClearGrid()
    {
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                int xCoord = prevCoords.x + x;
                int yCoord = prevCoords.y - (grid.GetLength(1) - y) + 1;

                if (xCoord > controller.size.x || yCoord < 1)
                    continue;

                Color color = Color.white;
                if (controller.grid[xCoord, yCoord])
                    color = Color.black;

                try
                { 
                    controller.spriteRenderers[xCoord, yCoord].color = color;
                }
                catch
                {
                    Debug.Log("Clear Error Stats");
                    Debug.Log("Prev Coords: " + prevCoords);
                    Debug.Log("Edit Coords: (" + xCoord + ", " + yCoord + ")");

                }


        }
        }
    }

    void DrawGrid(Vector2Int coords)
    {
        for(int y = 0; y < grid.GetLength(1); y++)
        {
            for(int x = 0; x < grid.GetLength(0); x++)
            {
                int xCoord = coords.x + x;
                int yCoord = coords.y - (grid.GetLength(1) - y) + 1;

                if (xCoord > controller.size.x || yCoord < 1)
                    continue;

                //Debug.Log(xCoord + "," + yCoord);

                Color color = Color.white;
                if (grid[x, y])
                    color = Color.black;

                try
                {
                    controller.spriteRenderers[xCoord, yCoord].color = color;
                }
                catch
                {
                    Debug.Log("Draw Error Stats");
                    Debug.Log("Coords: " + coords);
                    Debug.Log("Edit Coords: (" + xCoord + ", " + yCoord + ")");

                }
        }
        }
    }


    public void ActivatePlacement(bool[,] newGrid)
    {
        if(grid != null)
            prevSize = new Vector2Int(grid.GetLength(0), grid.GetLength(1));
        grid = newGrid;
        placing = true;
        isNewGrid = true;
    }


    Vector2Int GetCoords()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            try
            {
                return hit.transform.gameObject.GetComponent<CellContainer>().position;
            }
            catch
            {
                return new Vector2Int(-1, -1);
            }
        }
        return new Vector2Int(-1, -1);
    }

}
