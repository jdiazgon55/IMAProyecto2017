using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class CalibrationResult
{
	public int divisions;
	public float[][][] points;
	
	public void save(GameObject[,] handlers, string file)
	{
		XmlSerializer serializer = new XmlSerializer(typeof(CalibrationResult));
		TextWriter writer = new StreamWriter(file);
		
		int rows = handlers.GetLength(0);
		int cols = handlers.GetLength(1);
		
		points = new float[rows][][];
		
		for (int i = 0; i < rows; i++)
		{
			points[i] = new float[cols][];
			for (int j = 0; j < cols; j++)
			{
				points[i][j] = new float[2];
				points[i][j][0] = handlers[i, j].transform.position.x;
				points[i][j][1] = handlers[i, j].transform.position.y;
			}
		}
		serializer.Serialize(writer, this);
		writer.Close();
	}
	
	public static CalibrationResult load(string file)
	{
		StreamReader xmlInputStream = new StreamReader(file);
		if (xmlInputStream == null)
			return null;
		
		XmlSerializer deserializer = new XmlSerializer(typeof(CalibrationResult));
		CalibrationResult res = (CalibrationResult)deserializer.Deserialize(xmlInputStream);
		return res;
	}
}
