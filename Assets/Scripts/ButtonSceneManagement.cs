using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneManagement : MonoBehaviour
{
    void Awake()
    {
        Cursor.visible = true; // make sure the user can see the OS cursor
    }

    public void StartRunningTrials()
    {
        SceneManager.LoadScene("RunTrialsWithoutPoints");
    }

    public void StartCalibration()
    {
        SceneManager.LoadScene("CalibrateCursor");
    }
}
