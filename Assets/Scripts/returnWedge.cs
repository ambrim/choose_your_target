using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class returnWedge : MonoBehaviour
{
    Camera cam;
    TrackHandPos _hand;

    LineRenderer line;
    int segments;
    float anchorWedgeStart;
    float cwWedgeStart;
    float ccwWedgeStart;
    Color grey;

    public float anchorWedgeCenter { get; set; }

    // Start is called before the first frame update
    void Awake()
    {
        // get other objects in the game
        GameObject handObj = GameObject.Find("HandTracker");
        _hand = handObj.GetComponent<TrackHandPos>();

        grey = new Color(0.69f, 0.69f, 0.69f);

        segments = 25;
        line = gameObject.GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.useWorldSpace = true;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.startColor = grey;
        line.endColor = grey;

        cam = Camera.main;

        UpdateAnchorWedgeCenter(45); // just iniitialize at some arbitrary value
    }

    void Update()
    {
        // TODO: either fix recording hand positions here or offset to another script
        //_playerData.TakeHandSample(_hiddenPos, Time.realtimeSinceStartup, _playerData.F_returnToStart);

        float angleDiff = anchorWedgeCenter - ConvertAngle_Cart2LineRend(_hand.angle);
        // constrain angleDiff within +/- 180 degrees
        while (angleDiff > (Mathf.PI))
        {
            angleDiff -= (2 * Mathf.PI);
        }
        while (angleDiff < (-1 * Mathf.PI))
        {
            angleDiff += (2 * Mathf.PI);
        }

        float sendStart;
        if (Mathf.Abs(angleDiff) <= Mathf.PI / 3) sendStart = anchorWedgeStart;
        else if (angleDiff > 0) sendStart = ccwWedgeStart; // positive --> must be CCW of target angle
        else sendStart = cwWedgeStart;

        placeArc(sendStart);
    }


    public void placeArc(float startAngle)
    {
        float x;
        float y;
        float angle = startAngle; // recall that 0 is straight up for unity stuff

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(angle) * _hand.distance;
            y = Mathf.Cos(angle) * _hand.distance;

            line.SetPosition(i, new Vector3(x, y, 0));

            angle += ((2 * Mathf.PI/3) / segments);
        }
    }

    float ConvertAngle_Cart2LineRend(float angleIn)
    {
        // deals with angles in radians
        float output = -1 * angleIn + Mathf.PI / 2;
        return output;
    }

    public void UpdateAnchorWedgeCenter(float angleIn)
    {
        // takes angle in degrees, converts to radians then to LineRenderer space
        float value = Mathf.Deg2Rad * angleIn;
        value = ConvertAngle_Cart2LineRend(value);

        anchorWedgeCenter = value;
        anchorWedgeStart = value - (2 * Mathf.PI / 6);
        cwWedgeStart = anchorWedgeStart + (2 * Mathf.PI / 3);
        ccwWedgeStart = cwWedgeStart + (2 * Mathf.PI / 3);
    }
}
