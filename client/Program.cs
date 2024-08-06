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

    private Socket socket_client;
    private IPEndPoint serverEndpoint;

    public ClientUDP()
    {
        client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9000); // Verander het IP-adres en de poort indien nodig
        client_socket.Bind(serverEndpoint);
    }
    public void start()
    {
        try
        {
            SendHello();
            ReceiveWelcome();
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
        byte[] data = Encoding.ASCII.GetBytes(message);
        client_socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
        {
            State so = (State)ar.AsyncState;
            int bytes = server_socket.EndSend(ar);
            Console.WriteLine("SEND: {0}, {1}", bytes, text);
        }, state);
    }

    //TODO: [Receive Welcome]
    private void ReceiveWelcome()
    {
        try
        {
            client_socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = client_socket.EndReceiveFrom(ar, ref epFrom);
                client_socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                Console.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));
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