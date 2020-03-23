using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    public static Gravity instance;

    public static float Force;
    public float gForce = -9.82f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        Force = gForce;
    }

    public static Vector3 Acceleration()
    {
        return new Vector3(0, Force, 0);
    }
}
