using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public delegate void GunFiredDelegate(Vector3 position, Vector3 direction, long hitId);

public class Cowboy : MonoBehaviour
{
    public event GunFiredDelegate OnGunFired;
    public event Action<bool, int> OnGunStateChange;

    private List<string> HairVariants = new List<string>() { "FineGent", "HandleBar", "Soul" };
    private List<string> HatVariants = new List<string>() { "Sombero", "Hat", "TopHat" };
    private List<string> BodyVariants = new List<string>() { "Badge", "Poncho" };

    private List<GameObject> Decorations = new List<GameObject>();

    [SerializeField]
    private List<GameObject> m_HiddenObjects;

    [SerializeField]
    private Transform m_Head;
    [SerializeField]
    private Transform m_Body;
    [SerializeField]
    private Transform m_LeftHand;
    [SerializeField]
    private Transform m_RightHand;

    [SerializeField]
    private Holster m_Holster;

    private Player m_SteamPlayer;

    public bool CanGrabGun = true;

    public int GetSeed
    {
        get { return iRandomSeed; }
        set { iRandomSeed = value; }
    }
    private int iRandomSeed = 0;

    public void InitialiseAsVr(Player player)
    {
        SetFollow(m_Head, player.hmdTransform);
        SetupVrHand(m_LeftHand, SteamVR_Input_Sources.LeftHand);
        SetupVrHand(m_RightHand, SteamVR_Input_Sources.RightHand);

        var bodyCollider = m_Body.GetComponentInChildren<BodyCollider>();
        bodyCollider.gameObject.SetActive(false);

        iRandomSeed = UnityEngine.Random.Range(0, 9999);
        DecoratePlayer(iRandomSeed);

        foreach (var obj in m_HiddenObjects)
        {
            SetLayerRecursively(obj, LayerMask.NameToLayer("Water"));
        }

        m_SteamPlayer = player;
    }

    public void InitialiseAsNetwork(int iSeed)
    {
        DecoratePlayer(iSeed);
    }

    public void DecoratePlayer(int RandomSeed)
    {
        BinDecorations();

        UnityEngine.Random.InitState(RandomSeed);
        Transform hatDummy = m_Head.Find("hatdummy");

        AddAttachment(HatVariants[UnityEngine.Random.Range(0, HatVariants.Count)], hatDummy);

        Transform hairDummy = m_Head.Find("mustachdummy");
        AddAttachment(HairVariants[UnityEngine.Random.Range(0, HairVariants.Count)], hairDummy);

        Transform body = m_Body.Find("Model/Body");
        AddAttachment(BodyVariants[UnityEngine.Random.Range(0, BodyVariants.Count)], body);
    }

    private void BinDecorations()
    {
        foreach (GameObject obj in Decorations)
        {
            Destroy(obj);
        }

        Decorations.Clear();
    }

    private void AddAttachment(string name, Transform parent)
    {
        GameObject attachment = GameObject.Instantiate(Resources.Load<GameObject>(name) as GameObject);
        attachment.transform.SetParent(parent);
        attachment.transform.localPosition = Vector3.zero;
        attachment.transform.localRotation = Quaternion.identity;

        Decorations.Add(attachment);
    }

    public void GetData(ref Vector3 headPos, ref Quaternion headRot,
                        ref Vector3[] positions, ref Quaternion[] rotations)
    {
        headPos = m_Head.position;
        headRot = m_Head.rotation;

        positions[0] = m_RightHand.position;
        rotations[0] = m_RightHand.rotation;
        positions[1] = m_LeftHand.position;
        rotations[1] = m_LeftHand.rotation;
    }

    public void SetPositions(Vector3 headPos, Quaternion rotation,
                            Vector3[] handPositions, Quaternion[] handRotations)
    {
        m_Head.position = headPos;
        m_Head.rotation = rotation;

        m_RightHand.position = handPositions[0];
        m_RightHand.rotation = handRotations[0];
        m_LeftHand.position = handPositions[1];
        m_LeftHand.rotation = handRotations[1];
    }

    public bool HasGunInHand(Transform hand)
    {
        return m_Holster.HasGunInHand(GetLocalHand(hand));
    }

    public void OnGunFire(Transform hand)
    {
        m_Holster.FireGun(GetLocalHand(hand), OnGunFired);
    }

    public bool IsHoveringHolster(Transform hand)
    {
        var hoveredHand = m_Holster.Interactable.hoveredHand;
        return hoveredHand != null && hoveredHand.transform == hand;
    }

    public void OnHolsterGrabDown(Transform hand)
    {
        if (true == CanGrabGun)
        {
            m_Holster.OnGrabDown(GetLocalHand(hand));
            OnGunStateChange?.Invoke(true, (hand == m_RightHand) ? 0 : 1);
        }
    }

    public void OnHolsterGrabUp(Transform hand)
    {
        if (true == CanGrabGun)
        {
            m_Holster.OnGrabUp(GetLocalHand(hand));
            OnGunStateChange?.Invoke(false, (hand == m_RightHand) ? 0 : 1);
        }
    }

    public void FireGunShotEffect(Vector3 position, Vector3 direction)
    {
        m_Holster.FireGunShotEffect(position, direction);
    }

    public void KillPlayer(Vector3 direction)
    {
        //Launch player in this direction!!

    }

    public void SetGunState(bool bGrabbed, int iHandID)
    {
        var hand = (iHandID == 0) ? m_RightHand : m_LeftHand;
        if (true == bGrabbed)
        {
            m_Holster.OnGrabDown(hand);
        }
        else
        {
            m_Holster.OnGrabUp(hand);
        }
    }

    private static void SetupVrHand(Transform hand, SteamVR_Input_Sources handType)
    {
        hand.GetComponentInChildren<SteamVR_Behaviour_Skeleton>().enabled = true;
        hand.transform.localRotation = Quaternion.identity;
        hand.transform.localPosition = Vector3.zero;
        hand.transform.localScale = Vector3.one;
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;

        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private Transform GetLocalHand(Transform hand)
    {
        if (hand == m_SteamPlayer.leftHand.transform)
        {
            return m_LeftHand;
        }
        else if (hand == m_SteamPlayer.rightHand.transform)
        {
            return m_RightHand;
        }
        return hand;
    }

    private static void SetFollow(Transform src, Transform dst)
    {
        src.GetComponent<PositionFollow>().m_TargetToFollow = dst;
        src.GetComponent<RotationFollow>().m_TargetToFollow = dst;
    }
}
