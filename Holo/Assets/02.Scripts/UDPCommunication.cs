using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Runtime.CompilerServices;

#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
#else
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
#endif

[System.Serializable]
public class UDPMessageEvent : UnityEvent<string, string, byte[]>
{

}

public class UDPCommunication : Singleton<UDPCommunication>
{
    [Tooltip("port to listen for incoming data")]
    public string internalPort = "50930";

    [Tooltip("IP-Address for sending")]
    public string externalIP = "192.168.112.1";

    [Tooltip("Port for sending")]
    public string externalPort = "50931";

    [Tooltip("Send a message at Startup")]
    public bool sendPingAtStart = false;

    [Tooltip("Conten of Ping")]
    public string PingMessage = "hello";

    [Tooltip("Function to invoke at incoming packet")]
    public UDPMessageEvent udpEvent = null;

    public ConsoleManager cM = null;

    private readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    public Transform[] targets;


#if !UNITY_EDITOR

    //we've got a message (data[]) from (host) in case of not assigned an event
    void UDPMessageReceived(string host, string port, byte[] data)
    {
        Debug.Log("GOT MESSAGE FROM: " + host + " on port " + port + " " + data.Length.ToString() + " bytes ");
        cM.UnityLog("GOT MESSAGE FROM: " + host + " on port " + port + " " + data.Length.ToString() + " bytes ");
        ReceiveTartgetsTransforrm(data);
    }

    //Send an UDP-Packet
    public async void SendUDPMessage(string HostIP, string HostPort, byte[] data)
    {
        await _SendUDPMessage(HostIP, HostPort, data);
    }



    DatagramSocket socket;

    async void Start()
    {
        
        if (udpEvent == null)
        {
            udpEvent = new UDPMessageEvent();
            udpEvent.AddListener(UDPMessageReceived);
        }


        Debug.Log("Waiting for a connection...");
        cM.UnityLog("Waiting for a connection...");

        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;

        HostName IP = null;
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            IP = Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
            .SingleOrDefault(
                hn =>
                    hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                    == icp.NetworkAdapter.NetworkAdapterId);
            Debug.Log(IP.ToString());
            cM.UnityLog("HoloLens2 On " + IP.ToString());
            await socket.BindEndpointAsync(IP, internalPort);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            cM.UnityLog(e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            cM.UnityLog(SocketError.GetStatus(e.HResult).ToString());
            return;
        }

        if(sendPingAtStart)
            SendUDPMessage(externalIP, externalPort, Encoding.UTF8.GetBytes(PingMessage));

    }




    private async System.Threading.Tasks.Task _SendUDPMessage(string externalIP, string externalPort, byte[] data)
    {
        using (var stream = await socket.GetOutputStreamAsync(new Windows.Networking.HostName(externalIP), externalPort))
        {
            using (var writer = new Windows.Storage.Streams.DataWriter(stream))
            {
                writer.WriteBytes(data);
                await writer.StoreAsync();

            }
        }
    }


#else
    // to make Unity-Editor happy :-)

    UdpClient srv;
    IPEndPoint endPt;

    void Start()
    {
        
        try
        {
            srv = new UdpClient();
            endPt = new IPEndPoint(IPAddress.Parse(externalIP), int.Parse(internalPort));
            cM.UnityLog("Unity Editor On " + GetLocalIPAddress());
        }
        catch (SocketException e)
        {
            Debug.LogException(e);
        }
    }

    public void Send(string msg = "receive from Unity")
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
        srv.Send(data, data.Length, endPt);
        Debug.Log("[Send] " + endPt.ToString() + "로 " + data.Length + " 바이트 송신");
    }

    private void OnApplicationQuit()
    {
        if (srv != null)
            srv.Close();
        srv = null;
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    public void SendUDPMessage(string HostIP, string HostPort, byte[] data)
    {

    }

    private void FixedUpdate()
    {
        SendTargetsTransform();
    }

    public void SendTargetsTransform()
    {
        int i = 0;
        string result = "";
        foreach (Transform trans in targets)
        {
            result += Vec3ToStr(trans.position) + "&" + QuatToStr(trans.rotation) + "%";
            i++;
        }
        result = result.Substring(0, result.Length - 1);
        Send(result);
        Debug.Log(result);
    }

#endif


    static MemoryStream ToMemoryStream(Stream input)
    {
        try
        {                                         // Read and write in
            byte[] block = new byte[0x1000];       // blocks of 4K.
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                int bytesRead = input.Read(block, 0, block.Length);
                if (bytesRead == 0) return ms;
                ms.Write(block, 0, bytesRead);
            }
        }
        finally { }
    }

    // Update is called once per frame
    void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }



#if !UNITY_EDITOR
    private void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
         Debug.Log("GOT MESSAGE FROM: " + args.RemoteAddress.DisplayName);
         cM.UnityLog("GOT MESSAGE FROM: " + args.RemoteAddress.DisplayName);
        //Read the message that was received from the UDP  client.
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        MemoryStream ms = ToMemoryStream(streamIn);
        byte[] msgData = ms.ToArray();


        if (ExecuteOnMainThread.Count == 0)
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                Debug.Log("ENQEUED ");
                cM.UnityLog("ENQEUED ");
                if (udpEvent != null)
                    udpEvent.Invoke(args.RemoteAddress.DisplayName, internalPort, msgData);
            });
        }
    }


#endif

    public void ReceiveTartgetsTransforrm(byte[] data)
    {
        string[] transtr = Encoding.UTF8.GetString(data).Split('%');
        Debug.Log(Encoding.UTF8.GetString(data));
        int i = 0;
        foreach(Transform trans in targets)
        {
            trans.position = StrToVec3(transtr[i].Split('&')[0]);
            trans.rotation = StrToQuat(transtr[i].Split('&')[1]);
            i++;
        }
    }

    public bool CompareTransform(Transform a, Transform b)
    {
        if(a.position.Equals(b.position) || a.rotation.Equals(b.rotation))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CompareTransArray(Transform[] a, Transform[] b)
    {
        if (a.Length != b.Length)
            return false;

        for(int i = 0; i < a.Length; i++)
        {
            if (!CompareTransform(a[i], b[i]))
                return false;
        }

        return true;
    }

    public string Vec3ToStr(Vector3 pos)
    {
        return pos.x.ToString("0.000") + "," + pos.y.ToString("0.000") + "," + pos.z.ToString("0.000");
    }

    public string QuatToStr(Quaternion quat)
    {
        return quat.x.ToString("0.000") + "," + quat.y.ToString("0.000") + "," + quat.z.ToString("0.000") + "," + quat.w.ToString("0.000");
    }

    public Vector3 StrToVec3(string str)
    {
        string[] spl = str.Split(',');
        Vector3 pos = new Vector3(float.Parse(spl[0]), float.Parse(spl[1]), float.Parse(spl[2]));
        return pos;
    }

    public Quaternion StrToQuat(string str)
    {
        string[] spl = str.Split(',');
        Quaternion quat = new Quaternion(float.Parse(spl[0]), float.Parse(spl[1]), float.Parse(spl[2]), float.Parse(spl[3]));
        return quat;
    }
}