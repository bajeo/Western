using UnityEngine;

public class RotationFollow : MonoBehaviour
{
    public Transform m_TargetToFollow;
    private Transform m_Transform;

    private void Awake()
    {
        m_Transform = transform;
    }

    private void Update()
    {
        if (null != m_TargetToFollow)
        {
            m_Transform.rotation = m_TargetToFollow.rotation;
        }
    }
}
