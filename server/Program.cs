﻿using System;
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
    private Socket server_socket;
    private EndPoint clientEndpoint;
    public ServerUDP()
    {
        server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
        server_socket.Bind(new IPEndPoint(IPAddress.Any, 32000));
    }

    public void start()
    {
        try
        {
            Console.WriteLine("Listening...");
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
    public static Message DeserializeMessage(string jsonMessage)
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
            Message ReceivedMessage = DeserializeMessage(jsonMessage);
            Console.WriteLine("Recieved from " + clientEndpoint + ": " + ReceivedMessage.Content);
            SendWelcome();
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
            Message Welcome = new Message();
            Welcome.Type = MessageType.Welcome;
            Welcome.Content = "WELCOME";
            string welcomeMessage = SerializeMessage(Welcome);
            byte[] data = Encoding.ASCII.GetBytes(welcomeMessage);
            server_socket.SendTo(data, clientEndpoint);
            Console.WriteLine($"Message sent to {clientEndpoint}: {Welcome.Content}\n");
            ReceiveThreshold();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending welcome message: {ex.Message}");
        }
    }
    //TODO: [Receive Data]
    private void ReceiveThreshold()
    {
        try
        {
            int receivedBytes = server_socket.ReceiveFrom(buffer, ref clientEndpoint);
            string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Message ReceivedMessage = DeserializeMessage(jsonMessage);
            threshold = Int32.Parse(ReceivedMessage.Content);
            Console.WriteLine("Recieved threshold from " + clientEndpoint + ": " + ReceivedMessage.Content);
            SendThresholdACK();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving message: {ex.Message}");
        }
    }
    //TODO: [Send Ack]
    private void SendThresholdACK()
    {
        try
        {
            Message ThresholdAck = new Message();
            ThresholdAck.Type = MessageType.Ack;
            ThresholdAck.Content = "ACK: Threshold is received\n";
            string Ack = SerializeMessage(ThresholdAck);
            byte[] data = Encoding.ASCII.GetBytes(Ack);
            server_socket.SendTo(data, clientEndpoint);
            ReceiveRequestData();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending message: {ex.Message}");
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
        }
    }
    //TODO: [Receive RequestData]
    private void ReceiveRequestData()
    {
        try
        {
            int receivedBytes = server_socket.ReceiveFrom(buffer, ref clientEndpoint);
            string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Message receivedMessage = DeserializeMessage(jsonMessage);
            Console.WriteLine($"Received data request from {clientEndpoint}: {receivedMessage.Content}\n");
            if (receivedMessage.Type == MessageType.RequestData && receivedMessage.Content == "hamlet.txt")
            {
                string[] fragments = SplitIntoFragments("hamlet.txt");
                SendData(fragments);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving data request: {ex.Message}");
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
        while(currentIndex < fragments.Length)
        {
            for (int i = 0; i < packetRate && currentIndex < fragments.Length; i++)
            {
                string message = $"{currentIndex} {fragments[currentIndex]}";
                byte[] data = Encoding.UTF8.GetBytes(message);

                server_socket.SendTo(data, clientEndpoint);
                Console.WriteLine($"Sent: {message}");
                packetsSent++;
                currentIndex++;
                Thread.Sleep(100 / packetRate);// timeout
                Console.WriteLine($"Packet rate: {packetRate}");
                Console.WriteLine($"Ack's received: {acksReceived}");
            }
            server_socket.ReceiveTimeout = 10;
            while (acksReceived < packetsSent)
            {
                try
                {
                    int receivedBytes = server_socket.ReceiveFrom(buffer, ref clientEndpoint);
                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    Message ackMessage = DeserializeMessage(jsonMessage);
                    Console.WriteLine($"Received ACK: {ackMessage.Content}\n");
                    acksReceived++;
                }
                catch (SocketException)
                {
                    Console.WriteLine("Timeout waiting for ACKs.");
                    break;
                }
                SlowStart();
            }
        }
        SendEnd();
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
        Console.WriteLine($"Threshold: {threshold}");
    }
    //TODO: [End sending data to client]

    //TODO: [Handle Errors]

    //TODO: [Send End]
    private void SendEnd()
    {
        Message End = new Message();
        End.Type = MessageType.End;
        End.Content = "End";
        string endMessage = SerializeMessage(End);
        byte[] data = Encoding.ASCII.GetBytes(endMessage);
        server_socket.SendTo(data, clientEndpoint);
    }
    //TODO: create all needed methods to handle incoming messages


}