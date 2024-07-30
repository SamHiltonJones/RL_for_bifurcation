using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WomersleyFlow : MonoBehaviour
{
    public float averageFlowRate = 450; // Average flow rate in μL/min
    public float flowRateVariation = 3.8f; // Flow rate variation in μL/min
    public float frequency = 1.0f; // Frequency of the periodic turbulence
    public float vesselRadius = 0.0005f; // Radius of the vessel in meters (0.5 mm)
    public float viscosity = 0.004f; // Viscosity of the fluid in Pa.s (approximation for blood)
    public float density = 1060f; // Density of the fluid in kg/m^3 (approximation for blood)

    public GameObject[] targetRegions; // Array of target regions (cylinders)

    private List<GameObject> magnetsAndObstacles;
    private const float volumeToVelocityFactor = 1e-9f / 60f; // Conversion factor from μL/min to m^3/s

    void Start()
    {
        magnetsAndObstacles = new List<GameObject>();
        UpdateMagnetsAndObstacles();
    }

    void Update()
    {
        UpdateMagnetsAndObstacles();

        if (magnetsAndObstacles.Count == 0)
        {
            return;
        }

        float time = Time.time;
        float flowRate = averageFlowRate + flowRateVariation * Mathf.Sin(2 * Mathf.PI * frequency * time);

        // Ensure flowRate is within a reasonable range
        if (flowRate < 0) flowRate = 0;

        // Convert flow rate from μL/min to velocity in m/s
        float meanVelocity = flowRate * volumeToVelocityFactor / (Mathf.PI * vesselRadius * vesselRadius);

        // Calculate Womersley number
        float omega = 2 * Mathf.PI * frequency;
        float womersleyNumber = vesselRadius * Mathf.Sqrt((omega * density) / viscosity);

        Debug.Log($"Flow Rate: {flowRate}, Mean Velocity: {meanVelocity}, Omega: {omega}, Density: {density}, Viscosity: {viscosity}, Womersley Number: {womersleyNumber}");

        // Apply the Womersley flow force to all magnets and obstacles
        for (int i = magnetsAndObstacles.Count - 1; i >= 0; i--)
        {
            if (magnetsAndObstacles[i] == null)
            {
                magnetsAndObstacles.RemoveAt(i);
            }
            else
            {
                ApplyWomersleyFlow(magnetsAndObstacles[i], meanVelocity, womersleyNumber, time);
            }
        }
    }

    void UpdateMagnetsAndObstacles()
    {
        // Find all magnets and obstacles in the scene by tags
        GameObject[] currentMagnets = GameObject.FindGameObjectsWithTag("magnet");
        GameObject[] currentObstacles = GameObject.FindGameObjectsWithTag("obstacle");

        // Add new magnets and obstacles to the list
        AddNewObjectsToList(currentMagnets);
        AddNewObjectsToList(currentObstacles);
    }

    void AddNewObjectsToList(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            if (!magnetsAndObstacles.Contains(obj))
            {
                magnetsAndObstacles.Add(obj);
            }
        }
    }

    void ApplyWomersleyFlow(GameObject obj, float meanVelocity, float womersleyNumber, float time)
    {
        Rigidbody objRb = obj.GetComponent<Rigidbody>();
        if (objRb == null)
        {
            Debug.LogError("No Rigidbody component found on the object: " + obj.name);
            return;
        }

        // Find the nearest target region's center axis
        GameObject nearestRegion = FindNearestRegion(obj.transform.position);
        if (nearestRegion == null)
        {
            Debug.LogError("No nearest target region found for the object: " + obj.name);
            return;
        }

        // Calculate the shortest distance to the central axis of the nearest target region
        float r = CalculateDistanceToAxis(obj.transform.position, nearestRegion) / 1000f; // Convert mm to meters
        float R = vesselRadius; // Radius of the vessel

        // Ensure r does not exceed the vessel radius
        r = Mathf.Clamp(r, 0, R);

        // Radial velocity profile (simplified for demonstration purposes)
        float u_r = meanVelocity * (1 - Mathf.Pow(r / R, 2));

        // Axial velocity with pulsatile component, avoiding division by zero
        float pulsatileComponent = (flowRateVariation / averageFlowRate) * Mathf.Sin(2 * Mathf.PI * frequency * time);
        float exponent = -Mathf.Pow(womersleyNumber, 2) * (1 - r / R);
        float exponentialDecay = Mathf.Exp(exponent);

        float u_z = meanVelocity * (1 + pulsatileComponent * exponentialDecay);

        Debug.Log($"Object: {obj.name}, r: {r}, R: {R}, meanVelocity: {meanVelocity}, u_r: {u_r}, pulsatileComponent: {pulsatileComponent}, exponent: {exponent}, exponentialDecay: {exponentialDecay}, u_z: {u_z}");

        // Ensure the velocity is finite and within a reasonable range
        if (float.IsInfinity(u_z) || float.IsNaN(u_z))
        {
            Debug.LogError($"Calculated velocity u_z is invalid: {u_z}");
            return;
        }

        // Get the local axis of the cylinder (assuming it runs along the y-axis)
        Vector3 localAxis = nearestRegion.transform.up;

        // Calculate the projection of the force along the local axis
        Vector3 projectedForce = localAxis.normalized * u_z;

        // Ensure the force direction is always towards the positive z direction
        if (projectedForce.z < 0)
        {
            projectedForce = -projectedForce;
        }

        // Apply force based on the calculated velocity profile along the local axis
        Vector3 force = projectedForce * objRb.mass;
        objRb.AddForce(force);

        // Debug statement to verify force application
        Debug.Log($"Applied force to {obj.name}: {force}");
    }

    GameObject FindNearestRegion(Vector3 position)
    {
        GameObject nearestRegion = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject region in targetRegions)
        {
            float distance = Vector3.Distance(position, region.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestRegion = region;
            }
        }

        return nearestRegion;
    }

    float CalculateDistanceToAxis(Vector3 objPosition, GameObject region)
    {
        Transform regionTransform = region.transform;

        // Get the local axis of the cylinder (assuming it runs along the y-axis)
        Vector3 localAxis = regionTransform.up;

        // Calculate the vector from the region's position to the object's position
        Vector3 toObj = objPosition - regionTransform.position;

        // Project this vector onto the local axis to find the closest point on the axis to the object
        Vector3 projectionOntoAxis = Vector3.Project(toObj, localAxis);

        // The vector from the object to the closest point on the axis
        Vector3 closestPointOnAxis = regionTransform.position + projectionOntoAxis;
        Vector3 distanceToAxis = objPosition - closestPointOnAxis;

        // Return the magnitude of this vector as the distance to the axis
        return distanceToAxis.magnitude;
    }
}
