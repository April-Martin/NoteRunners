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
        InvokeRepeating("spawnWall", 0f, 2f);
	}
	
	// Update is called once per frame
	void Update ()
    {
        movePlayerAndCamera();
	}

    void spawnWall()
    {
        Vector3 newWallWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0f, 5f));
        newWallWorldPosition.z = 0;
        newWallWorldPosition.x += (newWallWorldPosition.x * speedMultiplier * CameraSpeed) / 5;

        Debug.Log(newWallWorldPosition);
        GameObject instantiatedWall = Instantiate(Wall, newWallWorldPosition, Quaternion.identity);
    }

    void movePlayerAndCamera()
    {
        Vector3 deltaPos = (Vector3.right * CameraSpeed *  speedMultiplier * Time.deltaTime);
        Camera.main.transform.position += deltaPos;
        Player.transform.position += deltaPos;
    }
}
