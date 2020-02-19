using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDisplay : MonoBehaviour
{
    [SerializeField]
    public SaveData[] saves;
    [SerializeField]
    int page = 1;
    GameObject rightArrow, leftArrow;
    [SerializeField]
    GameObject[] optionImages;
    private void Start()
    {
        rightArrow = transform.Find("Right").gameObject;
        leftArrow = transform.Find("Left").gameObject;

        saves = new SaveData[7];
        saves[0] = new SaveData();
        saves[0].saveGrid = new bool[4,10];

        saves[1] = new SaveData();
        saves[1].saveGrid = new bool[5, 5];
        saves[2] = new SaveData();
        saves[2].saveGrid = new bool[2, 5];
        saves[3] = new SaveData();
        saves[3].saveGrid = new bool[10, 20];
        saves[4] = new SaveData();
        saves[4].saveGrid = new bool[6, 5];
        saves[5] = new SaveData();
        saves[5].saveGrid = new bool[2, 2];
        saves[6] = new SaveData();
        saves[6].saveGrid = new bool[9, 5];


        RenderOptions();
    }
    void RenderOptions()
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

        Debug.Log("Num: " +num);
        optionImages = new GameObject[num];

        for(int option = 1; option <= num; option++)
        {
            Debug.Log("Option: " + option);
            optionImages[option - 1] = new GameObject("Option " + option + " Image", typeof(RectTransform));
            optionImages[option - 1].transform.SetParent(transform.Find("Options").Find("Option " + option));
            optionImages[option - 1].transform.localPosition = Vector3.zero;

        }

        for (int option = page * 6 - 5; option <= saves.Length && option <= (page * 6); option++)
        {
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

            float offsetX = ((200f/optionGrid.GetLength(0)));
            float offsetY = ((200f/optionGrid.GetLength(1)));
            float offset = (200f / largeLength);
            Debug.Log(optionGrid.GetLength(1));
            for (int y = 1; y <= optionGrid.GetLength(1); y++)
            {
                Debug.Log("Y Value: " + y);
                for (int x = 1; x <= optionGrid.GetLength(0); x++) 
                {
                    GameObject obj = Instantiate(Resources.Load("CellUI") as GameObject);
                    obj.transform.parent.SetParent(optionImages[option - 1].transform);
                    obj.transform.localScale = new Vector2(scale, scale);
                    obj.name = "Cell: (" + x + ", " + y + ")";
                    obj.transform.localPosition = new Vector2(offset * (x - 0.5f - optionGrid.GetLength(0)/2f),offset * (y - 0.5f- optionGrid.GetLength(1) / 2f));
                   
                    //if(saves[option - 1].saveGrid[x, y])
                    //    obj.GetComponent<SpriteRenderer>().color = Color.black;
                }
            }
        }
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
}
