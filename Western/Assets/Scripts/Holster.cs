using System;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Holster : MonoBehaviour
{
    public Interactable Interactable => m_Interactable;

    [SerializeField]
    private AudioSource m_SfxGrab;
    [SerializeField]
    private Gun m_Gun;
    private Transform m_GunDummy;

    private Interactable m_Interactable;
    private Transform m_GunHand;

    private void Awake()
    {
        m_Interactable = m_Gun.GetComponent<Interactable>();
        m_GunDummy = m_Gun.transform.parent;
    }

    public bool HasGunInHand(Transform hand)
    {
        return m_GunHand == hand;
    }

    public void FireGun(Transform hand, GunFiredDelegate callback)
    {
        m_Gun.Fire(callback);
    }

    public void FireGunShotEffect(Vector3 position, Vector3 direction)
    {
        m_Gun.FireGunShotEffect(position, direction);
    }

    public void OnGrabDown(Transform hand)
    {
        m_Interactable.highlightOnHover = false;
        ToggleHandModel(hand, false);

        m_Gun.OnGrab(hand);
        m_SfxGrab.Play();

        m_GunHand = hand;
    }

    public void OnGrabUp(Transform hand)
    {
        m_Interactable.highlightOnHover = true;
        ToggleHandModel(hand, true);

        m_Gun.OnDrop(m_GunDummy);

        m_GunHand = null;
    }

    private static void ToggleHandModel(Transform hand, bool state)
    {
        var skeleton = hand.Find("Model");
        if (skeleton != null)
        {
            skeleton.gameObject.SetActive(state);
        }
    }
}
