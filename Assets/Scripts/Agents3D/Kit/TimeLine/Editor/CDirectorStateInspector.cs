using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Kit
{
	[CustomEditor(typeof(CDirectorState), true)]
	public class CDirectorStateInspector : EditorBase
	{
		CDirectorState m_Director;
		protected override void OnEnable()
		{
			base.OnEnable();
			m_Director = (CDirectorState)serializedObject.targetObject;
		}

		protected override void OnBeforeDrawGUI()
		{
			string directorState = m_Director.playableDirector.state.ToString("F");
			EditorGUILayout.LabelField("Director", directorState);
			var graph = m_Director.playableDirector.playableGraph;
			if (graph.IsValid())
			{
				if (graph.IsPlaying())
					EditorGUILayout.LabelField("Graph", "Playing");
				else if (graph.IsDone())
					EditorGUILayout.LabelField("Graph", "Done");
				else
					EditorGUILayout.LabelField("Graph", "Unknown");
			}
			else
			{
				EditorGUILayout.LabelField("Graph", "Invalid");
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Play"))
			{
				m_Director.Play();
			}

			if (GUILayout.Button("Pause"))
			{
				m_Director.Pause();
			}

			if (GUILayout.Button("Resume"))
			{
				m_Director.Resume();
			}

			if (GUILayout.Button("Stop"))
			{
				m_Director.Stop();
			}
			EditorGUILayout.EndHorizontal();


		}
	}
}