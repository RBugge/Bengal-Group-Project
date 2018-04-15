using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour {

    public GameObject cube;
    public bool rotation = true;

	// Use this for initialization
	void Start () {
        cube = GameObject.Find("Cube");

    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            rotation = !rotation;
        }

        if (rotation)
        {
            cube.transform.Rotate(0, 100 * Time.deltaTime, 0);
        }
	}
}
