using System;
using System.Net;
using System.Net.Sockets; 
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;


public class Server
{	
	private static Server instance = null;
	private static readonly object padlock = new object();

	Server(){}

	public static Server Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
					instance = new Server();
				return instance;
			}
		}
	}
  


	public void Execute()
	{
		//Get the machine's local network address
		IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName()); 
		IPAddress ipAddr = ipHost.AddressList[0];
		IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 5555);

		//Setup socket & start listening for connections
		TcpListener tcpListener = new TcpListener(localEndPoint);
		tcpListener.Start();
		Console.WriteLine("Server Started: {0}:5555", ipAddr);
		
		TcpClient clientSocket = default(TcpClient);

		Console.CancelKeyPress += delegate {
			Console.WriteLine("Ctrl+C detected: shutting down main server thread");
			clientSocket.Close();
			tcpListener.Stop();
		};
		
		int counter = 0;
		while(true) 
		{
			counter++;
			clientSocket = tcpListener.AcceptTcpClient();
			Console.WriteLine("Client {0} connected", counter);
			ClientHandler client = new ClientHandler();
			client.startClient(clientSocket, counter);
		}
	}

	public class ClientHandler
    {
        TcpClient clientSocket;
        int clientNumber;
        public void startClient(TcpClient clientSocket, int clientNumber)
        {
            this.clientSocket = clientSocket;
            this.clientNumber = clientNumber;
            Thread clientThread = new Thread(processRequests);
			clientThread.IsBackground = true;
            clientThread.Start();
        }
        private void processRequests()
        {
			NetworkStream networkStream = clientSocket.GetStream();
			StreamWriter streamWriter = new StreamWriter(networkStream);
			StreamReader streamReader = new StreamReader(networkStream);

            while (true)
            {
                try
                {
					string clientMsg = streamReader.ReadLine();

					//1. Reply with simple hello
					if(clientMsg.ToLower().Equals("hello"))
						streamWriter.WriteLine("Hi");
		
					//2. Reply with the time in UTC format
					else if(clientMsg.ToLower().Equals("time"))
					{
						DateTime currentUTCTime = DateTime.UtcNow;
						streamWriter.WriteLine(currentUTCTime);
					}
					
					//3. Reply with contents of specified directory
					else if(clientMsg.ToLower().StartsWith("dir"))
					{
						string[] args = clientMsg.Split(" ");
						if(args.Length != 2) 
							streamWriter.WriteLine("Please check inputs for dir command");
						else 
						{
							string path = args[1];
							try 
							{
								DirectoryInfo dirInfo = new DirectoryInfo(path);
								foreach (DirectoryInfo di in dirInfo.GetDirectories())
									streamWriter.WriteLine(di);
								foreach(FileInfo fi in dirInfo.GetFiles())
									streamWriter.WriteLine(fi);
							} 
							catch(DirectoryNotFoundException) 
							{	
								streamWriter.WriteLine("Directory does not exist");
							}
						}
					}

					//4. Graceful exit
					else if(clientMsg.ToLower().Equals("q"))
					{
						break;
					}

					//5. Unknown command
					else 
					{
						streamWriter.WriteLine("Unrecognized/unsupported command");
					}
					streamWriter.Flush();	
                }
				catch(NullReferenceException){
					Console.WriteLine("Client {0} shut down unexpectedly, closing connection", this.clientNumber);
					break;
				}
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }

			streamReader.Close();
			streamWriter.Close();
			networkStream.Close();

			clientSocket.Close();
			Console.WriteLine("Shutting down connection to client {0}", this.clientNumber);
        }
    } 
}

