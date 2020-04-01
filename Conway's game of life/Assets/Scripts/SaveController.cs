using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveController : MonoBehaviour
{
    //Singleton Instance
    public static SaveController main;

    //Instances 
    Controller controller;
    GameObject NamePanel;
    GameObject selectParent;
    Image image;

    //States
    [Header("Is Saving")]
    public bool saveMode;
    [HideInInspector] public bool awaitingName;
    bool selecting;

    //Coordinate drag data
    Vector2Int enterCoords;
    Vector2Int prevCoords;

    //Grid to return
    bool[,] grid;

    private void Awake()
    {
        //Assign singleton
        main = this;
    }
    
    void Start()
    {
        //Get Instances
        controller = Controller.main;
        image = GetComponent<Image>();
        NamePanel = transform.parent.parent.Find("OpenPanel").Find("SaveName").gameObject;

        //Hide NamePanel
        NamePanel.SetActive(false);
    }

    void Update()
    {
        //Set the colour of the save button
        if (saveMode)
            image.color = Color.yellow;
        else
            image.color = Color.white;

        //If wating for the player to click or has clicked and is draging
        if (!awaitingName && saveMode && !EventSystem.current.IsPointerOverGameObject())
        {
            //Get current coords
            Vector2Int coords = GetCoords();

            //On intial click set start coords 
            if (Input.GetMouseButtonDown(0))
            {
                if (coords == new Vector2Int(-1, -1))
                    return;
                enterCoords = coords;
                selecting = true;

            }

            //Render the outline if the mouse is stil down
            else if (Input.GetMouseButton(0) && selecting)
            {
                RenderSelect(coords);
            }

            //If the button is released generate a vector between the bottom left corner and top right corner of selection
            else if (Input.GetMouseButtonUp(0) && selecting)
            {
                if (coords == new Vector2Int(-1, -1))
                    coords = prevCoords;

                Vector2Int vector = enterCoords - coords;
                vector *= -1;
                if (vector.x < 0)
                {
                    vector.x = Mathf.Abs(vector.x);
                    enterCoords.x -= Mathf.Abs(vector.x);
                }
                if (vector.y < 0)
                {
                    vector.y = Mathf.Abs(vector.y);
                    enterCoords.y -= Mathf.Abs(vector.y);
                }
                
                //Get a bool[,] of the selection based of the vector and modifed enter coords
                grid = CalculateSection(vector);

                //Set states
                Controller.main.textOpen = true;
                awaitingName = true;
                NamePanel.SetActive(true);
                Editor.main.allowEditing = false;
                

            }
            prevCoords = coords;
        }
    }

    //PLace a outline arround the seclection
    void RenderSelect(Vector2Int coords)
    {
        //If the selection hasn't changed from last time make not changes
        if (coords == prevCoords)
            return;

        if (coords == new Vector2Int(-1, -1))
            coords = prevCoords;

        //Work out Coords of bottom left corner of selection and then vector to top right
        Vector2Int tempEnterCoords = enterCoords;
        Vector2Int vector = tempEnterCoords - coords;
        vector *= -1;
        if (vector.x < 0)
        {
            vector.x = Mathf.Abs(vector.x);
            tempEnterCoords.x -= Mathf.Abs(vector.x);
        }
        if (vector.y < 0)
        {
            vector.y = Mathf.Abs(vector.y);
            tempEnterCoords.y -= Mathf.Abs(vector.y);
        }

        //Size of grid
        Vector2Int size = Controller.main.size;

        //Delete old outline and create parent for new one
        Destroy(selectParent);
        selectParent = new GameObject("Select Image");


        //Makes the top and bottom edge
        for (int x = 0; x <= vector.x; x ++)
        {
            //Bottom
            GameObject obj = Instantiate(Resources.Load("SelectEdge") as GameObject);
            obj.name = "Bottom Edge " + x;
            obj.transform.parent = selectParent.transform;
            obj.transform.position = new Vector3((x + tempEnterCoords.x) / 2f - 0.25f - size.x / 4f, tempEnterCoords.y / 2f - 0.25f - size.y / 4f, -1);
            obj.transform.rotation = Quaternion.Euler(0, 0, 270);

            //Top
            obj = Instantiate(Resources.Load("SelectEdge") as GameObject);
            obj.name = "Top Edge " + x;
            obj.transform.parent = selectParent.transform;
            obj.transform.position = new Vector3((x + tempEnterCoords.x) / 2f - 0.25f - size.x / 4f, (tempEnterCoords.y + vector.y) / 2f - 0.25f - size.y / 4f, -1);
            obj.transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        //Makes the left and right edge
        for (int y = 0; y <= vector.y; y++)
        {
            //Left
            GameObject obj = Instantiate(Resources.Load("SelectEdge") as GameObject);
            obj.name = "Left Edge " + (y + 1);
            obj.transform.parent = selectParent.transform;
            obj.transform.position = new Vector3(tempEnterCoords.x / 2f - 0.25f - size.x / 4f, (tempEnterCoords.y + y) / 2f - 0.25f - size.y / 4f, -1);
            obj.transform.rotation = Quaternion.Euler(0, 0, 180);

            //Right
            obj = Instantiate(Resources.Load("SelectEdge") as GameObject);
            obj.name = "Right Edge " + (y + 1);
            obj.transform.parent = selectParent.transform;
            obj.transform.position = new Vector3((tempEnterCoords.x + vector.x) / 2f - 0.25f - size.x / 4f, (tempEnterCoords.y + y) / 2f - 0.25f - size.y / 4f, -1);
        }

    }

    //Adds new entry to the list
    void InsertElement(SaveData data)
    {
        SaveData[] oldSaves = SaveDisplay.main.saves;
        SaveData[] newSaves = new SaveData[oldSaves.Length + 1];
        for (int i = 0; i < oldSaves.Length; i++)
        {
            newSaves[i] = oldSaves[i];
        }
        newSaves[oldSaves.Length] = data;
        SaveDisplay.main.saves = newSaves;
        SaveDisplay.main.SaveToFile();
    }

    //Works out the grid to save
    bool[,] CalculateSection(Vector2Int vector)
    {
        bool[,] grid = new bool[Mathf.Abs(vector.x) + 1, Mathf.Abs(vector.y) + 1];
        for (int y = 1; y <= vector.y + 1; y++)
        {
            for (int x = 1; x <= vector.x + 1; x++)
            {
                grid[x - 1, y - 1] = controller.grid[enterCoords.x + x - 1, enterCoords.y + y - 1];
            }
        }
        return grid;
    }

    //Gets Coords of the cell the mouse is above
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

    //After dialogue box has been accepted save and add grid to list
    public void Save()
    {
        //Create new save object
        SaveData data = new SaveData();

        //Get text from dialogue box
        InputField input = NamePanel.transform.Find("InputField").GetComponent<InputField>();
        data.name = input.text;
        input.text = "";

        //Add grid to save object
        data.saveGrid = grid;

        //Add to list
        InsertElement(data);

        //Close the dialogue box and end saving
        Cancel();
    }

    //Disable dialogue box and end the saving process
    public void Cancel()
    {
        Controller.main.textOpen = false;
        GetComponent<Image>().color = Color.white;
        NamePanel.SetActive(false);
        Editor.main.allowEditing = true;
        awaitingName = false;
        saveMode = false;
        selecting = false;
        Destroy(selectParent);
    }

    //Activate or disabler savemode
    public void ActivateSaves()
    {
        if(Controller.main.start)
        {
            return;
        }

        if (!saveMode)
            GetComponent<Image>().color = Color.yellow;
        else
            GetComponent<Image>().color = Color.white;

        saveMode = !saveMode;
        Editor.main.allowEditing = !saveMode;
    }




}

