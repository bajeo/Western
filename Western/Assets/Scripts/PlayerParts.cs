using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerParts : MonoBehaviour
{
    public event Action<Vector3, Vector3> OnPullTrigger;
    public event Action<bool> OnGunGrabbed;

    public static PlayerParts Singleton;
    private List<Transform> HandPositions = new List<Transform>();
    private Transform Head;

	// Use this for initialization
	void Awake ()
    {
        Singleton = this;

        Head = Camera.main.transform;
        //Get Transforms from Steam VR
        HandPositions.Add(transform.Find("SteamVRObjects/LeftHand"));
        HandPositions.Add(transform.Find("SteamVRObjects/RightHand"));

        //Holster holster = GetComponentInChildren<Holster>();
        //holster.OnGrabbedGun += OnGunChangeState;
    }

    public void GetHandData(ref Vector3 headPos, ref Quaternion headRotation, 
                            ref Vector3[] positions, ref Quaternion[] rotations)
    {
        headPos = Head.position;
        headRotation = Head.rotation;

        for(int i =0; i < HandPositions.Count; i++)
        {
            positions[i] = HandPositions[i].position;
            rotations[i] = HandPositions[i].rotation;
        }
    }

    private void OnGunChangeState(bool state)
    {
        OnGunGrabbed?.Invoke(state);
    }

    private void OnClientShootWeapon()
    {        
        //If gun hand??
        OnPullTrigger?.Invoke(Head.position, Head.forward);
    }
}