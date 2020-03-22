using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayStat : MonoBehaviour
{
    [Header("Set Type")]
    [SerializeField] Statistics.Stat type;

    [Header("Is it the frame rate text")]
    [SerializeField] bool frameRate;

    //Insatnces
    Statistics statistics;
    TextMeshProUGUI text;


    void Start()
    {
        //Assign instances
        statistics = Statistics.main;
        text = GetComponent<TextMeshProUGUI>();

        //If this is the framerate script then assign the text value to the correct number
        if (frameRate)
        {
            if(statistics.framesPerSecond != 0)
            {
                text.text = Math.Round(statistics.framesPerSecond, 4).ToString();
            }
            else
            {
                text.text = "N/A";
            }
        }
        //Else assign the the text to the correct number based off data type
        else
        {
            if (statistics.stats[(int)type] != null)
            {
                if (statistics.stats[(int)type].Count != 0)
                    text.text = statistics.stats[(int)type][statistics.stats[(int)type].Count - 1].ToString();
                else
                    text.text = "N/A";
            }
            else
                text.text = "N/A";
        }
    }

    
    void Update() 
    {
        //If this is the framerate script then assign the text value to the correct number
        if (frameRate)
        {
            if (statistics.framesPerSecond != 0)
            {
                text.text = Math.Round(statistics.framesPerSecond, 4).ToString();
            }
            else
            {
                text.text = "N/A";
            }
        }
        //Else assign the the text to the correct number based off data type
        else
        {
            if (statistics.stats[(int)type].Count != 0)
                text.text = statistics.stats[(int)type][statistics.stats[(int)type].Count - 1].ToString();
            else
                text.text = "N/A";
        }
    }
}
