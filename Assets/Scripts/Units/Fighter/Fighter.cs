using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : UnitBase
{
    public float range;
    public Threat target;

    public GameObject bulletPrefab;
    private GameObject m_Particle;

    void Awake()
    {
        base.Awake();
        type = ObjectType.FIGHTER;
        GetComponent<Rigidbody>().maxAngularVelocity = 100f;
    }

    public void EngageTarget(Threat target) 
    {
        this.target = target;
        AddOrder((go) => { StartCoroutine(AttackTarget(target)); });
    }

    IEnumerator AttackTarget(Threat target) 
    {
        yield return new WaitForFixedUpdate();

        Coroutine coA = StartCoroutine(PursuitTarget(target));
        Coroutine coB = StartCoroutine(FireAtTarget(target));

        while (target != null) 
        {
            yield return new WaitForFixedUpdate();
        }
        StopCoroutine(coA);
        StopCoroutine(coB);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 600f, Physics.AllLayers, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hitColliders)
        {
            if (hit.GetComponent<Threat>())
            {
                StartCoroutine(AttackTarget(hit.GetComponent<Threat>()));
                yield break;
            }
        }
        yield return new WaitForFixedUpdate();
        NextOrder();
    }

    IEnumerator PursuitTarget(Threat target) 
    {
        Vector3 lastPos = Vector3.zero;
        float retargetRange = 10f;
        yield return new WaitForFixedUpdate();
        while (true && target != null)
        {
            if (!m_Agent.HasDestination() || (target.transform.position - lastPos).magnitude > retargetRange) 
            {
                m_Agent.SetDestination(target.transform.position);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator FireAtTarget(Threat target) 
    {
        yield return new WaitForFixedUpdate();
        while (true && target != null)
        {
            if ((transform.position - target.transform.position).magnitude < range) 
            {
                StartCoroutine(Shoot(target));
                yield return new WaitForSeconds(0.75f);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator Shoot(Threat target) 
    {
        Debug.LogWarning("FIRE FIRE FIRE");
        m_Particle = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(target.transform.position - transform.position));
        ParticleSystem ps = m_Particle.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = (target.transform.position - transform.position).magnitude / main.startSpeed.constant;
        yield return new WaitForSeconds((target.transform.position - transform.position).magnitude / main.startSpeed.constant);
        Destroy(ps.gameObject);
        target.Hit();
    }
}
