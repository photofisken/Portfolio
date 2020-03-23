using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drag : MonoBehaviour
{
    public static Vector3 GetDragForce(float coefficientOfDrag, float density, Vector3 velocity, float area)
    {
        return coefficientOfDrag * density * velocity * velocity.magnitude * area;
    }
}
