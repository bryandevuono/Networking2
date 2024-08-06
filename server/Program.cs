using System;
using System.Data.SqlTypes;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using MessageNS;


// Do not modify this class
class Program
{
    static void Main(string[] args)
    {
        ServerUDP sUDP = new ServerUDP();
        sUDP.start();
    }
}

class ServerUDP
{

    //TODO: implement all necessary logic to create sockets and handle incoming messages
    // Do not put all the logic into one method. Create multiple methods to handle different tasks.
    private UdpClient server;
    private IPEndPoint clientEndpoint;

    public ServerUDP()
    {
        server = new UdpClient(9000); // Luisteren op poort 9000
        clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
    }

    public void start()
    {
        try
        {
            while (true)
            {
                ReceiveMessage();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            server.Close();
        }
    }

    //TODO: create all needed objects for your sockets 

    //TODO: keep receiving messages from clients
    // you can call a dedicated method to handle each received type of messages

    //TODO: [Receive Hello]
    private void ReceiveMessage()
    {
        try
        {
            var data = server.Receive(ref clientEndpoint);
            string message = Encoding.UTF8.GetString(data);
            Console.WriteLine($"Received: {message}");

            if (message == "HELLO")
            {
                SendWelcome();
            }
            else
            {
                Console.WriteLine("Unexpected message received");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving message: {ex.Message}");
        }
    }

    //TODO: [Send Welcome]
    private void SendWelcome()
    {
        try
        {
            string welcomeMessage = "WELCOME";
            byte[] data = Encoding.UTF8.GetBytes(welcomeMessage);
            server.Send(data, data.Length, clientEndpoint);
            Console.WriteLine("Sent: WELCOME");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending welcome message: {ex.Message}");
        }
    }


    //TODO: [Receive Data]

    //TODO: [Send Ack]

    //TODO: [Receive RequestData]

    //TODO: [Send Data]

    //TODO: [Implement your slow-start algorithm considering the threshold] 

    //TODO: [End sending data to client]

    //TODO: [Handle Errors]

    //TODO: [Send End]

    //TODO: create all needed methods to handle incoming messages


}