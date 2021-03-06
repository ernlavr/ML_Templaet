using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace SP
{

	/// <summary>
	/// Sends and receives TCP traffic
	/// </summary>
	public class NetworkManagerTCP : Singleton<NetworkManagerTCP>
	{
		/// <summary>
		/// IP Address of the server that we connect to
		/// </summary>
		public string ipAddress = "192.168.0.172";
		/// <summary>
		/// Port of the server that we connect to
		/// </summary>
		public int port = 8052;

		#region private members
		/// <summary>
		/// TCP socket reference
		/// </summary>
		private TcpClient socketConnection;
		/// <summary>
		/// Receiver thread
		/// </summary>
		private Thread clientReceiveThread;
		/// <summary>
		/// Bool used as a test button
		/// </summary>
		public bool test = false;
		#endregion

		// Use this for initialization 	
		void Start()
		{
			print("TCP Endpoint: " + ipAddress + ":" + port);
			ConnectToTcpServer(ipAddress, port);
		}

		public string getConnectionStatus()
        {
			if (socketConnection.Connected)
			{
				return "Connected " + socketConnection.Client.RemoteEndPoint.ToString();
			}
			else return "Not Connected";
        }

		private void Update()
		{
			if (test)
			{
				test = false;
				sendTestMessage();
			}
		}

		/// <summary>
        /// Immediately stops the TCP data listening thread and set TcpClient to null
        /// </summary>
		public void disconnectTcp()
        {
			clientReceiveThread.Abort();
			socketConnection = null;
        }

		/// <summary> 	
		/// Setup socket connection. 	
		/// </summary> 	
		public void ConnectToTcpServer(string ip, int port)
		{
			this.ipAddress = ip;
			this.port = port;
			try
			{
				clientReceiveThread = new Thread(new ThreadStart(ListenForData));
				clientReceiveThread.IsBackground = true;
				clientReceiveThread.Start();
			}
			catch (Exception e)
			{
				Debug.Log("On client connect exception " + e);
			}
		}
		/// <summary> 	
		/// Runs in background clientReceiveThread; Listens for incomming data. 	
		/// </summary>     
		private void ListenForData()
		{
			try
			{
				socketConnection = new TcpClient(ipAddress, port);
				byte[] bytes = new Byte[1024];
				while (true)
				{
					// Get a stream object for reading 				
					using (NetworkStream stream = socketConnection.GetStream())
					{
						int length;

						// Read incomming stream into byte arrary. 					
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							// Convert byte array to string message. 						
							string serverMessage = Encoding.ASCII.GetString(incommingData);
							Debug.Log("server message received as: " + serverMessage);
						}
					}
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("Socket exception: " + socketException);
			}
		}

		/// <summary>
		/// Function for sending a test string message over TCP
		/// </summary>
		public void sendTestMessage()
		{
			print("Sending a message");
			if (socketConnection == null)
			{
				print("Socket connection is null, attempting to reconnect..");
				ConnectToTcpServer(ipAddress, port);
			}
			sendStringMessage("Dipshit TCP");
		}

		public void sendStringMessage(string msg)
		{
			byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(msg);
			sendBytes(clientMessageAsByteArray);
		}


		/// <summary> 	
		/// Send message to server using socket connection. 	
		/// </summary> 	
		public void sendBytes(byte[] message)
		{
			if (socketConnection == null)
			{
				Debug.LogError("SP: Attempting to send bytes over TCP but socketConnection is null");
				return;
			}
			try
			{
				// Get a stream object for writing. 			
				NetworkStream stream = socketConnection.GetStream();
				if (stream.CanWrite)
				{
					// Write byte array to socketConnection stream.
					stream.Write(message, 0, message.Length);
					Debug.Log("TCP message sent successfully");
				}
				else
				{
					Debug.LogError("SP: TCP Network stream cannot write.");
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("Socket exception: " + socketException);
			}
		}
	}
}