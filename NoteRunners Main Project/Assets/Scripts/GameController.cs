using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public GameObject Wall;

    public PlayerMovement Player;
    public float CameraSpeed = 0.5f;

    internal float speedMultiplier = 1f;
    private Dictionary<string, float> NotePosLookup;

	// Use this for initialization
	void Start () 
    {
        NotePosLookup = new Dictionary<string, float>();
        FillNoteLookup();
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

    void FillNoteLookup()
    {
        NotePosLookup.Add("D4", -.5f);
        NotePosLookup.Add("E4", 0f);
        NotePosLookup.Add("F4", .5f);
        NotePosLookup.Add("G4", 1f);
        NotePosLookup.Add("A4", 1.5f);
        NotePosLookup.Add("B4", 2f);
        NotePosLookup.Add("C5", 2.5f);
        NotePosLookup.Add("D5", 3f);
        NotePosLookup.Add("E5", 3.5f);
        NotePosLookup.Add("F5", 4f);
        NotePosLookup.Add("G5", 4.5f);
	}



}
