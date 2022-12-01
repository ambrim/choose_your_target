using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class CursorPos : MonoBehaviour
{
    public float x { get; set; }
    public float y { get; set; }
    public float rotation { get; set; }
    public bool display { get; set; }
    public bool record { get; set; }
    public float fbdelay { get; set; }
    public bool available { get; set; }
    public bool active { get; set; }

    [SerializeField] string Label;
    public string labelflag { get; set; } // might not be necessary to have this?

    public List<float> xlist { get; set; }
    public List<float> ylist { get; set; }
    public List<float> tmlist { get; set; }

    TrackHandPos hand;
    PlayerData _playerData;
    
    // this appears to get called every time the object is set to active... I didn't think that was supposed to happen
    void Awake()
    {
        // get access to handpos
        GameObject handInfoObj = GameObject.Find("HandTracker");
        hand = handInfoObj.GetComponent<TrackHandPos>();

        // get access to playerData
        GameObject gameInfoObj = GameObject.Find("GameInfo");
        //_trialIterator = gameInfoObj.GetComponent<IterateTrials>();
        _playerData = gameInfoObj.GetComponent<PlayerData>();

        //rotation = 0; // set default to 0
        //fbdelay = 0;
        //active = false;

        labelflag = Label;
    }

    public void setPosition(float xin, float yin)
    {
        x = xin;
        y = yin;
        displayPosition();
    }

    public void setNextPosition(float distance, float handangle)
    {
        // assumes that start pos is 0,0 (this is true as of 3/9/2022
        rotation = rotation;
        distance = distance;
        x = distance * Mathf.Cos(((rotation * Mathf.PI) / 180) + handangle);
        y = distance * Mathf.Sin(((rotation * Mathf.PI) / 180) + handangle);

        // needed to use handangle instead of hand.angle because of initialization order of things
    }

    public void displayPosition()
    {
        transform.position = new Vector3(x, y, transform.position.x);
        float tm = Time.realtimeSinceStartup;
        if (record)
        {
            if (xlist.Count == 0)
            {
                xlist.Add(x);
                ylist.Add(y);
                tmlist.Add(tm);
            }
        }
    }

    public void save()
    {
        record = false; // make sure we're not still adding to the x and y lists
        available = false; // indicate that this object is busy

        if (xlist.Count > 0)
        {
            string filebase;
            if (_playerData.numTrials < 10) filebase = _playerData.savePath + "/T000" + _playerData.numTrials.ToString();
            else if (_playerData.numTrials < 100) filebase = _playerData.savePath + "/T00" + _playerData.numTrials.ToString();
            else if (_playerData.numTrials < 1000) filebase = _playerData.savePath + "/T0" + _playerData.numTrials.ToString();
            else filebase = _playerData.savePath + "/T" + _playerData.numTrials.ToString();


            // hand positions
            string filename = filebase + "_cursor" + labelflag + ".csv";
            List<string[]> rowData = new List<string[]>();
            // manually create headers
            string[] rowDataTemp = new string[4];
            rowDataTemp[0] = "X";
            rowDataTemp[1] = "Y";
            rowDataTemp[2] = "T";
            rowData.Add(rowDataTemp);
            // populate columns
            for (int i = 0; i < xlist.Count; i++)
            {
                rowDataTemp = new string[4];
                rowDataTemp[0] = xlist[i].ToString(); // X
                rowDataTemp[1] = ylist[i].ToString(); // Y
                rowDataTemp[2] = tmlist[i].ToString(); // T
                rowData.Add(rowDataTemp);
            }
            WriteFile(rowData, filename); // save csv

            // refresh data storage lists and set the object to available
            xlist = new List<float>();
            ylist = new List<float>();
            tmlist = new List<float>();
        }
        available = true;
    }

    void WriteFile(List<string[]> rowData, string filename)
    {
        StreamWriter outStream = System.IO.File.CreateText(filename);

        for (int i = 0; i < rowData.Count - 1; i++)
        {
            outStream.WriteLine(string.Join(",", rowData[i]));
        }
        outStream.Write(string.Join(",", rowData[rowData.Count - 1]));
        outStream.Close();
    }
}
