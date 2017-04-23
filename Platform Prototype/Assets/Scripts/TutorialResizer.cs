using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialResizer : MonoBehaviour {

    private Image[] imgs = new Image[4];
    private float targetHeightRatio = .55f;
	// Use this for initialization
	void Start () {
		for (int i=0; i<transform.childCount; i++)
        {
            imgs[i] = transform.GetChild(i).GetComponent<Image>();
        }
	}
	
	// Update is called once per frame
	void Update () {
        float screenHeight = Camera.main.pixelHeight;
        float imgHeight = imgs[0].rectTransform.rect.height;
        float scaleFactor = screenHeight / imgHeight * targetHeightRatio;
        foreach (Image img in imgs)
        {
            img.rectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        }

	}
}
