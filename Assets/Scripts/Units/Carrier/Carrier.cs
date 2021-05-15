using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

[RequireComponent(typeof(Resource))]
public class Carrier : UnitBase
{
    Resource resource;

#if UNITY_EDITOR
    public Resource testTargetFrom;
    public Resource testTargetTo;
#endif

    void Awake()
    {
        base.Awake();
        resource = GetComponent<Resource>();
        StartCoroutine(DemoRun());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#if UNITY_EDITOR
    IEnumerator DemoRun() 
    {
        yield return new WaitForSeconds(3f);
        DeliverResource(testTargetFrom, testTargetTo);
    }
#endif

    void DeliverResource(Resource from, Resource to) 
    {
        AddOrder((go) => { go.StartCoroutine(Goto(from.gameObject));});
        AddOrder((go) => { go.StartCoroutine(TakeResource(from)); });
        AddOrder((go) => { go.StartCoroutine(Goto(to.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(GiveResource(to)); });
        AddOrder((go) => { go.StartCoroutine(Goto(from.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(TakeResource(from)); });
        AddOrder((go) => { go.StartCoroutine(Goto(to.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(GiveResource(to)); });
        AddOrder((go) => { go.StartCoroutine(Goto(from.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(TakeResource(from)); });
        AddOrder((go) => { go.StartCoroutine(Goto(to.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(GiveResource(to)); });
        AddOrder((go) => { go.StartCoroutine(Goto(from.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(TakeResource(from)); });
        AddOrder((go) => { go.StartCoroutine(Goto(to.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(GiveResource(to)); });
        AddOrder((go) => { go.StartCoroutine(Goto(from.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(TakeResource(from)); });
        AddOrder((go) => { go.StartCoroutine(Goto(to.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(GiveResource(to)); });
        AddOrder((go) => { go.StartCoroutine(Goto(from.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(TakeResource(from)); });
        AddOrder((go) => { go.StartCoroutine(Goto(to.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(GiveResource(to)); });
        AddOrder((go) => { go.StartCoroutine(Goto(from.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(TakeResource(from)); });
        AddOrder((go) => { go.StartCoroutine(Goto(to.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(GiveResource(to)); });
        AddOrder((go) => { go.StartCoroutine(Goto(from.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(TakeResource(from)); });
        AddOrder((go) => { go.StartCoroutine(Goto(to.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(GiveResource(to)); });
        AddOrder((go) => { go.StartCoroutine(Goto(from.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(TakeResource(from)); });
        AddOrder((go) => { go.StartCoroutine(Goto(to.gameObject)); });
        AddOrder((go) => { go.StartCoroutine(GiveResource(to)); });
    }

    IEnumerator TakeResource(Resource target) 
    {
        yield return new WaitForEndOfFrame();
        Debug.Log(m_Collider);
        Debug.Log(resource);
        Collider[] hitColliders = Physics.OverlapSphere(m_Collider.bounds.center, resource.TransferRange);

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
            Debug.LogError("NO TARGET");
            NextOrder();
            yield break;
        }

        while (target.TransferBusy)
        {
            yield return new WaitForEndOfFrame();
        }
        resource.TransferFrom(target);

        yield return new WaitForEndOfFrame();

        while (resource.TransferBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        NextOrder();
    }

    IEnumerator GiveResource(Resource target)
    {
        yield return new WaitForEndOfFrame();
        Collider[] hitColliders = Physics.OverlapSphere(m_Collider.bounds.center, resource.TransferRange);

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
            Debug.LogError("NO TARGET");
            NextOrder();
            yield break;
        }

        while (target.TransferBusy)
        {
            yield return new WaitForEndOfFrame();
        }
        resource.TransferTo(target);

        yield return new WaitForEndOfFrame();

        while (resource.TransferBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        NextOrder();
    }


}
