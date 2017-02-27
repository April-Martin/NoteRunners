using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{

    //X position of the left edge of the camera in World Units. Caluculate this once.
    static float xCameraLeftEdgeInWU;

    void OnBecameInvisible()
    {
        xCameraLeftEdgeInWU = Camera.main.transform.position.x + Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, -10)).x
                                 - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10)).x;

        Debug.Log(xCameraLeftEdgeInWU);

        //Make sure the platform "dissapeared" off the left side of the screen so we know we're done with it.
        if (transform.position.x < xCameraLeftEdgeInWU)
        {
            Destroy(this.gameObject);
        }
    }
}
