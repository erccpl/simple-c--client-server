using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class Client
{
	static void Main(string[] args)
	{
		//Get the machine's local network address
		IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName()); 
		IPAddress ipAddr = ipHost.AddressList[0]; 
		IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 5555);

		TcpClient serverSocket = null;
		try
		{
			//Setup the socket on the client side
			serverSocket = new TcpClient();
			serverSocket.Connect(localEndPoint);
			Console.WriteLine("Connected to Server");

			//Setup full-duplex comminunication channels on this end
			NetworkStream networkStream = serverSocket.GetStream();
			StreamReader streamReader = new StreamReader(networkStream);
			StreamWriter streamWriter = new StreamWriter(networkStream);
	
			//Infinite request handler loop, terminates when user input 'q'
			while(true)
			{
				Console.Write("> ");
				string serverMsg = Console.ReadLine();
				streamWriter.WriteLine(serverMsg);
				streamWriter.Flush();
				
				if(serverMsg.ToLower().Equals("q"))
					break;
				
				string responseMsg = streamReader.ReadLine();
				if(responseMsg == null){
					Console.WriteLine("The server has been shut down");
					break;
				}

				Console.WriteLine(responseMsg);
				while(streamReader.Peek() != -1) 
					Console.WriteLine(streamReader.ReadLine());
				
				Console.WriteLine();	
			}
		}
		catch(SocketException)
		{
			Console.Error.WriteLine("Error: connection refused (is the server started?)");
		}
		catch (System.IO.IOException) 
		{
			Console.WriteLine("IOException: The server socket has been shutdown");
		}
		catch (NullReferenceException) 
		{
			Console.WriteLine("NullRefException: The server socket has been shutdown");
		}
		catch(Exception e)
		{
			Console.WriteLine(e.ToString());
		}
		finally 
		{	
			if(serverSocket.Connected){
				serverSocket.Close();
			}
		}
	}
} 
