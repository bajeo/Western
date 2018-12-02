using UnityEngine;

public enum ImpactEffect
{
    Blood,
    Wood,
    Metal
}

public class Gun : MonoBehaviour
{
    
    [SerializeField]
    private Vector3 m_PositionOffset;
    [SerializeField]
    private Vector3 m_RotationOffset;

    [SerializeField]
    private Transform m_Barrel;
    [SerializeField]
    private Transform m_GunshotRoot;

    private ParticleSystem[] m_Gunshot;
    private Transform m_Transform;
    private RaycastHit m_HitInfo;

    [SerializeField]
    private GameObject m_TracerFX;

    private void Awake()
    {
        m_Transform = transform;

        m_Gunshot = m_GunshotRoot.GetComponentsInChildren<ParticleSystem>();
    }

    public void Fire(GunFiredDelegate callback)
    {
        long hitId = -1;
        
        if (GetRaycast(m_Barrel.position, m_Barrel.forward, ref m_HitInfo) == true)
        {
            Debug.Log($"Shot a {m_HitInfo.collider.name}\n", m_HitInfo.collider);
            var cowboy = m_HitInfo.collider.GetComponentInParent<Cowboy>();
            if (cowboy != null)
            {
                var networkClient = cowboy.GetComponentInParent<NetworkClient>();
                if (networkClient != null)
                {
                    if (networkClient.netId.IsEmpty() == false)
                    {
                        hitId = networkClient.netId.Value;
                        CreateEffect(m_HitInfo.point, m_HitInfo.normal, ImpactEffect.Blood);
                    }
                }                
            }
            else
            {
                if(m_HitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Breakable"))
                {
                    var breakable = m_HitInfo.collider.GetComponent<BreakableTarget>();

                    if(null != breakable)
                    {
                        breakable.Break(m_HitInfo.point);
                    }
                }

                CreateEffect(m_HitInfo.point, m_HitInfo.normal, ImpactEffect.Wood);
            }
        }

        FireGunShotEffect(m_Barrel.position, m_Barrel.forward);
        callback?.Invoke(m_Barrel.position, m_Barrel.forward, hitId);
    }

    private bool GetRaycast(Vector3 position, Vector3 direction, ref RaycastHit info)
    {
        var ray = new Ray(position, direction);
        if (Physics.Raycast(ray, out info, 500f) == true)
        {
            return true;
        }

        return false;
    }

    public void CreateEffect(Vector3 position, Vector3 direction, ImpactEffect effect)
    {
        GameObject obj = ParticleHolder.Singleton.GetParticle(effect);

        if(null != obj)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            GameObject.Instantiate(obj, position, rotation);
        }
    }

    public void FireGunShotEffect(Vector3 position, Vector3 direction)
    {
        Debug.DrawLine(position, position + direction * 500f, Color.cyan, 2f);

        //m_GunshotRoot.position = position;
        // m_GunshotRoot.forward = direction;

        CreateTracer(position, direction);

        foreach (var particleSystem in m_Gunshot)
        {
            particleSystem.Stop();
            particleSystem.Play();
        }

        Debug.Log("Do a gun shot effect\n", this);
    }

    public void CreateTracer(Vector3 position, Vector3 direction)
    {
        GameObject tracer = GameObject.Instantiate(m_TracerFX) as GameObject;

        if (null != tracer)
        {
            BulletTracer bt = tracer.GetComponent<BulletTracer>();

            if(null != bt)
            {
                bt.Init(position, direction);
            }
        }
    }

    public void OnGrab(Transform hand)
    {
        Reparent(hand, m_PositionOffset, m_RotationOffset);
    }

    public void OnDrop(Transform parent)
    {
        Reparent(parent, Vector3.zero, Vector3.zero);
    }

    private void Reparent(Transform hand, Vector3 position, Vector3 rotation)
    {
        m_Transform.SetParent(hand, false);
        m_Transform.localEulerAngles = rotation;
        m_Transform.localPosition = position;
        m_Transform.localScale = Vector3.one;
    }
}
