using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Sockets.Socket;


public class SocketServerImage : MonoBehaviour
{
    private Texture2D tex;
    private RawImage rawImage;
    private byte[] imageData;
    // Start is called before the first frame update
    async void Start()
    {
        tex = new Texture2D(1, 1);
        rawImage = GetComponent<RawImage>();
        await Task.Run(() => StartServer());
    }

    // Update is called once per frame
    void Update()
    {
        tex.LoadImage(imageData);
        rawImage.texture = tex;
    }

    private void StartServer()
    {
        var ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        var ipAddress = ipHostEntry.AddressList[0];
        var localEndPoint = new IPEndPoint(ipAddress, 9998);

        var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            while (true)
            {
                print("Waiting connection ... ");
                var clientSocket = listener.Accept();

                var bytes = new byte[1024];
                string data = null;
                
                var numByte = clientSocket.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, numByte);
                print("Text received - > { " + data +" }");
                var fileSize = int.Parse(data);
                var message = Encoding.ASCII.GetBytes("File size received: " + fileSize);
                clientSocket.Send(message);

                bytes = new byte[fileSize];
                numByte = clientSocket.Receive(bytes);

                imageData = bytes;

                data = null;
                data += Encoding.ASCII.GetString(bytes, 0, numByte);
                print("Text received - > { " + data +" }");
                
                message = Encoding.ASCII.GetBytes("File data received: " + numByte);
                clientSocket.Send(message);
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            
        }
        catch (Exception exception)
        {
            print(exception.ToString());
        }
    }
}
