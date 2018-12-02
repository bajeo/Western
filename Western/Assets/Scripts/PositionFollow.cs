using UnityEngine;

public class PositionFollow : MonoBehaviour
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
            m_Transform.position = m_TargetToFollow.position;
        }
    }
}
