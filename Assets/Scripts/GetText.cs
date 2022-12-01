using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System;

public class GetText : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] string localDirectory;
    GameObject subjectField;
    GameObject trialOrderField;
    InputField subjectID;
    InputField trialOrder;
    string playerFilePath;
    string trialOrderFilePath;

    private void Start()
    {
        subjectField = GameObject.Find("InputSubjectID");
        subjectID = subjectField.GetComponent<InputField>();
        trialOrderField = GameObject.Find("InputTrialOrder");
        trialOrder = trialOrderField.GetComponent<InputField>();
        playerFilePath = localDirectory + "/LoadIn/playerID.csv";
        trialOrderFilePath = localDirectory + "/LoadIn/playerTrialOrder.csv";

        if (File.Exists(playerFilePath)) subjectID.text = ReadStringFromCSV(playerFilePath);
        if (File.Exists(trialOrderFilePath)) trialOrder.text = ReadStringFromCSV(trialOrderFilePath);
    }

    public void StoreID()
    {        
        InputField input = subjectField.GetComponent<InputField>();
        WriteFile(input.text, playerFilePath);
    }

    public void StoreTrialOrder()
    {
        
        InputField input = trialOrderField.GetComponent<InputField>();
        WriteFile(input.text, trialOrderFilePath);
    }

    void WriteFile(string data, string filename)
    {
        StreamWriter outStream = System.IO.File.CreateText(filename);
        outStream.Write(data);
        outStream.Close();
    }

    string ReadStringFromCSV(string path)
    {
        TextAsset csvFile = new TextAsset(System.IO.File.ReadAllText(path));
        return csvFile.text;
    }
}
