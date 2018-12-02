using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkBase : NetworkBehaviour
{
    public virtual void ServerInit(int NetID) { }
    public virtual void Sync() { }

}
public class NetworkClient : NetworkBase
{
    //private Color[] PlayerColors = new Color[] { Color.cyan, Color.green, Color.white, Color.red, Color.yellow, Color.magenta };
    private Vector3 headPosition;
    private Quaternion headRotation;
    private Vector3[] handPositions = new Vector3[2];
    private Quaternion[] handRotations = new Quaternion[2];

    private Cowboy m_Cowboy;

    private bool m_bIsReadyToStart = false;
    public bool Ready => m_bIsReadyToStart;

    // Use this for initialization
    void Start()
    {
        if (true == isLocalPlayer)
        {
            StartLocalPlayer();
        }
        else
        {
            StartNetworkPlayer();
        }
    }

    public override void ServerInit(int NetID)
    {

    }

    public void ServerSetPosition(Vector3 position, Quaternion rotation)
    {
        ClientSetPosition(position, rotation);
        if (true == isServer)
        {
            RpcSetPosition(position, rotation);
        }
    }

    private void ClientSetPosition(Vector3 position, Quaternion rotation)
    {
        if (true == isLocalPlayer)
        {
            VrPlayer vrplayer = FindObjectOfType<VrPlayer>();
            vrplayer.transform.position = position;
            vrplayer.transform.rotation = rotation;
        }
        else
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }

    private void StartLocalPlayer()
    {
        VrPlayer vrplayer = FindObjectOfType<VrPlayer>();
        m_Cowboy = vrplayer.GetCowBoy;

        CmdUpdateSeed(m_Cowboy.GetSeed);

        m_Cowboy.OnGunStateChange += OnPlayerGrabGun;
        m_Cowboy.OnGunFired += OnPlayerFiredGun;
    }

    private void StartNetworkPlayer()
    {
        GameObject cowboy = GameObject.Instantiate(Resources.Load<GameObject>("Cowboy"));

        //Create representation of the player connected
        m_Cowboy = cowboy.GetComponent<Cowboy>();
        m_Cowboy.transform.SetParent(transform);
        m_Cowboy.transform.localPosition = Vector3.zero;
        m_Cowboy.transform.localRotation = Quaternion.identity;
    }

    private void OnPlayerShoot(Vector3 position, Vector3 direction)
    {
        if (true == isLocalPlayer)
        {
            //Inform server we shot

        }
    }

    private void SetPlayerScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

    private void Update()
    {
        if (true == isLocalPlayer)
        {
            if (null != m_Cowboy)
            {
                m_Cowboy.GetData(ref headPosition, ref headRotation, ref handPositions, ref handRotations);
                // PlayerParts.Singleton.GetHandData(ref headPosition, ref headRotation, ref handPositions, ref handRotations);
                CmdUpdatePosition(headPosition, headRotation, handPositions, handRotations);

                Vector3 bodyPos = headPosition;
                bodyPos.y = 0f;
                Vector3 rootPos = m_Cowboy.transform.position;
                rootPos.y = 0f;
                bool canStart = (Vector3.Distance(bodyPos, rootPos) < 0.5f);
                if(m_bIsReadyToStart != canStart)
                {
                    CmdReadyToStart(canStart);
                    m_bIsReadyToStart = canStart;
                }
            }
        }
    }

    public override void Sync()
    {
        if (true == isServer)
        {
            RpcUpdateSeed(m_Cowboy.GetSeed);

            //Sync position data
            RpcUpdatePosition(headPosition, headRotation, handPositions, handRotations);
        }
    }

    private void OnPlayerGrabGun(bool bState, int iHandID)
    {
        if (true == isLocalPlayer)
        {
            //Inform server
            CmdGrabbedGun(bState, iHandID);
        }
    }

    private void OnPlayerFiredGun(Vector3 position, Vector3 direction, long hitId)
    {
        CmdGunFired(position, direction, hitId);
    }

    private void CreateShootEffect(Vector3 position, Vector3 direction)
    {
        //Create nice effect here

    }

    private void ServerCalculatePlayerHit(Vector3 position, Vector3 direction)
    {
        if (true == isServer)
        {
            //Ray cast and determine if player was hit
            CreateShootEffect(position, direction);

            //Inform clients of shoot position and direction
            RpcShoot(position, direction);
        }
    }

    public void ServerStartGame()
    {
        if (true == isServer)
        {
            RpcStartGame();
        }
    }

    public void ServerResetGame()
    {
        if (true == isServer)
        {
            RpcResetGame();
        }
    }

    #region networking

    [Command(channel = 1)]
    private void CmdReadyToStart(bool bReady)
    {
        if(false == isLocalPlayer)
        {
            m_bIsReadyToStart = bReady;
        }

        NetworkServerManager.Singleton.CanWeStartKillingYet();
    }

    [ClientRpc(channel = 1)]
    private void RpcStartGame()
    {
        Core.Instance.GameStarted();
    }

    [ClientRpc(channel = 1)]
    private void RpcResetGame()
    {
        Core.Instance.PlayerJoined();
    }

    [ClientRpc(channel = 1)]
    private void RpcSetPosition(Vector3 position, Quaternion rotation)
    {
        ClientSetPosition(position, rotation);
    }

    [Command(channel = 1)]
    private void CmdUpdatePosition(Vector3 head, Quaternion rotation, Vector3[] hands, Quaternion[] rotations)
    {
        if (false == isLocalPlayer)
        {
            headPosition = head;
            headRotation = rotation;
            handPositions = hands;
            handRotations = rotations;
            m_Cowboy.SetPositions(head, rotation, hands, rotations);
        }

        RpcUpdatePosition(head, rotation, hands, rotations);
    }

    [ClientRpc(channel = 1)]
    private void RpcUpdatePosition(Vector3 head, Quaternion rotation, Vector3[] hands, Quaternion[] rotations)
    {
        if (false == isLocalPlayer)
        {
            if (null != m_Cowboy)
            {
                //Update positions etc...
                m_Cowboy.SetPositions(head, rotation, hands, rotations);
            }
        }
    }

    [Command(channel = 1)]
    private void CmdUpdateSeed(int iSeed)
    {
        m_Cowboy.GetSeed = iSeed;
        if (false == isLocalPlayer)
        {
            m_Cowboy.InitialiseAsNetwork(iSeed);
        }

        RpcUpdateSeed(iSeed);
    }

    [ClientRpc(channel = 1)]
    private void RpcUpdateSeed(int iSeed)
    {
        if (false == isLocalPlayer)
        {
            m_Cowboy.InitialiseAsNetwork(iSeed);
        }
    }

    [Command(channel = 1)]
    private void CmdGrabbedGun(bool bGunGrabbed, int iHandID)
    {
        Debug.Log("Server: " + bGunGrabbed + " id: " + iHandID);
        if(false == isLocalPlayer)
        {
            m_Cowboy.SetGunState(bGunGrabbed, iHandID);
        }

        RpcGrabbedGun(bGunGrabbed, iHandID);
    }

    [ClientRpc(channel = 1)]
    private void RpcGrabbedGun(bool bGunGrabbed, int iHandID)
    {
        if (false == isLocalPlayer)
        {
            Debug.Log("Client: " + bGunGrabbed + " id: " + iHandID);
            m_Cowboy.SetGunState(bGunGrabbed, iHandID);
        }
    }

    [Command(channel = 1)]
    private void CmdShoot(Vector3 position, Vector3 direction)
    {
        //Run logic... using players gun position and direction... adjust with accuracy
        ServerCalculatePlayerHit(position, direction);
    }

    [ClientRpc(channel = 1)]
    private void RpcShoot(Vector3 position, Vector3 direction)
    {
        CreateShootEffect(position, direction);
    }


    [Command(channel = 1)]
    private void CmdGunFired(Vector3 position, Vector3 direction, long hitId)
    {
        RpcGunFired(position, direction, hitId);

        if (hitId > 0)
        {
            RpcPlayerShot(direction, (uint)hitId);
        }
    }

    [ClientRpc(channel = 1)]
    private void RpcGunFired(Vector3 position, Vector3 direction, long hitId)
    {
        if (false == isLocalPlayer)
        {
            // do visual
            m_Cowboy.FireGunShotEffect(position, direction);
            Debug.Log($"There was a gunshot. {hitId} was shot\n");
        }
    }

    [ClientRpc(channel = 1)]
    private void RpcPlayerShot(Vector3 direction, uint hitId)
    {
        if (netId.Value == hitId)
        {
            // this client died
            Debug.Log("You shot me\n");
            m_Cowboy.KillPlayer(direction);
        }
    }
    #endregion
}