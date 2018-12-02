using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleHolder : MonoBehaviour
{
    public static ParticleHolder Singleton;

    private Dictionary<ImpactEffect, GameObject> m_DictOfParticleSystems = new Dictionary<ImpactEffect, GameObject>();

	// Use this for initialization
	void Start ()
    {
        Singleton = this;

        m_DictOfParticleSystems.Add(ImpactEffect.Blood, Resources.Load<GameObject>("CFX2_Blood"));
        m_DictOfParticleSystems.Add(ImpactEffect.Wood, Resources.Load<GameObject>("CFX2_GroundWoodHit"));

    }
	
	public GameObject GetParticle(ImpactEffect sName)
    {
        if(true == m_DictOfParticleSystems.ContainsKey(sName))
        {
            return m_DictOfParticleSystems[sName];
        }

        return null;
    }
}
