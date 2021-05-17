using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

[RequireComponent(typeof(Resource))]
public class Container : MonoBehaviour
{
    public Resource resource;

    protected void Awake()
    {
        resource = GetComponent<Resource>();
    }

    public float GetPool() 
    {
        return resource.Pool;
    }
}
