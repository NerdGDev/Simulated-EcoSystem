using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

[RequireComponent(typeof(Resource))]
public class Extractor : Container
{
    public float GenerationRate;
    private float GenRateSecond;

    Rigidbody rb;

    public GameObject rockPrefab;
    public GameObject rockTarget;

    public Quaternion rot;

    private void Awake()
    {
        base.Awake();
        GenRateSecond = GenerationRate / 60f;
        rb = GetComponent<Rigidbody>();
        transform.TransformDirection(Vector3.up);
        rb.centerOfMass = new Vector3();
        rb.angularVelocity = transform.TransformDirection(Vector3.up) * Random.Range(0.12f, 0.5f); 
        Instantiate(rockPrefab, rockTarget.transform.position, rockTarget.transform.rotation);
    }

    void FixedUpdate()
    {
        resource.Pool = resource.Pool + (GenRateSecond * Time.fixedDeltaTime) > resource.MaxPool ? resource.MaxPool : resource.Pool + (GenRateSecond * Time.fixedDeltaTime);
    }
}
