using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

public class Client
{
	static void Main(string[] args)
	{
		string localHost = "127.0.0.1";
		TcpClient serverSocket;

		try
		{
			serverSocket = new TcpClient(localHost, 5555);
			Console.WriteLine("Connected to Server");
		}
		catch
		{
			Console.WriteLine("Failed to connect to {0}:5555", localHost);
			return;
		}

		NetworkStream networkStream = serverSocket.GetStream();
		StreamReader streamreader = new StreamReader(networkStream);
		StreamWriter streamwriter = new StreamWriter(networkStream);
		
		try
		{
			while(true)
			{
				Console.Write("> ");
				string serverMsg = Console.ReadLine();			
				streamwriter.WriteLine(serverMsg);
				streamwriter.Flush();

				if(serverMsg.ToLower().Equals("q")){
					break;
				}

				string responseMsg = streamreader.ReadLine();
				Console.WriteLine(responseMsg);
				while(streamreader.Peek() != -1) {
					Console.WriteLine(streamreader.ReadLine());
				}	
			}
		}
		catch
		{
			Console.WriteLine("Exception reading from the server") ;
		}

		//Cleanup and graceful exit
		streamreader.Close();
		networkStream.Close();
		streamwriter.Close();
	}
} 