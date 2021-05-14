using UnityEngine;
using UnityEngine.Networking;

namespace Kit
{
	public class ObserverMovement : MonoBehaviour
	{
		[Header("Common")]
		public float m_Speed = 0.5f;
		public float m_HighSpeed = 2f;

		[Header("Optional")]
		//public NetworkIdentity m_NetworkIdentity;
		public Camera m_Camera;

		private float m_CurrentSpeed = 0f;
		private bool m_ToggleSpeed = false;
		private Vector3 m_LocalTranslate = Vector3.zero;
		private Vector2 m_LocalLook = Vector2.zero;

		void OnValidate()
		{
			
			if (m_Camera == null)
				m_Camera = GetComponentInChildren<Camera>(true);
			
		}
		
		void InputUpdate()
		{
			// Mouse look
			if ((Input.GetMouseButton(1) || Input.GetMouseButton(2)) &&
				Cursor.lockState != CursorLockMode.Locked)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
			if ((Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) &&
				Cursor.lockState != CursorLockMode.None)
			{
				Cursor.lockState = CursorLockMode.None;
			}

			if (Cursor.lockState == CursorLockMode.Locked)
			{
				m_LocalLook.x = -Input.GetAxis("Mouse Y");
				m_LocalLook.y = Input.GetAxis("Mouse X");
			}

			// Speed
			if (!m_ToggleSpeed &&
				Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
			{
				m_ToggleSpeed = true;
			}
			else if (m_ToggleSpeed &&
				(Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)) &&
				!(Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift)))
			{
				m_ToggleSpeed = false;
			}
			m_CurrentSpeed = (m_ToggleSpeed) ? m_HighSpeed : m_Speed;

			// Movement
			if (Input.GetAxis("Vertical") != 0)
			{
				m_LocalTranslate += transform.forward * m_CurrentSpeed * Input.GetAxis("Vertical");
			}
			if (Input.GetAxis("Horizontal") != 0)
			{
				m_LocalTranslate += transform.right * m_CurrentSpeed * Input.GetAxis("Horizontal");
			}

			if (Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q))
			{
				m_LocalTranslate += transform.up * m_CurrentSpeed * 0.5f;
			}
			if (Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
			{
				m_LocalTranslate += -transform.up * m_CurrentSpeed * 0.5f;
			}
		}

		void Update()
		{
			LocalPlayerUpdate();			
		}

		void NetworkNPCUpdate()
		{
			if (m_Camera != null)
				m_Camera.enabled = false;
		}

		void LocalPlayerUpdate()
		{
			InputUpdate();

			// Apply
			transform.Translate(m_LocalTranslate, Space.World);
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				transform.Rotate(Vector3.right, m_LocalLook.x, Space.Self);
				transform.Rotate(Vector3.up, m_LocalLook.y, Space.World);
			}

			// Reset
			m_LocalTranslate = Vector3.zero;
			m_LocalLook = Vector2.zero;
		}
	}
}