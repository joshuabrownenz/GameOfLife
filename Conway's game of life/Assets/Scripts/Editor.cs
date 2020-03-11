using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Editor : MonoBehaviour
{
    public static Editor main;
    public bool allowEditing;
    [SerializeField]
    Controller controller;
    SavePlacer savePlacer;
    public Vector2Int mostRecentCoords;
    public float editTime;
    Vector2Int enterCoords;
    List<Vector2Int> editedSquares;
    Vector2Int newCoords;
    bool isDragEditing, isShift;
    public int shiftLength, prevShiftLength;
    public bool isX, prevIsX;
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
        if (!controller.start && allowEditing)
        {
            if(Input.GetMouseButtonUp(0) && (isDragEditing || isShift) && !savePlacer.justPlaced)
            {
                
                saveEdits();
                isShift = false;
                isDragEditing = false;
                controller.OnEdit();
                Statistics.main.ClearGraphs();
                Controller.main.CalculateInitialValues();
                //controller.historyLimit = controller.historyIndex;
            }
            newCoords = GetCoords(newCoords);

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !savePlacer.justPlaced)
            {
                editedSquares = new List<Vector2Int>();
                isDragEditing = true;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                {
                    isShift = true;
                    isDragEditing = false;
                    shiftLength = 1;
                    prevShiftLength = 1;
                    isX = true;
                    prevIsX = true;

                }
                enterCoords = newCoords;
                on = !controller.grid[enterCoords.x, enterCoords.y];
                controller.ModifyView(enterCoords, on);
                editedSquares.Add(enterCoords);
            }

            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject() && isDragEditing)
            {
                if (!editedSquares.Contains(newCoords))
                {
                    controller.ModifyView(newCoords, on);
                    editedSquares.Add(newCoords);
                }
            }

            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject() && isShift)
            {
                Vector2Int vector = enterCoords - newCoords;
                vector *= -1;
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
                int multiplier = shiftLength/Mathf.Abs(shiftLength);
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
                prevShiftLength = shiftLength;
                prevIsX = isX;
                
            }
        }
    }

    void saveEdits()
    {
        if (isDragEditing)
        {
            foreach (Vector2Int coords in editedSquares)
            {
                //Debug.Log("Saved");
                controller.Modify(coords, on);
            }
        }
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
        int trues = 0;
        foreach (bool b in controller.grid)
        {
            if (b)
                trues++;
        }
        //Debug.Log("True Grid: " + trues);



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

    Vector2Int GetCoords(Vector2Int coords)
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
                return coords;
            }
        }
        return coords;
    }
}
