using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using UnityEngine.SceneManagement;

public class CursorCalibration : MonoBehaviour
{
    [SerializeField] string username;
    string startScene = "startingScreen";

    string savePath;
    string calibFilename;

    float addme_x = 0f;
    float addme_y = 0f;
    float step = 0.05f;

    float screen_height_mm;
    float screen_length_mm;
    float tab_length;
    float tab_height;
    float xscaleFactor;
    float yscaleFactor;

    Camera cam;

    void Awake()
    {
        Cursor.visible = false;

        Application.targetFrameRate = 500; // do this to try to get the max sampling rate out of the tablet (should be up to 125 Hz, but somehow can get more than that)

        savePath = "C:/Users/" + username + "/Desktop/ChooseYourTarget/LoadIn/";
        calibFilename = "C:/Users/" + username + "/Desktop/ChooseYourTarget/LoadIn/cursorCalib.csv";

        if (File.Exists(calibFilename))
        {
            TextAsset csvFile = new TextAsset(System.IO.File.ReadAllText(calibFilename));
            string[] records = csvFile.text.Split('\n'); // split csv by line
            foreach (string record in records)
            {
                string[] values = record.Split(','); // split csv by comma-separated fields
                string header = values[0];
                if (header == "AddX") addme_x = float.Parse(values[1]);
                else if (header == "AddY") addme_y = float.Parse(values[1]);
            }
        }

        // set up camera
        cam = Camera.main; // the first gameobject tagged "MainCamera"

        // this is all specific for the monitor being used. Would probably be better to load this data in via a settings file.
        screen_height_mm = 269f;
        screen_length_mm = 477f;
        tab_length = 325f; // tablet length mm.. %This will need to be updated based on the tablet being used.ATM all same size.
        tab_height = 203f; // tablet height mm..

        xscaleFactor = tab_length / screen_length_mm;
        yscaleFactor = tab_height / screen_height_mm;
    }


    void Update()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition); //
        float x = mousePos.x * xscaleFactor; // this is the xscaleFactor in TrackHandPos
        float y = mousePos.y * yscaleFactor; // this is the yscaleFactor in 
        transform.position = new Vector3(x + addme_x, y + addme_y, transform.position.z);

        if (Input.GetKeyDown(KeyCode.RightArrow)) addme_x += step;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) addme_x -= step;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) addme_y += step;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) addme_y -= step;
        else if (Input.GetKeyDown(KeyCode.Return)) SaveCalibration();
        else if (Input.GetKeyDown(KeyCode.S)) SceneManager.LoadScene("StartScene");
    }

    public void SaveCalibration()
    {
        List<string[]> rowData = new List<string[]>();
        // manually populate values
        string[] rowDataTemp = new string[2];
        rowDataTemp[0] = "AddX";
        rowDataTemp[1] = addme_x.ToString();
        rowData.Add(rowDataTemp);

        rowDataTemp = new string[2];
        rowDataTemp[0] = "AddY";
        rowDataTemp[1] = addme_y.ToString();
        rowData.Add(rowDataTemp);
        WriteFile(rowData, calibFilename);
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
