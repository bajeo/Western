using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {
    public Vector3 m_fRotateSpeed;
    private Transform m_Transform;
	// Use this for initialization
	void Start () {
        m_Transform = transform;
	}
	
	// Update is called once per frame
	void Update () {
        m_Transform.Rotate(m_fRotateSpeed * Time.deltaTime);
	}
}
