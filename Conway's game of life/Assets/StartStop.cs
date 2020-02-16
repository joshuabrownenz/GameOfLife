using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StartStop : MonoBehaviour
{
    Controller controller;
    Text text;
    Button myselfButton;
    // Start is called before the first frame update
    void Start()
    {
        controller = Controller.controller;
        myselfButton = GetComponent<Button>();
        text = transform.Find("Text").GetComponent<Text>();
        myselfButton.onClick.AddListener(() => StartButton());
        text.text = "Start";
    }

    void StartButton()
    {
        controller.start = true;
        text.text = "Pause";
        myselfButton.onClick.RemoveListener(() => StartButton());
        myselfButton.onClick.AddListener(() => StopButton());
    }

    void StopButton()
    {
        controller.start = false;
        text.text = "Resume";
        myselfButton.onClick.RemoveListener(() => StopButton());
        myselfButton.onClick.AddListener(() => StartButton());
    }

}
