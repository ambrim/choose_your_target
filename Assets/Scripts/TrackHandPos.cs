using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] // NEED TO ADD AUDIO CLICK SOUND WHEN PASS TARGET DISTANCE
public class TrackHandPos : MonoBehaviour
{
    public float x { get; set; }
    public float y { get; set; }
    public float addx { get; set; } // NEED TO REMEMBER TO SET THESE WHEN THE GAME GETS SET UP -- DO NOT WANT TO INITIALIZE WITH A POTENTIALLY WRONG VALUE
    public float addy { get; set; }
    public float startx { get; set; }
    public float starty { get; set; }
    public float angle { get; set; }
    public bool record { get; set; }
    public bool available { get; set; }
    public float distance { get; set; }

    public List<float> xlist { get; set; }
    public List<float> ylist { get; set; }
    public List<float> tmlist { get; set; }

    [SerializeField] bool absoluteMouse;

    AudioSource audioSource;
    Camera cam;
    PlayerData _playerData;

    float tm;

    // below parameters will have to change if using a different monitor
    // can I use ints here if they later need to be multiplied/added to floats?
    float screen_height_mm;
    float screen_length_mm;
    float tab_length;
    float tab_height;
    float xscaleFactor;
    float yscaleFactor;

    

    // setup stuff that happens when the game starts
    void Awake()
    {
        // should this next line be put into another script?
        Application.targetFrameRate = 250; // do this to try to get the max sampling rate out of the tablet (should be up to 125 Hz, but somehow can get more than that)

        // get access to playerData
        GameObject gameInfoObj = GameObject.Find("GameInfo");
        //_trialIterator = gameInfoObj.GetComponent<IterateTrials>();
        _playerData = gameInfoObj.GetComponent<PlayerData>();

        // set up camera
        cam = Camera.main; // the first gameobject tagged "MainCamera"

        // set up audio source
        audioSource = GetComponent<AudioSource>();

        record = false; // by default not recording hand position, assuming that the game starts in an instruction mode
        available = true; // by default available to start recording hand position

        // this is all specific for the monitor being used. Would probably be better to load this data in via a settings file.
        screen_height_mm = 269f;
        screen_length_mm = 477f;
        tab_length = 325f; // tablet length mm.. %This will need to be updated based on the tablet being used.ATM all same size.
        tab_height = 203f; // tablet height mm..

        xscaleFactor = tab_length / screen_length_mm;
        yscaleFactor = tab_height / screen_height_mm;

        if (!absoluteMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
            x = 1;
            y = 1;
        }
    }

    // get the stylus position and update according to cursor calibration
    // record the position in lists if record is true
    void Update()
    {
        tm = Time.realtimeSinceStartup;


        if (absoluteMouse)
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            
            x = mousePos.x * xscaleFactor + addx;
            y = mousePos.y * yscaleFactor + addy;
        } else
        {
            float xmov = Input.GetAxis("Mouse X");
            float ymov = Input.GetAxis("Mouse Y");

            x = x + xmov;
            y = y + ymov;
        }

        distance = Vector2.Distance(new Vector2(x, y), new Vector2(startx, starty)); // hand distance from starting location
        angle = Mathf.Atan2(y, x);

        if (record)
        {
            // do not log hand position if it is the same as last sample
            if (xlist.Count == 0)
            {
                xlist.Add(x);
                ylist.Add(y);
                tmlist.Add(tm);
            }
            else if (Vector2.Distance(new Vector2(x,y),
                new Vector2(xlist[xlist.Count - 1], ylist[ylist.Count - 1])) > 0) 
            {
                xlist.Add(x);
                ylist.Add(y);
                tmlist.Add(tm);
            }
        }
    }

    public void playSound()
    {
        audioSource.PlayOneShot(audioSource.clip, 0.7F);
    }

    public void save()
    {
        record = false; // make sure we're not still adding to the x and y lists
        available = false; // indicate that this object is busy

        string filebase;
        if (_playerData.numTrials < 10) filebase = _playerData.savePath + "/T000" + _playerData.numTrials.ToString();
        else if (_playerData.numTrials < 100) filebase = _playerData.savePath + "/T00" + _playerData.numTrials.ToString();
        else if (_playerData.numTrials < 1000) filebase = _playerData.savePath + "/T0" + _playerData.numTrials.ToString();
        else filebase = _playerData.savePath + "/T" + _playerData.numTrials.ToString();


        // hand positions
        string filename = filebase + "_hand.csv";
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
