using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public GameObject Wall;

    public PlayerMovement Player;
    public float CameraSpeed = 0.5f;

    internal float speedMultiplier = 1f;


	// Use this for initialization
	void Start () 
    {
        InvokeRepeating("spawnWall", 0f, 3f);
	}
	
	// Update is called once per frame
	void Update ()
    {
        movePlayerAndCamera();
	}

    void spawnWall()
    {

        Vector3 newWallWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0f, 10f));
        // Offset by half of wall size; makes sure wall spawns fully offscreen
        newWallWorldPosition.x += Wall.transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x / 2;  
        GameObject instantiatedWall = Instantiate(Wall, newWallWorldPosition, Quaternion.identity);


        float hoopCenter = .5f * Random.Range(-1, 9);
        float hoopRange = Random.Range(1, 3);
        instantiatedWall.GetComponent<Wall>().SetAttributes(hoopCenter, hoopRange);
    }

    void movePlayerAndCamera()
    {
        Vector3 deltaPos = (Vector3.right * CameraSpeed * speedMultiplier * Time.deltaTime);
        Camera.main.transform.position += deltaPos;
        Player.transform.position += deltaPos;
    }


}
