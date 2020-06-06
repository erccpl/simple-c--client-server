using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class Client
{
	static void Main(string[] args)
	{
		int portNum = 5555;

		//Get the machine's local network address
		IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName()); 
		IPAddress ipAddr = ipHost.AddressList[0]; 
		IPEndPoint localEndPoint = new IPEndPoint(ipAddr, portNum);

		TcpClient serverSocket = new TcpClient();
		try
		{
			//Setup the socket on the client side
			serverSocket.Connect(localEndPoint);
			Console.WriteLine("Connected to Server @{0}:{1}", ipAddr, portNum);

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
				if(responseMsg == null) {
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
		catch (IOException) 
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