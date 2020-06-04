using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

public class Client
{
	static void Main(string[] args)
	{
		IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName()); 
		IPAddress ipAddr = ipHost.AddressList[0]; 
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 5555);
	
		TcpClient serverSocket;

		try
		{
			serverSocket = new TcpClient();
			serverSocket.Connect(localEndPoint);
			Console.WriteLine("Connected to Server");
		}
		catch
		{
			Console.WriteLine("Failed to connect to {0}:5555", ipAddr);
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
				Console.WriteLine();	
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