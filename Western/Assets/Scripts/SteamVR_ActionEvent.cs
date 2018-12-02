using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using Valve.VR.InteractionSystem;

[System.Serializable]
public class HandEvent : UnityEvent<Hand> { }

public class SteamVR_ActionEvent : MonoBehaviour
{
    [SerializeField]
    private SteamVR_Action_Boolean m_Action;
    [SerializeField]
    private Hand m_Hand;

    [SerializeField]
    private HandEvent m_OnDown = new HandEvent();
    [SerializeField]
    private HandEvent m_OnHold = new HandEvent();
    [SerializeField]
    private HandEvent m_OnUp = new HandEvent();

    private void OnEnable()
    {
        m_Action.AddOnChangeListener(OnActionChange, m_Hand.handType);
    }

    private void OnDisable()
    {
        m_Action.RemoveOnChangeListener(OnActionChange, m_Hand.handType);
    }

    private void OnActionChange(SteamVR_Action_In action)
    {
        if (m_Action.GetStateDown(m_Hand.handType) == true)
        {
            m_OnDown.Invoke(m_Hand);
        }
        else if (m_Action.GetStateUp(m_Hand.handType) == true)
        {
            m_OnUp.Invoke(m_Hand);
        }
        else if (m_Action.GetState(m_Hand.handType) == true)
        {
            m_OnHold.Invoke(m_Hand);
        }
    }
}
