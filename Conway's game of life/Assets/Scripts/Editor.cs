using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Editor : MonoBehaviour
{
    public static Editor main;

    Controller controller;
    public Vector2Int mostRecentCoords;
    public float editTime;
    // Start is called before the first frame update
    private void Awake()
    {
        main = this;
    }
    void Start()
    {
        controller = Controller.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(!controller.start)
        {
            if(Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == null)
            {
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
                {
                    controller.Modify(vector2Int, false);

                }
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
