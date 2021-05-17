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
    private List<GameObject> m_Particle;

    void Awake()
    {
        base.Awake();
        resource = GetComponent<Resource>();
        type = ObjectType.CARRIER;
    }

    public void MineFromNode(MineralMasterNode masterNode)
    {
        AddOrder((go) => { StartCoroutine(Goto(masterNode.gameObject)); });        
    }

    public void SearchForNodes(MineralMasterNode masterNode) 
    {
        float poolLimit = resource.MaxPool - resource.Pool;
        foreach (MineralNode node in masterNode.m_Nodes) 
        {
            if (poolLimit - node.value >= 0) 
            {
                AddOrder((go) => { StartCoroutine(Goto(node.gameObject)); });
                AddOrder((go) => { StartCoroutine(HarvestNode(node)); });
                poolLimit = poolLimit - node.value;
            }
        }
    }

    IEnumerator HarvestNode(MineralNode node) 
    {
        yield return new WaitForFixedUpdate();
        StartCoroutine(StartMiningAnim(node));
        yield return new WaitForSeconds(8f);
        StopCoroutine(StartMiningAnim(node));
        StartCoroutine(EndMiningAnim());
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
            ps.Add(m_Particle[x].GetComponent<ParticleSystem>());
        }

        while (true)
        {
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
        yield return new WaitForFixedUpdate();
        for (int x = 0; x < miningSockets.Length; x++)
        {
            Destroy(m_Particle[x]);
        }
        m_Particle.Clear();
    }
}
