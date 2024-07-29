using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicLineRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;

    void Start()
    {
        StartCoroutine(FindAndSetPathPoints());
    }

    IEnumerator FindAndSetPathPoints()
    {
        // Wait for a short period to ensure all PathPoints are instantiated
        yield return new WaitForSeconds(1.0f);

        // Find all PathPoints (Clone) objects
        GameObject[] pathPointClones = GameObject.FindGameObjectsWithTag("PathPointClone");

        // Sort the points by their names
        List<GameObject> sortedPathPoints = new List<GameObject>(pathPointClones);
        sortedPathPoints.Sort((a, b) => a.name.CompareTo(b.name));

        // Collect positions from sorted points
        List<Vector3> pathPositions = new List<Vector3>();
        foreach (GameObject point in sortedPathPoints)
        {
            pathPositions.Add(point.transform.position);
            Debug.Log("Sorted Path Point: " + point.name + " at position " + point.transform.position);
        }

        // Set positions in the LineRenderer
        lineRenderer.positionCount = pathPositions.Count;
        lineRenderer.SetPositions(pathPositions.ToArray());
    }
}
