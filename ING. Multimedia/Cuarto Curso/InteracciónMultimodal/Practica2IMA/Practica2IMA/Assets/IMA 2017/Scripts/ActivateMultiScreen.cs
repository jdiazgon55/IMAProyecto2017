using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateMultiScreen : MonoBehaviour 
{
	void Start () 
	{
		Debug.Log("Pantallas conectadas: " + Display.displays.Length);

		// Activar todas las pantallas
		for (int i = 1; i < Display.displays.Length; i++)
			Display.displays[i].Activate();
		
	}
}
