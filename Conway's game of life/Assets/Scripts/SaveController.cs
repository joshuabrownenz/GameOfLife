using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveController : MonoBehaviour
{

    Vector2Int enterCoords;
    Controller controller;
    bool saveMode;
    GameObject NamePanel;
    bool awaitingName;
    bool[,] grid;
    bool selecting;
    Vector2Int prevCoords;
    GameObject selectParent;
    // Start is called before the first frame update
    void Start()
    {
        controller = Controller.main;
        NamePanel = transform.parent.Find("OpenPanel").Find("SaveName").gameObject;
        NamePanel.SetActive(false);
    }
    

    // Update is called once per frame
    void Update()
    {
        if (!awaitingName && saveMode)
        {
            Vector2Int coords = GetCoords();
            if (Input.GetMouseButtonDown(0))
            {
                if (coords == new Vector2Int(-1, -1))
                    return;
                enterCoords = coords;
                selecting = true;

            }
            else if (Input.GetMouseButton(0) && selecting)
            {
                RenderSelect(coords);
            }
            else if (Input.GetMouseButtonUp(0) && selecting)
            {
                if (coords == new Vector2Int(-1, -1))
                    coords = prevCoords;
                Vector2Int vector = enterCoords - coords;
                vector *= -1;

                //Debug.Log("Enter Coords: " + enterCoords);
                //Debug.Log("Exit Coords: " + coords);
                //Debug.Log("Vector " + vector);
                if (vector.x < 0)
                {
                    //Debug.Log("Adjust X");
                    vector.x = Mathf.Abs(vector.x);
                    enterCoords.x -= Mathf.Abs(vector.x);
                }
                if (vector.y < 0)
                {
                    //Debug.Log("Adjust Y");
                    vector.y = Mathf.Abs(vector.y);
                    enterCoords.y -= Mathf.Abs(vector.y);
                }


                

                //Debug.Log("Adjusted Enter Coords: " + enterCoords);
                //Debug.Log("Adjusted Vector " + vector);

                

                grid = CalculateSection(vector);
                awaitingName = true;
                NamePanel.SetActive(true);
                Editor.main.allowEditing = false;
                

            }
            prevCoords = coords;
        }
    }

    void RenderSelect(Vector2Int coords)
    {
        if (coords == prevCoords)
            return;

        if (coords == new Vector2Int(-1, -1))
            coords = prevCoords;

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
        Vector2Int size = Controller.main.size;

        Destroy(selectParent);
        selectParent = new GameObject("Select Image");

        Debug.Log("Adjusted Enter Coords: " + tempEnterCoords);
        Debug.Log("Adjusted Vector " + vector);

        for (int x = 0; x <= vector.x; x ++)
        {
            GameObject obj = Instantiate(Resources.Load("SelectEdge") as GameObject);
            obj.name = "Bottom Edge " + x;
            obj.transform.parent = selectParent.transform;
            obj.transform.position = new Vector3((x + tempEnterCoords.x) / 2f - 0.25f - size.x / 4f, tempEnterCoords.y / 2f - 0.25f - size.y / 4f, -1);
            obj.transform.rotation = Quaternion.Euler(0, 0, 270);

            obj = Instantiate(Resources.Load("SelectEdge") as GameObject);
            obj.name = "Top Edge " + x;
            obj.transform.parent = selectParent.transform;
            obj.transform.position = new Vector3((x + tempEnterCoords.x) / 2f - 0.25f - size.x / 4f, (tempEnterCoords.y + vector.y) / 2f - 0.25f - size.y / 4f, -1);
            obj.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        for (int y = 0; y <= vector.y; y++)
        {
            GameObject obj = Instantiate(Resources.Load("SelectEdge") as GameObject);
            obj.name = "Left Edge " + (y + 1);
            obj.transform.parent = selectParent.transform;
            obj.transform.position = new Vector3(tempEnterCoords.x / 2f - 0.25f - size.x / 4f, (tempEnterCoords.y + y) / 2f - 0.25f - size.y / 4f, -1);
            obj.transform.rotation = Quaternion.Euler(0, 0, 180);

            obj = Instantiate(Resources.Load("SelectEdge") as GameObject);
            obj.name = "Right Edge " + (y + 1);
            obj.transform.parent = selectParent.transform;
            obj.transform.position = new Vector3((tempEnterCoords.x + vector.x) / 2f - 0.25f - size.x / 4f, (tempEnterCoords.y + y) / 2f - 0.25f - size.y / 4f, -1);
        }

        //obj.transform.position = new Vector2(x / 2f - 0.25f - size.x / 4f, y / 2f - 0.25f - size.y / 4f);

    }

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

    bool[,] CalculateSection(Vector2Int vector)
    {
        bool[,] grid = new bool[Mathf.Abs(vector.x) + 1, Mathf.Abs(vector.y) + 1];
        for (int y = 1; y <= vector.y + 1; y++)
        {
            for (int x = 1; x <= vector.x + 1; x++)
            {
                //Debug.Log("Grid Cell: (" + (x - 1) + ", " + (y - 1) + ") Check Cell: (" + (enterCoords.x + x - 1) + ", " + (enterCoords.y + y - 1) + ") and it is " + controller.grid[enterCoords.x + x - 2, enterCoords.y + y - 2]);
                grid[x - 1, y - 1] = controller.grid[enterCoords.x + x - 1, enterCoords.y + y - 1];
            }
        }
        return grid;
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

    public void Save()
    {
        InputField input = NamePanel.transform.Find("InputField").GetComponent<InputField>();
        SaveData data = new SaveData();
        data.name = input.text;
        data.saveGrid = grid;
        input.text = "";
        InsertElement(data);
        Cancel();
    }

    public void Cancel()
    {
        NamePanel.SetActive(false);
        Editor.main.allowEditing = true;
        awaitingName = false;
        saveMode = false;
        selecting = false;
        Destroy(selectParent);
    }


    public void ActivateSaves()
    {
        saveMode = !saveMode;
        Editor.main.allowEditing = !saveMode;
    }




}

