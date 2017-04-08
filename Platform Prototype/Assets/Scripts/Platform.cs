/*
 *  Note: The line renderer's points are listed in clockwise order from top-right
 * 
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Platform : MonoBehaviour
{
    //X position of the left edge of the camera in World Units. Calculate this once.
    static float xCameraLeftEdgeInWU;
    private bool isFilled = false;
    public float height = .9f;
    private float width;
    private LineRenderer outline;
    private Vector3[] outlineCorners;
    private SpriteRenderer fillSprite;

    void Awake()
    {

        outline = GetComponent<LineRenderer>();
        outlineCorners = new Vector3[outline.numPositions];
        outline.GetPositions(outlineCorners);
		outlineCorners[3].y = outlineCorners[4].y = (height-outline.startWidth) / 2 * height;
		outlineCorners [0].y = (height / 2 - outline.startWidth);
		outlineCorners [1].y = outlineCorners [2].y = -(height - outline.startWidth) / 2 * height;
        outline.SetPositions(outlineCorners);

        fillSprite = transform.GetChild(1).GetComponent<SpriteRenderer>();
		float targetHeight = height - 2 * outline.startWidth;
		fillSprite.transform.localScale = new Vector3 (fillSprite.transform.localScale.x, targetHeight / fillSprite.bounds.size.y, 1);
        //fillSprite.enabled = false;
    }

    public void SetPlatFilled(bool filled)
    {
        if (filled)
        {
            isFilled = true;
            fillSprite.enabled = true;
        }
        else
        {
            isFilled = false;
            fillSprite.enabled = false;
        }
    }

	public void SetPlatColor(Color c)
    {
		
		outline.startColor = new Color(c.r * .3f, c.g * .3f, c.b * .3f);
		outline.endColor = new Color(c.r * .3f, c.g * .3f, c.b * .3f);

		if (isFilled)
			fillSprite.color = c;
		else
			fillSprite.color = c;
		
		/*
		outline.startColor = c;
		outline.endColor = c;

		if (isFilled)
			fillSprite.color = c;
		else
			fillSprite.color = new Color(c.r * .9f, c.g * .9f, c.b * .9f);

		*/
        return;
    }

    public void SetPlatWidth(float w)
    {
        width = w;
		outlineCorners[0].x = (width-outline.startWidth) / 2;
		outlineCorners[1].x = (width-outline.startWidth) / 2;
		outlineCorners[2].x = -(width-outline.startWidth) / 2;
		outlineCorners[3].x = -(width-outline.startWidth) / 2;
		outlineCorners[4].x = (width-outline.startWidth) / 2 + outline.startWidth / 2;
		//Debug.Log ("outline.startWidth = " + outline.startWidth);
		//Debug.Log ("outline.endWidth = " + outline.endWidth);

        outline.SetPositions(outlineCorners);

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.size = new Vector2(width, col.size.y);

     //   if (isFilled)
        {
            float startWidth = fillSprite.bounds.size.x;
			float targetWidth = width - 2 * outline.startWidth;
            fillSprite.transform.localScale = new Vector3(fillSprite.transform.localScale.x * targetWidth / startWidth, fillSprite.transform.localScale.y, 1);
        }

        return;
    }


    public void SetRangeMarker(int range, Color target)
    {
        LineRenderer rangeIndicator = transform.GetChild(0).GetComponent<LineRenderer>();
        rangeIndicator.sortingLayerName = "-1";

        Vector3 offset = new Vector3(width / 2 + outline.startWidth, 0, 0);
        rangeIndicator.SetPosition(0, transform.position - offset);
        rangeIndicator.SetPosition(1, transform.position + offset);
        rangeIndicator.widthMultiplier = range * 1.25f;

        rangeIndicator.startColor = new Color(target.r, target.g, target.b, .4f);
        rangeIndicator.endColor = new Color(target.r, target.g, target.b, .4f);        
    }

    public void DisableRangeMarker()
    {
        transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
    }

    
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
     
}
