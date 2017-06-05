using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexHandler : MonoBehaviour 
{	
	public Camera camera;
	
	private bool selected = false;
	Material m;
	private Vector3 offset;
	
	public static GameObject[,] handlers;
	public Vector2 index;
	
	private int frameSelected = 0;
	
	// Use this for initialization
	void Start () 
	{
		m = GetComponent<MeshRenderer>().material;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Comprobar si se debe seleccionar un punto al hacer clic con el ratón
		if (Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.LeftShift))
			HandleInput();	
		
		// Deseleccionar los puntos al levantar el clic
		if (Input.GetMouseButtonUp(0))
			selected = false;
		
		// Evitar que la lógica de selección se aplica más de una vez por punto y frame
		if (frameSelected != Time.frameCount)
		{
			if (Input.GetKeyDown(KeyCode.Space))
				selectAll();
			
			if (Input.GetKeyDown(KeyCode.LeftArrow))
				selectLeft();
			
			if (Input.GetKeyDown(KeyCode.RightArrow))
				selectRight();
			
			if (Input.GetKeyDown(KeyCode.UpArrow))
				selectUp();
			
			if (Input.GetKeyDown(KeyCode.DownArrow))
				selectDown();
		}
		
		// Aplicar el color y posición adecuados si el punto está seleccionado
		if (selected)
		{
			m.color = Color.yellow;
			if ( Input.GetMouseButton(0) )
				transform.position = mousePos() + offset;
		}
		else
		{
			m.color = Color.red;
		}
	}
	
	// Comprobar la selección de un nuevo punto con el ratón
	void HandleInput()
	{
		if (selected)
		{
			setSelected();
		}
		
		Ray inputRay = camera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if (Physics.Raycast(inputRay, out hit))
		{
			VertexHandler vHandler = hit.collider.GetComponent<VertexHandler>();
			if (vHandler == this)
			{
				setSelected();
				if (Input.GetKey(KeyCode.LeftControl))
					selectRow();
				if (Input.GetKey(KeyCode.LeftAlt))
					selectCol();
			}
			
		}		
	}
	
	// Marcar el punto como seleccionado
	public void setSelected()
	{
		selected = true;
		offset = transform.position - mousePos();
		frameSelected = Time.frameCount;
	}
	
	// Calcular la posición del ratón en coordenadas normalizadas
	Vector3 mousePos()
	{
		Vector3 m = new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height, 0);
		return m;
	}
	
	//***************************************************
	// Funciones de selección múltiple de puntos
	void selectAll()
	{
		for (int i=0; i<handlers.GetLength(0); i++)
		{
			for (int j=0; j<handlers.GetLength(1); j++)
				handlers[i, j].GetComponent<VertexHandler>().setSelected();
		}
	}
	
	void selectRow()
	{
		for (int i=0; i<handlers.GetLength(1); i++)
		{
			handlers[(int)index.x, i].GetComponent<VertexHandler>().setSelected();
		}
	}
	
	void selectCol()
	{
		for (int i=0; i<handlers.GetLength(0); i++)
		{
			handlers[i, (int)index.y].GetComponent<VertexHandler>().setSelected();
		}
	}
	
	
	void selectLeft()
	{
		if (selected && ((int)index.x > 0))
		{
			selected = false;
			handlers[(int)index.x - 1, (int)index.y].GetComponent<VertexHandler>().setSelected();
		}
	}
	
	void selectRight()
	{
		if (selected && ((int)index.x < (handlers.GetLength(0)-1)))
		{
			selected = false;
			handlers[(int)index.x + 1, (int)index.y].GetComponent<VertexHandler>().setSelected();
		}
	}
	
	void selectUp()
	{
		if (selected && ((int)index.y < (handlers.GetLength(1)-1)))
		{
			selected = false;
			handlers[(int)index.x, (int)index.y + 1].GetComponent<VertexHandler>().setSelected();
		}
	}
	
	void selectDown()
	{
		if (selected && ((int)index.y > 0))
		{
			selected = false;
			handlers[(int)index.x, (int)index.y - 1].GetComponent<VertexHandler>().setSelected();
		}
	}
	
}
