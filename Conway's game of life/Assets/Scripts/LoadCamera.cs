using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCamera : MonoBehaviour
{
    //Instances
    Controller controller;
    Camera view;

    [Header("Constants")]
    [SerializeField] Vector2 scrollLimits;
    [SerializeField] float zoomRate, scrollSpeed, dragSpeed, startAmount;

    //Data
    Vector2 previousMousePostion;

    // Start is called before the first frame update
    void Start()
    {
        //Assign Static Instances
        view = GetComponent<Camera>();
        controller = Controller.main;

        //Assign intial camera position
        transform.position = new Vector3(0, 0, -5);

        //Calculate scroll limits
        scrollLimits.y = controller.size.y / 4 + 0.02f * controller.size.x;
        float scrollTest = controller.size.x / (4 * (16 / 9f)) + 0.02f * controller.size.x;
        if (scrollTest > scrollLimits.y)
            scrollLimits.y = scrollTest;
    }

    // Update is called once per frame
    void Update()
    {
        //Adjust the zoom of the camera based off the mouse wheel
        #region Scroll
        float scroll = Input.GetAxis("MouseScrollWheel");

        //Check maximum speed
        if (scroll > scrollSpeed)
            scroll = scrollSpeed;
        if (scroll < -scrollSpeed)
            scroll = -scrollSpeed;

        //Muliply the scroll factor by the current size
        scroll *= view.orthographicSize;

        //Set the zoom of the camera
        view.orthographicSize += scroll * Time.deltaTime * zoomRate;

        //Check if its over the limits and correct
        if(view.orthographicSize > scrollLimits.y)
            view.orthographicSize = scrollLimits.y;
        if (view.orthographicSize < scrollLimits.x)
            view.orthographicSize = scrollLimits.x;

        #endregion

        //Adjust the position of the camera based off a right click drag
        #region RightDrag
        if(Input.GetMouseButton(1))
        {
            //Reset previous mouse position if the mouse has just been pressed
            if (Input.GetMouseButtonDown(1))
            {
                previousMousePostion = new Vector2(Input.mousePosition.x - Camera.main.pixelWidth, Input.mousePosition.y - Camera.main.pixelHeight);
            }

            //Calculate Vector
            float translateX = (previousMousePostion.x - (Input.mousePosition.x - Camera.main.pixelWidth)) * Time.deltaTime * dragSpeed * view.orthographicSize * startAmount;
            float translateY = (previousMousePostion.y - (Input.mousePosition.y - Camera.main.pixelHeight)) * Time.deltaTime * dragSpeed * view.orthographicSize * startAmount;

            //Adjust camera
            transform.position += new Vector3(translateX, translateY, 0);

            //Set previous mouse postion
            previousMousePostion = new Vector2(Input.mousePosition.x - Camera.main.pixelWidth, Input.mousePosition.y - Camera.main.pixelHeight);
        }
        
        #endregion
    }


}
