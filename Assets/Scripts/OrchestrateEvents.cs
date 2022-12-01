using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class OrchestrateEvents : MonoBehaviour
{
   
    TargetInteraction _target;
    IterateTrials _trialIterator;
    GameObject _start;
    CursorPos _cursor0;
    CursorPos _cursor1;
    CursorPos _cursor2;
    TrackHandPos _hand;
    PlayerData _playerData;
    returnWedge _returnGuide;
    ShowInstructions _instructions;
    Text _trialTextDisplay;
    Camera cam;
    Text _choiceText;

    Vector2 startPos;
    Vector2 _hiddenPos;
    bool _beLoggingTrace;
    bool _cursorTracksMouse;
    bool _ignoringStartMvt;
    bool _returningToStart;
    bool holdInitiated;
    bool playHitSound;
    float handAngle;
    float holdTime;

    public string gamePhase { get; set; }
    public int eventCode { get; set; }
    public int codeReturn { get; } = 0;
    public int codeHold { get; } = 1;
    public int codeHoldCancelled { get; } = 2;
    public int codeTargetOn { get; } = 3;
    public int codePassedTarget { get; } = 4;
    public int codeEPFBOn { get; } = 5;
    public int codeEPFBOff { get; } = 6;
    // possible gamePhase values are:
    //      instr, return, hold, reach, fb, fboption


    void Awake()
    {
        Cursor.visible = false; // hide mouse cursor

        // not sure I need this in every script? Need to look up
        Application.targetFrameRate = 250; // do this to try to get the max sampling rate out of the tablet (should be up to 125 Hz, but somehow can get more than that)
        QualitySettings.vSyncCount = 0; // this step is essential, otherwise Unity will ignore the targeted framerate

        // get references to external objects
        _start = GameObject.FindGameObjectWithTag("StartLocation");

        GameObject gameInfoObj = GameObject.Find("GameInfo");
        _trialIterator = gameInfoObj.GetComponent<IterateTrials>();
        _playerData = gameInfoObj.GetComponent<PlayerData>();
        _playerData.Refresh();

        GameObject targetGameObj = GameObject.Find("Target");
        _target = targetGameObj.GetComponent<TargetInteraction>();

        GameObject returnGameObj = GameObject.Find("returnGuide");
        _returnGuide = returnGameObj.GetComponent<returnWedge>();

        GameObject c0Group = GameObject.Find("Cursor0Group");
        _cursor0 = c0Group.GetComponent<CursorPos>();
        GameObject c1Group = GameObject.Find("Cursor1Group");
        _cursor1 = c1Group.GetComponent<CursorPos>();
        GameObject c2Group = GameObject.Find("Cursor2Group");
        _cursor2 = c2Group.GetComponent<CursorPos>();

        GameObject handObj = GameObject.Find("HandTracker");
        _hand = handObj.GetComponent<TrackHandPos>();

        GameObject instrObj = GameObject.Find("TextEvents");
        _instructions = instrObj.GetComponent<ShowInstructions>();

        GameObject TextTag = GameObject.Find("ChoiceText");
        _choiceText = TextTag.GetComponent<Text>();

        GameObject trialTextObj = GameObject.Find("TrialText");
        _trialTextDisplay = trialTextObj.GetComponent<Text>();

        cam = Camera.main; // the first gameobject tagged "MainCamera"



        // fill in values for variables
        startPos = new Vector2(_start.transform.position.x, _start.transform.position.y);
        gamePhase = "instr";

        // turn off cursor, start, and target for now
        _cursor0.gameObject.SetActive(false);
        _cursor1.gameObject.SetActive(false);
        _cursor2.gameObject.SetActive(false);
        _start.gameObject.SetActive(false);
        _target.gameObject.SetActive(false);
        _choiceText.gameObject.SetActive(false);
        _trialTextDisplay.gameObject.SetActive(false);
        _returnGuide.gameObject.SetActive(false);

        // set things up for trial 1, assuming that we want the unlabeled cursor in the beginning
        _cursor0.active = true;
        _cursor1.active = false;
        _cursor2.active = false;
    }

    // Update is called once per frame
    void Update()
    {
        // possible gamePhase values are:
        //      instr, return, hold, reach, fb, fboption, busy
        // if gamePhase == instr, wait for participant to press spacebar before continuing the game
        // if gamePhase == return, show the return to center arc thing
        // if gamePhase == hold, show the cursor(s). if hold time elapses, show target. if leave center before hold elapses, go back to return.
        // if gamePhase == reach, do not show the cursor(s). when the hand passes the target distance, set a timer to show the endpoint feedback/feedback choice
        // if gamePhase == fb, don't change anything until the timer elapses. Can use this opportunity to save data. When timer elapses, enter return phase and set next trial conditions
        // if gamePhase == fbonline, don't change anything until the user makes a choice about which rotation they want to counter. Then, detect keypress and save trialdata. Then enter return phase and set next trial conditions.
        if (gamePhase == "instr")
        {
            gamePhase = "busy";
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // turn start and return guide on, progress to next phase
                _start.gameObject.SetActive(true);
                _returnGuide.gameObject.SetActive(true);

                _instructions.HideInstructions();

                gamePhase = "return";
                eventCode = codeReturn;
            }
            else
            {
                gamePhase = "instr";
            }
        } else if (gamePhase == "return")
        {
            if (_hand.distance < _start.transform.localScale.x / 4) // hand is inside start circle
            {
                // stop showing wedge and start showing appropriate cursor
                _returnGuide.gameObject.SetActive(false);
                if ((_cursor0.active) || (_cursor1.active && _cursor2.active)) // if both active, don't want to show any rotation yet
                {
                    _cursor0.gameObject.SetActive(true);
                    _cursor0.setPosition(_hand.x, _hand.y);
                } else if (_cursor1.active) // person selected cursor 1
                {
                    _cursor1.gameObject.SetActive(true);
                    _cursor1.setPosition(_hand.x, _hand.y);
                } else if (_cursor2.active) // person selected cursor 2
                {
                    _cursor2.gameObject.SetActive(true);
                    _cursor2.setPosition(_hand.x, _hand.y);
                }

                // enter the hold phase and set a timer to display the target
                _trialTextDisplay.gameObject.SetActive(true);
                StartCoroutine("TargetOnDelay");
                gamePhase = "hold";
                eventCode = codeHold;

                //if (_hand.distance < _cursor0.transform.localScale.x / 4) // cursor should be fully inside the target
                //{
                //    // enter the hold phase and set a timer to display the target
                //    _trialTextDisplay.gameObject.SetActive(true);
                //    StartCoroutine("TargetOnDelay");
                //    gamePhase = "hold";
                //    eventCode = codeHold;
                //}
                logHandPos();
            }
        } else if (gamePhase == "hold")
        {
            if (_hand.distance > _start.transform.localScale.x / 2) // hand is outside the start circle
            {
                // cancel the target appearance
                StopCoroutine("TargetOnDelay");

                // stop showing cursor and start showing wedge
                if ((_cursor0.active) || (_cursor1.active && _cursor2.active)) _cursor0.gameObject.SetActive(false);
                else if (_cursor1.active) _cursor1.gameObject.SetActive(false);
                else if (_cursor2.active) _cursor2.gameObject.SetActive(false);
                
                _returnGuide.gameObject.SetActive(true);

                _trialTextDisplay.gameObject.SetActive(false);
                gamePhase = "return";
                eventCode = codeHoldCancelled;
            } else
            {
                // update the hand position (veridical to minimize adaptation at the center)
                if ((_cursor0.active) || (_cursor1.active && _cursor2.active)) _cursor0.setPosition(_hand.x, _hand.y);
                else if (_cursor1.active) _cursor1.setPosition(_hand.x, _hand.y);
                else if (_cursor2.active) _cursor2.setPosition(_hand.x, _hand.y);
            }
            logHandPos();
        } else if (gamePhase == "reach")
        {
            // we do not want to update the cursor position, we just want to be checking for the hand position for now
            if (_hand.distance >= _target.Distance)
            {
                gamePhase = "busy";
                eventCode = codePassedTarget;
                logHandPos();
                // hand passed target, so play sound and record position for cursor to be at
                _hand.playSound();
                if (_cursor0.active) _cursor0.setNextPosition(_target.Distance, _hand.angle);
                if (_cursor1.active) _cursor1.setNextPosition(_target.Distance, _hand.angle);
                if (_cursor2.active) _cursor2.setNextPosition(_target.Distance, _hand.angle);

                // enter feedback phase and set timer for the fb to display/elapse
                if (_trialIterator.choiceAvailable) // go into fboption phase next
                {
                    StartCoroutine("FBOptionDelay");
                } else // go into fb phase next
                {
                    StartCoroutine("FBOnDelay");
                }
            } else
            {
                logHandPos();
            }
        }
        else if (gamePhase == "fb")
        {
            logHandPos();
        }
        else if (gamePhase == "fboption")
        {
            // wait for keypress to decide what to do on the next trial
            gamePhase = "busy";
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // log choice for next trial
                _playerData.Choice = "1";
                _playerData.ChoiceMadeTm = Time.realtimeSinceStartup;

                // turn everything off, save data, and increment trial
                // let something orchestrating the saving of data handle the trial increment
                _target.gameObject.SetActive(false);
                _choiceText.gameObject.SetActive(false);
                eventCode = codeEPFBOff;
                logHandPos();
                _trialIterator.SaveTrialData();
                _playerData.SaveTrialData();
                if (_cursor0.active) _cursor0.gameObject.SetActive(false);
                if (_cursor1.active) _cursor1.gameObject.SetActive(false);
                if (_cursor2.active) _cursor2.gameObject.SetActive(false);
                _cursor0.active = false;
                _cursor1.active = true;
                _cursor2.active = false;

                _trialTextDisplay.gameObject.SetActive(false);
                _trialIterator.NextTrialGivenSelection();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // log choice for next trial
                _playerData.Choice = "2";
                _playerData.ChoiceMadeTm = Time.realtimeSinceStartup;

                // turn everything off, save data, and increment trial
                // let something orchestrating the saving of data handle the trial increment
                _target.gameObject.SetActive(false);
                _choiceText.gameObject.SetActive(false);
                eventCode = codeEPFBOff;
                logHandPos();
                _trialIterator.SaveTrialData();
                _playerData.SaveTrialData();
                if (_cursor0.active) _cursor0.gameObject.SetActive(false);
                if (_cursor1.active) _cursor1.gameObject.SetActive(false);
                if (_cursor2.active) _cursor2.gameObject.SetActive(false);
                _cursor0.active = false;
                _cursor1.active = false;
                _cursor2.active = true;

                _trialTextDisplay.gameObject.SetActive(false);
                _trialIterator.NextTrialGivenSelection();
            }
            else
            {
                logHandPos();
                gamePhase = "fboption"; // check for keypress again
            }
        } else if (gamePhase == "busy") {
            logHandPos();
        }
    }

    void logHandPos()
    {
        if (_playerData.HandX.Count > 0)
        {
            if (_hand.x != _playerData.HandX[_playerData.HandX.Count - 1] ||
                _hand.y != _playerData.HandY[_playerData.HandY.Count - 1] ||
                eventCode != _playerData.HandSampleFlags[_playerData.HandSampleFlags.Count - 1])
            {
                _playerData.HandX.Add(_hand.x);
                _playerData.HandY.Add(_hand.y);
                _playerData.HandTm.Add(Time.realtimeSinceStartup);
                _playerData.HandSampleFlags.Add(eventCode);
            }
        }
        else
        {
            _playerData.HandX.Add(_hand.x);
            _playerData.HandY.Add(_hand.y);
            _playerData.HandTm.Add(Time.realtimeSinceStartup);
            _playerData.HandSampleFlags.Add(eventCode);
        }
    }

    IEnumerator TargetOnDelay()
    {
        holdTime = (Random.value * _playerData.HoldAddRand) + _playerData.HoldDur;
        yield return new WaitForSeconds(holdTime);
        _target.gameObject.SetActive(true);
        _target.SetAngle(_target.Angle);
        eventCode = codeTargetOn;
        logHandPos();
        gamePhase = "reach";
    }

    IEnumerator FBOnDelay()
    {
        //Debug.Log("delaying fb by: " + _trialIterator.fbDelay.ToString());
        yield return new WaitForSeconds(_trialIterator.fbDelay);
        eventCode = codeEPFBOn;
        logHandPos();
        if (_cursor0.active) _cursor0.displayPosition();
        if (_cursor1.active) _cursor1.displayPosition();
        if (_cursor2.active) _cursor2.displayPosition();
        gamePhase = "fb"; // fb doesn't actually do anything here
        StartCoroutine("FBOffDelay");
    }

    IEnumerator FBOffDelay()
    {
        yield return new WaitForSeconds(_playerData.FBDur);
        eventCode = codeEPFBOff;
        logHandPos();
        _trialIterator.SaveTrialData();
        _playerData.SaveTrialData();
        _cursor0.gameObject.SetActive(false);
        _cursor1.gameObject.SetActive(false);
        _cursor2.gameObject.SetActive(false);
        _target.gameObject.SetActive(false);
        _trialTextDisplay.gameObject.SetActive(false);
        _trialIterator.NextTrial(); // change trial info and stuff in trial iterator
        // add something here about saving data
    }

    IEnumerator FBOptionDelay()
    {
        //Debug.Log("delaying fb and giving option");
        yield return new WaitForSeconds(_trialIterator.fbDelay);
        eventCode = codeEPFBOn;
        logHandPos();
        _playerData.ChoiceAvailableTm = Time.realtimeSinceStartup;
        _choiceText.gameObject.SetActive(true);
        _cursor0.gameObject.SetActive(false);
        _cursor1.gameObject.SetActive(true);
        _cursor1.displayPosition();
        _cursor2.gameObject.SetActive(true);
        _cursor2.displayPosition();
        _trialTextDisplay.gameObject.SetActive(false);
        _choiceText.gameObject.SetActive(true);
        gamePhase = "fboption";
    }
}
