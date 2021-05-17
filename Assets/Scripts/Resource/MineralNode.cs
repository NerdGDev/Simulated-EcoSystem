using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resourcing 
{
    public class MineralNode : MonoBehaviour
    {

        public float value;

        public bool Assigned;
        public bool Harvesting;

        private void Awake()
        {
            value = Random.Range(1,16) * 10;
        }

        public void Assign() 
        {
            Assigned = true;
        }

        public void Harvest() 
        {
            Harvesting = true;
        }


    }
}

