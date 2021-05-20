using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

[RequireComponent(typeof(Resource))]
public class Miner : UnitBase
{
    Resource resource;

    public GameObject[] miningSockets;

    public GameObject m_ParticleObject;
    private List<GameObject> m_Particle = new List<GameObject>();

    void Awake()
    {
        base.Awake();
        resource = GetComponent<Resource>();
        type = ObjectType.MINER;
    }

    public void MineFromNode(MineralMasterNode masterNode)
    {
        AddOrder((go) => { StartCoroutine(Goto(masterNode.gameObject, 50f)); });
        AddOrder((go) => { StartCoroutine(SearchForNodes(masterNode)); });
    }

    IEnumerator SearchForNodes(MineralMasterNode masterNode) 
    {
        state = "Searching for New Nodes";
        yield return new WaitForFixedUpdate();
        float poolLimit = resource.MaxPool - resource.Pool;
        foreach (MineralNode node in masterNode.m_Nodes) 
        {
            //Debug.LogError("Looking at Mineral Node");
            if (poolLimit - node.value >= 0 && !node.Assigned) 
            {
                //Debug.LogWarning("Node Found");
                node.Assign();
                AddOrder((go) => { StartCoroutine(Goto(node.gameObject)); });
                AddOrder((go) => { StartCoroutine(HarvestNode(node)); });                
                poolLimit = poolLimit - node.value;
            }            
        }
        yield return new WaitForFixedUpdate();
        AddOrder((go) => { StartCoroutine(Goto(home.gameObject)); });
        AddOrder((go) => { StartCoroutine(GiveResource(home.gameObject.GetComponent<Resource>())); });
        NextOrder(); 
    }

    IEnumerator GiveResource(Resource target)
    {
        state = "Delivering Resources";
        yield return new WaitForFixedUpdate();
        Collider[] hitColliders = Physics.OverlapSphere(m_Collider.bounds.center, resource.TransferRange, Physics.AllLayers, QueryTriggerInteraction.Collide);
        state = "Finding Station";
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
            state = "NO TARGET";
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

    IEnumerator HarvestNode(MineralNode node) 
    {
        state = "Harvesting node - value : " + node.value;
        //Debug.LogWarning("Harvesting Node");
        yield return new WaitForFixedUpdate();
        StartCoroutine(StartMiningAnim(node));
        node.Harvest();
        yield return new WaitForSeconds(node.value / 10f);
        resource.Pool = resource.Pool + node.value;
        node.Kill();
        StopCoroutine(StartMiningAnim(node));
        yield return new WaitForFixedUpdate();
        StartCoroutine(EndMiningAnim());
        NextOrder();
    }

    IEnumerator StartMiningAnim(MineralNode node)
    {
        yield return new WaitForFixedUpdate();
        GameObject go = node.gameObject;
        List<ParticleSystem> ps = new List<ParticleSystem>();
        for (int x = 0; x < miningSockets.Length; x++)
        {   
            GameObject psobj = Instantiate(m_ParticleObject, miningSockets[x].transform.position, Quaternion.LookRotation(go.transform.position - miningSockets[x].transform.position));
            m_Particle.Add(psobj);
            ps.Add(psobj.GetComponent<ParticleSystem>());
        }

        while (true)
        {
            GetComponent<Rigidbody>().rotation = Quaternion.LookRotation(go.transform.position - transform.position);
            for (int x = 0; x < miningSockets.Length; x++)
            {
                var main = ps[x].main;
                m_Particle[x].transform.position = miningSockets[x].transform.position;
                m_Particle[x].transform.rotation = Quaternion.LookRotation(go.transform.position - miningSockets[x].transform.position);
                main.startLifetime = (go.transform.position - miningSockets[x].transform.position).magnitude / main.startSpeed.constant;
                yield return new WaitForFixedUpdate();
            }
        }

    }

    IEnumerator EndMiningAnim()
    {
        //Debug.LogWarning("Ending Anim");
        yield return new WaitForFixedUpdate();
        foreach (GameObject go in m_Particle) 
        {
            //Debug.LogWarning("Destroy Anim");
            //Debug.LogWarning(go);
            Destroy(go);
        }
        yield return new WaitForFixedUpdate();
        m_Particle.Clear();
    }
}
