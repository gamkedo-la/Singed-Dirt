using UnityEngine;
using System.Collections;

public class CameraFlyingControl : MonoBehaviour 
{
    [SerializeField] bool m_allowLock = false;

	[SerializeField] float m_forwardSpeedStart = 10f;
	[SerializeField] float m_sidewaysSpeedStart = 10f;
	[SerializeField] float m_verticalSpeedStart = 10f;

	[Range(1, 2)]
	[SerializeField] float m_speedMultiplier = 1.5f;

	[SerializeField] float m_sensitivityX = 15f;
	[SerializeField] float m_sensitivityY = 15f;

	[SerializeField] float m_minLookAngle = -90f;
	[SerializeField] float m_maxLookAngle = 90f;

	private float m_rotationX;
	private float m_rotationY;
	private Quaternion m_originalRotation;

	private bool m_enabled = true;
	//private GameObject m_player;
    //private Transform m_camera;

	private float m_forwardSpeed;
	private float m_sidewaysSpeed;
	private float m_verticalSpeed;

	//private bool m_followPlayer;


	void Awake()
	{
		m_originalRotation = Quaternion.identity;
		//m_player = GameObject.FindGameObjectWithTag(Tags.Player);
        //m_camera = Camera.main.transform;
	}


	void Update () 
	{
		if (Input.anyKey)
		{
			float multiplier = 1f + Time.unscaledDeltaTime * (m_speedMultiplier - 1f);
			m_forwardSpeed *= multiplier;
			m_sidewaysSpeed *= multiplier;
			m_verticalSpeed *= multiplier;
		}
		else
		{
			m_forwardSpeed = m_forwardSpeedStart;
			m_sidewaysSpeed = m_sidewaysSpeedStart;
			m_verticalSpeed = m_verticalSpeedStart;
		}

        //if (Input.GetKeyDown(KeyCode.Mouse1) && m_player != null)
        //	m_followPlayer = !m_followPlayer;

        if (m_allowLock && Input.GetKeyDown(KeyCode.Mouse0))
            m_enabled = !m_enabled;

        if (m_allowLock && Input.GetKeyDown(KeyCode.Mouse1))
        {
            switch (Cursor.lockState)
            {
                case (CursorLockMode.None):
                    Cursor.lockState = CursorLockMode.Locked;
                    break;

                case (CursorLockMode.Locked):
                    Cursor.lockState = CursorLockMode.None;
                    break;
            }
        }

        if (!m_enabled)
			return;

		//if (m_followPlayer)
		//{
		//	transform.LookAt(m_player.transform.position);
		//	SetRotationValues();
		//}
		//else
		//{
			// Note: have to use Raw here since GetAxis stays at 0 if Time.timeScale is also 0
			float v = Input.GetAxisRaw ("Vertical");
			float h = Input.GetAxisRaw ("Horizontal");
			float u = Input.GetAxisRaw ("Elevation");

			transform.Translate(Vector3.forward * m_forwardSpeed * Time.unscaledDeltaTime * v);
			transform.Translate(Vector3.right * m_sidewaysSpeed * Time.unscaledDeltaTime * h);
			transform.Translate(Vector3.up * m_verticalSpeed * Time.unscaledDeltaTime * u);

			m_rotationX += Input.GetAxis("Mouse X") * m_sensitivityX;
		 	m_rotationY += Input.GetAxis("Mouse Y") * m_sensitivityY;

			m_rotationY = Mathf.Clamp (m_rotationY, m_minLookAngle, m_maxLookAngle);

			Quaternion xQuaternion = Quaternion.AngleAxis (m_rotationX, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis (m_rotationY, -Vector3.right);

        transform.localRotation = m_originalRotation * xQuaternion * yQuaternion;
		//}
	}


	void OnEnable()
	{
		SetRotationValues();
	}


	private void SetRotationValues()
	{
		// Axes are a bit weird because mouse and rotation axes are in different frames of reference
		m_rotationX = transform.rotation.eulerAngles.y;
		m_rotationY = -transform.rotation.eulerAngles.x;

		if (m_rotationY > m_maxLookAngle)
			m_rotationY -= 360f;
		else if (m_rotationY < m_minLookAngle)
			m_rotationY += 360f;
	}
}
