using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resourcing;

namespace Resourcing 
{    
    public class MineralMasterNode : MonoBehaviour
    {
        Rigidbody rb;

        public int NodeMax;
        public GameObject MineralNode;

        public List<MineralNode> m_Nodes { get; private set; } = new List<MineralNode>();


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.angularVelocity = new Vector3(0, 0.1f, 0);
            StartCoroutine(ManageNode());
        }

        

        IEnumerator ManageNode() 
        {
            yield return new WaitForFixedUpdate();
            while (true) 
            {
                if (m_Nodes.Count < NodeMax)
                {
                    Instantiate(MineralNode, transform.position + (Random.insideUnitSphere * 50f), new Quaternion());
                }
                yield return new WaitForSeconds(12f);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}

