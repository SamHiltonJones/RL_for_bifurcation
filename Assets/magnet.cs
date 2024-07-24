using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class magnet : MonoBehaviour
{
   public Pole[] poles;
   public Vector3 unit_vec = Vector3.zero;
   public Rigidbody RigidBody;


   void Start()
   {
       poles = GetComponentsInChildren<Pole>();


       if (poles.Length >= 2)
       {
           Vector3 vectorBetweenPoles = poles[1].transform.position - poles[0].transform.position;
           unit_vec = vectorBetweenPoles.normalized;
       }
       else
       {
           Debug.LogError("There are not enough poles to calculate a vector.");
       }
   }
}