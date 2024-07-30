using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObstacleManager : MonoBehaviour
{
    public GameObject obstaclePrefab; // Prefab for the dynamic obstacle
    public int obstacleCount = 3; // Number of obstacles to generate
    public float obstacleSpeed = 1f; // Speed at which obstacles move
    public GameObject[] targetRegions; // Array of target regions (cylinders)

    private List<GameObject> obstacles = new List<GameObject>();

    void Start()
    {
        GenerateObstacles();
    }

    void Update()
    {
        MoveObstacles();
        CheckAndRegenerateObstacles();
    }

    public void GenerateObstacles()
    {
        for (int i = 0; i < obstacleCount; i++)
        {
            RegenerateObstacle();
        }
    }

    public Vector3 GeneratePointWithinCylinder(GameObject region)
    {
        Transform transform = region.transform;

        Vector3 center = transform.position;
        float height = transform.localScale.y;
        float radius = transform.localScale.x / 2; // Assuming the cylinder is aligned with the y-axis

        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Random.Range(0f, radius);
        float x = distance * Mathf.Cos(angle);
        float z = distance * Mathf.Sin(angle);
        float y = Random.Range(center.y - height / 2, center.y + height / 2);

        Vector3 point = center + transform.rotation * new Vector3(x, y - center.y, z);

        return point;
    }

    void MoveObstacles()
    {
        foreach (var obstacle in obstacles)
        {
            if (obstacle != null)
            {
                // Define a movement pattern or random movement
                Vector3 randomDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized;

                obstacle.transform.Translate(randomDirection * obstacleSpeed * Time.deltaTime);
            }
        }
    }

    void CheckAndRegenerateObstacles()
    {
        List<GameObject> obstaclesToRemove = new List<GameObject>();

        foreach (var obstacle in obstacles)
        {
            if (obstacle != null && obstacle.transform.position.z > 9.2)
            {
                obstaclesToRemove.Add(obstacle);
            }
        }

        foreach (var obstacle in obstaclesToRemove)
        {
            if (obstacles.Contains(obstacle))
            {
                obstacles.Remove(obstacle);
                Destroy(obstacle);
                RegenerateObstacle();
            }
        }
    }

    void RegenerateObstacle()
    {
        GameObject selectedRegion = targetRegions[Random.Range(targetRegions.Length - 2, targetRegions.Length)];
        Vector3 randomPosition = GeneratePointWithinCylinder(selectedRegion);

        GameObject newObstacle = Instantiate(obstaclePrefab, randomPosition, Quaternion.identity);
        newObstacle.tag = "obstacle"; // Set the tag to "obstacle"
        obstacles.Add(newObstacle);
    }

    public List<GameObject> GetObstacles()
    {
        return obstacles;
    }
}
