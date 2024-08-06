using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MessageNS;

// This comment is useless but if you remove it you will fail the assignment;
class Program //DO NOT CHANGE THIS CLASS
{
    static void Main(string[] args)
    {
        ClientUDP cUDP = new ClientUDP();
        cUDP.start();
    }
}

class ClientUDP
{

    //TODO: implement all necessary logic to create sockets and handle incoming messages
    // Do not put all the logic into one method. Create multiple methods to handle different tasks.

    private UdpClient client;
    private IPEndPoint serverEndpoint;

    public ClientUDP()
    {
        client = new UdpClient();
        serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9000); // Verander het IP-adres en de poort indien nodig
    }
    public void start()
    {
        try
        {
            SendHello();
            // ReceiveWelcome();
            // SendThreshold(10);
            // ReceiveAck();
            // RequestData("example.txt");
            // ReceiveData();
            // SendAck();
            // ReceiveEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }
    //TODO: create all needed objects for your sockets 

    //TODO: [Send Hello message]
    private void SendHello()
    {
        string message = "HELLO";
        byte[] data = Encoding.UTF8.GetBytes(message);
        client.Send(data, data.Length, serverEndpoint);
        Console.WriteLine("Sent: HELLO");
    }

    //TODO: [Receive Welcome]
    private void ReceiveWelcome()
    {
        var response = client.Receive(ref serverEndpoint);
        string message = Encoding.UTF8.GetString(response);
        Console.WriteLine($"Received: {message}");
    }


    //TODO: [Send Data(threshold)]

    //TODO: [Receive ACK]

    //TODO: [Send RequestData]

    //TODO: [Receive Data]

    //TODO: [Send ACK]

    //TODO: [Receive END]

    //TODO: [Handle Errors]
}