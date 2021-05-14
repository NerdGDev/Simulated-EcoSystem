using UnityEngine;
using System;
using System.Collections.Generic;

namespace Kit
{
    public enum DrawMethod
    {
        OnDrawGizmosSelected = 0,
        OnDrawGizmos,
        Both
    }
    public enum DrawType
    {
        Point = 0,
        Bounds,
        LocalCube,
        Circle,
        Cylinder,
        Cone,
        Arrow,
        Plane,

        // org method
        Sphere,
        Camera,
		Direction
    }
    public class GizmosExtendHelper : MonoBehaviour
    {
        public DrawMethod drawMethod = DrawMethod.OnDrawGizmos;
        public DrawType type = DrawType.Point;

        public Color color = Color.white;
        public float size = 1f;

        private void OnDrawGizmosSelected()
        {
            if (drawMethod == DrawMethod.OnDrawGizmosSelected)
                DrawGizmos();
        }
        private void OnDrawGizmos()
        {
            if ( drawMethod >= DrawMethod.OnDrawGizmos )
                DrawGizmos();
        }
        private void DrawGizmos()
        {
            switch (type)
            {
                default:
                case DrawType.Point:
                    GizmosExtend.DrawPoint(transform.position, color, size);
                    break;
                case DrawType.Bounds:
                    GizmosExtend.DrawBounds(new Bounds(transform.position, transform.lossyScale), color);
                    break;
                case DrawType.LocalCube:
                    GizmosExtend.DrawLocalCube(transform.localToWorldMatrix, Vector3.one * size, color);
                    break;
                case DrawType.Circle:
                    GizmosExtend.DrawCircle(transform.position, transform.up, color, size);
                    break;
                case DrawType.Cylinder:
                    GizmosExtend.DrawCylinder(
                        transform.position.PointOnDistance(transform.up, size * .5f),
                        transform.position.PointOnDistance(-transform.up, size * .5f), color, size);
                    break;
                case DrawType.Cone:
                    GizmosExtend.DrawCone(transform.position, transform.up, color, size);
                    break;
                case DrawType.Arrow:
                    GizmosExtend.DrawArrow(transform.position, transform.forward, color, size);
                    break;
                case DrawType.Sphere:
					GizmosExtend.DrawSphere(transform.position, size, color);
					break;
				case DrawType.Direction:
					GizmosExtend.DrawDirection(transform.position, transform.forward, size, color);
					break;
                case DrawType.Plane:
                    GizmosExtend.DrawPlane(transform, size * 2f, size, color);
                    break;
                case DrawType.Camera:
                    Camera camera = this.GetComponent<Camera>();
                    if(camera!=null)
                        GizmosExtend.DrawFrustum(camera, color);
                    else
                        Gizmos.DrawRay(transform.position, transform.forward);
                    break;
            }
        }
    }
}