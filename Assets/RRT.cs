using System.Collections.Generic;
using UnityEngine;




public class RRT
{
    public class Node
    {
        public Vector3 Position;
        public Node Parent;




        public Node(Vector3 position, Node parent = null)
        {
            Position = position;
            Parent = parent;
        }
    }




    public List<Node> Nodes;
    private GameObject[] targetRegions;
    private float stepSize;
    private int maxIterations;
    private System.Random random;




    public RRT(GameObject[] targetRegions, float stepSize, int maxIterations)
    {
        this.targetRegions = targetRegions;
        this.stepSize = stepSize;
        this.maxIterations = maxIterations;
        Nodes = new List<Node>();
        random = new System.Random();
    }




    public Node GetRandomNode()
    {
        GameObject region = targetRegions[random.Next(targetRegions.Length)];
        Vector3 point = GeneratePointWithinCylinder(region);
        return new Node(point);
    }




    public Node GetNearestNode(Vector3 position)
    {
        Node nearest = null;
        float minDist = float.MaxValue;
        foreach (Node node in Nodes)
        {
            float dist = Vector3.Distance(node.Position, position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = node;
            }
        }
        return nearest;
    }




    public bool IsValidPoint(Vector3 point)
    {
        foreach (GameObject region in targetRegions)
        {
            if (IsPointInCylinder(point, region))
            {
                return true;
            }
        }
        return false;
    }




    public bool IsValidPath(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);
        int steps = Mathf.CeilToInt(distance / stepSize);




        for (int i = 1; i <= steps; i++)
        {
            Vector3 intermediatePoint = from + direction * stepSize * i;
            if (!IsValidPoint(intermediatePoint))
            {
                Debug.Log("Invalid path segment: " + intermediatePoint);
                return false;
            }
        }
        return true;
    }




    public Vector3 Steer(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        return from + direction * stepSize;
    }




    public List<Vector3> GeneratePath(Vector3 start, Vector3 target)
    {
        Nodes.Clear();
        Nodes.Add(new Node(start));




        for (int i = 0; i < maxIterations; i++)
        {
            Node randomNode = GetRandomNode();
            Node nearestNode = GetNearestNode(randomNode.Position);
            Vector3 newPoint = Steer(nearestNode.Position, randomNode.Position);




            if (IsValidPoint(newPoint) && IsValidPath(nearestNode.Position, newPoint))
            {
                Node newNode = new Node(newPoint, nearestNode);
                Nodes.Add(newNode);




                if (Vector3.Distance(newPoint, target) < stepSize)
                {
                    Node targetNode = new Node(target, newNode);
                    Nodes.Add(targetNode);
                    return BuildPath(targetNode);
                }
            }
        }




        return null; // Path not found within the maximum iterations
    }




    private List<Vector3> BuildPath(Node targetNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = targetNode;
        while (currentNode != null)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }
        path.Reverse();
        return path;
    }




    private Vector3 GeneratePointWithinCylinder(GameObject region)
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




    private bool IsPointInCylinder(Vector3 point, GameObject region)
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
}




