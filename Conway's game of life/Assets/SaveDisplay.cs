using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveDisplay : MonoBehaviour
{
    public static SaveDisplay main;
    [SerializeField]
    public SaveData[] saves;
    [SerializeField]
    int page = 1;
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
    private void Start()
    {
        main = this;


        openPanel = transform.Find("OpenPanel").gameObject;
        openPanel.SetActive(false);

        for (int i = 0; i < 6; i++)
        {
            buttons[i] = transform.Find("OpenPanel").Find("Options").Find("Option " + (i + 1)).Find("Button").GetComponent<Button>();
            loadButtons[i] = transform.Find("OpenPanel").Find("Options").Find("Option " + (i + 1)).Find("Load").GetComponent<Button>();
        }

        rightArrow = transform.Find("OpenPanel").Find("Right").gameObject;
        leftArrow = transform.Find("OpenPanel").Find("Left").gameObject;

        saves = new SaveData[0];

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
                large[option - 1] = true;
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

        Debug.Log((option - (page - 1) * 6) - 1);
        Debug.Log(saves[option - 1].name);
        buttons[(option - (page - 1) * 6) - 1].transform.Find("Text").GetComponent<Text>().text = saves[option - 1].name;

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
        scale = (200f / largeLength) / 100f;

        float offset = (200f / largeLength);
        for (int y = 1; y <= optionGrid.GetLength(1); y++)
        {
            for (int x = 1; x <= optionGrid.GetLength(0); x++)
            {
                GameObject obj = Instantiate(Resources.Load("CellUI") as GameObject);
                obj.transform.SetParent(optionImages[option - (page - 1) * 6 - 1].transform);
                obj.transform.localScale = new Vector2(scale, scale);
                obj.name = "Cell: (" + x + ", " + y + ")";
                obj.transform.localPosition = new Vector2(offset * (x - 0.5f - optionGrid.GetLength(0) / 2f), offset * (y - 0.5f - optionGrid.GetLength(1) / 2f));
                if (saves[option - 1].saveGrid[x - 1, y - 1])
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
            StopAllCoroutines();
            StartCoroutine(movePanel());
            
        }
        else
        {
            //Open
            open = true;
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
}
