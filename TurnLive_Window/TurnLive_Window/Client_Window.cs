using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.ObjectModel;

namespace TurnLive_Window
{
	class Client_Window
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public int WindowId { get; set; }

		public User user { get; set; }
		public ObservableCollection<Status> ClientStatuses;
		public ObservableCollection<User> WaitClients;

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
		public Dcb cbGetStatuses;
		public Dcb cbShowClient;

		public delegate void DcbStr(string str);
		public DcbStr cbUpdateInfo;
		public delegate void DcbStrBool(string str, bool ClientActionEnable);
		public DcbStrBool cbShowMessage;
		
		public Client_Window()
		{
			Log = new ObservableCollection<string>();
			tiReconnect = new System.Timers.Timer();
			tiReconnect.Elapsed += tiReconnectTick;
			tiReconnect.Interval = 10000;
			ClientStatuses = new ObservableCollection<Status>();
			WaitClients = new ObservableCollection<User>();
			LoadSettings();
			foreach (string s in App.Arguments)
			{
				if (s.IndexOf("-H") > -1)
					Host = s.Substring(2);
				if (s.IndexOf("-P") > -1)
					try { Port = int.Parse(s.Substring(2)); }
					catch { Port = 25565; }
				if (s.IndexOf("-W") > -1)
					try { WindowId = int.Parse(s.Substring(2)); }
					catch { WindowId = 0; }
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
			WindowId = int.Parse(props.get("WindowId", "0"));
		}
		public void SaveSettings()
		{
			PropertiesFile props = new PropertiesFile(AppDomain.CurrentDomain.BaseDirectory + "properties.properties");
			props.set("Host", Host);
			props.set("Port", Port.ToString());
			props.set("WindowId", WindowId.ToString());
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

		public void GetFromServerClientStatuses()
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "WindowGetClientStatuses";
			messageToSend.Data = "";
			Send(messageToSend);
		}
		public void ParseClientStatuses(string ClientStatusesString)
		{
			while (ClientStatusesString.IndexOf("/;") > -1)
			{
				string strStatus = ClientStatusesString.Substring(0, ClientStatusesString.IndexOf("/;"));
				int Id = 0;
				string Name = "";
				string NameInWindow = "";
				try
				{
					Id = int.Parse(strStatus.Substring(0, strStatus.IndexOf("/,")));
					strStatus = strStatus.Substring(strStatus.IndexOf("/,") + 2);
					Name = strStatus.Substring(0, strStatus.IndexOf("/,"));
					strStatus = strStatus.Substring(strStatus.IndexOf("/,") + 2);
					NameInWindow = strStatus;
				}
				finally
				{
					if (NameInWindow != "")
						App.Current.Dispatcher.Invoke((Action)delegate { ClientStatuses.Add(new Status(Id, Name, NameInWindow)); } );
				}
				ClientStatusesString = ClientStatusesString.Substring(ClientStatusesString.IndexOf("/;") + 2);
			}
			App.Current.Dispatcher.Invoke((Action)delegate { cbGetStatuses(); });
		} // "id/,name/,name_in_window/;id/,..."
		public void GetFromServerClient(bool Show = true)
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "WindowGetClient";
			messageToSend.Data = WindowId.ToString() + "/;" + Show.ToString() + "/;";
			Send(messageToSend);
		}
		public void ParseClient(string ClientString)
		{
			if (ClientString.IndexOf("error") > -1)
			{
				cbShowMessage(ClientString.Substring(5), true);
				return;
			} 
			try
			{
				user = new User();
				if (ClientString.IndexOf("/;") > -1)
				{
					try
					{
						user.Id = int.Parse(ClientString.Substring(0, ClientString.IndexOf("/,")));
						ClientString = ClientString.Substring(ClientString.IndexOf("/,") + 2);
						user.TurnPrefix = ClientString.Substring(0, ClientString.IndexOf("/,"));
						ClientString = ClientString.Substring(ClientString.IndexOf("/,") + 2);
						user.Index = int.Parse(ClientString.Substring(0, ClientString.IndexOf("/,")));
						ClientString = ClientString.Substring(ClientString.IndexOf("/,") + 2);
						user.StatusId = int.Parse(ClientString.Substring(0, ClientString.IndexOf("/,")));
						ClientString = ClientString.Substring(ClientString.IndexOf("/,") + 2);
						user.StatusTime = ClientString.Substring(0, ClientString.IndexOf("/,"));
						ClientString = ClientString.Substring(ClientString.IndexOf("/,") + 2);
						user.IsLive = ((ClientString.Substring(0, ClientString.IndexOf("/;")) == "t") ? true : false);
					}
					catch (Exception ex) { LogAdd(ex.Message); }
					ClientString = ClientString.Substring(ClientString.IndexOf("/;") + 2);
				}
				while (ClientString.IndexOf("/;") > -1)
				{
					try
					{
						string IName = ClientString.Substring(0, ClientString.IndexOf("/,"));
						ClientString = ClientString.Substring(ClientString.IndexOf("/,") + 2);
						string IValue = ClientString.Substring(0, ClientString.IndexOf("/;"));
						user.Infos.Add(new UserInfo(IName, IValue));
					}
					catch (Exception ex) { LogAdd(ex.Message); }
					ClientString = ClientString.Substring(ClientString.IndexOf("/;") + 2);
				}
				App.Current.Dispatcher.Invoke((Action)delegate { cbShowClient(); });
			}
			catch (Exception ex) { LogAdd(ex.Message); }
		} // "Id/,Index/,StatusId/,StatusTime/,IsLive/,PathName/,TurnName/;InfoName/,InfoValue/;InfoName/,..."
		public void SendToServerClientStatus(int StatusId)
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "WindowSetClientStatus";
			messageToSend.Data = WindowId.ToString() + "/;" + user.Id.ToString() + "/;" + StatusId.ToString() + "/;";
			Send(messageToSend);
		}
		public void SendToServerSetBackWaitClient(int ClientId)
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "WindowSetBackWaitClient";
			messageToSend.Data = WindowId.ToString() + "/;" + ClientId.ToString() + "/;";
			Send(messageToSend);
		}
		public void GetFromServerCountInfo()
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "WindowGetCountInfo";
			messageToSend.Data = WindowId.ToString();
			Send(messageToSend);
		}
		public void GetFromServerWaitClients()
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "WindowGetWaitClients";
			messageToSend.Data = WindowId.ToString();
			Send(messageToSend);
		}
		public void ParseWaitClients(string ClientString)
		{
			if (ClientString.IndexOf("error") > -1)
			{
				LogAdd(ClientString.Substring(5));
				return;
			}
			try
			{
				App.Current.Dispatcher.Invoke((Action)delegate { WaitClients.Clear(); });
				User cl;
				while (ClientString.IndexOf("/;") > -1)
				{
					try
					{
						cl = new User();
						cl.Id = int.Parse(ClientString.Substring(0, ClientString.IndexOf("/,")));
						ClientString = ClientString.Substring(ClientString.IndexOf("/,") + 2);
						cl.TurnPrefix = ClientString.Substring(0, ClientString.IndexOf("/,"));
						ClientString = ClientString.Substring(ClientString.IndexOf("/,") + 2);
						cl.Index = int.Parse(ClientString.Substring(0, ClientString.IndexOf("/,")));
						ClientString = ClientString.Substring(ClientString.IndexOf("/,") + 2);
						cl.StatusTime = ClientString.Substring(0, ClientString.IndexOf("/,"));
						ClientString = ClientString.Substring(ClientString.IndexOf("/,") + 2);
						cl.IsLive = ((ClientString.Substring(0, ClientString.IndexOf("/;")) == "t") ? true : false);
						App.Current.Dispatcher.Invoke((Action)delegate { WaitClients.Add(cl); });
					}
					catch (Exception ex) { LogAdd(ex.Message); }
					ClientString = ClientString.Substring(ClientString.IndexOf("/;") + 2);
				}
			}
			catch (Exception ex) { LogAdd(ex.Message); }
		} // "Id/,Index/,StatusId/,StatusTime/,IsLive/,PathName/,TurnName/;InfoName/,InfoValue/;InfoName/,..."
		public void RePlayClient()
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "WindowRePlayClient";
			messageToSend.Data = user.Id.ToString();
			Send(messageToSend);
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
				cbShowMessage("Соединение установлено", true);
			}
			catch (Exception ex)
			{
				LogAdd("!OnConnect: " + ex.Message);
				cbShowMessage("Ошибка подключения.", false);
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
						cbShowMessage("Сервер разорвал подключение.", false);
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
					case "WindowGetClientStatuses": ParseClientStatuses(messageReceived.Data); break;
					case "WindowGetClient": ParseClient(messageReceived.Data); break;
					case "WindowGetCountInfo": App.Current.Dispatcher.Invoke((Action)delegate { cbUpdateInfo(messageReceived.Data); }); break;
					case "WindowGetWaitClients": ParseWaitClients(messageReceived.Data); break;
					case "WindowSetClientStatus": break;
					case "WindowSetBackWaitClient": break;
					case "WindowRePlayClient": break;
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
						cbShowMessage("Сервер разорвал подключение.", false);
						tiReconnect.Start();
					}
			}
		}
	}
}