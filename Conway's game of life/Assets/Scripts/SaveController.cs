using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveController : MonoBehaviour
{

    Vector2Int enterCoords;
    Controller controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = Controller.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2Int coords = GetCoords();
        if (Input.GetMouseButtonDown(0))
        {
            
            enterCoords = coords;
        }
        else if(Input.GetMouseButton(0))
        { 

        }
        else if(Input.GetMouseButtonUp(0))
        {
            Vector2Int vector = enterCoords - coords;
            vector *= -1;
        }
    }


    bool[,] calculateSection(Vector2Int vector)
    {
        bool[,] grid = new bool[vector.x + 1,vector.y + 1];
        for (int y = 0; y < vector.y; y++)
        {
            for (int x = 0; x < vector.x; x++)
            {
                grid[x, y] = controller.grid[enterCoords.x + x, enterCoords.y + y];
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
}

