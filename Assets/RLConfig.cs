using System.Collections.Generic;
using UnityEngine;
using NumSharp;

public static class RLConfig
{
    public const float Pi = 3.1416f;
    public const float Radius = 0.000250f;
    public const float Length = 0.000800f;
    public const float ForceMultiplier = 5.0f;
    public const float HeuristicSpeed = 0.5f;
    public const float RoiXY = 0.009f;
    public const float RoiZ = 0.02f;
    public const float ThresholdXY = 0.05f;
    public const float ThresholdZ = 0.05f;
    public const float ThresholdXYZ = 0.05f;
    public const float WorkspaceSizeXY = 20.0f;
    public const float WorkspaceSizeZ = 20.0f;
    public const float MaxSteps = 20000;
    public const float MagMoment = 0.1f;

    public static readonly NDArray Bvec = np.multiply(new double[,] {
        {-0.3784, -0.6537, 0.3784, 0.6537, -0.4818, -0.1650, 0.4818, 0.1650 },
        {-0.6537, 0.3784, 0.6537, -0.3784, -0.1650, 0.4818, 0.1650, -0.4818 },
        {0.0457, 0.0457, 0.0457, 0.0457, 0.6525, 0.6525, 0.6525, 0.6525 }
    }, 0.001f);


    public static readonly NDArray GradX = new double[,] { 
        {-0.0195, -0.0057, -0.0195, -0.0057, 0.0202, -0.0344, 0.0202, -0.0344 },
        {-0.0189, 0.0160, -0.0189, 0.0160, 0, 0, 0, 0 },
        {0.0118, 0.0128, -0.0118, -0.0128, 0.0369, 0, -0.0369, 0 }
    };

    public static readonly NDArray GradY = new double[,] { 
        {-0.0160, 0.0189, -0.0160, 0.0189, 0, 0, 0, 0 },
        {-0.0057, -0.0195, -0.0057, -0.0195, -0.0344, 0.0202, -0.0344, 0.0202 },
        {0.0128, -0.0118, -0.0128, 0.0118, 0, -0.0369, 0, 0.0369 }
    };

    public static readonly NDArray GradZ = new double[,] { 
        {0.0078, 0.0069, -0.0078, -0.0069, 0.0344, 0, -0.0344, 0 },
        {0.0069, -0.0078, -0.0069, 0.0078, 0, -0.0344, 0, 0.0344 },
        {0.0136, 0.0136, 0.0136, 0.0136, -0.0183, -0.0183, -0.0183, -0.0183 }
    };

    public static readonly NDArray G = new double[,] { 
        {-0.0195, -0.0057, -0.0195, -0.0057, 0.0202, -0.0344, 0.0202, -0.0344 },
        {-0.0189, 0.0160, -0.0189, 0.0160, 0, 0, 0, 0 },
        {0.0118, 0.0128, -0.0118, -0.0128, 0.0369, 0, -0.0369, 0 },
        {-0.0160, 0.0189, -0.0160, 0.0189, 0, 0, 0, 0 },
        {-0.0057, -0.0195, -0.0057, -0.0195, -0.0344, 0.0202, -0.0344, 0.0202 },
        {0.0128, -0.0118, -0.0128, 0.0118, 0, -0.0369, 0, 0.0369 },
        {0.0078, 0.0069, -0.0078, -0.0069, 0.0344, 0, -0.0344, 0 },
        {0.0069, -0.0078, -0.0069, 0.0078, 0, -0.0344, 0, 0.0344 },
        {0.0136, 0.0136, 0.0136, 0.0136, -0.0183, -0.0183, -0.0183, -0.0183 }
    };
}
