using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTracer : MonoBehaviour {

    public float m_fDestroyTime = 0.5f;
    public float distancePerFrame = 15f;
    public float reflectAngle = 70f;
    private Transform m_Transform;
    private RaycastHit m_HitInfo;

    [SerializeField]
    private int iMaxReflections = 1;
    private int m_iReflectionCount = 0;

    public void Init(Vector3 position, Vector3 direction)
    {
        m_Transform = transform;
        m_Transform.position = position;
        m_Transform.forward = direction;

        StartCoroutine(DestoryIfOld());
    }

    IEnumerator DestoryIfOld()
    {
        yield return new WaitForSeconds(m_fDestroyTime);
        Destroy(gameObject);
    }
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 point = m_Transform.position + (m_Transform.forward * distancePerFrame);

        if(true == GetRaycast(m_Transform.position, m_Transform.forward, ref m_HitInfo))
        {
            point = m_HitInfo.point;
            Debug.DrawRay(point, m_HitInfo.normal);

            float reflectionAngle = Vector3.Angle(m_HitInfo.normal, -m_Transform.forward);
            if (reflectionAngle > reflectAngle)
            {
                Debug.Log("Deflect: " + reflectionAngle);
                //Reflect
                m_Transform.forward = Vector3.Reflect(m_Transform.forward, m_HitInfo.normal);

                m_iReflectionCount++;
            }
            else
            {
                //Absorb and destroy
                this.enabled = false;
            }
        }

        m_Transform.position = point;

        if(m_iReflectionCount > iMaxReflections)
        {
            this.enabled = false;
        }
	}

    private bool GetRaycast(Vector3 position, Vector3 direction, ref RaycastHit info)
    {
        var ray = new Ray(position, direction);
        if (Physics.Raycast(ray, out info, distancePerFrame) == true)
        {
            return true;
        }

        return false;
    }
}
