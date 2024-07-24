using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WomersleyFlow : MonoBehaviour
{
    public float averageFlowRate = 450000.6f; // Average flow rate in μL/min
    public float flowRateVariation = 3.8f; // Flow rate variation in μL/min
    public float frequency = 0.5f; // Frequency of the periodic turbulence
    public float vesselRadius = 0.005f; // Radius of the vessel in meters
    public float viscosity = 0.004f; // Viscosity of the fluid in Pa.s (approximation for blood)
    public float density = 1060f; // Density of the fluid in kg/m^3 (approximation for blood)


    private List<GameObject> magnets;
    private const float volumeToVelocityFactor = 1e-9f / 60f; // Conversion factor from μL/min to m^3/s


    // Start is called before the first frame update
    void Start()
    {
        // Find all magnets in the scene by tag
        magnets = new List<GameObject>(GameObject.FindGameObjectsWithTag("magnet"));
        if (magnets == null || magnets.Count == 0)
        {
            Debug.LogError("No magnets found with the tag: magnet");
        }
        else
        {
            Debug.Log("Found " + magnets.Count + " magnets with the tag: magnet");
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (magnets == null || magnets.Count == 0)
        {
            return;
        }


        float time = Time.time;
        float flowRate = averageFlowRate + flowRateVariation * Mathf.Sin(2 * Mathf.PI * frequency * time);


        // Convert flow rate from μL/min to velocity in m/s
        float meanVelocity = flowRate * volumeToVelocityFactor / (Mathf.PI * vesselRadius * vesselRadius);


        // Calculate Womersley number
        float omega = 2 * Mathf.PI * frequency;
        float womersleyNumber = vesselRadius * Mathf.Sqrt(omega * density / viscosity);


        // Apply the Womersley flow force to all magnets
        foreach (GameObject magnet in magnets)
        {
            ApplyWomersleyFlow(magnet, meanVelocity, womersleyNumber, time);
        }
    }


    void ApplyWomersleyFlow(GameObject magnet, float meanVelocity, float womersleyNumber, float time)
    {
        Rigidbody magnetRb = magnet.GetComponent<Rigidbody>();
        if (magnetRb == null)
        {
            Debug.LogError("No Rigidbody component found on the magnet: " + magnet.name);
            return;
        }


        // Calculate the instantaneous velocity profile using Womersley solution
        // This is a simplified approach and assumes laminar flow


        float r = magnet.transform.position.magnitude; // Distance from the center of the vessel
        float R = vesselRadius; // Radius of the vessel


        // Ensure r does not exceed the vessel radius
        r = Mathf.Clamp(r, 0, R);


        // Radial velocity profile (simplified for demonstration purposes)
        float u_r = meanVelocity * (1 - Mathf.Pow(r / R, 2));


        // Axial velocity with pulsatile component, avoiding division by zero
        float pulsatileComponent = (flowRateVariation / averageFlowRate) * Mathf.Sin(2 * Mathf.PI * frequency * time);
        float exponentialDecay = Mathf.Exp(-Mathf.Pow(womersleyNumber, 2) * (1 - r / R));


        float u_z = meanVelocity * (1 + pulsatileComponent * exponentialDecay);


        // Ensure the velocity is finite and within a reasonable range
        if (float.IsInfinity(u_z) || float.IsNaN(u_z))
        {
            Debug.LogError($"Calculated velocity u_z is invalid: {u_z}");
            return;
        }


        // Apply force based on the calculated velocity profile
        Vector3 force = new Vector3(0, 0, u_z * magnetRb.mass);
        magnetRb.AddForce(force);
    }
}






