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
    private State state = new State();

    private AsyncCallback recv = null;
    public class State
    {
        public byte[] buffer = new byte[bufSize];
    }
    //TODO: [Send Hello message]
    private void SendHello()
    {
        try
        {
            string message = "HELLO";
            byte[] data = Encoding.ASCII.GetBytes(message);
            client_socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, serverEndpoint, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = client_socket.EndSendTo(ar);
                Console.WriteLine("SEND: {0}, {1}", bytes, message);
            }, state);
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
            client_socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref serverEndpoint, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = client_socket.EndReceiveFrom(ar, ref serverEndpoint);
                string receivedMessage = Encoding.ASCII.GetString(so.buffer, 0, bytes);
                if (receivedMessage == "WELCOME")
                {
                    Console.WriteLine("Welcome from the server");
                }
                client_socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref serverEndpoint, recv, so);
            }, state);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving message: {ex.Message}");
        }
    }


    //TODO: [Send Data(threshold)]

    //TODO: [Receive ACK]

    //TODO: [Send RequestData]

    //TODO: [Receive Data]

    //TODO: [Send ACK]

    //TODO: [Receive END]

    //TODO: [Handle Errors]
}