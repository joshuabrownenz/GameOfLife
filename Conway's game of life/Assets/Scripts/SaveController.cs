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
                enterCoords = coords;
                selecting = true;

            }
            else if (Input.GetMouseButton(0))
            {

            }
            else if (Input.GetMouseButtonUp(0) && selecting)
            {
                if (coords == new Vector2Int(-1, -1))
                    return;
                Vector2Int vector = enterCoords - coords;
                vector *= -1;

                Debug.Log("Enter Coords: " + enterCoords);
                Debug.Log("Exit Coords: " + coords);
                Debug.Log("Vector " + vector);
                if (vector.x < 0)
                {
                    Debug.Log("Adjust X");
                    enterCoords.x -= Mathf.Abs(vector.x);
                }
                if (vector.y < 0)
                {
                    Debug.Log("Adjust Y");
 
                    enterCoords.y -= Mathf.Abs(vector.y);
                }

                vector.x = Mathf.Abs(vector.x);
                vector.y = Mathf.Abs(vector.y);

                Debug.Log("Adjusted Enter Coords: " + enterCoords);
                Debug.Log("Adjusted Vector " + vector);

                

                grid = CalculateSection(vector);
                awaitingName = true;
                NamePanel.SetActive(true);
                Editor.main.allowEditing = false;
                

            }
        }
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
        }

    bool[,] CalculateSection(Vector2Int vector)
    {
        bool[,] grid = new bool[Mathf.Abs(vector.x) + 1, Mathf.Abs(vector.y) + 1];
        for (int y = 1; y <= vector.y + 1; y++)
        {
            for (int x = 1; x <= vector.x + 1; x++)
            {
                Debug.Log("Grid Cell: (" + (x - 1) + ", " + (y-1) + ") Check Cell: (" + (enterCoords.x + x - 1) + ", " + (enterCoords.y + y - 1) + ") and it is " + controller.grid[enterCoords.x + x - 2, enterCoords.y + y - 2]);
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
        NamePanel.SetActive(false);
        Editor.main.allowEditing = true;
    }

    public void ActivateSaves()
    {
        saveMode = !saveMode;
        Editor.main.allowEditing = !saveMode;
    }
}

