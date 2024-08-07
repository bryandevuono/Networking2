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
    private Socket server_socket;
    private EndPoint clientEndpoint;
    public ServerUDP()
    {
        server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientEndpoint = new IPEndPoint(IPAddress.Any, 11000);
        server_socket.Bind(clientEndpoint);
    }

    public void start()
    {
        try
        {
            Console.WriteLine("Listening...")
            while (true)
            {
                ReceiveMessage();
                Thread.Sleep(10);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {

        }   
    }

    //TODO: create all needed objects for your sockets 
    private const int bufSize = 8 * 1024;
    private State state = new State();

    private AsyncCallback recv = null;
     public class State
    {
        public byte[] buffer = new byte[bufSize];
    }
    //TODO: keep receiving messages from clients
    // you can call a dedicated method to handle each received type of messages

    //TODO: [Receive Hello]
    private void ReceiveMessage()
    {
        try
        {
            server_socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref clientEndpoint, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = server_socket.EndReceiveFrom(ar, ref clientEndpoint);
                server_socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref clientEndpoint, recv, so);
                Console.WriteLine("RECV: {0}: {1}, {2}", clientEndpoint.ToString(), bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));
            }, state);
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
            byte[] data = Encoding.ASCII.GetBytes(welcomeMessage);
            server_socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = server_socket.EndSend(ar);
                Console.WriteLine("SEND: {0}, {1}", bytes, welcomeMessage);
            }, state);
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