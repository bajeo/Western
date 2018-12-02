using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public enum ConnectionStatus
{
    None,
    Connecting,
    Connected,
    FailedConnection,
    AwaitingReconnection,
    Reconnecting,
    Initialising,
    ServerInitialised
}

public class NetworkServerManager : NetworkManager
{
    public Action OnRestartHostSearch;
    public Action OnServerStarted;
    public static NetworkServerManager Singleton = null;
    [SerializeField]
    private ConnectionStatus m_ConnectionStatus = ConnectionStatus.None;

    private Dictionary<int, NetworkBase> m_ServerClientList = new Dictionary<int, NetworkBase>();

    public float m_SpawnRadius = 3.5f;

    // Use this for initialization
    void Start()
    {
        Singleton = this;        
    }

    private void OnDestroy()
    {
        Shutdown();
    }

    private void UpdateServerIP(string sServerIP)
    {
        networkAddress = sServerIP;
    }

    public void CanWeStartKillingYet()
    {
        if (m_ServerClientList.Count > 0 && true == AllPlayersReady())
        {
            Debug.Log("We can start killing now");
            ResetGame();

            StopAllCoroutines();
            StartCoroutine(DelayStart());
        }
    }

    private bool AllPlayersReady()
    {
        foreach (KeyValuePair<int, NetworkBase> kvp in m_ServerClientList)
        {
            NetworkClient client = kvp.Value as NetworkClient;

            if (null != client)
            {
                if(false == client.Ready)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void InitialiseServer()
    {
        Debug.Log("Initialising Connection as Server");
        m_ConnectionStatus = ConnectionStatus.Initialising;
        try
        {
            NetworkServer.Reset();
            StartHost();
        }
        catch (System.Exception e)
        {
            Debug.Log("Could not make server: " + e.Message);
        }
    }

    public void InitialiseClient()
    {
        Debug.Log("Initialising Connection as Client");
        m_ConnectionStatus = ConnectionStatus.Connecting;

        StartClient();
    }

    private void SyncAllClients()
    {
        foreach (KeyValuePair<int, NetworkBase> kvp in m_ServerClientList)
        {
            kvp.Value.Sync();
        }
    }

    private void Update()
    {
        //Add player
        if(Input.GetKeyDown(KeyCode.P))
        {
            var player = Instantiate(Resources.Load<GameObject>("VRClient"), Vector3.zero, Quaternion.identity) as GameObject;
            SetupPlayer(player, UnityEngine.Random.Range(0, 9999));
        }
    }

    #region ServerSide
    public override void OnStartServer()
    {
        base.OnStartServer();

        m_ConnectionStatus = ConnectionStatus.ServerInitialised;
        Debug.Log("Server Started!");

        OnServerStarted?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

        Debug.LogWarning("Server detected a client connection!!\n");
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        Debug.LogWarning("Server detected a client disconnection!!\n");

        //Client disconnected... remove from system
        if (true == m_ServerClientList.ContainsKey(conn.connectionId))
        {
            m_ServerClientList.Remove(conn.connectionId);
        }
        Debug.Log("Num connected clients: " + m_ServerClientList.Keys.Count);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        var stream = extraMessageReader.ReadMessage<StringMessage>();
        string clientType = stream.value;

        var player = Instantiate(Resources.Load<GameObject>(clientType), Vector3.zero, Quaternion.identity) as GameObject;

        //Setup network link on this loaded object to connected client
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

        SyncAllClients();

        SetupPlayer(player, conn.connectionId);

        Debug.Log("Num connected clients: " + m_ServerClientList.Keys.Count);
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(3.0f);

        StartGame();
    }

    private void SetupPlayer(GameObject player, int iD)
    {
        //We may need a reference to this for better handling
        NetworkClient netClient = player.GetComponent<NetworkClient>();

        netClient.ServerInit(iD);

        if (false == m_ServerClientList.ContainsKey(iD))
        {
            m_ServerClientList.Add(iD, netClient);
        }

        //Sync each clients position
        int iCount = m_ServerClientList.Keys.Count;
        int iNum = 0;

        foreach (KeyValuePair<int, NetworkBase> kvp in m_ServerClientList)
        {
            NetworkClient client = kvp.Value as NetworkClient;

            if (null != client)
            {
                float normalisedPositionInCircle = Mathf.InverseLerp(0, iCount, iNum);
                float angle = 360f * normalisedPositionInCircle;
                Vector3 position = Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * m_SpawnRadius);

                client.ServerSetPosition(position, Quaternion.Euler(0f, angle + 180f, 0f));
                iNum++;
            }
        }
    }

    public void StartGame()
    {
        if (m_ServerClientList.Count > 0)
        {
            foreach (KeyValuePair<int, NetworkBase> kvp in m_ServerClientList)
            {
                NetworkClient client = kvp.Value as NetworkClient;

                if (null != client)
                {
                    client.ServerStartGame();
                }
            }
        }
    }

    public void ResetGame()
    {
        if (m_ServerClientList.Count > 0)
        {
            foreach (KeyValuePair<int, NetworkBase> kvp in m_ServerClientList)
            {
                NetworkClient client = kvp.Value as NetworkClient;

                if (null != client)
                {
                    client.ServerResetGame();
                }
            }
        }
    }
    #endregion

    #region Client Side
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        Debug.Log("Client connected to server");
        m_ConnectionStatus = ConnectionStatus.Connected;

        //Inform server of our initial state
        string sMessage = "VRClient";
        StringMessage msg = new StringMessage(sMessage);

        ClientScene.AddPlayer(conn, 0, msg);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.LogWarning("Disconnected from server!!");
        m_ConnectionStatus = ConnectionStatus.AwaitingReconnection;

        StopClient();
        OnRestartHostSearch?.Invoke();
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);
        Debug.LogWarning("A network error occurred!!");
        m_ConnectionStatus = ConnectionStatus.FailedConnection;

        StopClient();
        OnRestartHostSearch?.Invoke();
    }
    #endregion
}