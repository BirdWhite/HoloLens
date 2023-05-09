using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine.tvOS;

public class UDPServer : MonoBehaviour
{
    UdpClient srv;
    IPEndPoint endPt;

    public string IP = "127.0.0.1";
    public int Port = 50001;
    

    void Start()
    {
        try
        {
            srv = new UdpClient();
            endPt = new IPEndPoint(IPAddress.Parse(IP), Port);
        }
        catch(SocketException e)
        {
            Debug.LogException(e);
        }
        Send("안녕");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Send(string msg = "receive from Unity")
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
        srv.Send(data, data.Length, endPt);
        Debug.Log("[Send] " + endPt.ToString() +"로 " + data.Length + " 바이트 송신");
    }

    public void ShutDown()
    {
        if (srv != null)
            srv.Close();
        srv = null;
    }

}
