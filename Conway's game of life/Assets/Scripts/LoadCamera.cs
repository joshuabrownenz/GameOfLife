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
    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<Camera>();
        controller = Controller.controller;
        transform.position = new Vector3((controller.size.x) / 4f,( controller.size.y)/ 4f, -5);
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

    }

    
}
