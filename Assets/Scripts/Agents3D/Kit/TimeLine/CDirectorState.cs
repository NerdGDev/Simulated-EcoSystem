// #define TIMELINE_DEBUG
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Kit
{
	[RequireComponent(typeof(PlayableDirector))]
	public class CDirectorState : MonoBehaviour
	{
		[SerializeField] PlayableDirector m_Director;
		public UnityEvent EVENT_Start;
		public UnityEvent EVENT_Pause;
		public UnityEvent EVENT_Resume;
		public UnityEvent EVENT_End;

		private bool m_Started = false, m_Paused = false, m_Done = true;
		private bool IsIdle { get { return !m_Started && !m_Paused && m_Done; } }

		public PlayableDirector playableDirector { get { return m_Director; } }

		private void Reset()
		{
			OnValidate();
		}

		private void OnValidate()
		{
			if (m_Director == null)
				m_Director = GetComponent<PlayableDirector>();
		}

		private void OnDisable()
		{
			if (!IsIdle && ShouldTriggerEventEnd())
				TriggerEventEnd();
		}

		private void Update()
		{
			PlayableGraph graph = m_Director.playableGraph;

			if (graph.IsValid())
			{
				bool isPlaying = graph.IsPlaying();
				if (isPlaying && !m_Started && _IsHoldModeEndOrIgnore())
				{
					m_Started = true;
					m_Done = false;
					EVENT_Start.Invoke();
#if TIMELINE_DEBUG
				Debug.LogWarning("Start");
#endif
				}
				else if (!isPlaying && !graph.IsDone() && !m_Paused &&
					_IsHoldModeEndOrIgnore())
				{
					m_Paused = true;
					EVENT_Pause.Invoke();
#if TIMELINE_DEBUG
				Debug.LogWarning("Pause");
#endif
				}
				else if (isPlaying && m_Paused)
				{
					m_Paused = false;
					EVENT_Resume.Invoke();
#if TIMELINE_DEBUG
				Debug.LogWarning("Resume");
#endif
				}
				else if (!m_Paused && !m_Done && ShouldTriggerEventEnd())
				{
					TriggerEventEnd();
				}
			}
			else
			{
				if (IsIdle)
				{
					// normal update before started or stopped.
				}
				else if (ShouldTriggerEventEnd())
				{
					TriggerEventEnd();
				}
				else
				{
					throw new System.InvalidProgramException("Non-handled status please fix this");
				}
			}
		}

		private bool _IsHoldModeEndOrIgnore()
		{
			if (m_Director.extrapolationMode == DirectorWrapMode.Hold)
				return m_Director.duration != m_Director.time;
			return true; // ignore this condition;
		}

		private bool ShouldTriggerEventEnd()
		{
			var graph = m_Director.playableGraph;
			if (!graph.IsValid())
				return true;

			switch (m_Director.extrapolationMode)
			{
				default:
				throw new System.NotImplementedException();

				case DirectorWrapMode.None:
				return !graph.IsPlaying();

				case DirectorWrapMode.Hold:
				return m_Director.duration == m_Director.time;

				case DirectorWrapMode.Loop:
				return false; // never end.
			}
		}

		private void TriggerEventEnd()
		{
			if (m_Done)
				throw new System.InvalidProgramException();

			m_Started = false;
			m_Paused = false;
			m_Done = true;
			EVENT_End.Invoke();
#if TIMELINE_DEBUG
		Debug.LogWarning("Done");
#endif
		}

		#region Redirection
		public void Play()
		{
			m_Director.Play();
		}

		public void Play(PlayableAsset playableAsset)
		{
			m_Director.Play(playableAsset);
		}

		public void Play(PlayableAsset playableAsset, DirectorWrapMode directorWrapMode)
		{
			m_Director.Play(playableAsset, directorWrapMode);
		}

		public void Pause()
		{
			m_Director.Pause();
		}

		public void Resume()
		{
			m_Director.Resume();
		}

		public void Stop()
		{
			m_Director.Stop();
		}
		#endregion
	}
}