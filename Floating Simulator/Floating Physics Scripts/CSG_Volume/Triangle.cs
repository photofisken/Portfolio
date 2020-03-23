using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Triangle
{
    public Vector3[] vertices = new Vector3[3];
    public Vector3 position;
    
    public Triangle(Vector3[] points)
    {
        position = Vector3.zero;

        // For all the vertices in the triangle (3) create a MyVertex with the triangle(s) it is in
        for (int i = 0; i < Mathf.Min(points.Length, 3); i++)
        {
            vertices[i] = points[i];
            position += points[i];
        }

        position /= vertices.Length;
    }

}
