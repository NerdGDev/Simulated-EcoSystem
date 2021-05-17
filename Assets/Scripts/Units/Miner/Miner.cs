using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

[RequireComponent(typeof(Resource))]
public class Miner : UnitBase
{
    Resource resource;

    public GameObject[] MiningSockets;

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

    }
}
