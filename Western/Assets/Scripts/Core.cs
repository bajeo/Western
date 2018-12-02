using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Core : MonoBehaviour
{
    public static Core Instance;

    [SerializeField]
    private GameObject[] m_StartUpObjects;
    [SerializeField]
    private GameObject m_GoObject;

    [SerializeField]
    private Vector3 OffsetToPlayer;
    VrPlayer vrplayer;


    // Use this for initialization
    void Awake()
    {
        Instance = this;

        vrplayer = GameObject.FindObjectOfType<VrPlayer>();
    }
	
	public void PlayerJoined()
    {
        Debug.Log("Resetting sequence");
        ResetSequence();
        StartCoroutine(StartSequence());
    }

    private void ResetSequence()
    {
        //Stop sound fx etc...
        vrplayer = GameObject.FindObjectOfType<VrPlayer>();

        if (null != vrplayer)
        {
            vrplayer.GetCowBoy.CanGrabGun = false;
        }
    }

    private IEnumerator StartSequence()
    {
        Vector3 postiion = vrplayer.transform.position + (vrplayer.transform.rotation * OffsetToPlayer);
        //Play sound fx etc...
        GameObject.Instantiate(m_StartUpObjects[2], postiion, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        GameObject.Instantiate(m_StartUpObjects[1], postiion, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        GameObject.Instantiate(m_StartUpObjects[0], postiion, Quaternion.identity);
    }

    public void GameStarted()
    {
        //Allow gun grabbing

        Debug.Log("Game Started");

        if (null != vrplayer)
        {
            vrplayer.GetCowBoy.CanGrabGun = true;
        }

        Vector3 postiion = vrplayer.transform.position + (vrplayer.transform.rotation * OffsetToPlayer);
        //GO!!!
        GameObject.Instantiate(m_GoObject, postiion, Quaternion.identity);
    }
}