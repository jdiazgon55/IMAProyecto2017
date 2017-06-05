using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Camera))]

public class OrthoSizeAspect : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		Camera camera;
		camera = GetComponent<Camera>();
		camera.orthographicSize = 0.5f;
		camera.aspect = 1;	
	}
}
