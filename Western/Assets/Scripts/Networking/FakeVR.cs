using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeVR : MonoBehaviour
{
    private int iNumHands = 2;
    private Transform Head;
    private GameObject[] Hands;
    private Vector3[] currentHandPositions;
    private Quaternion[] currentHandRotations;

    //Previous positions... used for detecting teleportations and network smoothing
    //private Vector3 lastHeadPosition;
    private Vector3[] lastHandPositions;
    
    //private List<Renderer> m_CharacterMaterial = new List<Renderer>();
    //private List<Renderer> m_TeleportMaterial = new List<Renderer>();

    public void Init()
    {
        GameObject root = Instantiate(Resources.Load<GameObject>("NetPlayer") as GameObject);
        Head = root.transform.Find("Head");

       // m_CharacterMaterial.AddRange(GetRenderers(Head));

        currentHandPositions = new Vector3[iNumHands];
        currentHandRotations = new Quaternion[iNumHands];
        lastHandPositions = new Vector3[iNumHands];
        Hands = new GameObject[iNumHands];

        for (int i = 0; i < iNumHands; i++)
        {
            GameObject hand = Instantiate(Resources.Load<GameObject>("NetViveController") as GameObject);
            hand.transform.SetParent(transform);
            //m_CharacterMaterial.AddRange(GetRenderers(hand));
            Hands[i] = hand;
            currentHandPositions[i] = Hands[i].transform.position;
            lastHandPositions[i] = Hands[i].transform.position;
            currentHandRotations[i] = Hands[i].transform.rotation;
        }
    }

    public void SetPositions(Vector3 head, Quaternion rotation, Vector3[] hands, Quaternion[] rotations)
    {
        Head.transform.position = head;
        Head.transform.rotation = rotation;

        for (int i = 0; i < hands.Length; i++)
        {
            Hands[i].transform.position = hands[i];
            Hands[i].transform.rotation = rotations[i];
            currentHandPositions[i] = hands[i];
            currentHandRotations[i] = rotations[i];
        }
        
       // lastHeadPosition = Head.transform.position;
    }

    public void SetGunState(bool bGrabbed)
    {

    }

    public Vector3 GetHeadPosition()
    {
        return Head.transform.position;
    }

    public Quaternion GetHeadRotation()
    {
        return Head.transform.rotation;
    }

    public Vector3[] GetHandPositions()
    {
        for (int i = 0; i < Hands.Length; i++)
        {
            currentHandPositions[i] = Hands[i].transform.position;
        }

        return currentHandPositions;
    }

    public Quaternion[] GetHandRotations()
    {
        for (int i = 0; i < Hands.Length; i++)
        {
            currentHandRotations[i] = Hands[i].transform.rotation;
        }

        return currentHandRotations;
    }
}