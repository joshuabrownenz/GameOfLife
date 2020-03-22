using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Editor : MonoBehaviour
{
    //Singleton instance
    public static Editor main;

    [Header("Allow Editing")]
    public bool allowEditing;


    //Instances
    Controller controller;
    SavePlacer savePlacer;

    //Coordinate data
    [HideInInspector] public Vector2Int mostRecentCoords;
    Vector2Int enterCoords;
    Vector2Int newCoords;

    //Drag data
    bool isDragEditing, isShift;
    List<Vector2Int> editedSquares;
    
    [HideInInspector] public int shiftLength, prevShiftLength;
    [HideInInspector] public bool isX, prevIsX;

    bool on;
    // Start is called before the first frame update
    private void Awake()
    {
        main = this;
    }
    void Start()
    {
        controller = Controller.main;
        savePlacer = SavePlacer.main;
        allowEditing = true;
    }

    // Update is called once per frame
    void Update()
    {
        //If the simulation is not running and editing is enabled 
        if (!controller.start && allowEditing)
        {
            //If a the button is relased and editing has been taking place
            if(Input.GetMouseButtonUp(0) && (isDragEditing || isShift) && !savePlacer.justPlaced)
            {
                //Saves grid in history
                controller.OnEdit();

                //Applies edits to grid
                saveEdits();

                //Clear graphes and data
                Statistics.main.ClearGraphs();
                Controller.main.CalculateInitialValues();

                //reset stats
                isShift = false;
                isDragEditing = false;
                
            }


            //Get coords the cell the mouse is over, (newCoords) is the default value the value that is returned if the mouse is not over anyting
            bool isOver = false;
            newCoords = GetCoords(newCoords, out isOver);

            //On first click of edit
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !savePlacer.justPlaced && isOver)
            {
                //Set defaults 
                editedSquares = new List<Vector2Int>();
                isDragEditing = true;

                //If shift is held activate shift editing
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                {
                    //Set defaults
                    isShift = true;
                    isDragEditing = false;
                    shiftLength = 1;
                    prevShiftLength = 1;
                    isX = true;
                    prevIsX = true;

                }
                //Start position
                enterCoords = newCoords;

                //Wether this edit is turning on or turning off cells
                on = !controller.grid[enterCoords.x, enterCoords.y];

                //Set the view of a cell
                controller.ModifyView(enterCoords, on);

                //Add cell to list of cells to be edited
                editedSquares.Add(enterCoords);
            }

            //If the mouse is held while drag editing is functioning
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject() && isDragEditing)
            {
                //Change the state of the cell if it has not already been changed
                if (!editedSquares.Contains(newCoords))
                {
                    controller.ModifyView(newCoords, on);
                    editedSquares.Add(newCoords);
                }
            }

            //Calaculate the cells to turn on if it is a shift drag
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject() && isShift)
            {
                //Calculate the number of cells in both directions
                Vector2Int vector = enterCoords - newCoords;
                vector *= -1;

                //Decide what direction up or down to display the shift drag based off which direction is longer
                if(Mathf.Abs(vector.x) >= Mathf.Abs(vector.y))
                {
                    if(vector.x == 0)
                    {
                        return;
                    }
                    shiftLength = vector.x + vector.x / Mathf.Abs(vector.x);
                    isX = true;
                }
                else
                {
                    if (vector.y == 0)
                    {
                        return;
                    }
                    shiftLength = vector.y + vector.y / Mathf.Abs(vector.y);
                    isX = false;
                }

                //Create negative multiplier 1 if shift length is down or left (negative vectors) else is postive 1
                int multiplier = shiftLength/Mathf.Abs(shiftLength);

                //If the length has changed then set the original cells back to default values
                if((shiftLength != prevShiftLength) || (isX != prevIsX))
                {

                    int prevMultiplier = prevShiftLength / Mathf.Abs(prevShiftLength);
                    for (int index = 0; index < Mathf.Abs(prevShiftLength); index++)
                    {
                        if (prevIsX)
                        {
                            controller.ModifyView(new Vector2Int(enterCoords.x + (index * prevMultiplier), enterCoords.y), controller.grid[enterCoords.x + (index * prevMultiplier), enterCoords.y]);
                        }
                        else
                        {
                            controller.ModifyView(new Vector2Int(enterCoords.x, enterCoords.y + (index * prevMultiplier)), controller.grid[enterCoords.x, enterCoords.y + (index * prevMultiplier)]);
                        }
                    }
                }

                //Set cells to new values (but only its colour not changing its actual state) 
                for(int index = 0; index < Mathf.Abs(shiftLength); index++)
                {
                    if(isX)
                    {
                        controller.ModifyView(new Vector2Int(enterCoords.x + (index * multiplier), enterCoords.y), on);
                    }
                    else
                    {
                        controller.ModifyView(new Vector2Int(enterCoords.x, enterCoords.y + (index * multiplier)), on);
                    }
                }

                //Set history stats
                prevShiftLength = shiftLength;
                prevIsX = isX;
                
            }
        }
    }

    //Set the edited cell(s) to their new values 
    void saveEdits()
    {
        //If it was a drag edit that happened then it sets the state of each cell to its new state
        if (isDragEditing)
        {
            foreach (Vector2Int coords in editedSquares)
            {
                controller.Modify(coords, on);
            }
        }
        //Else it was a shift edit so set the series of cells to their new state
        else
        {
            int multiplier = shiftLength / Mathf.Abs(shiftLength);
            for (int index = 0; index < Mathf.Abs(shiftLength); index++)
            {
                if (isX)
                {
                    controller.Modify(new Vector2Int(enterCoords.x + (index * multiplier), enterCoords.y), on);
                }
                else
                {
                    controller.Modify(new Vector2Int(enterCoords.x, enterCoords.y + (index * multiplier)), on);
                }
            }

        }
        //int trues = 0;
        //foreach (bool b in controller.grid)
        //{
        //    if (b)
        //        trues++;
        //}



    }

    //void SwitchCell()
    //{
    //    Vector2Int coords = GetCoords();
    //    if (coords != new Vector2Int(-1, -1))
    //    {
    //        Vector2Int vector2Int = coords;
    //        if (controller.grid[vector2Int.x, vector2Int.y])
    //        {
    //            controller.Modify(vector2Int, false);

    //        }
    //        else
    //            controller.Modify(vector2Int, true);
    //    }
    //}

     //Sends out a ray cast and retrives the CellContainer from the cell and gets the cells coordinate
    Vector2Int GetCoords(Vector2Int coords, out bool isOver)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            try
            {
                isOver = true;
                return hit.transform.gameObject.GetComponent<CellContainer>().position;
            }
            catch
            {
                isOver = false;
                return coords;
            }
        }
        isOver = false;
        return coords;
    }
}
