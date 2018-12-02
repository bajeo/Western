using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
    public float m_fDestroyTime = 0.5f;

	// Use this for initialization
	IEnumerator Start ()
    {
        yield return new WaitForSeconds(m_fDestroyTime);
        Destroy(gameObject);
	}
}
