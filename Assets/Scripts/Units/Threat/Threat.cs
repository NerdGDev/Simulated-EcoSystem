using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Threat : UnitBase
{
    public float rad;
    public int health;

    private void Awake()
    {
        base.Awake();
    }

    public void Hit() 
    {
        health--;
        if (health <= 0) 
        {
            Destroy(gameObject, 0.5f);
        }
    }

    public void StartThreat() 
    {
        StartCoroutine(Run());
    }

    public void StartThreat(Threat threat)
    {
        StartCoroutine(Run(threat));
    }

    IEnumerator Run() 
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(ThreatPatrol());
    }

    IEnumerator Run(Threat threat)
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(ThreatPatrol(threat));
    }

    IEnumerator ThreatPatrol() 
    {
        yield return new WaitForSeconds(2f);

        while (true) 
        {
            m_Agent.SetDestination(Random.insideUnitSphere * rad);

            yield return new WaitForSeconds(2f);

            while (m_Agent.HasDestination())
            {
                yield return new WaitForSeconds(UpdateFrequency);
            }
        }
    }

    IEnumerator ThreatPatrol(Threat threat)
    {
        Vector3 lastPos = Vector3.zero;
        float retargetRange = 200f;
        yield return new WaitForSeconds(2f);

        while (true && threat != null)
        {
            if (!m_Agent.HasDestination() || (threat.transform.position - lastPos).magnitude > retargetRange)
            {
                m_Agent.SetDestination(threat.transform.position);
            }
            yield return new WaitForFixedUpdate();
        }
        StartThreat();
    }
}
