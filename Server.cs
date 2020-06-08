using System;
using System.Net;
using System.Net.Sockets; 
using System.IO;
using System.Threading;


public class Server
{	
	private static Server _instance = null;
	private static readonly object padlock = new object();

	private Server(){}

	public static Server Instance
	{
		get
		{
			lock (padlock)
			{
				if (_instance == null)
					_instance = new Server();
				return _instance;
			}
		}
	}
  
	public void Execute()
	{
		int portNum = 5555;

		//Get the machine's local network address and create server address
		IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName()); 
		IPAddress ipAddr = ipHost.AddressList[0];
		IPEndPoint localEndPoint = new IPEndPoint(ipAddr, portNum);

		//Setup socket & start listening for connections
		TcpListener tcpListener = new TcpListener(localEndPoint);
		tcpListener.Start();
		Console.WriteLine("Server Started: {0}:{1}", ipAddr, portNum);
		
		TcpClient clientSocket = default(TcpClient);
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

			try
			{
				while (true)
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
							catch(UnauthorizedAccessException) 
							{	
								streamWriter.WriteLine("Server does not have permission to access this directory");
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

				clientSocket.Close();
				Console.WriteLine("Shutting down connection to client {0}", this.clientNumber);
			}
			catch(NullReferenceException){
				Console.WriteLine("Client {0} shut down unexpectedly, closing connection", this.clientNumber);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			finally
			{
				clientSocket.Close();	
			}
        }

    }
} 