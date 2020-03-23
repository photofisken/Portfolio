using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeCalculator
{
    public static float GetTotalVolume(Mesh mesh)
    {
        float totalVolume = 0f;

        for (int i = 0; i < mesh.triangles.Length; i+=3)
        {
            Vector3 p1 = mesh.vertices[mesh.triangles[i + 0]];
            Vector3 p2 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 p3 = mesh.vertices[mesh.triangles[i + 2]];

            // Calculate the volume from the three vertices and the oririn = 0;
            float volume = Mathf.Abs(Vector3.Dot(p1, Vector3.Cross(p2, p3))) / 6f;
            totalVolume += volume;
        }

        return totalVolume;
    }

    public static float GetVolume(ref List<Triangle> underWaterTriangles, Transform meshTransform, Vector3 intersectPosition)   // TODO: CHECK IF REF IMPROVES PERFORMANCE
    {
        float underWaterVolume = 0f;
        Vector3 medianPoint = CalculateMedianPoint(ref underWaterTriangles);

        for (int i = 0; i < underWaterTriangles.Count; i++)
        {
            // The triangle from the mesh will form the base of a tetrahedron, so we can calculate the volume of the mesh
            Vector3 a = underWaterTriangles[i].vertices[0];
            Vector3 b = underWaterTriangles[i].vertices[1];
            Vector3 c = underWaterTriangles[i].vertices[2];

            // V = |(a - d) dot ((b - d) cross (c - d))| / 6 , (A third of the determinant of the base of a tetrahedron times the height (median point))
            float volume = Mathf.Abs(Vector3.Dot(a - medianPoint, Vector3.Cross(b - medianPoint, c - medianPoint))) / 6f;
            underWaterVolume += volume;
        }

        return underWaterVolume;
    }

    static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    static Vector3 CalculateMedianPoint(ref List<Triangle> underWaterTriangles)
    {
        Vector3 medianPoint = Vector3.zero;
        int vertexIndex = 0;

        for (int i = 0; i < underWaterTriangles.Count; i++)
        {
            for (int j = 0; j < underWaterTriangles[i].vertices.Length; j++)
            {
                medianPoint += underWaterTriangles[i].vertices[j];
                vertexIndex++;
            }
        }

        return medianPoint /= vertexIndex;
    }
}
