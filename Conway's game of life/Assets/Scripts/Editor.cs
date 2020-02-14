using UnityEngine;

public class Editor : MonoBehaviour
{

    Controller controller;
    Vector2Int coords;
    // Start is called before the first frame update
    void Start()
    {
        controller = Controller.controller;
    }

    // Update is called once per frame
    void Update()
    {
        if(!controller.start)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Debug.Log("Ray");
                SwitchCell();
            }
        }
    }

    void SwitchCell()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            if(hit.transform.gameObject != null)
            {
                Vector2Int vector2Int = hit.transform.gameObject.GetComponent<CellContainer>().position;
                if (controller.grid[vector2Int.x, vector2Int.y])
                    controller.Modify(vector2Int, false);
                else
                    controller.Modify(vector2Int, true);


            }
        }
        else
        {
            Debug.Log("No Hit");
        }
    }
}
