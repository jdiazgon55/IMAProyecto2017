using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CR_Surface : MonoBehaviour 
{
	
	public Camera camera;			// Cámara ortográfica que renderiza el grid sobre el que se aplica la deformación
	public GameObject handler;		// Prefab que se utilizará para colocar los puntos de control 
	GameObject[,] handlers = null;	// Matriz de puntos de control (se crean dinamicamente)
	Vector3[,] points = null;		// Matriz de puntos creados a partir de los puntos de control
	public MeshFilter meshObject;	// Objeto sobre el que se aplica la malla calculada a partir de los puntos e control
	
	public float tension = 0.5f;	// Tensión para la interpolación de Catmull-Rom
	public int patches = 4;			// Número de divisiones para crear los puntos de control
	public int divisions = 10;		// Número de divisiones entre puntos de control
	
	private CalibrationResult calibrationResult=null;	// Resultados de la calibración (para leer y escribir en un fichero xml)
	
	void Start () 
	{
		// Intentar cargar una calibración previa
		load();								
		
		// Iniciar los puntos de control
		if (calibrationResult != null)
			initHandlers(calibrationResult.points);
		else
			initHandlers(null);
	}
	
	void Update () 
	{
		// Recalcular los puntos de control
		computePoints(divisions);
		
		// Dibujar los puntos de control (solo para depuración)
		//drawPoints();
		
		// Actualizar la malla a partir de los puntos de control
		updateMesh();
		
		// Si se pulsa la tecla S almacenra la posición de los puntos de control
		if (Input.GetKeyDown(KeyCode.S))
			save();
		
		// Si se pulsa la tecla H ocultar/mostrar los puntos de control
		if (Input.GetKeyDown(KeyCode.H))
			foreach (GameObject g in handlers)
				g.SetActive(!g.activeSelf);

	}
	
	// Crear los puntos de control
	void initHandlers(float[][][] points)
	{
		float size = 1.0f;
		int parts = patches;
		
		float cellSize = size / parts;
		Vector3 origin = new Vector3(-size/2.0f, -size/2.0f, 0);
		
		handlers = new GameObject[parts + 1,parts + 1]; 
		for (int i = 0; i <= parts; i++)
			for (int j = 0; j <= parts; j++)
			{
				// En cada punto de control se instancia un nuevo manejador
				Vector3 pos = origin + new Vector3(i * cellSize, j * cellSize, 0);
				GameObject g = Instantiate(handler, pos, Quaternion.identity);
				g.layer = gameObject.layer;
				g.transform.parent = this.transform;
				float tam = cellSize / 6.0f;
				g.transform.localScale = new Vector3(tam, tam, tam);
				if (points == null)
					g.transform.localPosition = pos;
				else
					g.transform.localPosition = new Vector3(points[i][j][0], points[i][j][1], 0);
				g.GetComponent<VertexHandler>().camera = camera;
				
				g.GetComponent<VertexHandler>().index = new Vector2(i, j);
				handlers[i,j] = g;
			}
		
		VertexHandler.handlers = handlers;
				
	}
	
	// Producto escalar de dos vectores de tamaño 4
	float dot(Vector4 v, Vector4 u)
	{
		return v[0] * u[0] + v[1] * u[1] + v[2] * u[2] + v[3] * u[3];	
	}
	
	// Multiplicación de un vector de tamaño 4 por una matrix de 4x4
	Vector4 mult(Vector4 v, Matrix4x4 m)
	{
		Vector4 res = new Vector4(0, 0, 0, 0);
		for (int i = 0; i < 4; i++)
			for (int j = 0; j < 4; j++)
				res[i] += v[j] * m[i, j];
		return res;
	}
	
	// Función de interpolación de Catmull-Rom en 2 dimensiones
	Vector3 CatmullRomPoint(float u, float w, int i, int j)
	{
		float t = tension;
		
		Matrix4x4 B = new Matrix4x4();
		B.SetRow(0, new Vector4(-t, 2.0f-t, t-2.0f, t));
		B.SetRow(1, new Vector4(2.0f*t, t-3.0f, 3.0f-2.0f*t, -t));
		B.SetRow(2, new Vector4(-t, 0, t, 0));
		B.SetRow(3, new Vector4(0, 1, 0, 0));
		
		Matrix4x4 BT = B.transpose;
		
		int r = handlers.GetLength(0)-1;
		int c = handlers.GetLength(1)-1;
		
		Matrix4x4 PX = new Matrix4x4();
		Matrix4x4 PY = new Matrix4x4();
		for (int x = -1; x < 3; x++)
		{
			for (int y = -1; y < 3; y++)
			{
				int ii = Mathf.Max(Mathf.Min(i + x, r),0);
				int jj = Mathf.Max(Mathf.Min(j + y, c),0);
				
				PX[x+1,y+1] = handlers[ii,jj].transform.localPosition.x;
				PY[x+1,y+1] = handlers[ii,jj].transform.localPosition.y;
			}
		}
		
		Vector4 vu = new Vector4(u * u * u, u * u, u, 1);
		Vector4 vw = new Vector4(w * w * w, w * w, w, 1);
		
		Matrix4x4 BPBX = B * PX * BT;
		Matrix4x4 BPBY = B * PY * BT;
		
		float resX = dot(vu,mult(vw, BPBX));
		float resY = dot(vu,mult(vw, BPBY));
 		
		return new Vector3(resX,resY,0);
	}
	
	// Calcular los puntos de la malla a partir de los puntos de control
	void computePoints(int res)
	{
		int rows = handlers.GetLength(0);
		int cols = handlers.GetLength(1);
		
		points = new Vector3[(rows-1)*res +1, (cols-1)*res +1];
		
		// Parches
		for (int i = 0; i < rows-1; i++)
			for (int j = 0; j < cols-1; j++)
			{	
				// Divisiones
				for (int ri = 0; ri <= res; ri++)
				{
					for (int rj = 0; rj <= res; rj++)
					{
						points[i * res + ri, j * res + rj] = CatmullRomPoint(
							(float)ri / (float)res, 
							(float)rj / (float)res,
							i, j);
					}
				}
			}
	}
	
	// Dibujar todos los puntos de la malla (solo para depuración)
	void drawPoints()
	{
		int rows = points.GetLength(0);
		int cols = points.GetLength(1); 
		
		for (int i = 0; i < rows - 1; i++)
			for (int j = 0; j < cols - 1; j++)
			{
				Debug.DrawLine(points[i, j], points[i + 1, j]);
				Debug.DrawLine(points[i, j], points[i, j + 1]);
			}
		
	}
	
	// Actualizar la geometría de la malla a partir de los vertices calculados
	void updateMesh()
	{
		Mesh m = meshObject.mesh;

		int rows = points.GetLength(0);
		int cols = points.GetLength(1); 
		
		Vector3[] vertex = new Vector3[rows * cols];
		Vector2[] uv = new Vector2[vertex.Length];
		Vector3[] normals = new Vector3[vertex.Length];
		
		//ArrayList tris = new ArrayList();
		int[] tris = new int[(rows - 1) * (cols - 1) * 6];
		int cont = 0;

		for (int i = 0; i < rows; i++)
			for (int j = 0; j < cols; j++)
			{
				//Debug.DrawLine(points[i, j], points[i + 1, j]);
				//Debug.DrawLine(points[i, j], points[i, j + 1]);
				
				int index = i * cols + j;
				vertex[index] = points[i, j];
				uv[index] = new Vector2((float)i / (rows - 1),(float)j / (cols-1));
				normals[index] = new Vector3(0, 0, 1);
				
				if ((i < rows - 1) && (j < cols - 1))
				{
					
					tris[cont] = index;
					tris[cont+2] = index + cols;
					tris[cont+1] = index + 1;
					cont += 3;
					
					
					tris[cont] = index + 1;
					tris[cont+2] = index + cols;
					tris[cont+1] = index + cols + 1;
					
					cont += 3;
					
				}
					
			}
		m.vertices = vertex;
		m.normals = normals;
		m.uv = uv;
		m.triangles = tris;
	}

	// Almacenar la posición de los puntos de control en un fichero XML
	void save()
	{
		CalibrationResult cr = new CalibrationResult();
		cr.divisions = divisions;
		
		// El fichero se almacena en la carpeta StreamingAssets con el nombre del objeto
		cr.save(handlers, Application.streamingAssetsPath + "/" + this.gameObject.name + ".xml");
	}
	
	// Cargar la posición de los puntos de control de un fichero XML
	void load()
	{
		try
		{
			// El fichero se carga de la carpeta StreamingAssets con el nombre del objeto
			calibrationResult = CalibrationResult.load(Application.streamingAssetsPath + "/" + this.gameObject.name + ".xml");
			if (calibrationResult == null)
				return;
		
			divisions = calibrationResult.divisions;
		}
		catch
		{
		}
	}
	
}
