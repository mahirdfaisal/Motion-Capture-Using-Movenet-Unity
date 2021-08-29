using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System;



public class DataModel {
    public double x { set; get; }
    public double y { set; get; }
    public double c { set; get; }

    public override string ToString()
    {
        return "X: " + x + " Y: " + y;
    }
}
public class Server : MonoBehaviour
{
    Thread mThread;
    public string connectionIP = "10.10.10.27";
    public int connectionPort = 25001;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    bool running;

    string locationRawData = "[0.12796443700790405, 0.4780223071575165, 0.39164668321609497] [0.11339150369167328, 0.4921885132789612, 0.6720894575119019] [0.1140444427728653, 0.4628220200538635, 0.6235530376434326] [0.12344381213188171, 0.5129931569099426, 0.6968255639076233] [0.12569940090179443, 0.44120511412620544, 0.6167572140693665] [0.21283303201198578, 0.5692193508148193, 0.8574579358100891] [0.2119312733411789, 0.3883322477340698, 0.8632873296737671] [0.34900590777397156, 0.607222318649292, 0.6717122197151184] [0.3476785123348236, 0.35717687010765076, 0.7525168061256409] [0.4528948664665222, 0.624661386013031, 0.641278088092804] [0.4636473059654236, 0.34296971559524536, 0.7848643064498901] [0.4848814010620117, 0.5339097380638123, 0.7894537448883057] [0.4835352599620819, 0.4268706440925598, 0.8636891841888428] [0.6920539736747742, 0.5487847328186035, 0.8724942207336426] [0.6915709376335144, 0.41103336215019226, 0.7130178809165955] [0.8701896667480469, 0.5571780204772949, 0.8650961518287659] [0.8707324266433716, 0.4061724543571472, 0.8933596611022949]";

    public GameObject[] Landmarks = new GameObject[17];

    string ProcessedLocationData;

    // Position is the data being received in this example
    Vector3 position = Vector3.zero;

    string CurrentPositionRawData = "[0.12796443700790405, 0.4780223071575165, 0.39164668321609497] [0.11339150369167328, 0.4921885132789612, 0.6720894575119019] [0.1140444427728653, 0.4628220200538635, 0.6235530376434326] [0.12344381213188171, 0.5129931569099426, 0.6968255639076233] [0.12569940090179443, 0.44120511412620544, 0.6167572140693665] [0.21283303201198578, 0.5692193508148193, 0.8574579358100891] [0.2119312733411789, 0.3883322477340698, 0.8632873296737671] [0.34900590777397156, 0.607222318649292, 0.6717122197151184] [0.3476785123348236, 0.35717687010765076, 0.7525168061256409] [0.4528948664665222, 0.624661386013031, 0.641278088092804] [0.4636473059654236, 0.34296971559524536, 0.7848643064498901] [0.4848814010620117, 0.5339097380638123, 0.7894537448883057] [0.4835352599620819, 0.4268706440925598, 0.8636891841888428] [0.6920539736747742, 0.5487847328186035, 0.8724942207336426] [0.6915709376335144, 0.41103336215019226, 0.7130178809165955] [0.8701896667480469, 0.5571780204772949, 0.8650961518287659] [0.8707324266433716, 0.4061724543571472, 0.8933596611022949]";

    private void Update()
    {
        
        ProcessRawData(CurrentPositionRawData);
    }

    private void Start()
    {
        // Receive on a separate thread so Unity doesn't freeze waiting for data
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
    }

    public static string GetLocalIPAddress()
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

    void GetInfo()
    {
        localAdd = IPAddress.Parse(connectionIP);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();

        client = listener.AcceptTcpClient();


        running = true;
        while (running)
        {
            Connection();
        }
        listener.Stop();
    }

    void Connection()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
        // Passing data as strings, not ideal but easy to use
        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (dataReceived != null)
        {
            if (dataReceived == "stop")
            {
                // Can send a string "stop" to kill the connection
                running = false;
            }
            else
            {
                /*// Convert the received string of data to the format we are using
                position = 10f * StringToVector3(dataReceived);*/
                
                locationRawData = dataReceived;
                //print(locationRawData);
                CurrentPositionRawData = dataReceived;
                nwStream.Write(buffer, 0, bytesRead);
            }
        }
    }

    public void ProcessRawData(string loc) {

        ProcessedLocationData = loc;
        //print(loc);

        //splitArray[0];
        //Removing First [
        ProcessedLocationData = ProcessedLocationData.Substring(1);
        ProcessedLocationData = ProcessedLocationData.Remove(ProcessedLocationData.Length - 1, 1);
        ProcessedLocationData = ProcessedLocationData.Replace("][", ",");
        char[] delimiterChars = { ' ', ',' };
        var points = ProcessedLocationData.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
        DataModel[] vectors = new DataModel[17];
        for (int i = 0; i < points.Length; i += 3)
        {
            DataModel pointData = new DataModel();
            pointData.x = double.Parse(points[i]);
            pointData.y = double.Parse(points[i + 1]);
            pointData.y = double.Parse(points[i + 2]);

            if (i < 3)
            {
                vectors[i] = pointData;
            }
            else
            {
                int index = (int)(i / 3);
                vectors[index] = pointData;
            }
        }

        for (int i = 0; i < 17; i++)
        {
            if (float.Parse(vectors[i].y.ToString()) < 0.8f)
            {
                Landmarks[i].SetActive(false);
                break;
            }
            else if (!Landmarks[i].activeSelf) {
                Landmarks[i].SetActive(true);
            }

            Landmarks[i].transform.localPosition = new Vector3(float.Parse(vectors[i].x.ToString()), float.Parse(vectors[i].y.ToString()), 0);
            
            //print(Landmarks[i].transform.localPosition);
        }
    }

    // Use-case specific function, need to re-write this to interpret whatever data is being sent
    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // Split the elements into an array
        string[] sArray = sVector.Split(',');

        // Store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
}