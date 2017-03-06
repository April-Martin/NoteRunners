using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Platform : MonoBehaviour
{

    //X position of the left edge of the camera in World Units. Calculate this once.
    static float xCameraLeftEdgeInWU;

    void OnBecameInvisible()
    {
        xCameraLeftEdgeInWU = Camera.main.transform.position.x + Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, -10)).x
                                 - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10)).x;

        //Make sure the platform "disappeared" off the left side of the screen so we know we're done with it.
        if (transform.position.x < xCameraLeftEdgeInWU)
        {
            Destroy(this.gameObject);
        }
    }

    public void SetRangeMarker(int range, Color target)
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.sortingLayerName = "-1";

        Vector3 offset = new Vector3(GetComponent<SpriteRenderer>().bounds.size.x / 2, 0, 0);
        lr.SetPosition(0, transform.position - offset);
        lr.SetPosition(1, transform.position + offset);
        lr.widthMultiplier = range * 1.25f;

        lr.startColor = new Color(target.r, target.g, target.b, .4f);
        lr.endColor = new Color(target.r, target.g, target.b, .4f);        
    }

    public void DisableRangeMarker()
    {
        GetComponent<LineRenderer>().enabled = false;
    }
}
