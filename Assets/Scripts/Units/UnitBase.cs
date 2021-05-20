using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlyAgent.Navigation;
using FlyAgent.Agents;
using Kit;

[RequireComponent(typeof(FlyAgent.Agents.FlyAgent)), RequireComponent(typeof(Visualise)), RequireComponent(typeof(Debuggable))]
public class UnitBase : ManageObject
{
    protected string state;

    public bool Busy;

    protected bool goingHome = false;

    protected FlyAgent.Agents.FlyAgent m_Agent;

    public delegate void OrderDelegate(MonoBehaviour go);
    protected Queue<OrderDelegate> OrderQueue = new Queue<OrderDelegate>();

    protected Collider m_Collider;    

    public HomeManager home;

    public virtual void Awake()
    {
        state = "Awake";
        m_Agent = GetComponent<FlyAgent.Agents.FlyAgent>();
        m_Collider = GetComponent<Collider>();
        //Debug.Log(m_Agent);
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
        state = "Thinking";
        OrderQueue.Dequeue();
        if (OrderQueue.Count > 0)
        {
            OrderQueue.Peek().Invoke(this);
        }
        else if (home != null) 
        {
            //Debug.LogError("Returning Home");
            AddOrder((go) => { go.StartCoroutine(Goto(home.gameObject)); });
            AddOrder((go) => { go.StartCoroutine(LandAtHome()); });
        }
            
    }

    protected IEnumerator Goto(GameObject target)
    {
        state = "Traveling to Destination";
        if (target.GetComponent<SphereCollider>())
        {
            m_Agent.m_ArrivedDistance = 
                target.GetComponent<SphereCollider>().radius * 1.5f > 25f ? 
                target.GetComponent<SphereCollider>().radius * 1.5f : 25f;
            m_Agent.m_BrakingDistance = 
                target.GetComponent<SphereCollider>().radius * 2f > 50f ?
                target.GetComponent<SphereCollider>().radius * 2f : 50f;
        }
        else
        {
            m_Agent.m_BrakingDistance = 50f;
            m_Agent.m_ArrivedDistance = 25f;
        }

        //Debug.Log(gameObject.name);
        //Debug.Log(target.name);
        //Debug.Log(target.transform.position);
        //Debug.Log(m_Agent);

        m_Agent.SetDestination(target.transform.position);

        yield return new WaitForSeconds(2f);

        while (m_Agent.HasDestination())
        {
            yield return new WaitForSeconds(UpdateFrequency);
        }

        NextOrder();
    }

    protected IEnumerator Goto(GameObject target, float accuracy)
    {
        state = "Traveling to Destination";
        if (target.GetComponent<SphereCollider>())
        {
            m_Agent.m_ArrivedDistance =
                target.GetComponent<SphereCollider>().radius * 1.2f > 25f ?
                target.GetComponent<SphereCollider>().radius * 1.2f : 25f;
            m_Agent.m_BrakingDistance =
                target.GetComponent<SphereCollider>().radius * 1.5f > 50f ?
                target.GetComponent<SphereCollider>().radius * 1.5f : 50f;
        }
        else
        {
            m_Agent.m_BrakingDistance = 50f;
            m_Agent.m_ArrivedDistance = 25f;
        }

        //Debug.Log(gameObject.name);
        //Debug.Log(target.name);
        //Debug.Log(target.transform.position);
        //Debug.Log(m_Agent);

        m_Agent.SetDestination(target.transform.position + (Random.insideUnitSphere * accuracy));

        yield return new WaitForSeconds(2f);

        while (m_Agent.HasDestination())
        {
            yield return new WaitForSeconds(UpdateFrequency);
        }

        NextOrder();
    }

    protected IEnumerator LandAtHome() 
    {
        state = "Landing at Home";
        goingHome = true;
        yield return new WaitForSeconds(UpdateFrequency);

        GameObject hangar = home.GetHangar();

        m_Agent.m_ArrivedDistance = 10f;
        m_Agent.m_BrakingDistance = 80f;

        yield return new WaitForSeconds(UpdateFrequency);

        m_Agent.SetDestination(hangar.transform.position);
        //Debug.Log("Docking Sent");

        state = "Traveling to Home";
        while (m_Agent.HasDestination() && (m_Agent.m_Pilot.m_PathState != Pilot.ePathState.Idle || !(m_Agent.m_Pilot.m_PathState >= (Pilot.ePathState)100)))
        {
            yield return new WaitForSeconds(UpdateFrequency);
        }

        //Debug.Log("Docking Done");
        state = "Docking";

        Destroy(this.gameObject, 0.2f);

        home.Dock(this);

        

    }

    protected IEnumerator Land(HomeManager manager)
    {
        state = "Landing at new Home";
        goingHome = true;
        yield return new WaitForSeconds(UpdateFrequency);

        GameObject hangar = manager.GetHangar();

        m_Agent.m_ArrivedDistance = 10f;
        m_Agent.m_BrakingDistance = 80f;

        yield return new WaitForSeconds(UpdateFrequency);

        m_Agent.SetDestination(hangar.transform.position);
        //Debug.Log("Docking Sent");

        while (m_Agent.HasDestination() && (m_Agent.m_Pilot.m_PathState != Pilot.ePathState.Idle || !(m_Agent.m_Pilot.m_PathState >= (Pilot.ePathState)100)))
        {
            yield return new WaitForSeconds(UpdateFrequency);
        }

        //Debug.Log("Docking Done");

        manager.Dock(this);

        Destroy(this.gameObject, 0f);

    }

    private void OnDrawGizmosSelected()
    {
        GizmosExtend.DrawLabel(transform.position + new Vector3(0, 4, 0), state);
    }
}
