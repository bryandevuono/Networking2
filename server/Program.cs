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
        while (true)
        {
            sUDP.start();
        }
    }
}

class ServerUDP
{

    //TODO: implement all necessary logic to create sockets and handle incoming messages
    // Do not put all the logic into one method. Create multiple methods to handle different tasks.
    private Socket server_socket;
    private EndPoint clientEndpoint;
    private bool isClientConnected = false;
    private List<string> fragmentsList = new List<string>();

    public ServerUDP()
    {
        try
        {
            server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
            server_socket.Bind(new IPEndPoint(IPAddress.Any, 32000));
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"There was a problem while creating the socket: {ex}");
        }
    }

    public void start()
    {
        try
        {
            Console.WriteLine("Listening...");
            isClientConnected = false;
            ReceiveMessage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    //TODO: create all needed objects for your sockets 
    private const int bufSize = 8 * 1024;
    public byte[] buffer = new byte[bufSize];
    public int threshold;
    public int acksReceived = 0;
    int packetsSent;
    int packetRate = 1;
    //TODO: keep receiving messages from clients
    // you can call a dedicated method to handle each received type of messages
    public static string SerializeMessage(Message message)
    {
        return JsonSerializer.Serialize(message);
    }
    public static Message? DeserializeMessage(string jsonMessage)
    {
        return JsonSerializer.Deserialize<Message>(jsonMessage);
    }
    //TODO: [Receive Hello]
    private void ReceiveMessage()
    {
        try
        {
            int receivedBytes = server_socket.ReceiveFrom(buffer, ref clientEndpoint);
            string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Message? ReceivedMessage = DeserializeMessage(jsonMessage);

            if (ReceivedMessage != null && ReceivedMessage.Type == MessageType.Hello)
            {
                Console.WriteLine("Recieved from " + clientEndpoint + ": " + ReceivedMessage.Content);
                isClientConnected = true;
                SendWelcome();
            }
            else
            {
                Console.WriteLine("Received a null or invalid message.");
            }
        }
        catch (SocketException ex)
        {
            HandleClientDisconnection(ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving message: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while sending welcome message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            server_socket.SendTo(data, clientEndpoint);
            start();
        }
    }

    //TODO: [Send Welcome]
    private void SendWelcome()
    {
        try
        {
            Message Welcome = new Message();
            Welcome.Type = MessageType.Welcome;
            Welcome.Content = "WELCOME";
            string welcomeMessage = SerializeMessage(Welcome);
            byte[] data = Encoding.ASCII.GetBytes(welcomeMessage);
            server_socket.SendTo(data, clientEndpoint);
            Console.WriteLine($"Message sent to {clientEndpoint}: {Welcome.Content}\n");
            ReceiveThreshold();
        }
        catch (SocketException ex)
        {
            HandleClientDisconnection(ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending welcome message: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while sending welcome message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            server_socket.SendTo(data, clientEndpoint);
            start();
        }
    }
    //TODO: [Receive Data]
    private void ReceiveThreshold()
    {
        try
        {
            server_socket.ReceiveTimeout = 5000;
            int receivedBytes = server_socket.ReceiveFrom(buffer, ref clientEndpoint);
            string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Message? ReceivedMessage = DeserializeMessage(jsonMessage);
            if (ReceivedMessage != null && !string.IsNullOrEmpty(ReceivedMessage.Content) && int.TryParse(ReceivedMessage.Content, out int parsedThreshold) && ReceivedMessage.Type == MessageType.Data)
            {
                threshold = parsedThreshold;
                Console.WriteLine("Recieved threshold from " + clientEndpoint + ": " + ReceivedMessage.Content);
                SendThresholdACK();
            }
            else
            {
                Console.WriteLine("Invalid or missing threshold received.");
            }
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
        {
            Console.WriteLine("Timeout waiting for threshold");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = "Timeout waiting for threshold.";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            server_socket.SendTo(data, clientEndpoint);
            start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving message: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while receiving message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            server_socket.SendTo(data, clientEndpoint);
            start();
        }
    }
    //TODO: [Send Ack]
    private void SendThresholdACK()
    {
        try
        {
            Message ThresholdAck = new Message
            {
                Type = MessageType.Ack,
                Content = "ACK: Threshold is received\n"
            };
            string Ack = SerializeMessage(ThresholdAck);
            byte[] data = Encoding.ASCII.GetBytes(Ack);
            server_socket.SendTo(data, clientEndpoint);
            ReceiveRequestData();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending message: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while receiving message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            server_socket.SendTo(data, clientEndpoint);
            start();
        }
    }
    private void SendDataAck(Message message)
    {
        try
        {
            string Ack = SerializeMessage(message);
            byte[] data = Encoding.ASCII.GetBytes(Ack);
            server_socket.SendTo(data, clientEndpoint);
            Console.WriteLine($"Message sent to {clientEndpoint}: {message.Content}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending message: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while sending message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            server_socket.SendTo(data, clientEndpoint);
            start();
        }
    }
    //TODO: [Receive RequestData]
    private void ReceiveRequestData()
    {
        try
        {
            int receivedBytes = server_socket.ReceiveFrom(buffer, ref clientEndpoint);
            string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Message? receivedMessage = DeserializeMessage(jsonMessage);

            if (receivedMessage != null)
            {
                Console.WriteLine($"Received data request from {clientEndpoint}: {receivedMessage.Content}\n");
                isClientConnected = true;

                if (receivedMessage.Type == MessageType.RequestData && receivedMessage.Content == "hamlet.txt")
                {
                    string[] fragments = SplitIntoFragments("hamlet.txt");
                    SendData(fragments);
                }
            }
            else
            {
                Console.WriteLine("Received a null or invalid message.");
            }
        }
        catch (SocketException ex)
        {
            HandleClientDisconnection(ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving data request: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while receiving data request: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            server_socket.SendTo(data, clientEndpoint);
            start();
        }
    }

    //TODO: [Send Data]
    private string[] SplitIntoFragments(string filePath)
    {
        const int MAX_UDP_PAYLOAD = 508;
        string data = File.ReadAllText(filePath);
        int fragmentCount = (data.Length + MAX_UDP_PAYLOAD - 1) / MAX_UDP_PAYLOAD;
        string[] fragments = new string[fragmentCount];

        for (int i = 0; i < fragmentCount; i++)
        {
            int startIndex = i * MAX_UDP_PAYLOAD;
            int length = Math.Min(MAX_UDP_PAYLOAD, data.Length - startIndex);
            fragments[i] = data.Substring(startIndex, length);
        }
        return fragments;
    }
    private void SendData(string[] fragments)
    {
        int currentIndex = 0;
        while (currentIndex < fragments.Length)
        {
            for (int i = 0; i < packetRate && currentIndex < fragments.Length; i++)
            {
                string message = $"{currentIndex:0000}{fragments[currentIndex]}";
                byte[] data = Encoding.UTF8.GetBytes(message);

                server_socket.SendTo(data, clientEndpoint);
                Console.WriteLine($"Sent fragment number: {currentIndex}");
                packetsSent++;
                fragmentsList.Add(fragments[currentIndex]);
                currentIndex++;
            }
            while (acksReceived < packetsSent)
            {
                try
                {
                    server_socket.ReceiveTimeout = 5000; // sad flow
                    int receivedBytes = server_socket.ReceiveFrom(buffer, ref clientEndpoint);
                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    Message? ackMessage = DeserializeMessage(jsonMessage);
                    // Console.WriteLine($"Received ACK: {ackMessage.Content}\n");
                    Console.WriteLine($"Received ACK for fragment number: {acksReceived}\n");
                    acksReceived++;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    Console.WriteLine("Timeout waiting for ACKs.");
                    currentIndex = acksReceived;// sad flow
                    Message Error = new Message();
                    Error.Type = MessageType.Error;
                    Error.Content = "Timeout waiting for ACKs.";
                    string ErrorMessage = SerializeMessage(Error);
                    byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
                    server_socket.SendTo(data, clientEndpoint);
                    break;
                }
            }
            SlowStart();
            Console.WriteLine($"Current index: {currentIndex}, Acks received: {acksReceived}, Threshold: {threshold}, packet rate: {packetRate}"); // delete later
        }
        WriteToFile();
        SendEnd();
    }

    private void WriteToFile()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter("hamlet_output.txt"))
            {
                foreach (var fragment in fragmentsList)
                {
                    writer.Write(fragment);
                }
            }
            Console.WriteLine("Data written to a text file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending it to the file: {ex.Message}");
        }
    }

    //TODO: [Implement your slow-start algorithm considering the threshold] 
    private void SlowStart()
    {
        if (packetsSent <= threshold)
        {
            packetRate = packetRate * 2; // Increase the number of packets sent exponentialy
        }
        else
        {
            return;
        }
        Console.WriteLine($"Threshold: {threshold}");// remove later
    }
    //TODO: [End sending data to client]

    //TODO: [Handle Errors]
    private void HandleClientDisconnection(SocketException ex)
    {
        if (isClientConnected)
        {
            Console.WriteLine($"Client disconnected: : {ex.Message}");
            isClientConnected = false;
        }
        Console.WriteLine("Server is waiting for a new connection...");
    }
    //TODO: [Send End]
    private void SendEnd()
    {
        Message End = new Message();
        End.Type = MessageType.End;
        End.Content = "End";
        string endMessage = SerializeMessage(End);
        byte[] data = Encoding.ASCII.GetBytes(endMessage);
        server_socket.SendTo(data, clientEndpoint);

        //reset all variables
        acksReceived = 0;
        packetsSent = 0;
        packetRate = 1;
        fragmentsList.Clear();
        start();
    }
    //TODO: create all needed methods to handle incoming messages


}