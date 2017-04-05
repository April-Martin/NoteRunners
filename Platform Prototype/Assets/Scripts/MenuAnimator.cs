using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimator : MonoBehaviour
{
    public float xPivot = 0f;
    public float runnerDelay = 7f;
    public float runnerSpeed = 2f;
    public float repeatInterval = 15f;

    float timeElapsed = 0f;

    GameObject buddy;
    GameObject runner;
    TextMesh title;

    bool isRunning = false;

	// Use this for initialization
	void Start ()
    {
        buddy = GameObject.Find("Menu Buddy");
        runner = GameObject.Find("Menu Runner");
        title = GameObject.Find("Title").GetComponent<TextMesh>();

        Invoke("QueRunner", runnerDelay);
    }
	
	// Update is called once per frame
	void Update ()
    {
        title.color = Random.ColorHSV();

        if (buddy.transform.position.x < xPivot)
        {
            buddy.transform.position += (new Vector3(0, 1) - buddy.transform.position) * 0.05f;
        }
        else
        {
            buddy.transform.position += buddy.transform.position.x > 16f ? Vector3.zero : new Vector3((Mathf.Abs(buddy.transform.position.x) + 0.5f) * Time.deltaTime, 0);
        }

        if(isRunning)
        {
            runner.transform.position += runner.transform.position.x > 16f ? Vector3.zero : new Vector3(runnerSpeed, 0) * Time.deltaTime;
        }

        if(timeElapsed > repeatInterval)
        {
            runner.transform.position = buddy.transform.position = new Vector3(-15f, 1);
            isRunning = false;
            Invoke("QueRunner", runnerDelay);
            timeElapsed = 0f;
        }
        timeElapsed += Time.deltaTime;
    }

    void QueRunner()
    {
        Debug.Log("Go!");
        isRunning = true;
    }
}
