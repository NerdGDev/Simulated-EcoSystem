using System.Collections;
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

    public virtual void Awake()
    {
        m_Agent = GetComponent<FlyAgent.Agents.FlyAgent>();
        m_Collider = GetComponent<Collider>();
        Debug.Log(m_Agent);
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
            OrderQueue.Peek().Invoke(this);
    }

    protected IEnumerator Goto(GameObject target)
    {
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
}
