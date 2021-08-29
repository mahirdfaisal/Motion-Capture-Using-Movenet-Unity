using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{

    public Transform target;
    
    void Update()
    {
        Debug.DrawLine(transform.position, target.position, Color.red);
    }
}
