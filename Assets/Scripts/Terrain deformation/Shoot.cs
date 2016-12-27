using UnityEngine;
using System.Collections;

public class Shoot : MonoBehaviour
{
    [SerializeField] GameObject m_projectile;
    [SerializeField] float m_projectileSpeed = 50f;

    private bool m_inputAxisInUse;

	void Update()
    {
        int input = (int) Input.GetAxisRaw("Jump");

        if (input == 0)
        {
            m_inputAxisInUse = false;
        }
        else if (!m_inputAxisInUse && input  == 1 && m_projectile != null)
        {
            m_inputAxisInUse = true;
            var projectile = (GameObject) Instantiate(m_projectile, transform.position, transform.rotation);
   
            var rigidbody = projectile.GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.velocity = projectile.transform.forward * m_projectileSpeed;
        }
	}
}
