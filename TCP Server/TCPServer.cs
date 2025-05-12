
using DNS = System.Net.Dns;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
    public class TCPServer
    {
        private int defaultPort = 4096;
        private Byte[] dataBuffer = [];
        private string dataResult = string.Empty;
        private TcpListener serverInstance;

        public TCPServer(int port = 4096)
        {
            Console.WriteLine("Initializing TCP Server - Constructor Start");
            var Name = DNS.GetHostName();
            var IP = DNS.GetHostEntry(Name).AddressList.FirstOrDefault(x=> x.AddressFamily == AddressFamily.InterNetwork);
            Console.WriteLine($"Host Name - {Name}\nHost IP - {IP}");
            var internalPort = -1;

            if(port != defaultPort)
            {
                Console.WriteLine($"Assigning server to port - {port}");
                internalPort = port;
            }
            else
            {
                Console.WriteLine($"The selected port: {port}, conflicts with the Default Port, we will use that instead");
                internalPort = defaultPort;                
            }
            Console.WriteLine($"Creating server on IP: {IP} on the Port: {internalPort}");
            if(IP != null)
            {
                TcpListener Server = new TcpListener(IP, internalPort);
                serverInstance = Server;
            }
            
        }

        public void Process()
        {
            Console.WriteLine("Starting server!");
            serverInstance.Start();

            dataBuffer = new Byte[2048];
            dataResult = string.Empty;

            Console.WriteLine("Processing server");
            while(true)
            {
                Console.WriteLine("-Awaiting Connection-");
                using TcpClient client = serverInstance.AcceptTcpClient();
                Console.WriteLine($"Connected to - {client.Client.LocalEndPoint}");

                NetworkStream clientStream = client.GetStream();

                int i;

                while((i = clientStream.Read(dataBuffer, 0, dataBuffer.Length)) != 0)
                {
                    dataResult = Encoding.ASCII.GetString(dataBuffer, 0, i);
                    Console.WriteLine($"Received - '{dataResult}'");

                    //Parse message

                    //Answer the client
                    var responseData = "Success";
                    byte[] responseBuffer = Encoding.ASCII.GetBytes(responseData);


                    clientStream.Write(responseBuffer, 0, responseBuffer.Length);
                    Console.WriteLine("Response sent!");

                }
            }
        }
    }
}