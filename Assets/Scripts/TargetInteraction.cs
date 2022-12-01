using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetInteraction : MonoBehaviour
{

    SpriteRenderer m_SpriteRenderer;
    public float Distance { get; set; }
    public float x { get; set; }
    public float y { get; set; } 
    public float diameter { get; set; }

    GameObject _start;

    public float Angle { get; set; }
    public string ColorString { get; set; }

    // Start is called before the first frame update
    void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        m_SpriteRenderer.color = new Color(0, 0.6f, 0.9f); // change this to blue


        _start = GameObject.Find("Start");

        //Vector2 startPos = new Vector2(_start.transform.position.x, _start.transform.position.y);
        //Vector2 targetPos = new Vector2(transform.position.x, transform.position.y);
        //Distance = Vector2.Distance(startPos, targetPos);
        Distance = 8; // for now, just force distance to be 8
        x = transform.position.x;
        y = transform.position.y;
        diameter = transform.localScale.x; // assumes localscale set to same in x and y
    }

    public void SetColor(Color colorIn)
    {
        m_SpriteRenderer.color = colorIn;

        // this next part sucks but just need to do this quickly
        // probably just need to have a list of known colors in here and then select them based on a string sent to this function
        if (colorIn.r == 1 && colorIn.g == 0 && colorIn.b == 1) ColorString = "magenta";
        else if (colorIn.r == 0 && colorIn.g == 0.8f && colorIn.b == 0.2f) ColorString = "green";
        else if (colorIn.r == 0 && colorIn.g == 0.8f && colorIn.b == 1) ColorString = "cyan";
        else ColorString = "unknown";
    }

    public void SetAngle(float angleIn)
    {
        x = Distance * Mathf.Cos((angleIn * Mathf.PI) / 180);
        y = Distance * Mathf.Sin((angleIn * Mathf.PI) / 180);
        transform.position = new Vector3(x, y, transform.position.z);
        Angle = angleIn;
    }
}
