using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//Place on start button;
public class Buttons : MonoBehaviour
{
    Controller controller;
    Editor editor;

    //Buttons
    Button startButton;
    Text startText;
    Button stopButton;
    Button clear;
    
    // Start is called before the first frame update
    void Start()
    {
        //Get Instances
        editor = Editor.main;

        //Find and add listners to start stop buttons as well as seting it to the right state
        #region StartStop
        controller = Controller.main;
        startButton = transform.Find("Buttons").Find("Start").GetComponent<Button>();
        stopButton = transform.Find("Buttons").Find("Stop").GetComponent<Button>();
        startText = startButton.transform.Find("Text").GetComponent<Text>();
        startButton.onClick.AddListener(() => StartButton());
        stopButton.onClick.AddListener(() => StopButton());
        startText.text = "Start";
        stopButton.gameObject.SetActive(false);
        #endregion

        //Find Clear Button
        clear = transform.Find("Buttons").Find("Clear").GetComponent<Button>();
        clear.onClick.AddListener(() => Clear());
    }

    //void StartStartButton()
    //{
    //    StartCoroutine(StartButton());
    //}

    void StartButton()
    {
        //Tells the simulatation to start
        controller.start = true;

        //Set the start/stop button to the correct state
        startText.text = "Resume";
        stopButton.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);

        //Stop the saving process
        if(SaveController.main.saveMode)
        {
            SaveController.main.saveMode = false;
            Editor.main.allowEditing = false;
            if(SaveController.main.awaitingName)
            {
                SaveController.main.Cancel();
            }
        }

    }

    void StopButton()
    {
        //Tells the simulation to stop
        controller.start = false;

        //Sets the start/stop button to the correct state
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
    }

    void Clear()
    {
        //Set the start/stop button to the correct state
        startText.text = "Start";
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);

        //Pause the simulation
        controller.start = false;

        //Clear the grid and graphs
        controller.Clear();
        Statistics.main.ClearGraphs();

    }

    public void RandomCells()
    {
        //Randomize cells
        StartCoroutine(controller.RandomCellsCoroutine());

        //Set the start button to the correct state
        startText.text = "Start";
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
    }
}
