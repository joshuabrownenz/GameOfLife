using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class SaveDisplay : MonoBehaviour
{
    public static SaveDisplay main;
    [SerializeField]
    public SaveData[] saves;
    [SerializeField]
    public int page = 1;
    GameObject rightArrow, leftArrow;
    [SerializeField]
    GameObject[] optionImages;
    Button[] buttons = new Button[6];
    Button[] loadButtons = new Button[6];
    [SerializeField]
    Vector2 startPosition;
    [SerializeField]
    Vector2 finalPosition;
    [SerializeField]
    float moveSpeed;
    GameObject openPanel;
    bool[] large;
    bool open;

    Text openButtonText;
    private void Start()
    {
        main = this;

        openButtonText = transform.parent.Find("Open").Find("Text").GetComponent<Text>();
        openPanel = transform.Find("OpenPanel").gameObject;
        openPanel.SetActive(false);

        for (int i = 0; i < 6; i++)
        {
            buttons[i] = transform.Find("OpenPanel").Find("Options").Find("Option " + (i + 1)).Find("Button").GetComponent<Button>();
            loadButtons[i] = transform.Find("OpenPanel").Find("Options").Find("Option " + (i + 1)).Find("Load").GetComponent<Button>();
        }

        #region declearButtons
        buttons[0].onClick.AddListener(() => ActiveGridPlacement(1));
        buttons[1].onClick.AddListener(() => ActiveGridPlacement(2));
        buttons[2].onClick.AddListener(() => ActiveGridPlacement(3));
        buttons[3].onClick.AddListener(() => ActiveGridPlacement(4));
        buttons[4].onClick.AddListener(() => ActiveGridPlacement(5));
        buttons[5].onClick.AddListener(() => ActiveGridPlacement(6));
        #endregion
        rightArrow = transform.Find("OpenPanel").Find("Right").gameObject;
        leftArrow = transform.Find("OpenPanel").Find("Left").gameObject;

        LoadFromFile();

        //saves[0] = new SaveData();
        //saves[0].name = "Test";
        //saves[0].saveGrid = new bool[4,10];
        //saves[1] = new SaveData();
        //saves[1].name = "Test 2";
        //saves[1].saveGrid = new bool[5, 5];
        //saves[2] = new SaveData();
        //saves[2].name = "Test 3";
        //saves[2].saveGrid = new bool[2, 5];
        //saves[3] = new SaveData();
        //saves[3].name = "Test 4";
        //saves[3].saveGrid = new bool[10, 20];
        //saves[4] = new SaveData();
        //saves[4].name = "HI";
        //saves[4].saveGrid = new bool[6, 5];
        //saves[5] = new SaveData();
        //saves[5].name = "Test 6";
        //saves[5].saveGrid = new bool[2, 2];
        //saves[6] = new SaveData();
        //saves[6].name = "Test 7";
        //saves[6].saveGrid = new bool[9, 5];
        //saves[7] = new SaveData();
        //saves[7].name = "Test 8";
        //saves[7].saveGrid = new bool[10, 5];

        RenderOptions();
    }
    public void RenderOptions()
    {
        if(optionImages != null)
        {
            foreach(GameObject parent in optionImages)
            {
                Destroy(parent);
            }
        }
        int num = 6;
        if (page * 6 > saves.Length)
            num = saves.Length - (page - 1) * 6;


        optionImages = new GameObject[num];

        for(int option = 1; option <= num; option++)
        {
            optionImages[option - 1] = new GameObject("Option " + option + " Image", typeof(RectTransform));
            optionImages[option - 1].transform.SetParent(transform.Find("OpenPanel").Find("Options").Find("Option " + option));
            optionImages[option - 1].transform.localPosition = Vector3.zero;

        }
        
        bool[] beingUsed = new bool[6];
        large = new bool[6];
        for (int option = page * 6 - 5; option <= saves.Length && option <= (page * 6); option++)
        {
            beingUsed[option - (page - 1) * 6 - 1] = true;
            buttons[(option - (page - 1) * 6) - 1].transform.Find("Text").GetComponent<Text>().text = saves[option - 1].name;

            int size = saves[option - 1].saveGrid.GetLength(0) * saves[option - 1].saveGrid.GetLength(1);
            if (size > 400)
            {
                large[option - (page - 1) * 6 - 1] = true;
                continue;
            }

            bool[,] optionGrid = saves[option - 1].saveGrid;
            int largeLength;
            float scale;
            if (optionGrid.GetLength(0) > optionGrid.GetLength(1))
            {
                largeLength = optionGrid.GetLength(0);
            }
            else
            {
                largeLength = optionGrid.GetLength(1);
            }
            
            scale = (200f / largeLength)/100f;

            float offset = (200f / largeLength);
            for (int y = 1; y <= optionGrid.GetLength(1); y++)
            {
                for (int x = 1; x <= optionGrid.GetLength(0); x++) 
                {
                    GameObject obj = Instantiate(Resources.Load("CellUI") as GameObject);
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
            loadButtons[i].gameObject.SetActive(large[i]);
        }
        
    }

    public void LoadLargeGrid(int option)
    {

        buttons[option -  1].transform.Find("Text").GetComponent<Text>().text = saves[option - 1].name;

        bool[,] optionGrid = saves[option + (page - 1) * 6 - 1].saveGrid;
        int largeLength;
        float scale;
        if (optionGrid.GetLength(0) > optionGrid.GetLength(1))
        {
            largeLength = optionGrid.GetLength(0);
        }
        else
        {
            largeLength = optionGrid.GetLength(1);
        }
        scale = (200f / largeLength) / 100f;

        float offset = (200f / largeLength);
        for (int y = 1; y <= optionGrid.GetLength(1); y++)
        {
            for (int x = 1; x <= optionGrid.GetLength(0); x++)
            {
                GameObject obj = Instantiate(Resources.Load("CellUI") as GameObject);
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

        loadButtons[option - 1].gameObject.SetActive(false);
    }

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

    public void Open()
    {
        if(open)
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

    IEnumerator movePanel()
    {
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
        //Makes sure the file exists
        if (File.Exists(Application.persistentDataPath + "Saves.txt"))
        {
            file = File.Open(Application.persistentDataPath + "Saves.txt", FileMode.Open);
        }
        else
        {
            file = File.Create(Application.persistentDataPath + "Saves.txt");
            SaveData[] tempSaves = new SaveData[0];
            bF.Serialize(file, tempSaves);
        }
        //Gets the data from the save
        saves = (SaveData[])bF.Deserialize(file);
        file.Close();
    }

    void ActiveGridPlacement(int option)
    {
        GetComponent<SavePlacer>().ActivatePlacement(saves[option + ((page - 1) * 6) - 1].saveGrid);
        Open();
    }
}

//public void SaveGameManagerData(int slot)
//{
//    BinaryFormatter bF = new BinaryFormatter();
//    FileStream file;
//    //Makes sure the file to save to exists
//    if (File.Exists(Application.persistentDataPath + "/SavedData/slot" + slot + ".txt"))
//    {
//        file = File.Open(Application.persistentDataPath + "/SavedData/slot" + slot + ".txt", FileMode.Open);
//    }
//    else
//    {
//        file = File.Create(Application.persistentDataPath + "/SavedData/slot" + slot + ".txt");
//    }
//    //GameManagerData Holds all of the info in PGM
//    GameManagerData data = new GameManagerData();
//    #region Data
//    data.currentScene = currentScene;
//    data.previousScene = previousScene;

//    data.itemInventory = itemInventory;
//    data.possibleItems = possibleItems;
//    data.currentDialogueQuest = currentDialogueQuest;
//    data.characterQuests = characterQuests;
//    data.possibleQuests = possibleQuests;
//    data.activeQuests = activeQuests;

//    data.attackSpeedMulti = attackSpeedMulti;
//    data.attackRangeMulti = attackRangeMulti;
//    data.currentAttackMultiplier = currentAttackMultiplier;
//    data.smiteDamageMulti = smiteDamageMulti;
//    data.smiteDurationMulti = smiteDurationMulti;
//    data.lifeStealMulti = lifeStealMulti;
//    data.totalHealthMulti = totalHealthMulti;
//    data.damageResistMulti = damageResistMulti;
//    data.turtleResistMulti = turtleResistMulti;
//    data.turtleMultiMulti = turtleMultiMulti;
//    data.turtleDurationMulti = turtleDurationMulti;
//    data.movementResistMulti = movementResistMulti;
//    data.moveSpeedMulti = moveSpeedMulti;
//    data.jumpHeightMulti = jumpHeightMulti;
//    data.airAttackMulti = airAttackMulti;
//    data.instantKillChance = instantKillChance;
//    data.betterLootChance = betterLootChance;

//    data.hasMagic = hasMagic;
//    data.damageResistDuration = damageResistDuration;
//    data.smiteDuration = smiteDuration;

//    data.skillLevels = skillLevels;

//    data.tutorialComplete = tutorialComplete;
//    data.lastEnemyLevel = lastEnemyLevel;

//    data.totalExperience = totalExperience;

//    data.currentIndex = currentIndex;
//    data.currentWeapon = currentWeapon;
//    data.playerWeaponInventory = playerWeaponInventory;
//    data.playerStats = playerStats;

//    data.damageProgress = damageProgress;
//    data.tankProgress = tankProgress;
//    data.mobilityProgress = mobilityProgress;
//    data.attackSpeedUpgrades = attackSpeedUpgrades;
//    data.potionIsActive = potionIsActive;
//    data.activePotionType = activePotionType;

//    data.currentLeechMultiplier = currentLeechMultiplier;
//    data.potionCoolDownTime = potionCoolDownTime;

//    data.currentArmour = currentArmour;
//    data.comparingArmour = comparingArmour;

//    data.tripleJump = tripleJump;
//    data.hasSmite = hasSmite;
//    data.gripWalls = gripWalls;
//    data.maxedSpeed = maxedSpeed;
//    data.damageResist = damageResist;


//    data.completedQuests = completedQuests;
//    data.currentEnemyKills = currentEnemyKills;
//    #endregion
//    bF.Serialize(file, data);
//    file.Close();

//}

////Loads data from the save file
//public void LoadDataFromSave(int slot)
//{
//    SceneManager.LoadScene("Loading");
//    //Creates an object to hold the data so the new PGM can use it
//    GameObject empty = new GameObject("Load Scene Controller");
//    LoadSceneMonitor load = empty.AddComponent<LoadSceneMonitor>();

//    BinaryFormatter bF = new BinaryFormatter();
//    FileStream file;
//    //Makes sure the file exists
//    if (File.Exists(Application.persistentDataPath + "/SavedData/slot" + slot + ".txt"))
//    {
//        file = File.Open(Application.persistentDataPath + "/SavedData/slot" + slot + ".txt", FileMode.Open);
//    }
//    else
//    {
//        file = File.Create(Application.persistentDataPath + "/SavedData/SavedData/slot" + slot + ".txt");
//    }
//    //Gets the data from the save
//    GameManagerData data = (GameManagerData)bF.Deserialize(file);
//    //transfers to the holding object
//    load.data = data;
//    new GameObject("PersistantGameManager - Reload").AddComponent<PersistantGameManager>();
//    Destroy(gameObject);
//}