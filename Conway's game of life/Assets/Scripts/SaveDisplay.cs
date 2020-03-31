using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class SaveDisplay : MonoBehaviour
{
    //Singleton Instance
    public static SaveDisplay main;

    [Header("Cell Image Prefab ")]
    [SerializeField] GameObject cellUI;

    [Header("Array of saves")]
    [SerializeField] public SaveData[] saves;

    [Header("Max number of cells to display")]
    [SerializeField] int maxCells;

    //Page the display is showing
    [HideInInspector] public int page = 1;

    //left and right buttons
    GameObject rightArrow, leftArrow;

    [Header("Parent of each minigrid")]
    [SerializeField] GameObject[] optionImages;

    //Arrays of buttons
    Button[] buttons = new Button[6], deleteButtons = new Button[6];
    Button[] loadButtons = new Button[6];

    [Header("Panel tranformation")]
    [SerializeField] Vector2 startPosition;
    [SerializeField] Vector2 finalPosition;
    [SerializeField] float moveSpeed;

    //Panels
    GameObject openPanel, deletePanel;

    //Variables
    bool[] large;
    bool open;
    int readyToDelete;
    Text openButtonText;

    private void Start()
    {
        //Assign singleton
        main = this;

        //Assign instances and set to defualt states
        openButtonText = transform.parent.Find("Open").Find("Text").GetComponent<Text>();
        openPanel = transform.Find("OpenPanel").gameObject;
        openPanel.SetActive(false);
        deletePanel = transform.Find("DeletePanel").gameObject;
        deletePanel.SetActive(false);

        //Get each asspect of each display section and store in an array
        for (int i = 0; i < 6; i++)
        {
            buttons[i] = transform.Find("OpenPanel").Find("Options").Find("Option " + (i + 1)).Find("Button").GetComponent<Button>();
            deleteButtons[i] = transform.Find("OpenPanel").Find("Options").Find("Option " + (i + 1)).Find("Delete").GetComponent<Button>();
            loadButtons[i] = transform.Find("OpenPanel").Find("Options").Find("Option " + (i + 1)).Find("Load").GetComponent<Button>();
        }

        //Assign method to each button
        buttons[0].onClick.AddListener(() => ActiveGridPlacement(1));
        buttons[1].onClick.AddListener(() => ActiveGridPlacement(2));
        buttons[2].onClick.AddListener(() => ActiveGridPlacement(3));
        buttons[3].onClick.AddListener(() => ActiveGridPlacement(4));
        buttons[4].onClick.AddListener(() => ActiveGridPlacement(5));
        buttons[5].onClick.AddListener(() => ActiveGridPlacement(6));


        //Get each arrow
        rightArrow = transform.Find("OpenPanel").Find("Right").gameObject;
        leftArrow = transform.Find("OpenPanel").Find("Left").gameObject;

        //Retrieve from save
        LoadFromFile();

        //Display each grid
        RenderOptions();
    }

    /// <summary>
    /// Display each saved grid on page
    /// </summary>
    public void RenderOptions()
    {
        //Clears the previous grid
        if(optionImages != null)
        {
            foreach(GameObject parent in optionImages)
            {
                Destroy(parent);
            }
        }

        //Calculate how many options are displayed on a panel 
        int num = 6;
        if (page * 6 > saves.Length)
            num = saves.Length - (page - 1) * 6;

        //Create a parent object at each centre point of images
        optionImages = new GameObject[num];
        for(int option = 1; option <= num; option++)
        {
            optionImages[option - 1] = new GameObject("Option " + option + " Image", typeof(RectTransform));
            optionImages[option - 1].transform.SetParent(transform.Find("OpenPanel").Find("Options").Find("Option " + option));
            optionImages[option - 1].transform.localPosition = Vector3.zero;

        }

        //Array of states of each 'image'
        bool[] beingUsed = new bool[6];
        large = new bool[6];

        //Run through each option to be displayed on the panel 
        for (int option = page * 6 - 5; option <= saves.Length && option <= (page * 6); option++)
        {
            //Is being used
            beingUsed[option - (page - 1) * 6 - 1] = true;

            //Sets the name of the save as the text of the button
            buttons[(option - (page - 1) * 6) - 1].transform.Find("Text").GetComponent<Text>().text = saves[option - 1].name;

            //If there are more cells than max do not create rather prepare to create load large grid number
            int size = saves[option - 1].saveGrid.GetLength(0) * saves[option - 1].saveGrid.GetLength(1);
            if (size > maxCells)
            {
                large[option - (page - 1) * 6 - 1] = true;
                continue;
            }

            //Grid to display
            bool[,] optionGrid = saves[option - 1].saveGrid;

            //With corresponding data for correct sizing and offset
            int largeLength;
            if (optionGrid.GetLength(0) > optionGrid.GetLength(1))
            {
                largeLength = optionGrid.GetLength(0);
            }
            else
            {
                largeLength = optionGrid.GetLength(1);
            }
            float scale = (200f / largeLength)/100f;
            float offset = (200f / largeLength);

            //Load each cell in the display grid
            for (int y = 1; y <= optionGrid.GetLength(1); y++)
            {
                for (int x = 1; x <= optionGrid.GetLength(0); x++) 
                {
                    GameObject obj = Instantiate(cellUI);
                    obj.transform.SetParent(optionImages[option - (page - 1) * 6 - 1].transform);
                    obj.transform.localScale = new Vector2(scale, scale);
                    obj.name = "Cell: (" + x + ", " + y + ")";
                    obj.transform.localPosition = new Vector2(offset * (x - 0.5f - optionGrid.GetLength(0)/2f),offset * (y - 0.5f- optionGrid.GetLength(1) / 2f));
                    if (saves[option - 1].saveGrid[x - 1, y - 1])
                    {
                        obj.GetComponent<Image>().color = Color.black;
                    }
                }
            }
        }

        //Turns off buttons not being used
        for(int i = 0; i < 6; i++)
        {
            buttons[i].gameObject.SetActive(beingUsed[i]);
            deleteButtons[i].gameObject.SetActive(beingUsed[i]);
            loadButtons[i].gameObject.SetActive(large[i]);
        }
        
    }

    //Load a grid hidden as it is too large
    public void LoadLargeGrid(int option)
    {
        //buttons[option -  1].transform.Find("Text").GetComponent<Text>().text = saves[option + (page - 1) * 6 - 1].name;

        //Get grid with corresponding data
        bool[,] optionGrid = saves[option + (page - 1) * 6 - 1].saveGrid;
        int largeLength;
        if (optionGrid.GetLength(0) > optionGrid.GetLength(1))
        {
            largeLength = optionGrid.GetLength(0);
        }
        else
        {
            largeLength = optionGrid.GetLength(1);
        }
        float scale = (200f / largeLength) / 100f;
        float offset = (200f / largeLength);

        //Place each cell
        for (int y = 1; y <= optionGrid.GetLength(1); y++)
        {
            for (int x = 1; x <= optionGrid.GetLength(0); x++)
            {
                GameObject obj = Instantiate(cellUI);
                obj.transform.SetParent(optionImages[option - 1].transform);
                obj.transform.localScale = new Vector2(scale, scale);
                obj.name = "Cell: (" + x + ", " + y + ")";
                obj.transform.localPosition = new Vector2(offset * (x - 0.5f - optionGrid.GetLength(0) / 2f), offset * (y - 0.5f - optionGrid.GetLength(1) / 2f));
                if (saves[option + (page - 1) * 6 - 1].saveGrid[x - 1, y - 1])
                {
                    obj.GetComponent<Image>().color = Color.black;
                }
            }
        }

        //Turn off the load button
        loadButtons[option - 1].gameObject.SetActive(false);
    }

    //Manage the state of the buttons
    private void Update()
    {
        bool goLeft = true, goRight = true;
        if(page == 1)
        {
            goLeft = false;
        }
        if((page )* 6 >= saves.Length)
        {
            goRight = false;
        }
        leftArrow.SetActive(goLeft);
        rightArrow.SetActive(goRight);

    }

    //Change the page on a button press
    public void LeftArrow()
    {
        page--;
        RenderOptions();
    }
    public void RightArrow()
    {
        page++;
        RenderOptions();
    }

    //Open and close the panel
    public void Open()
    {
        if (!SaveController.main.saveMode)
        {
            if (open)
            {
                //Close
                open = false;
                openButtonText.text = "Open";
                StopAllCoroutines();
                StartCoroutine(movePanel());

            }
            else
            {
                //Open
                open = true;
                openButtonText.text = "Close";
                StopAllCoroutines();
                StartCoroutine(movePanel());
            }
        }

    }

    //Open dialogue box asking for confirmation of deletion
    public void DeleteSaves(int option)
    {
        readyToDelete = option + (page - 1) * 6 - 1;
        deletePanel.SetActive(true);
    }

    //Close the delete panel
    public void CloseDeletePanel()
    {
        deletePanel.SetActive(false);
    }

    //Delete a save
    public void DeleteSavesConfirmed()
    {
        //Create a new array and copy all but the grid you want to delete across from it
        SaveData[] newData = new SaveData[saves.Length - 1];
        bool skipped = false;
        int index = 0;
        for (int i = 0; i < saves.Length; i++)
        {
            
            if (i == readyToDelete && !skipped)
            {
                skipped = true;
                continue;
            }
            newData[index] = saves[i];
            index++;
        }

        //Set to new states
        deletePanel.SetActive(false);
        saves = newData;
        RenderOptions();

        SaveToFile();
    }

    //Runs the open and close anmation of the panel as well as acivating monitoring lock out varibales
    IEnumerator movePanel()
    {
        //Opens panel
        if (open)
        {
            Editor.main.allowEditing = false;
            openPanel.SetActive(true);
            RenderOptions();
            while (openPanel.transform.position.x < finalPosition.x)
            {
                openPanel.transform.position = new Vector2(openPanel.transform.position.x + (moveSpeed * Time.deltaTime), finalPosition.y);
                yield return null;

            }
            openPanel.transform.position = finalPosition;
        }
        //Closes panel
        else
        {
            Editor.main.allowEditing = true;
            if (optionImages != null)
            {
                foreach (GameObject parent in optionImages)
                {
                    Destroy(parent);
                }
            }
            while (openPanel.transform.position.x > startPosition.x)
            {
                openPanel.transform.position = new Vector2(openPanel.transform.position.x - (moveSpeed * Time.deltaTime), finalPosition.y);
                yield return null;
            }
            openPanel.transform.position = startPosition;

            
            openPanel.SetActive(false);
        }
    }

    public void SaveToFile()
    {
        BinaryFormatter bF = new BinaryFormatter();
        FileStream file;

        //Makes sure the file to save to exists
        if (File.Exists(Application.persistentDataPath + "Saves.txt"))
        {
            file = File.Open(Application.persistentDataPath + "Saves.txt", FileMode.Open);
        }
        //Else creates a new one
        else
        {
            file = File.Create(Application.persistentDataPath + "Saves.txt");
        }
        bF.Serialize(file, saves);
        file.Close();
    }

    void LoadFromFile()
    {
        BinaryFormatter bF = new BinaryFormatter();
        FileStream file;
        //Makes sure the file exists then retives it
        if (File.Exists(Application.persistentDataPath + "Saves.txt"))
        {
            file = File.Open(Application.persistentDataPath + "Saves.txt", FileMode.Open);
            saves = (SaveData[])bF.Deserialize(file);
            file.Close();
        }
        //Creates a new one and set the data to a blank array
        else
        {
            file = File.Create(Application.persistentDataPath + "Saves.txt");
            SaveData[] tempSaves = new SaveData[0];
            bF.Serialize(file, tempSaves);
            saves = tempSaves;
            file.Close();
        }
        
    }

    //Starts placement of a grid object
    void ActiveGridPlacement(int option)
    {
        GetComponent<SavePlacer>().ActivatePlacement(saves[option + ((page - 1) * 6) - 1].saveGrid);
        Open();
    }
}