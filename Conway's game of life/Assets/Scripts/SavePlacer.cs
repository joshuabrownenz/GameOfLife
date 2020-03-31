using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePlacer : MonoBehaviour
{
    //Singleton Instance
    public static SavePlacer main;

    //Instances
    Controller controller;

    //States
    public bool placing, justPlaced, isNewGrid;

    //Grid to place
    bool[,] grid;

    //Previous position at which the 
    Vector2Int prevCoords;
   
    void Start()
    {
        //Assign instances
        main = this;
        controller = Controller.main;
        prevCoords = new Vector2Int(1, 1);
    }


    void Update()
    {
        //Set deafults
        justPlaced = false;

        //Get coords from raycast
        Vector2Int coords = GetCoords();

        //Mouse if not over grid use previous coordinates
        if (coords == new Vector2Int(-1, -1))
            coords = prevCoords;

        //If actively placing a save
        if (placing)
        {
            //If button is clicked place the grid
            if(Input.GetMouseButtonDown(0))
            {
                PlaceGrid(coords);
                placing = false;
                justPlaced = true;
                return;
            }

            //If position has changed from last time return previous area to defaults
            if(coords != prevCoords)
            {
                ClearGrid();
            }

            //Set cells to temporary values to desplay save
            DrawGrid(coords);
        }

        prevCoords = coords;
    }

    //Edit grid values to permantly change cell states
    void PlaceGrid(Vector2Int coords)
    {
        //Save previous display to undo data
        Controller.main.OnEdit();

        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                int xCoord = coords.x + x;
                int yCoord = coords.y - (grid.GetLength(1) - y) + 1;

                //If trying to place a cell outside of grid
                if (xCoord > controller.size.x || yCoord < 1)
                    continue;

                //Set the colour the cell will change to
                Color color = Color.white;
                if (grid[x, y])
                    color = Color.black;

                //Change the cells state
                controller.spriteRenderers[xCoord, yCoord].color = color;
                controller.grid[xCoord, yCoord] = grid[x, y];
            }
        }
    }

    //Go over the previously rendered cells and set back to actual values, so the design can be rendered in a new position
    void ClearGrid()
    {
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                int xCoord = prevCoords.x + x;
                int yCoord = prevCoords.y - (grid.GetLength(1) - y) + 1;

                //If outside of grid
                if (xCoord > controller.size.x || yCoord < 1)
                    continue;

                //Set colour the cell will be changed to
                Color color = Color.white;
                if (controller.grid[xCoord, yCoord])
                    color = Color.black;

                //Change to colour of the cell
                try
                { 
                    controller.spriteRenderers[xCoord, yCoord].color = color;
                }
                catch
                {
                    //Debug.Log("Clear Error Stats");
                    //Debug.Log("Prev Coords: " + prevCoords);
                    //Debug.Log("Edit Coords: (" + xCoord + ", " + yCoord + ")");
                }
            }
        }
    }

    //Draw grid from coord as bottom left corner
    void DrawGrid(Vector2Int coords)
    {
        for(int y = 0; y < grid.GetLength(1); y++)
        {
            for(int x = 0; x < grid.GetLength(0); x++)
            {
                int xCoord = coords.x + x;
                int yCoord = coords.y - (grid.GetLength(1) - y) + 1;


                //Skip if outside of grid
                if (xCoord > controller.size.x || yCoord < 1)
                    continue;

                //Set colour the grid will change into 
                Color color = Color.white;
                if (grid[x, y])
                    color = Color.black;

                //Set cells to set colours
                try
                {
                    controller.spriteRenderers[xCoord, yCoord].color = color;
                }
                catch
                {
                    //Debug.Log("Draw Error Stats");
                    //Debug.Log("Coords: " + coords);
                    //Debug.Log("Edit Coords: (" + xCoord + ", " + yCoord + ")");

                }
        }
        }
    }

    //Start placing
    public void ActivatePlacement(bool[,] newGrid)
    {
        grid = newGrid;
        placing = true;
        isNewGrid = true;
    }

    //Get coords of the cell the mouse is hovering over
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
