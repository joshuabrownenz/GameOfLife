using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayStat : MonoBehaviour
{
    [SerializeField] bool frameRate;
    Statistics statistics;
    TextMeshProUGUI text;
    [SerializeField] Statistics.Stat type;
    // Start is called before the first frame update
    void Start()
    {
        statistics = Statistics.main;
        text = GetComponent<TextMeshProUGUI>();
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

    // Update is called once per frame
    void Update() 
    {
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
        else
        {
            if (statistics.stats[(int)type].Count != 0)
                text.text = statistics.stats[(int)type][statistics.stats[(int)type].Count - 1].ToString();
            else
                text.text = "N/A";
        }
    }
}
