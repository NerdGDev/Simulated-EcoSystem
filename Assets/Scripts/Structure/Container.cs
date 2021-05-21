using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

[RequireComponent(typeof(Resource)), RequireComponent(typeof(Visualise))]
public class Container : MonoBehaviour
{
    public Resource resource;

    protected void Awake()
    {
        resource = GetComponent<Resource>();
    }

    protected void FixedUpdate()
    {
        SendData("Pool", resource.Pool.ToString());
    }

    public float GetPool() 
    {
        return resource.Pool;
    }

    protected void SendData(string field, string content)
    {
        GetComponent<Visualise>().AddDataField(field, content);
    }

    protected void SendShortData(string field, string content)
    {
        if (field == "State")
        {
            SendData("State", content);
        }
        GetComponent<Visualise>().AddShortData(field, content);
    }
}
