using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class VrPlayer : MonoBehaviour
{
    [SerializeField]
    private GameObject m_CowboyPrefab;

    private Cowboy m_Cowboy;
    public Cowboy GetCowBoy => m_Cowboy;

    private void Start()
    {
        var clone = Instantiate(m_CowboyPrefab, transform);
        clone.transform.localRotation = Quaternion.identity;
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localScale = Vector3.one;

        m_Cowboy = clone.GetComponent<Cowboy>();
        m_Cowboy.InitialiseAsVr(GetComponent<Player>());
    }

    public void OnGrabDown(Hand hand)
    {
        if (m_Cowboy.IsHoveringHolster(hand.transform) == true)
        {
            SteamVR_Input.gun.ActivateSecondary();
            m_Cowboy.OnHolsterGrabDown(hand.transform);
        }
    }

    public void OnGrabUp(Hand hand)
    {
        if (m_Cowboy.HasGunInHand(hand.transform) == true)
        {
            SteamVR_Input.gun.Deactivate();
            m_Cowboy.OnHolsterGrabUp(hand.transform);
        }
    }

    public void OnGunFire(Hand hand)
    {
        if (m_Cowboy.HasGunInHand(hand.transform) == true)
        {
            m_Cowboy.OnGunFire(hand.transform);
        }
    }
}
