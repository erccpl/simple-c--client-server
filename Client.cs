using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

public class Client
{
	static void Main(string[] args)
	{
		try
		{
			//Get the machine's local network address
			IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName()); 
			IPAddress ipAddr = ipHost.AddressList[0]; 
			IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 5555);
		
			//Setup the socket on the client side
			TcpClient serverSocket;
			serverSocket = new TcpClient();
			serverSocket.Connect(localEndPoint);
			Console.WriteLine("Connected to Server");

			//Setup full-duplex comminunication channels on this end
			NetworkStream networkStream = serverSocket.GetStream();
			StreamReader streamreader = new StreamReader(networkStream);
			StreamWriter streamwriter = new StreamWriter(networkStream);
		
			//Infinite request handler loop, terminates when user input 'q'
			while(true)
			{
				Console.Write("> ");
				string serverMsg = Console.ReadLine();			
				streamwriter.WriteLine(serverMsg);
				streamwriter.Flush();

				if(serverMsg.ToLower().Equals("q"))
					break;
				
				string responseMsg = streamreader.ReadLine();
				Console.WriteLine(responseMsg);
				while(streamreader.Peek() != -1) 
					Console.WriteLine(streamreader.ReadLine());
				
				Console.WriteLine();	
			}

			//Cleanup and graceful shutdown
			streamreader.Close();
			networkStream.Close();
			streamwriter.Close();
		}
		catch(Exception e)
		{
			Console.WriteLine(e.ToString());
		}
	}
} 
