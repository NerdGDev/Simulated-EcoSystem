using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resourcing 
{
    [RequireComponent(typeof(Rigidbody))]
    public class MineralNode : MonoBehaviour
    {
        Rigidbody rb;

        public MineralMasterNode master;
        public float value;

        public bool Assigned;
        public bool Harvesting;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.angularVelocity = Random.onUnitSphere;
            float size = Random.Range(5, 21);
            value = size * 10f;
            transform.localScale = new Vector3(size / 3, size / 3, size / 3);
        }

        public void Assign() 
        {
            Assigned = true;
        }

        public void Harvest() 
        {
            Harvesting = true;
        }

        public void Kill() 
        {
            Debug.LogWarning("Killing Node");
            master.RemoveNode(this);
            Destroy(this.gameObject);
        }

    }
}

