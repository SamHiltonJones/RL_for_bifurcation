using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePathGenerator : MonoBehaviour
{
    public GameObject startPoint; // The starting point of the path
    public GameObject pathPointPrefab; // Prefab for visualizing path points
    public int maxIterations = 1000; // Maximum iterations for the RRT algorithm
    public float stepSize = 1.0f; // Step size for the RRT algorithm
    public GameObject[] targetRegions; // Array of target regions (cylinders)

    private Vector3 targetPoint;
    private List<Vector3> pathPoints = new List<Vector3>();

    void Start()
    {
        GeneratePath();
    }

    public void GeneratePath()
    {
        GenerateRandomTargetPoint();
        CreateRRTPath();
        VisualizePath();
    }

    void GenerateRandomTargetPoint()
    {
        if (targetRegions.Length == 0)
        {
            Debug.LogError("No target regions specified.");
            return;
        }

        // Randomly select one of the target regions
        int randomIndex = Random.Range(0, targetRegions.Length);
        GameObject selectedRegion = targetRegions[randomIndex];

        // Generate a random point within the bounds of the selected region
        targetPoint = GeneratePointWithinCylinder(selectedRegion);

        Debug.Log("Target point generated in region " + selectedRegion.name + ": " + targetPoint);
    }

    Vector3 GeneratePointWithinCylinder(GameObject region)
    {
        Collider collider = region.GetComponent<Collider>();
        Transform transform = region.transform;

        Vector3 center = transform.position;
        float height = transform.localScale.y;
        float radius = transform.localScale.x / 2; // Assuming the cylinder is aligned with the y-axis

        Vector3 point;
        do
        {
            float angle = Random.Range(0f, Mathf.PI * 2);
            float distance = Random.Range(0f, radius);
            float x = distance * Mathf.Cos(angle);
            float z = distance * Mathf.Sin(angle);
            float y = Random.Range(center.y - height / 2, center.y + height / 2);

            point = center + transform.rotation * new Vector3(x, y - center.y, z);
        }
        while (!IsPointInCylinder(point, region));

        return point;
    }

    bool IsPointInCylinder(Vector3 point, GameObject region)
    {
        Collider collider = region.GetComponent<Collider>();
        Transform transform = region.transform;

        Vector3 localPoint = transform.InverseTransformPoint(point);
        Vector3 localCenter = transform.InverseTransformPoint(transform.position);

        float height = transform.localScale.y;
        float radius = transform.localScale.x / 2;

        float dx = localPoint.x - localCenter.x;
        float dz = localPoint.z - localCenter.z;
        float distanceSquared = dx * dx + dz * dz;

        bool withinRadius = distanceSquared <= radius * radius;
        bool withinHeight = localPoint.y >= localCenter.y - height / 2 && localPoint.y <= localCenter.y + height / 2;

        return withinRadius && withinHeight;
    }

    void CreateRRTPath()
    {
        RRT rrt = new RRT(targetRegions, stepSize, maxIterations);
        Vector3 start = startPoint.transform.position;

        List<Vector3> path = rrt.GeneratePath(start, targetPoint);
        if (path != null)
        {
            pathPoints = path;
            Debug.Log("Path successfully generated with RRT.");
        }
        else
        {
            Debug.LogError("Failed to generate a path with RRT.");
        }
    }

    void VisualizePath()
    {
        // Clear existing path points
        GameObject[] existingPathPoints = GameObject.FindGameObjectsWithTag("PathPointClone");
        foreach (GameObject point in existingPathPoints)
        {
            Destroy(point);
        }

        for (int i = 0; i < pathPoints.Count; i++)
        {
            GameObject pathPoint = Instantiate(pathPointPrefab, pathPoints[i], Quaternion.identity);
            pathPoint.tag = "PathPointClone"; // Tag the instantiated path point
            pathPoint.name = "PathPointClone_" + i.ToString("D4"); // Assign a unique name
            Debug.Log("Generated Path Point: " + pathPoint.name + " at position " + pathPoints[i]);
        }
    }

    public List<Vector3> GetPathPoints()
    {
        return pathPoints;
    }
}
