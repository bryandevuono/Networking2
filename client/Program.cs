using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MessageNS;

// This comment is useless but if you remove it you will fail the assignment;
// Julina Mercera 1055662
// Bryan de Vuono 1061043
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
    public int AcksSent;
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
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.HostUnreachable)
        {
            Console.WriteLine($"The remote host is unreachable. Details: {ex.Message}");
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.NetworkUnreachable)
        {
            Console.WriteLine($"The network is unreachable. Details: {ex.Message}");
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
        {
            Console.WriteLine("The remote host forcibly closed the connection. Details: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    //TODO: create all needed objects for your sockets 
    private const int bufSize = 8 * 1024;
    public byte[] buffer = new byte[bufSize];
    public int count = 0;
    int threshold = 256;
    public static string SerializeMessage(Message message)
    {
        return JsonSerializer.Serialize(message);
    }
    public static Message DeserializeMessage(string jsonMessage)
    {
        var message = JsonSerializer.Deserialize<Message>(jsonMessage);
        if (message == null)
        {
            throw new InvalidOperationException("Deserialization resulted in null.");
        }
        return message;
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
            Console.WriteLine($"Message sent to {serverEndpoint}: {Hello.Content}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending hello message: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while sending hello message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            client_socket.SendTo(data, serverEndpoint);
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
            if (ReceivedMessage.Type == MessageType.Welcome)
            {
                Console.WriteLine("Welcome from the server\n");
                SendTreshold();
            }
            else
            {
                Console.WriteLine("Error: Wrong Message type is sent");
                client_socket.Close();
            }
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
        {
            Console.WriteLine("SocketException: The remote host forcibly closed the connection. Details: " + ex.Message);
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending message: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while receiving message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            client_socket.SendTo(data, serverEndpoint);
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
                SendRequestData();
            }
            else
            {
                Console.WriteLine(ReceivedMessage.Content);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving message: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while receiving message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            client_socket.SendTo(data, serverEndpoint);
        }
    }
    //TODO: [Send RequestData]
    private void SendRequestData()
    {
        try
        {
            Message requestData = new Message();
            requestData.Type = MessageType.RequestData;
            requestData.Content = "hamlet.txt";
            string message = SerializeMessage(requestData);
            byte[] data = Encoding.UTF8.GetBytes(message);
            client_socket.SendTo(data, serverEndpoint);
            Console.WriteLine("Request for data sent.\n");
            ReceiveData();
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.NetworkUnreachable)
        {
            Console.WriteLine("Error while sending acknowledgment: unreachable network");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = "Error while sending acknowledgment: unreachable network";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            client_socket.SendTo(data, serverEndpoint);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error while receiving message: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while receiving message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            client_socket.SendTo(data, serverEndpoint);
        }
    }

    //TODO: [Receive Data]
    private void ReceiveData()
    {
        while (true)
        {
            int receivedBytes = client_socket.ReceiveFrom(buffer, ref serverEndpoint);
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            if(receivedMessage == null)
            {
                continue;
            }
            try
            {
                Message endMessage = DeserializeMessage(receivedMessage);
                if (endMessage.Type == MessageType.End)
                {
                    ReceiveEnd();
                }
            }
            catch(SocketException ex) when (ex.SocketErrorCode == SocketError.NetworkUnreachable)
            {
                Console.WriteLine($"Something went wrong while connecting: {ex}");
            }
            catch(SocketException ex)
            {
                Console.WriteLine($"Something went wrong while receiving the message: {ex}");
            }
            catch
            {
                Console.WriteLine($"Data received from server: {receivedMessage}\n");
                SendAck();
            }
        }
    }

    //TODO: [Send ACK]
    public void SendAck()
    {
        try
        {
            Message ackMessage = new Message();
            ackMessage.Type = MessageType.Ack;
            ackMessage.Content = $"{AcksSent}\n";
            string message = SerializeMessage(ackMessage);
            byte[] data = Encoding.UTF8.GetBytes(message);
            client_socket.SendTo(data, serverEndpoint);
            Console.WriteLine("Acknowledgment sent");
            AcksSent++;
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.NetworkUnreachable)
        {
            Console.WriteLine("Error while sending acknowledgment: unreachable network");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = "Error while sending acknowledgment: unreachable network";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            client_socket.SendTo(data, serverEndpoint);
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Error while sending acknowledgment: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while receiving message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            client_socket.SendTo(data, serverEndpoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending acknowledgment: {ex.Message}");
            Message Error = new Message();
            Error.Type = MessageType.Error;
            Error.Content = $"Error while receiving message: {ex.Message}";
            string ErrorMessage = SerializeMessage(Error);
            byte[] data = Encoding.ASCII.GetBytes(ErrorMessage);
            client_socket.SendTo(data, serverEndpoint);
        }
    }

    //TODO: [Receive END]
    private void ReceiveEnd()
    {
        try
        {
            Console.WriteLine("End");
            client_socket.Close();
            Environment.Exit(0);
            return;
        }
        catch
        {
            Console.WriteLine("Can't close the connection");
            return;
        }
    }
    //TODO: [Handle Errors]
}