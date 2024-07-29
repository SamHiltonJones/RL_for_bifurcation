using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class main_RL : Agent
{
    private float pi = 3.1416f;
    private float radius = 0.000250f;
    private float length = 0.000800f;
    public float I1 = 8f; public float I2 = 0f; public float I3 = -8f; public float I4 = 0f;
    public float I5 = -7f; public float I6 = 5f; public float I7 = 5f; public float I8 = -7f;
    public bool check_vel_zero = false;
    public bool checkFirst = false;
    public float force_mult = 5.0f;

    public magnet[] magnets;
    private int totalSteps = 0;
    public float huresticSpeed = 0.5f;
    private float roi_xy = 0.009f; //0.009f
    private float roi_z = 0.02f; //0.009f
    Vector3 agentPos = Vector3.zero;
    Vector3 targetPos = Vector3.zero;
    public float threshold_xy = 1.5f;
    public float threshold_z = 1.5f;
    public float threshold_xyz = 1.5f;
    private float wksp_size_xy = 20.0f;
    private float wksp_size_z = 20.0f;
    private float max_steps = 20000;
    private int step_inc = 0;
    public bool outOfBound = true;
    GameObject cube;
    Rigidbody rBody;
    float dist_xy = 0f;
    float dist_z = 0f;
    float dist_xyz = 0f;

    // Variables for line following
    public GameObject path; // The line path
    private LineRenderer lineRenderer;
    private List<Vector3> pathPoints = new List<Vector3>();
    private int currentPointIndex = 0;

    // Bifurcation cube
    public GameObject bifurcationCube;

    // Spline path generator
    private SplinePathGenerator splinePathGenerator;

    // Reset reason
    private string resetReason = "";

    NDArray Bvec = new double[,] { {-0.3784,   -0.6537,    0.3784,    0.6537,   -0.4818,   -0.1650,    0.4818,    0.1650 },
                                   {-0.6537,    0.3784,    0.6537,   -0.3784,   -0.1650,    0.4818,    0.1650,   -0.4818 },
                                   { 0.0457,    0.0457,    0.0457,    0.0457,    0.6525,    0.6525,    0.6525,    0.6525  }};

    NDArray GradX = new double[,] { { -0.0195,    -0.0057,   -0.0195,   -0.0057,    0.0202,    -0.0344,    0.0202,    -0.0344 },
                                    { -0.0189,    0.0160,   -0.0189,     0.0160,    0,        0,          0,          0 },
                                    { 0.0118,    0.0128,   -0.0118,    -0.0128,    0.0369,    0 ,        -0.0369,     0 }};

    NDArray GradY = new double[,] { { -0.0160,    0.0189,   -0.0160 ,   0.0189  ,  0       , 0        , 0       ,  0 },
                                    { -0.0057,   -0.0195,    -0.0057,   -0.0195 ,  -0.0344 ,  0.0202  , -0.0344 ,   0.0202 },
                                    { 0.0128 ,  -0.0118 ,  -0.0128  ,  0.0118   ,   0      ,  -0.0369 ,  0      ,   0.0369 }};

    NDArray GradZ = new double[,] { { 0.0078  ,  0.0069 ,  -0.0078  , -0.0069   , 0.0344   , 0       , -0.0344  ,  0 },
                                    { 0.0069  , -0.0078 ,  -0.0069  ,  0.0078   , 0        ,-0.0344  ,  0       ,  0.0344 },
                                    { 0.0136  ,  0.0136 ,   0.0136  ,  0.0136   ,-0.0183   ,-0.0183  , -0.0183  , -0.0183}};

    NDArray G = new double[,] { { -0.0195,    -0.0057,   -0.0195,   -0.0057,    0.0202,    -0.0344,    0.0202,    -0.0344 },
                                { -0.0189,    0.0160,   -0.0189,     0.0160,    0,        0,          0,          0 },
                                { 0.0118,    0.0128,   -0.0118,    -0.0128,    0.0369,    0 ,        -0.0369,     0 },
                                { -0.0160,    0.0189,   -0.0160 ,   0.0189  ,  0       , 0        , 0       ,  0 },
                                { -0.0057,   -0.0195,    -0.0057,   -0.0195 ,  -0.0344 ,  0.0202  , -0.0344 ,   0.0202 },
                                { 0.0128 ,  -0.0118 ,  -0.0128  ,  0.0118   ,   0      ,  -0.0369 ,  0      ,   0.0369 },
                                { 0.0078  ,  0.0069 ,  -0.0078  , -0.0069   , 0.0344   , 0       , -0.0344  ,  0 },
                                { 0.0069  , -0.0078 ,  -0.0069  ,  0.0078   , 0        ,-0.0344  ,  0       ,  0.0344 },
                                { 0.0136  ,  0.0136 ,   0.0136  ,  0.0136   ,-0.0183   ,-0.0183  , -0.0183  , -0.0183}};

    void Start()
    {
        magnets = FindObjectsOfType<magnet>();
        Bvec = Bvec * 0.001f;

        foreach (var magnet in magnets)
        {
            magnet.RigidBody = magnet.GetComponent<Rigidbody>();
        }

        rBody = magnets[0].GetComponent<Rigidbody>();

        // Initialize line renderer and points
        splinePathGenerator = path.GetComponent<SplinePathGenerator>();

        if (splinePathGenerator == null)
        {
            Debug.LogError("SplinePathGenerator component not found on the path GameObject.");
            return;
        }

        lineRenderer = path.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component not found on the path GameObject.");
            return;
        }

        GenerateNewPath();

        // Initialize target cube
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = pathPoints[currentPointIndex];
        cube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        var cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material.SetColor("_Color", Color.blue);
        cube.GetComponent<Collider>().enabled = false;
    }

    void GenerateNewPath()
    {
        if (splinePathGenerator != null)
        {
            splinePathGenerator.GeneratePath(); // Generate a new path
            pathPoints = splinePathGenerator.GetPathPoints();

            // Update the LineRenderer with the new path points
            if (pathPoints.Count > 0)
            {
                lineRenderer.positionCount = pathPoints.Count;
                lineRenderer.SetPositions(pathPoints.ToArray());
            }
            else
            {
                Debug.LogError("No path points generated by SplinePathGenerator.");
            }
        }
        else
        {
            Debug.LogError("SplinePathGenerator component is not assigned.");
        }
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log($"Episode Ended: {resetReason}");

        totalSteps = 0;
        foreach (var magnet in magnets)
        {
            if (Mathf.Abs(magnet.transform.position[0]) > roi_xy
                || Mathf.Abs(magnet.transform.position[1]) > roi_z
                || Mathf.Abs(magnet.transform.position[2]) > roi_xy)
            {
                magnet.transform.position = new Vector3(0, 5f, 9f);
                magnet.RigidBody.angularVelocity = Vector3.zero;
                magnet.RigidBody.velocity = Vector3.zero;
            }
        }

        GenerateNewPath();

        if (pathPoints.Count > 0)
        {
            cube.transform.position = pathPoints[0];
        }
        else
        {
            Debug.LogError("Path points are not generated.");
        }

        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        currentPointIndex = 0;
        checkFirst = true;

        resetReason = "";
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (magnets.Length == 0 || magnets[0] == null || cube == null)
        {
            Debug.LogError("CollectObservations: Missing references.");
            return;
        }

        agentPos[0] = Mathf.Round((magnets[0].transform.position[0] / wksp_size_xy) * 100.0f) * 0.01f;
        agentPos[1] = Mathf.Round((magnets[0].transform.position[2] / wksp_size_xy) * 100.0f) * 0.01f;
        agentPos[2] = Mathf.Round((magnets[0].transform.position[1] / wksp_size_z) * 100.0f) * 0.01f;

        targetPos[0] = Mathf.Round((cube.transform.position[0] / wksp_size_xy) * 100.0f) * 0.01f;
        targetPos[1] = Mathf.Round((cube.transform.position[2] / wksp_size_xy) * 100.0f) * 0.01f;
        targetPos[2] = Mathf.Round((cube.transform.position[1] / wksp_size_z) * 100.0f) * 0.01f;

        sensor.AddObservation(agentPos); // Normalize
        sensor.AddObservation(targetPos); // Normalize
    }

    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        I1 = Mathf.Round(vectorAction.ContinuousActions[0] * force_mult * 100.0f) * 0.01f;
        I2 = Mathf.Round(vectorAction.ContinuousActions[1] * force_mult * 100.0f) * 0.01f;
        I3 = Mathf.Round(vectorAction.ContinuousActions[2] * force_mult * 100.0f) * 0.01f;
        I4 = Mathf.Round(vectorAction.ContinuousActions[3] * force_mult * 100.0f) * 0.01f;
        I5 = Mathf.Round(vectorAction.ContinuousActions[4] * force_mult * 100.0f) * 0.01f;
        I6 = Mathf.Round(vectorAction.ContinuousActions[5] * force_mult * 100.0f) * 0.01f;
        I7 = Mathf.Round(vectorAction.ContinuousActions[6] * force_mult * 100.0f) * 0.01f;
        I8 = Mathf.Round(vectorAction.ContinuousActions[7] * force_mult * 100.0f) * 0.01f;

        foreach (var magnet in magnets)
        {
            force_calc(magnet);
        }

        dist_xy = Vector2.Distance(new Vector2(agentPos[0], agentPos[1]), new Vector2(targetPos[0], targetPos[1]));
        dist_z = Mathf.Abs(agentPos[2] - targetPos[2]);
        dist_xyz = Mathf.Abs(Vector3.Distance(agentPos, targetPos));

        totalSteps += 1;
        if (totalSteps > max_steps)
        {
            SetReward(-dist_xyz);
            resetReason = "Exceeded max steps";
            Debug.Log($"Episode ended due to: {resetReason}");
            EndEpisode();
        }

        SetReward(-dist_xyz);

        // Check for reaching the target point
        if (dist_xyz < threshold_xyz)
        {
            currentPointIndex++;
            if (currentPointIndex >= pathPoints.Count)
            {
                SetReward(100); // Reward for completing the path
                resetReason = "Completed the path";
                Debug.Log($"Episode ended due to: {resetReason}");
                EndEpisode();
            }
            else
            {
                cube.transform.position = pathPoints[currentPointIndex];
                SetReward(10); // Reward for reaching the next point
            }
        }

        // Check for collision with bifurcation cube
        if (Vector3.Distance(magnets[0].transform.position, bifurcationCube.transform.position) < threshold_xyz)
        {
            SetReward(-200);
            resetReason = "Collided with bifurcation cube";
            Debug.Log($"Episode ended due to: {resetReason}");
            EndEpisode();
        }

        if ((Mathf.Abs(agentPos[0]) > 0.8f) || (Mathf.Abs(agentPos[1]) > 0.8f))
        {
            SetReward(-200);
            resetReason = "Out of bounds";
            Debug.Log($"Episode ended due to: {resetReason}");
            magnets[0].transform.position = new Vector3(0, 5f, 9f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers action)
    {
        var continuousActionsOut = action.ContinuousActions;

        if (Input.GetAxis("Vertical") < 0)
        {
            continuousActionsOut[3 - 1] = huresticSpeed;
            continuousActionsOut[1 - 1] = 0;
            continuousActionsOut[2 - 1] = 0;
            continuousActionsOut[4 - 1] = 0;
        }
        if (Input.GetAxis("Vertical") > 0)
        {
            continuousActionsOut[1 - 1] = huresticSpeed;
            continuousActionsOut[2 - 1] = 0;
            continuousActionsOut[3 - 1] = 0;
            continuousActionsOut[4 - 1] = 0;
        }
        if (Input.GetAxis("Horizontal") < 0)
        {
            continuousActionsOut[4 - 1] = huresticSpeed;
            continuousActionsOut[1 - 1] = 0;
            continuousActionsOut[2 - 1] = 0;
            continuousActionsOut[3 - 1] = 0;
        }
        if (Input.GetAxis("Horizontal") > 0)
        {
            continuousActionsOut[2 - 1] = huresticSpeed;
            continuousActionsOut[1 - 1] = 0;
            continuousActionsOut[3 - 1] = 0;
            continuousActionsOut[4 - 1] = 0;
        }
        if (Input.GetButtonDown("Fire1"))
        {
            OnEpisodeBegin();
        }
    }

    void force_calc(magnet magnet)
    {
        var rbm = magnet.RigidBody;
        var mag_moment = 0.1f; //0.00027f

        NDArray I = new float[,] { { I1 }, { I2 }, { I3 }, { I4 }, { I5 }, { I6 }, { I7 }, { I8 } };

        NDArray G_total = np.matmul(G, I);
        NDArray G_total_reshape = np.transpose(np.reshape(G_total, (3, 3)));

        Vector3 unit_vector_mag = ((magnet.poles[0].transform.position - magnet.poles[1].transform.position)).normalized;
        NDArray m = new float[,] { { unit_vector_mag[0] }, { unit_vector_mag[2] }, { unit_vector_mag[1] } };
        m = m * mag_moment;

        NDArray F = np.matmul(G_total_reshape, m);

        // Extract values from NDArray and manually negate them where needed
        float m0 = m[0].Data<float>()[0];
        float m1 = m[1].Data<float>()[0];
        float m2 = m[2].Data<float>()[0];

        // Manually construct m_skew matrix
        NDArray m_skew = new float[,]
        {
            { 0, -m2, m1 },
            { m2, 0, -m0 },
            { -m1, m0, 0 }
        };

        NDArray B = np.matmul(Bvec, I);
        NDArray T = np.matmul(m_skew, B);

        // Torque
        var T_data = T.Data<float>();
        Vector3 T_vec = new Vector3(T_data[0], T_data[2], T_data[1]);

        // Force
        var F_data = F.Data<float>();
        Vector3 F_vec = new Vector3(F_data[0], F_data[2], F_data[1]);

        // Manually negate F_vec elements
        F_vec = new Vector3(-F_vec.x, -F_vec.y, -F_vec.z);

        Vector3 oth_force = other_forces(magnet);

        Vector3 total_force = F_vec + oth_force;

        rbm.AddTorque(T_vec / 200.0f);
        rbm.AddForce(total_force / 200.0f);

        if (check_vel_zero)
        {
            magnet.RigidBody.velocity = Vector3.zero;
            magnet.RigidBody.angularVelocity = Vector3.zero;
        }
    }

    Vector3 other_forces(magnet magnet)
    {
        var rbm = magnet.RigidBody;

        Vector3 Fg = new Vector3(0f, rbm.mass * 9.81f, 0f);

        return -Fg;
    }
}
