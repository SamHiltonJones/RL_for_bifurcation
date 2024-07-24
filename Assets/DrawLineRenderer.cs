using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrawLineRenderer : MonoBehaviour
{
    public List<Transform> controlPoints; // List of control points
    public LineRenderer lineRenderer;
    public int vertexCount = 12; // Number of vertices per segment


    void Start()
    {
        if (controlPoints == null || controlPoints.Count < 2)
        {
            Debug.LogError("Need at least 2 control points to draw a line.");
            return;
        }


        lineRenderer.positionCount = (controlPoints.Count - 1) * vertexCount;
        UpdateLineRenderer();
    }


    void Update()
    {
        UpdateLineRenderer();
    }


    void UpdateLineRenderer()
    {
        if (controlPoints == null || controlPoints.Count < 2)
        {
            Debug.LogError("Need at least 2 control points to draw a line.");
            return;
        }


        List<Vector3> pointList = new List<Vector3>();


        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            for (float t = 0; t <= 1; t += 1.0f / vertexCount)
            {
                Vector3 point = GetCatmullRomPosition(i, t);
                pointList.Add(point);
            }
        }


        lineRenderer.positionCount = pointList.Count;
        lineRenderer.SetPositions(pointList.ToArray());
    }


    Vector3 GetCatmullRomPosition(int index, float t)
    {
        // Indices for the points
        Vector3 p0 = controlPoints[Mathf.Clamp(index - 1, 0, controlPoints.Count - 1)].position;
        Vector3 p1 = controlPoints[index].position;
        Vector3 p2 = controlPoints[Mathf.Clamp(index + 1, 0, controlPoints.Count - 1)].position;
        Vector3 p3 = controlPoints[Mathf.Clamp(index + 2, 0, controlPoints.Count - 1)].position;


        // Catmull-Rom spline calculation
        float t2 = t * t;
        float t3 = t2 * t;


        Vector3 point = 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );


        return point;
    }
}






