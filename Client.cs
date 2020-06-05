using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

public class Client
{
	static void Main(string[] args)
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
		StreamReader streamReader = new StreamReader(networkStream);
		StreamWriter streamWriter = new StreamWriter(networkStream);

		try
		{	
			//Infinite request handler loop, terminates when user input 'q'
			while(true)
			{
				Console.Write("> ");
				string serverMsg = Console.ReadLine();
				if(serverSocket.Connected)	
				{		
					streamWriter.WriteLine(serverMsg);
					streamWriter.Flush();
				}
				else
					break;	
				

				if(serverMsg.ToLower().Equals("q"))
					break;
				
				string responseMsg = streamReader.ReadLine();
				if(responseMsg.Equals("end")){
					break;
				}

				Console.WriteLine(responseMsg);
				while(streamReader.Peek() != -1) 
					Console.WriteLine(streamReader.ReadLine());
				
				Console.WriteLine();	
			}
		}
		catch (System.IO.IOException) {
			Console.WriteLine("The server socket has been shutdown");
		}
		catch (NullReferenceException) {
			Console.WriteLine("The server socket has been shutdown");
		}
		catch(Exception e)
		{
			Console.WriteLine(e.ToString());
		}
		finally {
			networkStream.Close();
			streamReader.Close();
			streamWriter.Close();
		}
	}
} 
