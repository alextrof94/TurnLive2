using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.ObjectModel;

namespace TurnLive_Terminal
{
	class Client_Terminal
	{
		public string Host { get; set; }
		public int Port { get; set; }

		public User user { get; set; }
		public ObservableCollection<Turn> turns { get; set; }
		public ObservableCollection<TurnVisualization> turnsVisualization { get; set; }
		public ObservableCollection<Info> infos { get; set; }
		public ObservableCollection<Day> days { get; set; }
		public ObservableCollection<Date> dates { get; set; }

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
		public Dcb cbConnectErrorHideScreen;
		public Dcb cbDays;
		public Dcb cbTurnsVisualization;
		public Dcb cbTurns;
		public Dcb cbInfos;
		public Dcb cbDates;
		public Dcb cbClientPrint;

		public delegate void DcbStr(string str);
		public DcbStr cbShowMessage;

		public Client_Terminal()
		{
			messageReceived = new Message();
			Log = new ObservableCollection<string>();
			tiReconnect = new System.Timers.Timer();
			tiReconnect.Elapsed += tiReconnectTick;
			tiReconnect.Interval = 10000;
			turns = new ObservableCollection<Turn>();
			turnsVisualization = new ObservableCollection<TurnVisualization>();
			infos = new ObservableCollection<Info>();
			days = new ObservableCollection<Day>();
			dates = new ObservableCollection<Date>();
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
			user = new User();
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
	
		public void GetFromServerDates(int TurnId)
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "TerminalGetDates";
			messageToSend.Data = TurnId.ToString();
			Send(messageToSend);
		}
		public void ParseDates(string input)
		{
			if (input.IndexOf("error") < 0)
			{
				App.Current.Dispatcher.Invoke((Action)delegate { dates.Clear(); });
				while (input.IndexOf("/;") > -1)
				{
					string strDate = input.Substring(0, input.IndexOf("/;"));
					input = input.Substring(input.IndexOf("/;") + 2);
					int DayId = 0;
					bool Working = false;
					try
					{
						DayId = int.Parse(strDate.Substring(0, strDate.IndexOf("/,")));
						strDate = strDate.Substring(strDate.IndexOf("/,") + 2);
						Working = bool.Parse(strDate.Substring(0, strDate.IndexOf("/,")));
						strDate = strDate.Substring(strDate.IndexOf("/,") + 2);
					}
					catch(Exception ex) { LogAdd(ex.Message); }
					int NowDayId = Convert.ToInt32(DateTime.Today.DayOfWeek);
					if (NowDayId == 0) NowDayId = 7;
					if (Working && (DayId >= NowDayId))
					{
						if (strDate != "/!" && strDate.Length > 0)
						{
							App.Current.Dispatcher.Invoke((Action)delegate { dates.Add(new Date(DayId, Working)); });
							while (strDate.IndexOf("/,") > -1)
							{
								myTime mt = new myTime();
								try
								{
									mt = new myTime(strDate.Substring(0, strDate.IndexOf("/,")));
									strDate = strDate.Substring(strDate.IndexOf("/,") + 2);
								}
								finally
								{
									App.Current.Dispatcher.Invoke((Action)delegate { dates[dates.Count - 1].times.Add(mt); });
								}
							}
						}
					}
				}
				cbDates();
			}
			else
			{
				LogAdd(input.Substring(5));
			}
		}
		public void GetFromServerDays()
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "TerminalGetDays";
			messageToSend.Data = "";
			Send(messageToSend);
		}
		public void ParseDays(string input)
		{
			if (input.IndexOf("error") < 0)
			{
				days.Clear();
				while (input.IndexOf("/;") > -1)
				{
					string strTurn = input.Substring(0, input.IndexOf("/;"));
					input = input.Substring(input.IndexOf("/;") + 2);
					int Id = 0;
					bool Work = false;
					bool Terminal = false;
					try
					{
						Id = int.Parse(strTurn.Substring(0, strTurn.IndexOf("/,")));
						strTurn = strTurn.Substring(strTurn.IndexOf("/,") + 2);
						Work = bool.Parse(strTurn.Substring(0, strTurn.IndexOf("/,")));
						strTurn = strTurn.Substring(strTurn.IndexOf("/,") + 2);
						Terminal = bool.Parse(strTurn);
					}
					finally
					{
						days.Add(new Day(Id, Work, Terminal));
					}
				}
			}
			else
			{
				LogAdd(input.Substring(5));
			}
			App.Current.Dispatcher.Invoke((Action)delegate { cbDays(); });
		}
		public void GetFromServerTurns()
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "TerminalGetTurns";
			messageToSend.Data = "";
			Send(messageToSend);
		}
		public void ParseTurns(string input)
		{
			if (input.IndexOf("error") < 0)
			{
				turns.Clear();
				while (input.IndexOf("/;") > -1)
				{
					string strTurn = input.Substring(0, input.IndexOf("/;"));
					input = input.Substring(input.IndexOf("/;") + 2);
					int Id = 0;
					string Prefix = "";
					string Name = "";
					bool IsLive = false;
					try
					{
						Id = int.Parse(strTurn.Substring(0, strTurn.IndexOf("/,")));
						strTurn = strTurn.Substring(strTurn.IndexOf("/,") + 2);
						Prefix = strTurn.Substring(0, strTurn.IndexOf("/,"));
						strTurn = strTurn.Substring(strTurn.IndexOf("/,") + 2);
						Name = strTurn.Substring(0, strTurn.IndexOf("/,"));
						strTurn = strTurn.Substring(strTurn.IndexOf("/,") + 2);
						IsLive = bool.Parse(strTurn);
					}
					finally
					{
						turns.Add(new Turn(Id, Prefix, Name, IsLive));
					}
				}
			}
			else
			{
				LogAdd(input.Substring(5));
			}
			App.Current.Dispatcher.Invoke((Action)delegate { cbTurns(); });
		}
		public void GetFromServerTurnsVisualization()
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "TerminalGetTurnsVisualization";
			messageToSend.Data = "";
			Send(messageToSend);
		}
		public void ParseTurnsVisualization(string input)
		{
			if (input.IndexOf("error") < 0)
			{
				turnsVisualization.Clear();
				while (input.IndexOf("/;") > -1)
				{
					string strTurn = input.Substring(0, input.IndexOf("/;"));
					input = input.Substring(input.IndexOf("/;") + 2);
					int Id = 0;
					int TurnId = 0;
					string Name = "";
					int TvMainId = 0;
					bool Fms = false;
					try
					{
						Id = int.Parse(strTurn.Substring(0, strTurn.IndexOf("/,")));
						strTurn = strTurn.Substring(strTurn.IndexOf("/,") + 2);
						TurnId = int.Parse(strTurn.Substring(0, strTurn.IndexOf("/,")));
						strTurn = strTurn.Substring(strTurn.IndexOf("/,") + 2);
						Name = strTurn.Substring(0, strTurn.IndexOf("/,"));
						strTurn = strTurn.Substring(strTurn.IndexOf("/,") + 2);
						TvMainId = int.Parse(strTurn.Substring(0, strTurn.IndexOf("/,")));
						strTurn = strTurn.Substring(strTurn.IndexOf("/,") + 2);
						Fms = bool.Parse(strTurn);
					}
					finally
					{
						turnsVisualization.Add(new TurnVisualization(Id, TurnId, Name, TvMainId, Fms));
					}
				}
			}
			else
			{
				LogAdd(input.Substring(5));
			}
			App.Current.Dispatcher.Invoke((Action)delegate { cbTurnsVisualization(); });
		}
		public void GetFromServerInfos()
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "TerminalGetInfos";
			messageToSend.Data = "";
			Send(messageToSend);
		}
		public void ParseInfos(string input)
		{
			infos.Clear();
			while (input.IndexOf("/;") > -1)
			{
				string strInfo = input.Substring(0, input.IndexOf("/;"));
				input = input.Substring(input.IndexOf("/;") + 2);
				int Id = 0;
				bool Required = false;
				string Name = "";
				string Pattern = "";
				try
				{
					Id = int.Parse(strInfo.Substring(0, strInfo.IndexOf("/,")));
					strInfo = strInfo.Substring(strInfo.IndexOf("/,") + 2);
					string str = strInfo.Substring(0, strInfo.IndexOf("/,"));
					Required = ((str == "True") ? true : false);
					strInfo = strInfo.Substring(strInfo.IndexOf("/,") + 2);
					Name = strInfo.Substring(0, strInfo.IndexOf("/,"));
					strInfo = strInfo.Substring(strInfo.IndexOf("/,") + 2);
					Pattern = strInfo;
				}
				finally
				{
					infos.Add(new Info(Id, Required, Name, Pattern));
				}
			}

			App.Current.Dispatcher.Invoke((Action)delegate { cbInfos(); });
		}
		public void SendClientToServer(string msg)
		{
			Message messageToSend = new Message();
			messageToSend.Id = 0;
			messageToSend.Cmd = "TerminalAddClient";
			messageToSend.Data = msg;
			Send(messageToSend);
		}
		public void ParseClient(string input)
		{
			if (input.IndexOf("error") < 0)
			{
				try
				{
					if (input.IndexOf("/;") > -1)
					{
						string strInfo = input.Substring(0, input.IndexOf("/;"));
						input = input.Substring(input.IndexOf("/;") + 2);
						user.Id = int.Parse(strInfo.Substring(0, strInfo.IndexOf("/,")));
						strInfo = strInfo.Substring(strInfo.IndexOf("/,") + 2);
						user.Index = int.Parse(strInfo.Substring(0, strInfo.IndexOf("/,")));
						strInfo = strInfo.Substring(strInfo.IndexOf("/,") + 2);
						user.StatusTime = strInfo;
					}
					if (input.IndexOf("/;") > -1)
					{
						user.turnEnabled = bool.Parse(input.Substring(0, input.IndexOf("/;")));
						if (input.Length > 2)
							input = input.Substring(input.IndexOf("/;") + 2);
					}
				}
				finally
				{
					App.Current.Dispatcher.Invoke((Action)delegate { cbClientPrint(); });
				}
			}
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
				_opened = false;
				App.Current.Dispatcher.Invoke((Action)delegate { cbConnectErrorHideScreen(); });
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
				_opened = false;
				App.Current.Dispatcher.Invoke((Action)delegate { cbConnectErrorHideScreen(); });
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
						_opened = false;
						App.Current.Dispatcher.Invoke((Action)delegate { cbConnectErrorHideScreen(); });
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
					case "TerminalGetTurns": ParseTurns(messageReceived.Data); break;
					case "TerminalGetTurnsVisualization": ParseTurnsVisualization(messageReceived.Data); break;
					case "TerminalGetInfos": ParseInfos(messageReceived.Data); break;
					case "TerminalGetDates": ParseDates(messageReceived.Data); break;
					case "TerminalGetDays": ParseDays(messageReceived.Data); break;
					case "TerminalAddClient": ParseClient(messageReceived.Data); break;
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
						_opened = false;
						App.Current.Dispatcher.Invoke((Action)delegate { cbConnectErrorHideScreen(); });
					}
			}
		}
	}
}
