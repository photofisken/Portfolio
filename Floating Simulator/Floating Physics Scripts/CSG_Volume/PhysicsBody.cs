using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhysicsBody : MonoBehaviour
{
    // Adjustable factors
    public float mass;
    public float area = 1f;
    public float coefficientOfDrag = 1f;
    public float totalVolume;

    public Vector3 velocity;
    public Vector3 lift;    

    //Not adjustable factors (for now)
    [SerializeField] MeshFilter mf;
    [HideInInspector] public bool dragging = false;
    [HideInInspector] public bool start = true;
    [SerializeField] GameObject water;
    [SerializeField] GameObject ground;
    [SerializeField] Vector3 intersectPosition;
    float densityAir = 1.2f; // at around 15 degrees celcius outside, kg/m3
    float densityWater = 997f; // kg/m3  

    public List<Vector3> vertices;
    public List<Triangle> triangles;
    public List<Triangle> underWaterTriangles;

    [SerializeField] Text UIText;

    private void Start()
    {
        mf = GetComponent<MeshFilter>();
        water = GameObject.Find("Water");
        ground = GameObject.Find("Ground");
        intersectPosition = water.transform.position;
        totalVolume = VolumeCalculator.GetTotalVolume(mf.mesh);

        Intersection.ConvertToTriangles(mf.mesh, out vertices, out triangles);
    }

    private void FixedUpdate()
    {
        if (!dragging && !start)
        {
            velocity += Gravity.Acceleration();

            float time = Time.realtimeSinceStartup;

            // Get Triangle List
            List<Triangle> underWaterTriangles = Intersection.GetTrianglesUnderWater(ref triangles, ref vertices, transform, intersectPosition);

            // Get Volume
            float volumeUnderWater = VolumeCalculator.GetVolume(ref underWaterTriangles, transform, intersectPosition);

            if(UIText != null)
            {
                float totalVolumeRounded = Mathf.Round(totalVolume * 100f) / 100f;
                float volumeUnderWaterRounded = Mathf.Round(volumeUnderWater * 100f) / 100f;
                UIText.text = transform.name + " - Total Volume:" + totalVolumeRounded + ", Volume Under Water: " + volumeUnderWaterRounded;
            }

            // Calculate drag from air or water
            float dragDensity = volumeUnderWater > 0f ? densityWater : densityAir;
            Vector3 drag = Drag.GetDragForce(coefficientOfDrag, dragDensity, velocity, area);
            velocity -= drag / mass;

            // Calculate Lift
            Vector3 bouyancy = Buoyancy.Calculate(transform, intersectPosition, volumeUnderWater, totalVolume);

            // velocity += lift;
            velocity += bouyancy / mass * Time.fixedDeltaTime;

            if (transform.position.y < ground.transform.position.y)
            {
                Vector3 currentPosition = transform.position;
                currentPosition.y = ground.transform.position.y;
                transform.position = currentPosition;
                velocity = Vector3.zero;
            }
            
            ApplyVelocity();
        }
    }

    public void ApplyVelocity()
    {
        transform.position += velocity * Time.fixedDeltaTime;
    }
}
