using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class PlayerData : MonoBehaviour
{
    //Member variables can be referred to as
    //fields.
    public int numTrials { get; set; }
    public int numPoints { get; set; }
    public int pointsPossible { get; set; }
    public bool showDebug { get; set; }
    public float HoldAddRand { get; set; }
    public string ChoiceInstructions { get; set; }
    public float HoldDur { get; set; }
    public float FBDur { get; set; }

    public List<float> HandX { get; set; }
    public List<float> HandY { get; set; }
    public List<float> HandTm { get; set; }
    public List<int> HandSampleFlags { get; set; }

    public string Choice { get; set; }
    public float ChoiceAvailableTm { get; set; }
    public float ChoiceMadeTm { get; set; }

    public string playerID { get; set; }
    public string savePath { get; set; }

    [SerializeField] string _directory;
    public string LocalDirectory { get; set; }

    public string filebase { get; set; }

    public float mouseAddX { get; set; }
    public float mouseAddY { get; set; }

    public int F_holding { get; } = 1;
    public int F_reachStart { get; } = 2;
    public int F_reaching { get; } = 3;
    public int F_reachEnd { get; } = 4;
    public int F_returnToStart { get; } = 0;


    void Awake()
    {
        LocalDirectory = _directory;

        numTrials = 1;
        numPoints = 0;
        pointsPossible = 1;
        showDebug = false;

        string path = _directory + "/LoadIn/playerID.csv";
        TextAsset csvFile = new TextAsset(System.IO.File.ReadAllText(path));
        string[] records = csvFile.text.Split('\n'); // split csv by line
        playerID = records[0];

        savePath = _directory + "/Data/" + playerID;
        if (Directory.Exists(savePath)==false) Directory.CreateDirectory(savePath);

        // get pen to cursor calibration
        string calibFilename = _directory + "/LoadIn/cursorCalib.csv";

        if (File.Exists(calibFilename))
        {
            csvFile = new TextAsset(System.IO.File.ReadAllText(calibFilename));
            records = csvFile.text.Split('\n'); // split csv by line
            foreach (string record in records)
            {
                string[] values = record.Split(','); // split csv by comma-separated fields
                string header = values[0];
                if (header == "AddX") mouseAddX = float.Parse(values[1]);
                else if (header == "AddY") mouseAddY = float.Parse(values[1]);
            }
        }
        else
        {
            mouseAddX = 0f;
            mouseAddY = 0f;
        }

    }

    public void IncrementTrialNum()
    {
        numTrials++;
    }

    void RefreshHandSampleLists()
    {
        HandX = new List<float>();
        HandY = new List<float>();
        HandTm = new List<float>();
        HandSampleFlags = new List<int>();
    }

    void RefreshChoiceValues()
    {
        Choice = "";
        ChoiceAvailableTm = -1;
        ChoiceMadeTm = -1;
    }

    public void Refresh()
    {
        RefreshHandSampleLists();
        RefreshChoiceValues();
    }

    public void SaveTrialData()
    {
        if (numTrials < 10) filebase = savePath + "/T000" + numTrials.ToString();
        else if (numTrials < 100) filebase = savePath + "/T00" + numTrials.ToString();
        else if (numTrials < 1000) filebase = savePath + "/T0" + numTrials.ToString() ;
        else filebase = savePath + "/T" + numTrials.ToString();

        
        // hand positions
        string filename = filebase + "_hand.csv";
        List<string[]> rowData = new List<string[]>();
        // manually create headers
        string[] rowDataTemp = new string[4];
        rowDataTemp[0] = "X";
        rowDataTemp[1] = "Y";
        rowDataTemp[2] = "T";
        rowDataTemp[3] = "Flag";
        rowData.Add(rowDataTemp);
        // populate columns
        for (int i = 0; i < HandX.Count; i++)
        {
            rowDataTemp = new string[4];
            rowDataTemp[0] = HandX[i].ToString(); // X
            rowDataTemp[1] = HandY[i].ToString(); // Y
            rowDataTemp[2] = HandTm[i].ToString(); // T
            rowDataTemp[3] = HandSampleFlags[i].ToString(); // flag
            rowData.Add(rowDataTemp);
        }
        WriteFile(rowData, filename);

        if (ChoiceAvailableTm > -1)
        {
            // choices
            filename = filebase + "_choice.csv";
            rowData = new List<string[]>();
            // manually create headers
            rowDataTemp = new string[3];
            rowDataTemp[0] = "Choice";
            rowDataTemp[1] = "PromptTime";
            rowDataTemp[2] = "ChoiceTime";
            rowData.Add(rowDataTemp);

            rowDataTemp = new string[3];
            rowDataTemp[0] = Choice.ToString(); ;
            rowDataTemp[1] = ChoiceAvailableTm.ToString();
            rowDataTemp[2] = ChoiceMadeTm.ToString();
            rowData.Add(rowDataTemp);
            WriteFile(rowData, filename);
            RefreshChoiceValues();
        }

        //RefreshCursorSampleLists();
        RefreshHandSampleLists();
    }

    void WriteFile(List<string[]> rowData, string filename)
    {
        StreamWriter outStream = System.IO.File.CreateText(filename);

        for (int i = 0; i < rowData.Count - 1; i++)
        {
            outStream.WriteLine(string.Join(",", rowData[i]));
        }
        outStream.Write(string.Join(",", rowData[rowData.Count-1]));
        outStream.Close();
    }
}
