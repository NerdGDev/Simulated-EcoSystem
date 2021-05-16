using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

[RequireComponent(typeof(Resource))]
public class Container : MonoBehaviour
{
    public Resource resource;

    private void Awake()
    {
        resource = GetComponent<Resource>();
    }

    public float GetPool() 
    {
        return resource.Pool;
    }
}
