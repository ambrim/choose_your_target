using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class IterateTrials : MonoBehaviour
{
    public float Rotation0 { get; set; } // I think these rotation values will be unnnecessary?
    public float Rotation1 { get; set; }
    public float Rotation2 { get; set; }
    public bool choiceAvailable { get; set; }
    public float fbDelay { get; set; }
    public float fbDur { get; set; }
    public string choiceMade { get; set; } // not sure this has to be a string?


    List<float> _rotations;
    List<float> _leftrotations;
    List<float> _rightrotations;
    List<float> _targetAngles;
    List<float> _feedbackDelays;
    List<int> _choices;
    List<string> _trialText;
    List<string> _instructionStrings;
    
    List<Color> _colorList;

    //set up colors
    Color magentaRGB;
    Color greenRGB;
    Color cyanRGB;

    float RotationGreen;
    float RotationMagenta;

    TargetInteraction _target;
    CursorPos _cursor0;
    CursorPos _cursor1;
    CursorPos _cursor2;
    PlayerData _playerData;
    ShowInstructions _instructions;
    returnWedge _returnGuide;
    Text _playerHUD;
    Text _trialTextDisplay;
    OrchestrateEvents _orchestrateEvents;
    GameObject _start;
    TrackHandPos _hand;


    string choiceInstructionString;


    void Awake()
    {
        _start = GameObject.FindGameObjectWithTag("StartLocation");

        GameObject targetGameObj = GameObject.Find("Target");
        _target = targetGameObj.GetComponent<TargetInteraction>();

        GameObject c0Group = GameObject.Find("Cursor0Group");
        _cursor0 = c0Group.GetComponent<CursorPos>();
        GameObject c1Group = GameObject.Find("Cursor1Group");
        _cursor1 = c1Group.GetComponent<CursorPos>();
        GameObject c2Group = GameObject.Find("Cursor2Group");
        _cursor2 = c2Group.GetComponent<CursorPos>();

        GameObject gameInfoObj = GameObject.Find("GameInfo");
        _playerData = gameInfoObj.GetComponent<PlayerData>();
        _orchestrateEvents = gameInfoObj.GetComponent<OrchestrateEvents>();

        GameObject instructionsObj = GameObject.Find("TextEvents");
        _instructions = instructionsObj.GetComponent<ShowInstructions>();

        GameObject returnGameObj = GameObject.Find("returnGuide");
        _returnGuide = returnGameObj.GetComponent<returnWedge>();

        GameObject playerHUD = GameObject.Find("PlayerHUD");
        _playerHUD = playerHUD.GetComponent<Text>();

        GameObject trialTextObj = GameObject.Find("TrialText");
        _trialTextDisplay = trialTextObj.GetComponent<Text>();

        GameObject handObj = GameObject.Find("HandTracker");
        _hand = handObj.GetComponent<TrackHandPos>();

        // set up colors
        magentaRGB = new Color(1, 0, 1);
        greenRGB = new Color(0, 0.8f, 0.2f);
        cyanRGB = new Color(0, 0.6f, 0.9f);
        _colorList = new List<Color>(3);
        _colorList.Add(cyanRGB);
        _colorList.Add(greenRGB);
        _colorList.Add(magentaRGB);

        // load experimenter specified setting and order path pairs
        string path = _playerData.LocalDirectory + "/LoadIn/playerTrialOrder.csv";
        TextAsset csvFile = new TextAsset(System.IO.File.ReadAllText(path));
        string trialOrderPath = _playerData.LocalDirectory + "/LoadIn/SessionSetups/" + csvFile.text + "Orders.csv";
        string experimentSettingPath = _playerData.LocalDirectory + "/LoadIn/SessionSetups/" + csvFile.text + "Settings.csv";
        string cursorCalibPath = _playerData.LocalDirectory + "/LoadIn/cursorCalib.csv";

        // TODO: DECIDE WHERE THESE SETTINGS SHOULD BE STORED
        // load game settings (this has to be coherent with the trialOrder)
        csvFile = new TextAsset(System.IO.File.ReadAllText(experimentSettingPath));
        string[] records = csvFile.text.Split('\n'); // split csv by line
        foreach (string record in records)
        {
            string[] values = record.Split(','); // split csv by comma-separated fields
            string header = values[0];
            if (header == "FBDur") _playerData.FBDur = float.Parse(values[1]);
            else if (header == "HoldTime") _playerData.HoldDur = float.Parse(values[1]);
            else if (header == "RandHold") _playerData.HoldAddRand = float.Parse(values[1]);
            else if (header == "ChoiceInstructions") _playerData.ChoiceInstructions = values[1];
        }
        fbDur = _playerData.FBDur;

        csvFile = new TextAsset(System.IO.File.ReadAllText(cursorCalibPath));
        records = csvFile.text.Split('\n'); // split csv by line
        foreach (string record in records)
        {
            string[] values = record.Split(','); // split csv by comma-separated fields
            string header = values[0];
            if (header == "AddX") _hand.addx = float.Parse(values[1]);
            else if (header == "AddY") _hand.addy = float.Parse(values[1]);
        }


        // load trial order
        _rotations = new List<float>(); // TO DO make the number of trials a serialized field or a settings input
        _leftrotations = new List<float>();
        _rightrotations = new List<float>();
        _targetAngles = new List<float>();
        _feedbackDelays = new List<float>();
        _choices = new List<int>();
        _instructionStrings = new List<string>();
        _trialText = new List<string>();
        csvFile = new TextAsset(System.IO.File.ReadAllText(trialOrderPath));
        records = csvFile.text.Split('\n'); // split csv by line
        foreach (string record in records)
        {
            string[] values = record.Split(','); // split csv by comma-separated fields
            string header = values[0];
            if (header == "rotations")
            {
                for (int i = 1; i < values.Length; i++)
                {
                    _rotations.Add(float.Parse(values[i]));
                }
            }
            else if (header == "leftRotation")
            {
                for (int i = 1; i < values.Length; i++)
                {
                    _leftrotations.Add(float.Parse(values[i]));
                }
            }
            else if (header == "rightRotation")
            {
                for (int i = 1; i < values.Length; i++)
                {
                    _rightrotations.Add(float.Parse(values[i]));
                }
            }
            else if (header == "targetAngles")
            {
                for (int i = 1; i < values.Length; i++)
                {
                    _targetAngles.Add(float.Parse(values[i]));
                }
            }
            else if (header == "feedbackDelays")
            {
                for (int i = 1; i < values.Length; i++)
                {
                    _feedbackDelays.Add(float.Parse(values[i]));
                }
            }
            else if (header == "choices")
            {
                for (int i = 1; i < values.Length; i++)
                {
                    _choices.Add(int.Parse(values[i]));
                }
            }
            else if (header == "trialText")
            {
                for (int i = 1; i < values.Length; i++)
                {
                    _trialText.Add(values[i]);
                }
            }
            else if (header == "instructionStrings")
            {
                for (int i = 1; i < values.Length; i++)
                {
                    _instructionStrings.Add(values[i]);
                }
            }
        }
        //Debug.Log(_rotations.Count);
        //Debug.Log(_leftrotations.Count);
        //Debug.Log(_rightrotations.Count);
        //Debug.Log(_targetAngles.Count);
        //Debug.Log(_feedbackDelays.Count);
        //Debug.Log(_choices.Count);
        //Debug.Log(_instructionStrings.Count);
        //Debug.Log(_targetTag.Count);

        // update playerHUD
        //_playerHUD.text = "Trial: 1/" + _rotations.Count.ToString() + "\nPoints: 0";
        _playerHUD.text = "Trial: 1/" + _rotations.Count.ToString();

        // establish settings for trial 1
        fbDelay = _feedbackDelays[0];
        _target.Angle = _targetAngles[0];
        _cursor0.fbdelay = _feedbackDelays[0];
        _cursor0.rotation = _rotations[0];
        _cursor1.fbdelay = _feedbackDelays[0];
        _cursor1.rotation = _rotations[0];
        _cursor2.fbdelay = _feedbackDelays[0];
        _cursor2.rotation = _rotations[0];
        _trialTextDisplay.text = _trialText[0];

        if (_instructionStrings[0].Length > 0)
        {
            _instructions.SetInstructions(_instructionStrings[0]);
            _instructions.AllowChoice = false;
        }

        //_returnGuide.gameObject.SetActive(false);

        choiceAvailable = false;
    }
    
    public void NextTrial()
    {
        _playerData.IncrementTrialNum();
        if (_playerData.numTrials > _targetAngles.Count)
        {
            //SceneManager.LoadScene(_nextBlockName); // NEED SOMETHING HERE THAT IS SENSITIVE TO WHETHER OR NOT EVERYTHING HAS BEEN SAVED
            StartCoroutine(EndGame());
        }
        else
        {
            int idx = _playerData.numTrials - 1;


            // TODO: clean up this if statement -- it's too complex right now
            if (_choices[idx] == 1)
            {
                choiceAvailable = true;
                _cursor0.active = false;
                _cursor1.active = true;
                _cursor2.active = true;
            } else
            {
                choiceAvailable = false;
                _cursor0.active = true;
                _cursor1.active = false;
                _cursor2.active = false;
            }

            fbDelay = _feedbackDelays[idx];
            _target.Angle = _targetAngles[idx];
            _cursor0.rotation = _rotations[idx];
            _cursor1.rotation = _leftrotations[idx];
            _cursor2.rotation = _rightrotations[idx];
            _trialTextDisplay.text = _trialText[idx];
            _cursor0.gameObject.SetActive(false);
            _cursor1.gameObject.SetActive(false);
            _cursor2.gameObject.SetActive(false);
            _target.gameObject.SetActive(false);

            if (_instructionStrings[idx].Length == 0)
            {
                if (idx > 0) _returnGuide.UpdateAnchorWedgeCenter(_targetAngles[idx - 1]);
                _returnGuide.gameObject.SetActive(true);
                _orchestrateEvents.eventCode = _orchestrateEvents.codeReturn;
                _orchestrateEvents.gamePhase = "return";
            } else
            {
                _instructions.SetInstructions(_instructionStrings[idx]);
                _instructions.AllowChoice = false; // TODO: Deprecate this
                _start.gameObject.SetActive(false); // hide start so doesn't interfere with instructions

                _instructions.DisplayInstructions();
                _orchestrateEvents.gamePhase = "instr";
            }
        }

        // update playerHUD
        //_playerHUD.text = "Trial: " + _playerData.numTrials.ToString() +
        //    "/" + _rotations.Count.ToString() + "\nPoints: " +
        //    _playerData.numPoints.ToString();
        _playerHUD.text = "Trial: " + _playerData.numTrials.ToString() +
            "/" + _rotations.Count.ToString();
        _playerData.pointsPossible = 1;
    }

    public void NextChoiceTrial(float rotnIn)
    {
        int idx = _playerData.numTrials - 1;
        _target.Angle = _targetAngles[idx];
        // TODO: deprecate this function? Need to accommodate selection of each cursor object instead of this method
        _cursor0.fbdelay = _feedbackDelays[idx];
        _cursor0.rotation = rotnIn;
    }

    public void NextTrialGivenSelection()
    {
        // there should never be an instruction on these trials, and the game should never end on one of these trials
        _playerData.IncrementTrialNum();
        int idx = _playerData.numTrials - 1;

        choiceAvailable = false;

        // could probably function this next part out since it's repeated from above
        fbDelay = _feedbackDelays[idx];
        _target.Angle = _targetAngles[idx];
        _cursor0.rotation = _rotations[idx-1];
        _cursor1.rotation = _leftrotations[idx-1]; // use the rotation seen on the previous trial
        _cursor2.rotation = _rightrotations[idx-1]; // use the rotation seen on the previous trial
        _trialTextDisplay.text = _trialText[idx];
        _cursor0.gameObject.SetActive(false);
        _cursor1.gameObject.SetActive(false);
        _cursor2.gameObject.SetActive(false);
        _target.gameObject.SetActive(false);

        _returnGuide.UpdateAnchorWedgeCenter(_targetAngles[idx - 1]);
        _returnGuide.gameObject.SetActive(true);
        _orchestrateEvents.eventCode = _orchestrateEvents.codeReturn;
        _orchestrateEvents.gamePhase = "return";
        
        // update playerHUD
        _playerHUD.text = "Trial: " + _playerData.numTrials.ToString() +
            "/" + _rotations.Count.ToString();
        _playerData.pointsPossible = 1;
    }

    void SetUpChoiceOptions(int idx)
    {
        Rotation1 = _rotations[idx-2];
        Rotation2 = _rotations[idx - 1];
    }

    void SetUpChoiceInstructions(int idx)
    {
        _instructions.SetInstructions(_playerData.ChoiceInstructions);
    }

    public void SaveTrialData()
    {
        string filebase;
        if (_playerData.numTrials < 10) filebase = _playerData.savePath + "/T000" + _playerData.numTrials.ToString();
        else if (_playerData.numTrials < 100) filebase = _playerData.savePath + "/T00" + _playerData.numTrials.ToString();
        else if (_playerData.numTrials < 1000) filebase = _playerData.savePath + "/T0" + _playerData.numTrials.ToString();
        else filebase = _playerData.savePath + "/T" + _playerData.numTrials.ToString();

        // trial settings
        string filename = filebase + "_settings.csv";
        List<string[]> rowData = new List<string[]>();
        // manually create headers
        string[] rowDataTemp = new string[17];
        rowDataTemp[0] = "TrialNumber";
        rowDataTemp[1] = "TargetAngle";
        rowDataTemp[2] = "ChoiceAvailable";
        rowDataTemp[3] = "Cursor0Rotation";
        rowDataTemp[4] = "Cursor0Visible";
        rowDataTemp[5] = "Cursor1Rotation";
        rowDataTemp[6] = "Cursor1Visible";
        rowDataTemp[7] = "Cursor2Rotation";
        rowDataTemp[8] = "Cursor2Visible";
        rowDataTemp[9] = "FBDelay";
        rowDataTemp[10] = "FBDur";
        rowDataTemp[11] = "FBX0";
        rowDataTemp[12] = "FBY0";
        rowDataTemp[13] = "FBX0";
        rowDataTemp[14] = "FBY0";
        rowDataTemp[15] = "FBX2";
        rowDataTemp[16] = "FBY2";
        rowData.Add(rowDataTemp);

        rowDataTemp = new string[17];
        rowDataTemp[0] = _playerData.numTrials.ToString();
        rowDataTemp[1] = _target.Angle.ToString();
        rowDataTemp[2] = choiceAvailable.ToString();
        rowDataTemp[3] = _cursor0.rotation.ToString();
        rowDataTemp[4] = _cursor0.active.ToString();
        rowDataTemp[5] = _cursor1.rotation.ToString();
        rowDataTemp[6] = _cursor1.active.ToString();
        rowDataTemp[7] = _cursor2.rotation.ToString();
        rowDataTemp[8] = _cursor2.active.ToString();
        rowDataTemp[9] = fbDelay.ToString();
        rowDataTemp[10] = _playerData.FBDur.ToString();
        if (_cursor0.active)
        {
            rowDataTemp[11] = _cursor0.x.ToString();
            rowDataTemp[12] = _cursor0.y.ToString();
        } else
        {
            rowDataTemp[11] = "";
            rowDataTemp[12] = "";
        }
        if (_cursor1.active)
        {
            rowDataTemp[13] = _cursor1.x.ToString();
            rowDataTemp[14] = _cursor1.y.ToString();
        }
        else
        {
            rowDataTemp[13] = "";
            rowDataTemp[14] = "";
        }
        if (_cursor2.active)
        {
            rowDataTemp[15] = _cursor2.x.ToString();
            rowDataTemp[16] = _cursor2.y.ToString();
        }
        else
        {
            rowDataTemp[15] = "";
            rowDataTemp[16] = "";
        }
        rowData.Add(rowDataTemp);

        // save file
        StreamWriter outStream = System.IO.File.CreateText(filename);

        for (int i = 0; i < rowData.Count - 1; i++)
        {
            outStream.WriteLine(string.Join(",", rowData[i]));
        }
        outStream.Write(string.Join(",", rowData[rowData.Count - 1]));
        outStream.Close();
    }

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(10);
        Application.Quit();
    }
}
