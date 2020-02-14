using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCamera : MonoBehaviour
{
    Controller controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = Controller.controller;
        transform.position = new Vector3((controller.size.x) / 4f,( controller.size.y)/ 4f, -5);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
