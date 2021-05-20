using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

[RequireComponent(typeof(Resource))]
public class Carrier : UnitBase
{
    Resource resource;

    void Awake()
    {
        base.Awake();
        resource = GetComponent<Resource>();
        type = ObjectType.CARRIER;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeliverResource(Resource from, Resource to) 
    {
        AddOrder((go) => { StartCoroutine(Goto(from.gameObject, 10f)); });
        AddOrder((go) => { StartCoroutine(TakeResource(from)); });
        AddOrder((go) => { StartCoroutine(Goto(to.gameObject, 20f)); });
        AddOrder((go) => { StartCoroutine(GiveResource(to)); });
    }

    IEnumerator TakeResource(Resource target) 
    {
        state = "Collecting Resources";
        yield return new WaitForFixedUpdate();
        //Debug.Log(m_Collider);
        //Debug.Log(resource);
        Collider[] hitColliders = Physics.OverlapSphere(m_Collider.bounds.center, resource.TransferRange, Physics.AllLayers, QueryTriggerInteraction.Collide);

        bool found = false;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform.GetComponent<Resource>() == target)
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            //Debug.LogError("NO TARGET");
            NextOrder();
            yield break;
        }

        state = "Waiting to Transfer";
        while (target.TransferBusy)
        {
            yield return new WaitForSeconds(UpdateFrequency);
        }

        state = "Transferring";
        resource.TransferFrom(target);

        yield return new WaitForSeconds(UpdateFrequency);

        while (resource.TransferBusy)
        {
            yield return new WaitForSeconds(UpdateFrequency);
        }

        NextOrder();
    }

    IEnumerator GiveResource(Resource target)
    {
        state = "Delivering Resources";
        yield return new  WaitForFixedUpdate();
        Collider[] hitColliders = Physics.OverlapSphere(m_Collider.bounds.center, resource.TransferRange, Physics.AllLayers, QueryTriggerInteraction.Collide);

        bool found = false;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform.GetComponent<Resource>() == target)
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            //Debug.LogError("NO TARGET");
            NextOrder();
            yield break;
        }

        state = "Waiting to Transfer";
        while (target.TransferBusy)
        {            
            yield return new WaitForSeconds(UpdateFrequency);
        }

        state = "Transferring";
        resource.TransferTo(target);

        yield return new WaitForSeconds(UpdateFrequency);

        while (resource.TransferBusy)
        {
            yield return new WaitForSeconds(UpdateFrequency);
        }

        NextOrder();
    }


}
