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
    public void start()
    {

    }
    //TODO: create all needed objects for your sockets 

    //TODO: [Send Hello message]

    //TODO: [Receive Welcome]

    //TODO: [Send Data(threshold)]
    
    //TODO: [Receive ACK]

    //TODO: [Send RequestData]

        //TODO: [Receive Data]

        //TODO: [Send ACK]

    //TODO: [Receive END]

    //TODO: [Handle Errors]
}