using System;
using System.Net;
using System.Net.Sockets; 
using System.IO;
using System.Text;

public class Server
{
	public static void Main()
	{
		try
		{
			//Setup a TCP socket and wait for a connection from a client
			IPAddress localHost = IPAddress.Parse("127.0.0.1");
			TcpListener tcpListener = new TcpListener(localHost, 5555);
			tcpListener.Start();
			Console.WriteLine("Server Started");

			TcpClient clientSocket = tcpListener.AcceptTcpClient();
			Console.WriteLine("Client Connected");

			NetworkStream networkStream = clientSocket.GetStream();
			StreamWriter streamWriter = new StreamWriter(networkStream);
			StreamReader streamReader = new StreamReader(networkStream);

			while(true)
			{
				string clientMsg = streamReader.ReadLine();

				//1. Reply with simple hello
				if( clientMsg.ToLower().Equals("hello") )
				{
					streamWriter.WriteLine("Hi");
				}

				//2. Reply with the time in UTC format
				else if( clientMsg.ToLower().Equals("time") )
				{
					DateTime currentUTCTime = DateTime.UtcNow;
					streamWriter.WriteLine(currentUTCTime);
				}

				//3. Reply with contents of specified directory
				else if( clientMsg.ToLower().StartsWith("dir") )
				{
					string[] args = clientMsg.Split(" ");
					if(args.Length != 2) 
					{
						streamWriter.WriteLine("Please check inputs for dir command");
					} 
					else 
					{
						string path = args[1];
						try 
						{
							DirectoryInfo dirInfo = new DirectoryInfo(@path);
							foreach (DirectoryInfo di in dirInfo.GetDirectories())
							{
								streamWriter.WriteLine(di);
							}
							foreach(FileInfo fi in dirInfo.GetFiles())
							{
								streamWriter.WriteLine(fi);
							}
						} 
						catch(DirectoryNotFoundException) 
						{	
							streamWriter.WriteLine("Directory does not exist");
						}
					}
				}

				//4. Graceful exit
				else if( clientMsg.ToLower().Equals("q") )
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

			//Perform cleanup actions
			streamReader.Close();
			networkStream.Close();
			streamWriter.Close();
			clientSocket.Close();
			Console.WriteLine("Shutting down server");
		}

		catch(Exception e)
		{
			Console.WriteLine(e.ToString());
		}
	}
}