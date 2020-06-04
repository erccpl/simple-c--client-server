using System;
using System.Net;
using System.Net.Sockets; 
using System.IO;
using System.Text;


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
		try
		{
			//Get the machine's local network address
			IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName()); 
    		IPAddress ipAddr = ipHost.AddressList[0];
        	IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 5555);

			//Setup socket & start listening for connections
			TcpListener tcpListener = new TcpListener(localEndPoint);
			tcpListener.Start();
			Console.WriteLine("Server Started: {0}:5555", ipAddr);

			//Blocking call, waiting for the client to connect
			TcpClient clientSocket = tcpListener.AcceptTcpClient();
			Console.WriteLine("Client Connected");

			//Setup full-duplex comminunication channels on this end
			NetworkStream networkStream = clientSocket.GetStream();
			StreamWriter streamWriter = new StreamWriter(networkStream);
			StreamReader streamReader = new StreamReader(networkStream);

			//Infinite request handler loop, terminates when it receives 'q' (for 'quit')
			while(clientSocket.Connected)
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

			//Perform cleanup actions for graceful shutdown
			streamReader.Close();
			networkStream.Close();
			streamWriter.Close();

			clientSocket.Close();
			Console.WriteLine("Shutting down server");
		}
		catch(NullReferenceException)
		{
			Console.WriteLine("Client exited unexpectedly");
		}
		catch(Exception e)
		{
			Console.WriteLine(e.ToString());
		}
	}
}

