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

    private Socket client_socket;
    private EndPoint serverEndpoint;

    public ClientUDP()
    {
        client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 32000); // Verander het IP-adres en de poort indien nodig
    }
    public void start()
    {
        try
        {
            SendHello();
            ReceiveWelcome();
            while (true)
            {
                Thread.Sleep(10);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client_socket.Close();
        }
    }
    //TODO: create all needed objects for your sockets 
    private const int bufSize = 8 * 1024;
    public byte[] buffer = new byte[bufSize];
    int threshold = 1472;
    public static string SerializeMessage(Message message)
    {
        return JsonSerializer.Serialize(message);
    }
    public static Message DeserializeMessage(string jsonMessage)
    {
        return JsonSerializer.Deserialize<Message>(jsonMessage);
    }
    //TODO: [Send Hello message]
    private void SendHello()
    {
        try
        {
            // Het sturen van messages is hier versimpeld "SendTo(data, receiver adress)"
            Message Hello = new Message();
            Hello.Type = MessageType.Hello;
            Hello.Content = "HELLO";
            string message = SerializeMessage(Hello);
            byte[] data = Encoding.UTF8.GetBytes(message);
            client_socket.SendTo(data, serverEndpoint);
            Console.WriteLine($"Message sent to {serverEndpoint}: {Hello.Content}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending hello message: {ex.Message}");
        }
    }

    //TODO: [Receive Welcome]
    private void ReceiveWelcome()
    {
        try
        {
            int receivedBytes = client_socket.ReceiveFrom(buffer, ref serverEndpoint);
            string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Message ReceivedMessage = DeserializeMessage(jsonMessage);
            if (ReceivedMessage.Content == "WELCOME")
            {
                Console.WriteLine("Welcome from the server");
                SendTreshold();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving message: {ex.Message}");
        }
    }


    //TODO: [Send Data(threshold)]
    private void SendTreshold()
    {
        try
        {
            Message Threshold = new Message();
            Threshold.Type = MessageType.Data;
            Threshold.Content = threshold.ToString();
            string message = SerializeMessage(Threshold);
            byte[] data = Encoding.UTF8.GetBytes(message);
            client_socket.SendTo(data, serverEndpoint);
            Console.WriteLine("Threshold sent");
            ReceiveThresholdACK();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while sending message: {e.Message}");
        }
    }
    //TODO: [Receive ACK]
    private void ReceiveThresholdACK()
    {
        try
        {
            int receivedBytes = client_socket.ReceiveFrom(buffer, ref serverEndpoint);
            string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Message ReceivedMessage = DeserializeMessage(jsonMessage);
            if (ReceivedMessage.Type == MessageType.Ack)
            {
                Console.WriteLine(ReceivedMessage.Content);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving message: {ex.Message}");
        }
    }
    //TODO: [Send RequestData]
    private void SendRequestData()
    {
        try
        {
            Message requestData = new Message();
            requestData.Type = MessageType.RequestData;
            requestData.Content = requestData.ToString();
            string message = SerializeMessage(requestData);
            byte[] data = Encoding.UTF8.GetBytes(message);
            client_socket.SendTo(data, serverEndpoint);
            Console.WriteLine("Request for data sent.");
            ReceiveData();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending data request: {ex.Message}");
        }
    }

    //TODO: [Receive Data]
    private void ReceiveData()
    {
        try
        {
            int receivedBytes = client_socket.ReceiveFrom(buffer, ref serverEndpoint);
            string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Message receivedMessage = DeserializeMessage(jsonMessage);
            if (receivedMessage.Type == MessageType.Data)
            {
                Console.WriteLine($"Data received from server: {receivedMessage.Content}");
                // sendAck(receivedMessage.Content);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving data: {ex.Message}");
        }
    }

    //TODO: [Send ACK]
    public void SendAck(string receivedContent)
    {
        try
        {
            Message ackMessage = new Message();
            ackMessage.Type = MessageType.Ack;
            ackMessage.Content = $"ACK: Data '{receivedContent}' received";
            string message = SerializeMessage(ackMessage);
            byte[] data = Encoding.UTF8.GetBytes(message);
            client_socket.SendTo(data, serverEndpoint);
            Console.WriteLine("Acknowledgment sent");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending acknowledgment: {ex.Message}");
        }
    }

    //TODO: [Receive END]

    //TODO: [Handle Errors]
}