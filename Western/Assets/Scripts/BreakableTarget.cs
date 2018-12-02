using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableTarget : MonoBehaviour
{
    public GameObject original;
    public GameObject[] Parts;
    private Vector3[] partPositions;
    private Quaternion[] partRotations;
    private Rigidbody[] rigidBodies;

    private Collider m_Collider;

    public float m_fRespawnTime = 5.0f;
    public float m_fForce = 1.0f;

	// Use this for initialization
	void Start ()
    {
        m_Collider = GetComponent<Collider>();
        partPositions = new Vector3[Parts.Length];
        partRotations = new Quaternion[Parts.Length];
        rigidBodies = new Rigidbody[Parts.Length];

        for (int i =0; i < Parts.Length; i++)
        {
            partPositions[i] = Parts[i].transform.position;
            partRotations[i] = Parts[i].transform.rotation;
            rigidBodies[i] = Parts[i].GetComponent<Rigidbody>();
        }

        ResetTarget();
    }

    public void Break(Vector3 impactPoint)
    {
        m_Collider.enabled = false;
        original.SetActive(false);
        for (int i = 0; i < Parts.Length; i++)
        {
            Parts[i].SetActive(true);
            Vector3 direction = (partPositions[i] - impactPoint).normalized;
            rigidBodies[i].useGravity = true;
            rigidBodies[i].isKinematic = false;
            rigidBodies[i].AddForceAtPosition(direction * m_fForce, impactPoint, ForceMode.Impulse);
        }

        StartCoroutine(Restore());
    }

    private IEnumerator Restore()
    {
        yield return new WaitForSeconds(m_fRespawnTime);
        ResetTarget();
    }

    private void ResetTarget()
    {
        for (int i = 0; i < Parts.Length; i++)
        {
            Parts[i].transform.position = partPositions[i];
            Parts[i].transform.rotation = partRotations[i];

            Parts[i].SetActive(true);
            rigidBodies[i].useGravity = false;
            rigidBodies[i].isKinematic = true;
            rigidBodies[i].velocity = Vector3.zero;
            rigidBodies[i].angularVelocity = Vector3.zero;
        }

        m_Collider.enabled = true;
        original.SetActive(true);
    }
}
