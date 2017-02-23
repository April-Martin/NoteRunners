using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buddy : MonoBehaviour
{
    public float xOffset = 1f;
    public float InterpolationFactor = 0.05f;

    PitchTester pt;
    GameController gc;

    SpriteRenderer spriteRenderer;
    Transform playerTransform;

	// Use this for initialization
	void Start ()
    {
        pt = GameObject.Find("Pitch Tester").GetComponent<PitchTester>();
        gc = GameObject.Find("Game Controller").GetComponent<GameController>();

        spriteRenderer = GameObject.Find("Buddy").GetComponent<SpriteRenderer>();
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        spriteRenderer.color = Random.ColorHSV();

        }
        else
        {
            targetPos = new Vector3(playerTransform.position.x + xOffset, transform.position.y, 0f);
        }

        transform.position = Vector3.Lerp(transform.position, targetPos, InterpolationFactor);
    }
}
