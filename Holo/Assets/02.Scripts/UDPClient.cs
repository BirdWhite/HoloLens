using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using System.Text;
using Microsoft.MixedReality.Toolkit.Diagnostics;

public class UDPClient : MonoBehaviour
{
    UdpClient srv;
    IPEndPoint endPt;
    string message;

    public string IP = "127.0.0.1";
    public int Port = 50001;

    public class MyUDPEvent : UnityEvent<string> { }
    public MyUDPEvent OnReceiveMessage = new MyUDPEvent();

    // Start is called before the first frame update
    void Start()
    {
        srv = new UdpClient(Port);
        srv.BeginReceive(OnReceive, null);
        OnReceiveMessage.AddListener(receiveMsg);
    }

    void OnReceive(IAsyncResult ar)
    {
        try
        {
            IPEndPoint ipEndPoint = null;
            byte[] data = srv.EndReceive(ar, ref ipEndPoint);
            message = System.Text.Encoding.UTF8.GetString(data);
            OnReceiveMessage.Invoke(message);
        }
        catch (SocketException e) { }

        srv.BeginReceive(OnReceive, null);
    }

    void receiveMsg (string msg)
    {
        Debug.Log(msg);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        OnReceiveMessage.RemoveAllListeners();
        if (srv != null)
            srv.Close();
        srv = null;
    }
}
