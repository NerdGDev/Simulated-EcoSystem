﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Kit;

namespace Resourcing 
{
    [RequireComponent(typeof(Collider)), RequireComponent(typeof(Visualise)), RequireComponent(typeof(Debuggable))]
    public class Resource : MonoBehaviour 
    {
        [Header("Resource")]
        public float StartingPool;        
        public float MaxPool;
        public float TransferRate;
        public float TransferRange;

        public float Pool;

        public bool TransferBusy;
        Resource TransferTarget;

        [Header("Visualised")]
        public bool Visualisation;
        public bool DisplayPool;

        public Collider m_Collider;

        private StringBuilder debugReport;
        private static GUIStyle InfoStyle;

        public void Awake()
        {            
            Pool = StartingPool > MaxPool ? MaxPool : StartingPool;
            m_Collider = GetComponent<Collider>();
            TransferBusy = false;
        }

        public void TransferTo(Resource target)
        {
            target.Load(this);
        }

        // <param name="target"> Target to Unload From </param>
        public void TransferFrom(Resource target)
        {
            Load(target);
        }

        public void Load(Resource sender)
        {
            if (TransferBusy) return;
            StartCoroutine(_Load(sender));
        }

        private IEnumerator _Load(Resource sender)
        {
            _LockTransfer(sender);
            while (true)
            {
                if (
                    Pool + (sender.TransferRate * Time.deltaTime) < MaxPool &&
                    sender.Pool - (sender.TransferRate * Time.deltaTime) > 0
                    )
                {
                    sender.Pool -= sender.TransferRate * Time.deltaTime;
                    Pool += sender.TransferRate * Time.deltaTime;
                }
                else if (Pool + (sender.TransferRate * Time.deltaTime) >= MaxPool)
                {
                    float overflow = sender.TransferRate * Time.deltaTime - (Pool + (sender.TransferRate * Time.deltaTime) - MaxPool);
                    sender.Pool -= overflow;
                    Pool += overflow;
                    break;
                }
                else if (sender.Pool - (sender.TransferRate * Time.deltaTime) <= 0)
                {
                    float overflow = sender.Pool;
                    sender.Pool -= overflow;
                    Pool += overflow;
                    break;
                }


                yield return new WaitForEndOfFrame();
            }
            _UnlockTransfer(sender);
        }

        private void _LockTransfer(Resource target)
        {
            TransferBusy = true;
            target.TransferBusy = true;
            TransferTarget = target;
            target.TransferTarget = this;
        }

        private void _UnlockTransfer(Resource target)
        {
            TransferBusy = false;
            target.TransferBusy = false;
            TransferTarget = null;
            target.TransferTarget = null;
        }

        private void OnDrawGizmos()
        {
            if (debugReport == null)
                debugReport = new StringBuilder(1000);
            else
                debugReport.Remove(0, debugReport.Length);

            debugReport.AppendLine("Pool: " + Pool.ToString() + " / " + MaxPool.ToString());
            debugReport.AppendLine("Busy: " + TransferBusy);

            debugReport.AppendLine("Target: " + ((TransferTarget != null) ? TransferTarget.name : "No Target"));
            GizmosExtend.DrawLabel(transform.position + Vector3.up, debugReport.ToString(), InfoStyle);

            if (Application.isPlaying)
            {
                if (TransferTarget)
                {
                    Gizmos.DrawLine(m_Collider.bounds.center, TransferTarget.m_Collider.bounds.center);
                }
            }
        }

    }
}

//{public float StartingPool { get; set; }
//public float Pool { get; set; }
//public float MaxPool { get; set; }
//public float TransferRate { get; set; }
//public bool Visualisation { get; set; }
//public bool DisplayPool { get; set; }
//public IResource TransferTarget { get; set; }
// }