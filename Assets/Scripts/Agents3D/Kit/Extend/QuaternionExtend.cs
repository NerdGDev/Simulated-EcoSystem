using UnityEngine;

namespace Kit
{
    public static class QuaternionExtend
    {
		/// <summary>Get the different between two Quaternion</summary>
		/// <param name="qtA"></param>
		/// <param name="qtB"></param>
		/// <returns>Different in Quaternion format</returns>
		/// <see cref="http://answers.unity3d.com/questions/35541/problem-finding-relative-rotation-from-one-quatern.html"/>
		public static Quaternion Different(this Quaternion qtA, Quaternion qtB)
        {
            return qtA.Inverse() * qtB;
        }

        /// <summary>Shortcut for Inverse rotation</summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Quaternion Inverse(this Quaternion self)
        {
            return Quaternion.Inverse(self);
        }

        /// <summary>Get conjugate quaternion based on current</summary>
        /// <param name="self"></param>
        /// <returns></returns>
        /// <see cref="http://stackoverflow.com/questions/22157435/difference-between-the-two-quaternions"/>
        public static Quaternion Conjugate(this Quaternion self)
        {
            return new Quaternion(-self.x, -self.y, -self.z, self.w);
        }

		/// <summary>Get the local right vector from quaternion.</summary>
		/// <param name="rotation"></param>
		/// <returns>Vector direction</returns>
		public static Vector3 GetRight(this Quaternion rotation)
        {
            return rotation * Vector3.right;
        }

		/// <summary>Get the local left vector from quaternion.</summary>
		/// <param name="rotation"></param>
		/// <returns>Vector direction</returns>
		public static Vector3 GetLeft(this Quaternion rotation)
		{
			return rotation * Vector3.left;
		}

		/// <summary>Get the local up vector from quaternion.</summary>
		/// <param name="rotation"></param>
		/// <returns>Vector direction</returns>
		public static Vector3 GetUp(this Quaternion rotation)
        {
            return rotation * Vector3.up;
        }

		/// <summary>Get the local down vector from quaternion.</summary>
		/// <param name="rotation"></param>
		/// <returns>Vector direction</returns>
		public static Vector3 GetDown(this Quaternion rotation)
		{
			return rotation * Vector3.down;
		}

        /// <summary>Get the local forward vector from quaternion.</summary>
        /// <param name="rotation"></param>
        /// <returns>Vector direction</returns>
        public static Vector3 GetForward(this Quaternion rotation)
        {
            return rotation * Vector3.forward;
        }

		/// <summary>Get the local backward vector from quaternion.</summary>
		/// <param name="rotation"></param>
		/// <returns>Vector direction</returns>
		public static Vector3 GetBackward(this Quaternion rotation)
		{
			return rotation * Vector3.back;
		}

		/// <see cref="http://sunday-lab.blogspot.hk/2008/04/get-pitch-yaw-roll-from-quaternion.html"/>
		/// <summary>Calculate pitch angle based on Vector3.forward & Vector3.up</summary>
		/// <returns>Pitch angle based on world forward & upward.</returns>
		public static float GetPitch(this Quaternion o)
        {
            return Mathf.Atan((2f * (o.y * o.z + o.w * o.x)) / (o.w * o.w - o.x * o.x - o.y * o.y + o.z * o.z));
        }

		/// <summary>Calculate pitch angle based on Vector3.forward & Vector3.up</summary>
		/// <returns>Yaw angle based on world forward & upward.</returns>
		public static float GetYaw(this Quaternion o)
        {
            return Mathf.Asin(-2f * (o.x * o.z - o.w * o.y));
        }

		/// <summary>Calculate pitch angle based on Vector3.forward & Vector3.up</summary>
		/// <returns>Roll angle based on world forward & upward.</returns>
		public static float GetRoll(this Quaternion o)
        {
            return Mathf.Atan((2f * (o.x * o.y + o.w * o.z)) / (o.w * o.w + o.x * o.x - o.y * o.y - o.z * o.z));
        }

        /// <summary>To find a signed angle between two quaternion, based on giving axis</summary>
        /// <param name="qtA">Quaternion</param>
        /// <param name="qtB">Quaternion</param>
        /// <param name="direction">The direction of quaternion, e.g. Vector3.forward</param>
        /// <param name="normal">The normal axis, e.g. Vector3.right</param>
        /// <returns></returns>
        public static float AngleBetweenDirectionSigned(this Quaternion qtA, Quaternion qtB, Vector3 direction, Vector3 normal)
        {
            return (qtA * direction).AngleBetweenDirectionSigned(qtB * direction, normal);
        }
    }
}