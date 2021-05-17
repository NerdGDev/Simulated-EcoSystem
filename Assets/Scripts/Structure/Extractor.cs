using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

[RequireComponent(typeof(Resource))]
public class Extractor : Container
{

    public float GenerationRate;
    private float GenRateSecond;

    private void Awake()
    {
        base.Awake();
        GenRateSecond = GenerationRate / 60f;
    }

    void FixedUpdate()
    {
        resource.Pool = resource.Pool + (GenRateSecond * Time.fixedDeltaTime) > resource.MaxPool ? resource.MaxPool : resource.Pool + (GenRateSecond * Time.fixedDeltaTime);
    }
}
