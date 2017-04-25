/*
 *  Note: The line renderer's points are listed in clockwise order from top-right
 * 
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Platform : MonoBehaviour
{
    public float height = .9f;
    public Sprite sharpImg;
    public Sprite flatImg;
    //X position of the left edge of the camera in World Units. Calculate this once.
    static float xCameraLeftEdgeInWU;
    private float width;
    private LineRenderer outline;
    private Vector3[] outlineCorners;
    private SpriteRenderer fillSprite;
    private SpriteRenderer modifierSprite;
    private Animator modifierAnim;
    private ParticleSystem vanishingParticles;
    private ParticleSystem teleportParticles;
    private bool hasModifier = false;

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

        modifierSprite = transform.GetChild(2).GetComponent<SpriteRenderer>();
        modifierAnim = transform.GetChild(2).GetComponent<Animator>();
        modifierSprite.enabled = false;

        vanishingParticles = transform.GetChild(3).GetComponent<ParticleSystem>();
        teleportParticles = transform.GetChild(4).GetComponent<ParticleSystem>();
    }
    
    public void PlayVanishingParticles()
    {
        var sh = vanishingParticles.shape;
        sh.radius = width/2;
        var em = vanishingParticles.emission;
        em.rateOverTimeMultiplier *= width / 2;
        vanishingParticles.Play();
    }

    public void PlayTeleportParticles()
    {
        teleportParticles.Play();
        return;
    }

    public void SetPlatSharp()
    {
        modifierAnim.SetTrigger(1);
        modifierSprite.sprite = sharpImg;
        modifierSprite.enabled = true;
        hasModifier = true;
    }

    public void SetPlatFlat()
    {
        modifierAnim.SetTrigger(0);

        modifierSprite.sprite = flatImg;
        modifierSprite.enabled = true;
        hasModifier = true;
    }

	public void SetPlatColor(Color c)
    {
		
		outline.startColor = new Color(c.r * .3f, c.g * .3f, c.b * .3f);
		outline.endColor = new Color(c.r * .3f, c.g * .3f, c.b * .3f);
		fillSprite.color = c;

        modifierSprite.color = c;

        Color min = new Color(c.r * .85f, c.g * .85f, c.b * .85f);
        Color max = new Color(c.r * 1.75f, c.g * 1.75f, c.b * 1.75f);
        var main = vanishingParticles.main;
        ParticleSystem.MinMaxGradient grad = new ParticleSystem.MinMaxGradient(min, max);
        main.startColor = grad;

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
        outline.SetPositions(outlineCorners);

        if (hasModifier)
            modifierSprite.transform.localPosition = new Vector3 (-width/2 + modifierSprite.bounds.size.x/2 + .2f, height/2 + modifierSprite.bounds.size.y/2 + .2f, 0);

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.size = new Vector2(width, col.size.y);

        float startWidth = fillSprite.bounds.size.x;
		float targetWidth = width - 2 * outline.startWidth;
        fillSprite.transform.localScale = new Vector3(fillSprite.transform.localScale.x * targetWidth / startWidth, fillSprite.transform.localScale.y, 1);

        teleportParticles.transform.localPosition = new Vector3(-width / 2 + .5f, height / 2 + .75f, 0);

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
