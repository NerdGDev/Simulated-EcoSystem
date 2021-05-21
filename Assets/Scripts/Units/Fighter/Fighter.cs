using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : UnitBase
{
    public float range;
    public Threat target;

    public GameObject bulletPrefab;
    private GameObject m_Particle;

    bool Pursuit = false;

    

    void Awake()
    {
        base.Awake();
        type = ObjectType.FIGHTER;
        rb.maxAngularVelocity = 100f;
    }

    private void FixedUpdate()
    {
        base.FixedUpdate();
        SendData("Target", "No");
        if (Pursuit) 
        {
            SendData("Target", "Yes");
        }
        if (!Pursuit && !goingHome) 
        {
            rb.AddForce(rb.velocity.normalized * 120f);
        }
    }

    public void EngageTarget(Threat target) 
    {
        this.target = target;
        AddOrder((go) => { StartCoroutine(AttackTarget(target)); });
    }

    IEnumerator AttackTarget(Threat target) 
    {
        state = "Engaging Threat";
        SendShortData("State", state);
        if (target == null) 
        {
            NextOrder();
            yield break;
        }
        yield return new WaitForFixedUpdate();

        Coroutine coA = StartCoroutine(PursuitTarget(target));
        Coroutine coB = StartCoroutine(FireAtTarget(target));
        Pursuit = true;
        while (target != null) 
        {
            yield return new WaitForSeconds(1f);            
        }
        Pursuit = false;
        StopCoroutine(coA);
        StopCoroutine(coB);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 600f, Physics.AllLayers, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hitColliders)
        {
            if (hit.GetComponent<Threat>())
            {
                this.target = hit.GetComponent<Threat>();
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
            if (!m_Agent.HasDestination() || Vector3.Distance(target.transform.position, lastPos) > retargetRange) 
            {
                m_Agent.SetDestination(target.transform.position);
            }
            rb.AddForce((target.transform.position - transform.position).normalized * (Vector3.Distance(target.transform.position, lastPos)/1.5f));
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
                yield return new WaitForSeconds(0.2f);
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
        main.startLifetime = ((target.transform.position - transform.position).magnitude / main.startSpeed.constant) + 0.1f;
        ps.Play();
        StartCoroutine(UpdateParticleAim(m_Particle, target));
        yield return new WaitForSeconds((target.transform.position - transform.position).magnitude / main.startSpeed.constant);
        StopCoroutine(UpdateParticleAim(m_Particle, target));
        Destroy(ps.gameObject);
        target.Hit();
    }

    IEnumerator UpdateParticleAim(GameObject ps, Threat target) 
    {
        while (true) 
        {
            ps.transform.rotation = Quaternion.LookRotation(target.transform.position - ps.transform.position);
            yield return new WaitForFixedUpdate();
        }
    }
}
