using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//Place on start button;
public class Buttons : MonoBehaviour
{
    Controller controller;
    Editor editor;
    #region StartStop
    Button startButton;
    Text startText;
    Button stopButton;
    Button clear;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        editor = Editor.main;
        #region StartStop
        controller = Controller.main;
        startButton = transform.Find("Start").GetComponent<Button>();
        stopButton = transform.Find("Stop").GetComponent<Button>();
        startText = startButton.transform.Find("Text").GetComponent<Text>();
        startButton.onClick.AddListener(() => StartButton());
        stopButton.onClick.AddListener(() => StopButton());
        startText.text = "Start";
        stopButton.gameObject.SetActive(false);
        #endregion
        clear = transform.Find("Clear").GetComponent<Button>();
        clear.onClick.AddListener(() => Clear());
    }

    //void StartStartButton()
    //{
    //    StartCoroutine(StartButton());
    //}

    void StartButton()
    {
        controller.start = true;
        startText.text = "Resume";
        stopButton.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);
    }

    void StopButton()
    {
        controller.start = false;
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
    }

    void Clear()
    {
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
        controller.start = false;
        controller.Clear();
        
    }
}
