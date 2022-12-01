using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ShowInstructions : MonoBehaviour
{

    private string InstrText;
    public bool AllowChoice { get; set; }
    
    GameObject canvasGO;

    private Text text;

    void Awake()
    {
        // Load the Arial font from the Unity Resources folder.
        Font arial;
        arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        AllowChoice = false;
        InstrText = "Move your mouse straight through the target and observe where the cursor goes.\nThe cursor movement will be rotated relative to your movement.\nThe next time that target appears try to hit the target by countering that rotation.\n\n[SPACE to continue]";

        // Create Canvas GameObject.
        canvasGO = new GameObject();
        canvasGO.name = "Canvas";
        canvasGO.AddComponent<Canvas>();
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Get canvas from the GameObject.
        Canvas canvas;
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Create the Text GameObject.
        GameObject textGO = new GameObject();
        textGO.transform.parent = canvasGO.transform;
        textGO.AddComponent<Text>();

        // Set Text component properties.
        text = textGO.GetComponent<Text>();
        text.font = arial;
        text.text = InstrText;
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;

        // Provide Text position and size using RectTransform.
        RectTransform rectTransform;
        rectTransform = text.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, 0, 0);
        rectTransform.sizeDelta = new Vector2(600, 200);

        DisplayInstructions();
    }


    public void HideInstructions()
    {
        canvasGO.SetActive(false);
    }

    public void DisplayInstructions()
    {
        canvasGO.SetActive(true);
    }

    public void SetInstructions(string stringIn)
    {
        InstrText = stringIn;
        text.text = InstrText;
    }
}
