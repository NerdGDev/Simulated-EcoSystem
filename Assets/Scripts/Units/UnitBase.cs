﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlyAgent.Navigation;
using FlyAgent.Agents;

[RequireComponent(typeof(FlyAgent.Agents.FlyAgent)), RequireComponent(typeof(Visualise)), RequireComponent(typeof(Debuggable))]
public class UnitBase : ManageObject
{
    public bool Busy;

    protected FlyAgent.Agents.FlyAgent m_Agent;

    protected Queue<OrderDelegate> OrderQueue = new Queue<OrderDelegate>();

    protected Collider m_Collider;

    public delegate void OrderDelegate(MonoBehaviour go);

    public HomeManager home;

    public virtual void Awake()
    {        
        m_Agent = GetComponent<FlyAgent.Agents.FlyAgent>();
        m_Collider = GetComponent<Collider>();
        Debug.Log(m_Agent);
        type = ObjectType.BASE;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected void AddOrder(OrderDelegate order)
    {
        OrderQueue.Enqueue(order);
        if (OrderQueue.Count == 1)
        {
            OrderQueue.Peek().Invoke(this);
        }
    }

    protected void NextOrder()
    {
        OrderQueue.Dequeue();
        if (OrderQueue.Count > 0)
        {
            OrderQueue.Peek().Invoke(this);
        }
        else if (home) 
        {
            Debug.LogError("Returning Home");
            AddOrder((go) => { go.StartCoroutine(Goto(home.gameObject)); });
            AddOrder((go) => { go.StartCoroutine(LandAtHome()); });
        }
            
    }

    protected IEnumerator Goto(GameObject target)
    {
        if (target.GetComponent<Collider>())
        {
            m_Agent.m_ArrivedDistance = 
                target.GetComponent<Collider>().bounds.size.magnitude > 30f ? 
                target.GetComponent<Collider>().bounds.size.magnitude : 30f;
            m_Agent.m_BrakingDistance = 
                target.GetComponent<Collider>().bounds.size.magnitude * 1.5f > 45f ?
                target.GetComponent<Collider>().bounds.size.magnitude * 1.5f : 45f;
        }
        else
        {
            m_Agent.m_BrakingDistance = 50f;
            m_Agent.m_ArrivedDistance = 30f;
        }

        Debug.Log(gameObject.name);
        Debug.Log(target.name);
        Debug.Log(target.transform.position);
        Debug.Log(m_Agent);

        m_Agent.SetDestination(target.transform.position);

        yield return new WaitForSeconds(2f);

        while (m_Agent.HasDestination())
        {
            yield return new WaitForFixedUpdate();
        }

        NextOrder();
    }

    protected IEnumerator LandAtHome() 
    {
        yield return new WaitForEndOfFrame();

        GameObject hangar = home.GetHangar();

        m_Agent.m_ArrivedDistance = 5f;
        m_Agent.m_BrakingDistance = 20f;

        yield return new WaitForEndOfFrame();

        m_Agent.SetDestination(hangar.transform.position);
        Debug.Log("Docking Sent");        

        while (m_Agent.HasDestination())
        {
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Docking Done");

        home.Dock(this);

        yield return new WaitForSeconds(1f);

        Destroy(this.gameObject, 0f);

    }

    protected IEnumerator Land(HomeManager manager)
    {
        yield return new WaitForEndOfFrame();

        GameObject hangar = manager.GetHangar();

        m_Agent.m_ArrivedDistance = 5f;
        m_Agent.m_BrakingDistance = 20f;

        yield return new WaitForEndOfFrame();

        m_Agent.SetDestination(hangar.transform.position);
        Debug.Log("Docking Sent");

        while (m_Agent.HasDestination())
        {
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Docking Done");

        home.Dock(this);

        yield return new WaitForSeconds(1f);

        Destroy(this.gameObject, 0f);

    }
}
