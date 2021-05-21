using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

namespace Resourcing 
{    
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Visualise))]
    public class MineralMasterNode : MonoBehaviour
    {
        Rigidbody rb;

        public int NodeMax;
        public float SpawnRate;
        public GameObject MineralNode;

        Visualise visualise;

        public List<MineralNode> m_Nodes { get; private set; } = new List<MineralNode>();


        private void Awake()
        {
            visualise = GetComponent<Visualise>();
            rb = GetComponent<Rigidbody>();
            rb.angularVelocity = Random.onUnitSphere / 5f;
            StartCoroutine(ManageNode());
        }

        private void FixedUpdate()
        {
            if (visualise != null) 
            {
                visualise.AddDataField("Nodes", m_Nodes.Count.ToString());
            }
        }

        IEnumerator ManageNode() 
        {
            yield return new WaitForFixedUpdate();
            while (true) 
            {
                if (m_Nodes.Count < NodeMax)
                {
                    GameObject go = Instantiate(MineralNode, transform.position + (Random.insideUnitSphere * 50f), new Quaternion());
                    m_Nodes.Add(go.GetComponent<MineralNode>());
                    go.GetComponent<MineralNode>().master = this;
                }
                yield return new WaitForSeconds(Random.Range(SpawnRate / 2f, SpawnRate * 2f));
                yield return new WaitForFixedUpdate();
            }
        }

        public void RemoveNode(MineralNode node) 
        {
            m_Nodes.Remove(node);
        }
    }
}

