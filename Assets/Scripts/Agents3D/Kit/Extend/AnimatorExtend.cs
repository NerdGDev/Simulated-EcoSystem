using UnityEngine;

namespace Kit
{
	public static class AnimatorExtend
	{
		public static bool HasParameter(this Animator animator, string parameterName)
		{
			int hash = Animator.StringToHash(parameterName);
			foreach (AnimatorControllerParameter param in animator.parameters)
			{
				if (param.nameHash == hash)
					return true;
			}
			return false;
		}
		public static bool HasParameter(this Animator animator, string parameterName, AnimatorControllerParameterType type)
		{
			int hash = Animator.StringToHash(parameterName);
			foreach (AnimatorControllerParameter param in animator.parameters)
			{
				if (param.type == type && param.nameHash == hash)
					return true;
			}
			return false;
		}
	}
}