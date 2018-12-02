using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OverriddenNetworkDiscovery : NetworkDiscovery
{
    private bool m_bServerFound = false;
    private string ProjectID = "Feeling_Lucky_Punk";

    public void Start()
    {
        //Dont initialise until we have completely loaded
        Initialise();
    }

    private void Initialise()
    {
        var netServerManager = GetComponent<NetworkServerManager>();
        netServerManager.OnRestartHostSearch += OnClientDisconnected;
        netServerManager.OnServerStarted += OnServerCreated;

        broadcastData = ProjectID;
        broadcastSubVersion = 1;
        broadcastInterval = 500;

        Debug.Log("Listening for server");
        Initialize();
        StartAsClient();

        StartCoroutine(OnServerNotFound());
    }

    private IEnumerator OnServerNotFound()
    {
        yield return new WaitForSeconds(3.0f);
                
        if(false == m_bServerFound)
        {
            Debug.Log("Server not found... creating server");
            StopBroadcast();
            StartAsServer();
            NetworkServerManager.Singleton.InitialiseServer();
        }
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        if(true == m_bServerFound)
        {
            return;
        }

        Debug.Log("broadcast recieved: " + data);
        //Project IDs match?
        if (0 == ProjectID.CompareTo(data))
        {
            //Shutdown this listener
            m_bServerFound = true;
            StopBroadcast();

            string ipAddress = fromAddress.Replace("::ffff:", "");
            string[] ipaddressSplit = ipAddress.Split('%');

            if(ipaddressSplit.Length > 0)
            {
                ipAddress = ipaddressSplit[0];
            }

            Debug.Log("Found server... attempting connection to: " + ipAddress);
            //Connect to server
            NetworkServerManager.Singleton.networkAddress = ipAddress;
            NetworkServerManager.Singleton.InitialiseClient();
        }
    }

    private void OnClientDisconnected()
    {
        Debug.Log("Client disconnected: searching for server");
        m_bServerFound = false;
        //StopBroadcast();
        Initialize();
        StartAsClient();

        StartCoroutine(OnServerNotFound());
    }

    private void OnServerCreated()
    {
        Debug.Log("Server started initialising broadcast");
        try
        {
            StopBroadcast();
        }
        catch(System.Exception e)
        {
            Debug.Log("Broadcast already stopped: " + e.Message);
        }

        Initialize();
        StartAsServer();
    }
}