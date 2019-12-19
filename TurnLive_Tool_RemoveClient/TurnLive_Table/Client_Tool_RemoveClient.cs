using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.ObjectModel;

namespace TurnLive_Tool_RemoveClient
{
	class Client_Tool_RemoveClient
	{
		public string Host { get; set; }
		public int Port { get; set; }

		private bool _opened = false;
		public bool Opened { get { return _opened; } }

		public Socket clientSocket; 
		private byte[] byteData = new byte[4096];
		private Message messageReceived;
		System.Timers.Timer tiReconnect;

		public ObservableCollection<string> Log;

		public delegate void Dcb();
		public Dcb cbConnect;
		public Dcb cbConnectError;

		public delegate void DcbStr(string str);
		public DcbStr cbShowMessage;
		public DcbStr cbShowResponse;
		
		public Client_Tool_RemoveClient()
		{
			Log = new ObservableCollection<string>();
			tiReconnect = new System.Timers.Timer();
			tiReconnect.Elapsed += tiReconnectTick;
			tiReconnect.Interval = 10000;
			LoadSettings();
			foreach (string s in App.Arguments)
			{
				if (s.IndexOf("-H") > -1)
					Host = s.Substring(2);
				if (s.IndexOf("-P") > -1)
					try { Port = int.Parse(s.Substring(2)); }
					catch { Port = 25565; }
			}
			SaveSettings();
		}

		public void tiReconnectTick(object sender, EventArgs e)
		{
			tiReconnect.Stop();
			App.Current.Dispatcher.Invoke((Action)delegate { cbConnectError(); });
		}

		public void LoadSettings()
		{
			PropertiesFile props = new PropertiesFile(AppDomain.CurrentDomain.BaseDirectory + "properties.properties");
			Host = props.get("Host", "127.0.0.1");
			Port = int.Parse(props.get("Port", "25565"));
		}
		public void SaveSettings()
		{
			PropertiesFile props = new PropertiesFile(AppDomain.CurrentDomain.BaseDirectory + "properties.properties");
			props.set("Host", Host);
			props.set("Port", Port.ToString());
			props.Save();
		}

		public void LogAdd(string str)
		{
			if (App.Current != null && App.Current.Dispatcher != null && Log != null)
			App.Current.Dispatcher.Invoke(
				(Action)delegate
				{
					Log.Add(DateTime.Now.ToString("HH:mm") + " " + str);
					if (Log.Count > 1000)
					{
						LogSave();
					}
				}
			);
		}
		public void LogSave()
		{
			string logsPath = AppDomain.CurrentDomain.BaseDirectory + "logs";
			if (!System.IO.Directory.Exists(logsPath))
				System.IO.Directory.CreateDirectory(logsPath);
			// delete old files
			string[] files = System.IO.Directory.GetFiles(logsPath);
			foreach (string file in files)
				if (DateTime.Now - System.IO.File.GetCreationTime(file) > TimeSpan.FromDays(14))
					System.IO.File.Delete(file);
			// create and save logFile
			System.IO.File.WriteAllLines(logsPath + "\\log" + DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + ".txt", Log);
			Log.Clear();
		}

		public void RemoveClient(string ClientId)
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "ToolRemoveClient";
			messageToSend.Data = ClientId;
			Send(messageToSend);
		}
		public void ParseResponse(string Response)
		{
			LogAdd(Response);
			cbShowResponse(Response);
			return;
		}
		public void Disconnect()
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "Disconnect";
			messageToSend.Data = "";
			Send(messageToSend);
		}

		private void Send(Message messageToSend)
		{
			byte[] byteData = messageToSend.ToByte();
			if (clientSocket.Connected)
			{
				clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
				LogAdd("> Отправка запроса: " + messageToSend.ToString());
			}
		}
		
		public void OpenClient()
		{
			try
			{
				clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(Host), Port);
				clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
				LogAdd("> Попытка соединения (" + ipEndPoint.Address.ToString() + ":" + ipEndPoint.Port.ToString() + ")");
			}
			catch (Exception ex)
			{
				LogAdd("!OpenClient: " + ex.Message);
				tiReconnect.Start();
			}
		}
		private void OnConnect(IAsyncResult ar)
		{
			try
			{
				clientSocket.EndConnect(ar);
				clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
				_opened = true;
				LogAdd("> Клиент присоединился");
				App.Current.Dispatcher.Invoke((Action)delegate { cbConnect(); });
				cbShowMessage("Соединение установлено");
			}
			catch (Exception ex)
			{
				LogAdd("!OnConnect: " + ex.Message);
				cbShowMessage("Ошибка подключения.");
				tiReconnect.Start();
			}
		}
		private void OnSend(IAsyncResult ar)
		{
			try
			{
				clientSocket.EndSend(ar);
			}
			catch (Exception ex)
			{
				LogAdd("!OnSend: " + ex.Message);
				if (ex is SocketException)
					if ((ex as SocketException).ErrorCode.ToString() == "10054")
					{
						cbShowMessage("Сервер разорвал подключение.");
						tiReconnect.Start();
					}
			}
		}
		private void OnReceive(IAsyncResult ar)
		{
			try
			{
				clientSocket.EndReceive(ar);
				messageReceived = new Message(byteData);

				LogAdd("> Получено: " + messageReceived.ToString());

				switch (messageReceived.Cmd)
				{
					case "ToolRemoveClient": ParseResponse(messageReceived.Data); break;
				}

				// продолжаем слушать сокет
				byteData = new byte[4096];
				clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
			}
			catch (Exception ex)
			{
				LogAdd("!OnReceive: " + ex.Message);
				if (ex is SocketException)
					if ((ex as SocketException).ErrorCode.ToString() == "10054")
					{
						cbShowMessage("Сервер разорвал подключение.");
						tiReconnect.Start();
					}
			}
		}
	}
}
