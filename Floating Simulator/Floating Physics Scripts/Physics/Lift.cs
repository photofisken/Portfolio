using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buoyancy
{

    public static Vector3 Calculate(Transform meshTransform, Vector3 intersectPosition, float volumeUnderWater, float totalVolume)
    {
        float densityAir = 1.2f; // at around 15 degrees celcius outside, kg/m3
        float densityWater = 997f; // kg/m3  
    
        // F = Volume * density * gForce (formula F = mg)
        float waterForce = volumeUnderWater * densityWater * -Gravity.Force;

        float volumeOverWater = totalVolume - volumeUnderWater;
        float airForce = volumeOverWater * densityAir * -Gravity.Force;

        Debug.Log("Under water " + volumeUnderWater + ", Over water " + volumeOverWater + ", Water Force: " + waterForce + ", Air Force: " + airForce);

        Vector3 liftForce = new Vector3(0, waterForce + airForce, 0);
        return liftForce;
    }

	public static Vector3 CalculateLiftNaive(float volumeUnderWater)
	{
        float densityWater = 997f; // kg/m3
		float waterForce = volumeUnderWater * densityWater * -Gravity.Force;
		return new Vector3(0f, waterForce, 0f);
	}
}
