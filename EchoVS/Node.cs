using System.Runtime.Serialization.Formatters.Binary;

namespace EchoVS
{
    public class Node
    {
        private string _name;
        private uint _size;
        private string _ip;
        private uint _port;

        // Connection details to connect to logger
        private const string LoggerIp = "192.168.2.2";
        private const uint LoggerPort = 1234;

        private readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        // Sum of the communicated size of received echo messages
        private uint _receivedSize = 0;

        // Constructor
        public Node(string name, uint size, string ip, uint port)
        {
            _name = name;
            _size = size;
            _ip = ip;
            _port = port;
        }


        // Methods
        // Will be called, when a new message arrives
        public void ReactTo(Message message)
        {
            
        }

        // Sends a message to the logger
        private void Log(string message)
        {
            

        }
    }
}