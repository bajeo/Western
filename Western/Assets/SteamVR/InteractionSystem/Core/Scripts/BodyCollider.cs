//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Collider dangling from the player's head
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(CapsuleCollider))]
    public class BodyCollider : MonoBehaviour
    {
        public Transform head;

        private CapsuleCollider capsuleCollider;

        //-------------------------------------------------
        void Awake()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }


        //-------------------------------------------------
        void FixedUpdate()
        {
            capsuleCollider.height = Mathf.Max(capsuleCollider.radius, transform.parent.position.y);
            transform.localPosition = Vector3.up * capsuleCollider.height * -0.5f;
        }
    }
}
