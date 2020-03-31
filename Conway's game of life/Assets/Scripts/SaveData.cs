using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Holds a grid and a name, is serialized as save data
[Serializable]
public class SaveData
{
    public string name;
    public bool[,] saveGrid;
}
