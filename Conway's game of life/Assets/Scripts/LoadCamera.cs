using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCamera : MonoBehaviour
{
    Controller controller;
    Camera view;
    [SerializeField]
    float zoomRate;
    [SerializeField]
    Vector2 scrollLimits;
    [SerializeField]
    float scrollSpeed;
    [SerializeField]
    float dragSpeed;
    Vector2 previousMousePostion;

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<Camera>();
        controller = Controller.controller;
        //transform.position = new Vector3((controller.size.x) / 4f,( controller.size.y)/ 4f, -5);
        transform.position = new Vector3(0, 0, -5);
        scrollLimits.y = controller.size.y / 4 + 1;
        float scrollTest = controller.size.x / (4 * (16 / 9f)) + 1 ;
        if (scrollTest > scrollLimits.y)
            scrollLimits.y = scrollTest;
    }

    // Update is called once per frame
    void Update()
    {
        #region Scroll
        float scroll = Input.GetAxis("MouseScrollWheel");
        if (scroll > scrollSpeed)
            scroll = scrollSpeed;
        if (scroll < -scrollSpeed)
            scroll = -scrollSpeed;

        scroll *= view.orthographicSize;

        view.orthographicSize += scroll * Time.deltaTime * zoomRate;
        if(view.orthographicSize > scrollLimits.y)
            view.orthographicSize = scrollLimits.y;
        if (view.orthographicSize < scrollLimits.x)
            view.orthographicSize = scrollLimits.x;

        #endregion
        #region RightDrag
        if(Input.GetMouseButton(1))
        {
            if (Input.GetMouseButtonDown(1))
            {
                previousMousePostion = new Vector2(Input.mousePosition.x - Camera.main.pixelWidth, Input.mousePosition.y - Camera.main.pixelHeight);
            }
            float translateX = (previousMousePostion.x - (Input.mousePosition.x - Camera.main.pixelWidth)) * Time.deltaTime * dragSpeed;
            float translateY = (previousMousePostion.y - (Input.mousePosition.y - Camera.main.pixelHeight)) * Time.deltaTime * dragSpeed;
            transform.position += new Vector3(translateX, translateY, 0);
            previousMousePostion = new Vector2(Input.mousePosition.x - Camera.main.pixelWidth, Input.mousePosition.y - Camera.main.pixelHeight);
        }
        
        #endregion
    }


}
