using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.ObjectModel;
using Npgsql;


namespace TurnLive_Server
{
	public class Server
	{
		//The main socket on which the server listens to the clients
		public Socket serverSocket;
		private bool _opened = false;
		public bool Opened { get { return _opened; } }
		List<ClientData> Clients;

		//Buffer
		private byte[] byteData = new byte[4096];
		//settings
		public int Port { get; set; }
		private string DBHost { get; set; }
		private string DBPort { get; set; }
		private string DBUser { get; set; }
		private string DBPass { get; set; }
		private string DBName { get; set; }

		public ObservableCollection<string> Log;

		public Queue<string> ClientsToSound;
		public System.Media.SoundPlayer player;
		public bool playing = false;

		public System.Threading.Thread SoundThread;

		public Server()
		{
			Clients = new List<ClientData>();
			Log = new ObservableCollection<string>();
			ClientsToSound = new Queue<string>();
			player = new System.Media.SoundPlayer();
			LoadSettings();
			SoundThread = new System.Threading.Thread(SoundThreadPlay);
		}

		private void SoundThreadPlay()
		{
			while (true)
			{
				ClientsToSoundPlay();
				System.Threading.Thread.Sleep(500);
			}
		}

		public void LoadSettings()
		{
			PropertiesFile props = new PropertiesFile(AppDomain.CurrentDomain.BaseDirectory + "properties.properties");
			Port = int.Parse(props.get("Port", "25565"));
			DBHost = props.get("DBHost", "127.0.0.1");
			DBPort = props.get("DBPort", "5432");
			DBUser = props.get("DBUser", "postgres");
			DBPass = props.get("DBPass", "post");
			DBName = props.get("DBName", "turnlive2");
		}
		public void SaveSettings()
		{
			PropertiesFile props = new PropertiesFile(AppDomain.CurrentDomain.BaseDirectory + "properties.properties");
			props.set("Port", Port.ToString());
			props.set("DBHost", DBHost);
			props.set("DBPort", DBPort);
			props.set("DBUser", DBUser);
			props.set("DBPass", DBPass);
			props.set("DBName", DBName);
			props.Save();
		}

		public void LogAdd(string str)
		{
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

		public void ClientsToSoundPlay()
		{
			if (ClientsToSound.Count > 0)
			{
				playing = true;
				string strCl = ClientsToSound.Dequeue();
				string prefix = "А";
				string index = "1";
				string window = "1";
				if (strCl.IndexOf("/;") > -1)
				{
					prefix = strCl.Substring(0, strCl.IndexOf("/;"));
					strCl = strCl.Substring(strCl.IndexOf("/;") + 2);
				}
				if (strCl.IndexOf("/;") > -1)
				{
					index = strCl.Substring(0, strCl.IndexOf("/;"));
					strCl = strCl.Substring(strCl.IndexOf("/;") + 2);
				}
				if (strCl.IndexOf("/;") > -1)
				{
					window = strCl.Substring(0, strCl.IndexOf("/;"));
				}
				PlayNumber(prefix, index, window);
				if (ClientsToSound.Count > 0)
					ClientsToSoundPlay();
				playing = false;
			}
		}
		public void ClientsToSoundAdd(string prefix, string index, string window)
		{
			ClientsToSound.Enqueue(prefix + "/;" + index + "/;" + window + "/;");
		}
		public void PlayNumber(string prefix, string index, string window)
		{
			player.SoundLocation = Environment.CurrentDirectory + @"\sounds\other\КЛИЕНТ.wav";
			player.PlaySync();
			player.SoundLocation = Environment.CurrentDirectory + @"\sounds\prefix\" + prefix + ".wav";
			player.PlaySync();
			if (Convert.ToInt32(index) / 1000 > 0)
			{
				player.SoundLocation = Environment.CurrentDirectory + @"\sounds\index\" + (int)(Convert.ToInt32(index) / 1000) + "000.wav";
				player.PlaySync();
			}
			if (Convert.ToInt32(index) / 100 % 10 > 0)
			{
				player.SoundLocation = Environment.CurrentDirectory + @"\sounds\index\" + (int)(Convert.ToInt32(index) % 1000 / 100) + "00.wav";
				player.PlaySync();
			}
			if (Convert.ToInt32(index) % 100 > 19)
			{
				player.SoundLocation = Environment.CurrentDirectory + @"\sounds\index\" + (int)(Convert.ToInt32(index) % 100 / 10) + "0.wav";
				player.PlaySync();
				if (Convert.ToInt32(index) % 10 > 0)
				{
					player.SoundLocation = Environment.CurrentDirectory + @"\sounds\index\" + (int)(Convert.ToInt32(index) % 10) + ".wav";
					player.PlaySync();
				}
			}
			if (Convert.ToInt32(index) % 100 > 0 && Convert.ToInt32(index) % 100 < 20)
			{
				player.SoundLocation = Environment.CurrentDirectory + @"\sounds\index\" + (int)(Convert.ToInt32(index) % 100) + ".wav";
				player.PlaySync();
			}
			//WINDOW
			player.SoundLocation = Environment.CurrentDirectory + @"\sounds\other\ОКНО.wav";
			player.PlaySync();
			if (Convert.ToInt32(window) % 100 > 19)
			{
				player.SoundLocation = Environment.CurrentDirectory + @"\sounds\index\" + (int)(Convert.ToInt32(window) % 100 / 10) + "0.wav";
				player.PlaySync();
			}
			if (Convert.ToInt32(window) % 100 > 0 && Convert.ToInt32(window) % 100 < 20)
			{
				player.SoundLocation = Environment.CurrentDirectory + @"\sounds\index\" + (int)(Convert.ToInt32(window) % 100) + ".wav";
				player.PlaySync();
			}
			playing = false;
		}
		//terminal
		public bool TerminalGetDays(ref string result)
		{
			//SELECT * FROM days ORDER BY id ASC;
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT * FROM days ORDER BY id ASC;";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					while (npgSqlDataReader.Read())
						result += string.Format("{0}/,{1}/,{2}/;",
								npgSqlDataReader.GetInt32(0), npgSqlDataReader.GetBoolean(1), npgSqlDataReader.GetBoolean(2));
				}
				else
					result = "";
				conn.Close();
			}
			catch (Exception ex)
			{
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}// "id/,work/,terminal/;id/,..."
		public bool TerminalGetDates(ref string result, string TurnId)
		{
			//SELECT id, working FROM days ORDER BY id ASC;
			//SELECT time_s, time_e FROM days_worktime WHERE day_id= "DayId" ;
			//SELECT length FROM days_turnlengths WHERE day_id= "DayId" AND turn_id= "TurnId" ;
			//SELECT COUNT(id) FROM clients WHERE date_part('hour', status_time)= "tTime.Hour" AND date_part('minute', status_time)= "tTime.Minute" AND status_time::date=' "date.ToString("yyyy-MM-dd")" ' AND is_live = false;
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				//GET DAYS
				List<bool> workings = new List<bool>();

				string sql = "SELECT id, working FROM days ORDER BY id ASC;";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (!npgSqlDataReader.HasRows)
				{
					result = "errorНе удалось получить дни";
					return true;
				}
				while (npgSqlDataReader.Read())
				{
					workings.Add(npgSqlDataReader.GetBoolean(1));
				}
				npgSqlDataReader.Close();
				for (int DayId = 1; DayId < 8; DayId++)
				{
					result += DayId + "/," + workings[DayId - 1] + "/,";
					if (!workings[DayId - 1])
					{
						result += "/;";
					}
					else
					{
						//GET INTERVALS
						List<myTimeInterval> Intervals = new List<myTimeInterval>();
						sql = "SELECT time_s, time_e FROM days_worktime WHERE day_id=" + DayId + ";";
						npgSqlCommand = new NpgsqlCommand(sql, conn);
						npgSqlDataReader = npgSqlCommand.ExecuteReader();
						if (!npgSqlDataReader.HasRows)
						{
							result = "errorНе удалось получить интервалы времени работы на день " + DayId;
							return true;
						}
						while (npgSqlDataReader.Read())
						{
							Intervals.Add(new myTimeInterval(new myTime(npgSqlDataReader.GetString(0)), 
								new myTime(npgSqlDataReader.GetString(1))));
						}
						npgSqlDataReader.Close();
						myTime TimeSumm = new myTime(0);
						foreach (myTimeInterval t in Intervals)
							TimeSumm.Main += (t.End.Main - t.Start.Main);
						//GET TURNLENGTH
						int TurnLength = 0;
						sql = "SELECT length FROM days_turnlengths WHERE day_id=" + DayId + " AND turn_id=" + TurnId + ";";
						npgSqlCommand = new NpgsqlCommand(sql, conn);
						npgSqlDataReader = npgSqlCommand.ExecuteReader();
						if (!npgSqlDataReader.HasRows)
						{
							result = "errorНе удалось получить длину очереди на день " + DayId + " и очередь " + TurnId;
							return true;
						}
						while (npgSqlDataReader.Read())
						{
							TurnLength = npgSqlDataReader.GetInt32(0);
						}
						npgSqlDataReader.Close();
						//CALCULATING
						if (TurnLength < 1)
							result += "/!/;";
						else
						{
							myTime MainInterval = new myTime(TimeSumm.Main / TurnLength);
							DateTime date = DateTime.Now.AddDays(DayId - Convert.ToInt32(DateTime.Now.DayOfWeek));
							foreach (myTimeInterval ti in Intervals)
							{
								myTime tTime = new myTime(ti.Start.Main);
								while (tTime.Main < ti.End.Main)
								{
									sql = "SELECT COUNT(id) FROM clients WHERE date_part('hour', status_time)=" + tTime.Hour
											+ " AND date_part('minute', status_time)=" + tTime.Minute
											+ " AND status_time::date='" + date.ToString("yyyy-MM-dd") + "'"
											+ " AND is_live = false;";
									npgSqlCommand = new NpgsqlCommand(sql, conn);
									npgSqlDataReader = npgSqlCommand.ExecuteReader();
									if (npgSqlDataReader.HasRows)
									{
										npgSqlDataReader.Read();
										int a = Convert.ToInt32(npgSqlDataReader.GetValue(0));
										if (a == 0)
											result += tTime.ToString() + "/,";
									}
									npgSqlDataReader.Close();
									tTime.Main += MainInterval.Main;
								}
							}
							npgSqlDataReader.Close();
							result += "/;";
						}
					}
				}
				conn.Close();
			}
			catch (Exception ex)
			{
				// something went wrong, and you wanna know why
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}// "id/,working/,time/,...time/;id/,..."
		public bool TerminalGetTurns(ref string result)
		{
			//SELECT * FROM turns ORDER BY id ASC;
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT * FROM turns ORDER BY id ASC;";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					while (npgSqlDataReader.Read())
						result += string.Format("{0}/,{1}/,{2}/,{3}/;",
								npgSqlDataReader.GetInt32(0), npgSqlDataReader.GetChar(1), npgSqlDataReader.GetString(2),
								npgSqlDataReader.GetBoolean(3));
				}
				else
					result = "";
				conn.Close();
			}
			catch (Exception ex)
			{
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}// "id/,prefix/,name/,is_live/;id/,..."
		public bool TerminalGetTurnsVisualization(ref string result)
		{
			//SELECT * FROM turns_visualization ORDER BY id ASC;
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT * FROM turns_visualization ORDER BY id ASC;";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					while (npgSqlDataReader.Read())
						result += string.Format("{0}/,{1}/,{2}/,{3}/,{4}/;",
								npgSqlDataReader.GetInt32(0), npgSqlDataReader.GetInt32(1), npgSqlDataReader.GetString(2),
								npgSqlDataReader.GetInt32(3), npgSqlDataReader.GetBoolean(4));
				}
				else
					result = "";
				conn.Close();
			}
			catch (Exception ex)
			{
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}// "id/,turn_id/,name/,tv_main_id/,fms/;id/,..."
		public bool TerminalGetInfos(ref string result)
		{
			//SELECT * FROM infos;
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT * FROM infos;";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					while (npgSqlDataReader.Read())
						result += string.Format("{0}/,{1}/,{2}/,{3}/;",
								npgSqlDataReader.GetInt32(0), npgSqlDataReader.GetBoolean(1), npgSqlDataReader.GetString(2), npgSqlDataReader.GetString(3));
				}
				else
					result = "";
				conn.Close();
			}
			catch (Exception ex)
			{
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}// "id/,required/,name/.pattern/;id/,..."
		public bool TerminalAddClient(ref string result, string input)
		{
			//SELECT fu_add_client( "TurnId" , "IsLive" , "StatusTime" ); 
			//// returning (client.id, client.index, client.status_time)
			//SELECT is_live FROM turns WHERE id= "TurnId" ;
			//SELECT length FROM days_turnlengths WHERE turn_id= "TurnId" AND day_id= "((today == 0) ? 7 : today)" ;
			//SELECT COUNT(id) FROM clients WHERE path_id IN (SELECT id FROM paths WHERE turn_id= "TurnId" ) AND status_time::date = ' "DateTime.Now.Date.ToString("yyyy-MM-dd")" ' AND (status_id = 1 OR status_id = 2 OR status_id = 8);
			//INSERT INTO client_info(client_id, info_id, value) VALUES( "ClientId" , "InfoId" , ' "Value" ');
			string TurnId = "0";
			string IsLive = "false";
			string StatusTime = "'2000-01-01 01:01:01.123'";
			if (input.IndexOf("/;") > -1)
			{
				try
				{
					TurnId = input.Substring(0, input.IndexOf("/,"));
					input = input.Substring(input.IndexOf("/,") + 2);
					IsLive = input.Substring(0, input.IndexOf("/,"));
					input = input.Substring(input.IndexOf("/,") + 2);
					StatusTime = input.Substring(0, input.IndexOf("/;")); 
				}
				catch { return false; }
				input = input.Substring(input.IndexOf("/;") + 2);
			}
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT fu_add_client(" + TurnId + ", " + IsLive + ", " + StatusTime + ");"; // (client.id, client.index, client.status_time)
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				string ClientId = "";
				if (npgSqlDataReader.HasRows)
				{
					if (npgSqlDataReader.Read())
					{
						result += npgSqlDataReader.GetValue(0).ToString().Substring(1).Replace(",", "/,").Replace(")", "/;").Replace("\"", "");
						ClientId = result.Substring(0, result.IndexOf("/,"));
					}
				}
				else
					result = "";
				npgSqlDataReader.Close();
				//closed turn?
				bool turnIsLive = false;
				sql = "SELECT is_live FROM turns WHERE id =" + TurnId + ";";
				npgSqlCommand = new NpgsqlCommand(sql, conn);
				npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
					if (npgSqlDataReader.Read())
						turnIsLive = npgSqlDataReader.GetBoolean(0);
				npgSqlDataReader.Close();
				if (turnIsLive)
				{
					int maxCount = 0;
					int today = Convert.ToInt32(DateTime.Now.DayOfWeek);
					sql = "SELECT length FROM days_turnlengths WHERE turn_id =" + TurnId + " AND day_id = " + ((today == 0) ? 7 : today) + ";";
					npgSqlCommand = new NpgsqlCommand(sql, conn);
					npgSqlDataReader = npgSqlCommand.ExecuteReader();
					if (npgSqlDataReader.HasRows)
						if (npgSqlDataReader.Read())
							maxCount = npgSqlDataReader.GetInt32(0);
					npgSqlDataReader.Close();
					int cCount = 0;
					sql = "SELECT COUNT(id) FROM clients WHERE path_id IN (SELECT id FROM paths WHERE turn_id = " + TurnId + ") AND status_time::date = '" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "' AND (status_id = 1 OR status_id = 2 OR status_id = 8);";
					npgSqlCommand = new NpgsqlCommand(sql, conn);
					npgSqlDataReader = npgSqlCommand.ExecuteReader();
					if (npgSqlDataReader.HasRows)
						if (npgSqlDataReader.Read())
							cCount = Convert.ToInt32(npgSqlDataReader.GetValue(0));
					npgSqlDataReader.Close();
					if (maxCount > 0)
					{
						if (cCount >= maxCount)
							result += false + "/;";
						else
							result += true + "/;";
					}
					else result += true + "/;";
					result += cCount + "/;";
				}
				else result += true + "/;";
				// other
				if (ClientId != "")
				{
					while (input.IndexOf("/;") > -1)
					{
						string strInfo = input.Substring(0, input.IndexOf("/;"));
						input = input.Substring(input.IndexOf("/;") + 2);
						int InfoId = 0;
						string Value = "";
						try
						{
							InfoId = int.Parse(strInfo.Substring(0, strInfo.IndexOf("/,")));
							strInfo = strInfo.Substring(strInfo.IndexOf("/,") + 2);
							Value = strInfo;
						}
						finally
						{
							sql = "INSERT INTO client_info(client_id, info_id, value) VALUES("+ ClientId +", "+ InfoId +", '"+ Value +"');";
							npgSqlCommand = new NpgsqlCommand(sql, conn);
							npgSqlDataReader = npgSqlCommand.ExecuteReader();
							npgSqlDataReader.Close();
						}
					}
				}
				conn.Close();
			}
			catch (Exception ex)
			{
				// something went wrong, and you wanna know why
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}
		//window
		public bool WindowGetClientStatuses(ref string result)
		{
			//SELECT * FROM statuses;
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT * FROM statuses;";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					while (npgSqlDataReader.Read())
						result += string.Format("{0}/,{1}/,{2}/;",
								npgSqlDataReader.GetInt32(0), npgSqlDataReader.GetString(1), npgSqlDataReader.GetString(2));
				}
				else
					result = "";
				conn.Close();
			}
			catch (Exception ex)
			{
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}
		public bool WindowGetClient(ref string result, string input)
		{
			//SELECT fu_get_client( "WindowId" );
			//SELECT prefix FROM turns WHERE id = (SELECT turn_id FROM clients, paths WHERE clients.id= "ClientId" AND clients.path_id = paths.id);
			//SELECT index, status_id, status_time, is_live FROM clients WHERE id= "ClientId" ;
			//SELECT infos.name, value FROM infos, client_info WHERE client_id= "ClientId" AND info_id = infos.id;
			//SELECT index FROM windows WHERE id= "WindowId" ;
			//SELECT show FROM paths WHERE id IN (SELECT path_id FROM clients WHERE id= "ClientId" );
			try
			{
				string WindowId = input.Substring(0, input.IndexOf("/;"));
				input = input.Substring(input.IndexOf("/;") + 2);
				bool WindowShow = bool.Parse(input.Substring(0, input.IndexOf("/;")));
				int ClientId = 0;
				string Prefix = "";
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				//Получаем ClientId
				string sql = "SELECT fu_get_client(" + WindowId + ");";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					if (npgSqlDataReader.Read())
						if (npgSqlDataReader.GetValue(0).ToString() != "")
							ClientId += npgSqlDataReader.GetInt32(0);
				}
				npgSqlDataReader.Close();
				//Если есть клиент - получаем его инфу.
				if (ClientId != 0)
				{
					int clientNumber = 0;
					int windowNumber = 0;
					result = ClientId + "/,";
					// get turn_prefix
					sql = "SELECT prefix FROM turns WHERE id = (SELECT turn_id FROM clients, paths "+
							"WHERE clients.id = " + ClientId + " AND clients.path_id = paths.id);";
					npgSqlCommand = new NpgsqlCommand(sql, conn);
					npgSqlDataReader = npgSqlCommand.ExecuteReader();
					if (npgSqlDataReader.HasRows)
						if (npgSqlDataReader.Read())
						{
							result += npgSqlDataReader.GetString(0) + "/,";
							Prefix = npgSqlDataReader.GetString(0);
						}
						else
							result = "errorPrefixGetError";
					else
						result = "errorPrefixGetError";
					npgSqlDataReader.Close();
					//get other main info
					sql = "SELECT index, status_id, status_time, is_live FROM clients WHERE id = " + ClientId + ";";
					npgSqlCommand = new NpgsqlCommand(sql, conn);
					npgSqlDataReader = npgSqlCommand.ExecuteReader();
					if (npgSqlDataReader.HasRows)
					{
						while (npgSqlDataReader.Read())
						{
							clientNumber = npgSqlDataReader.GetInt32(0);
							result += string.Format("{0}/,{1}/,{2}/,{3}/;",
									npgSqlDataReader.GetInt32(0), npgSqlDataReader.GetInt32(1), npgSqlDataReader.GetDateTime(2).ToString(), 
									npgSqlDataReader.GetBoolean(3));
						}
					}
					else
						result = "errorInfoGetError";
					npgSqlDataReader.Close();
					//get additional info
					sql = "SELECT infos.name, value FROM infos, client_info WHERE client_id = " + ClientId + " AND info_id = infos.id;";
					npgSqlCommand = new NpgsqlCommand(sql, conn);
					npgSqlDataReader = npgSqlCommand.ExecuteReader();
					if (npgSqlDataReader.HasRows)
						while (npgSqlDataReader.Read())
							result += string.Format("{0}/,{1}/;", npgSqlDataReader.GetString(0), npgSqlDataReader.GetString(1));
					npgSqlDataReader.Close();
					//get window Number
					sql = "SELECT index FROM windows WHERE id = " + WindowId + ";";
					npgSqlCommand = new NpgsqlCommand(sql, conn);
					npgSqlDataReader = npgSqlCommand.ExecuteReader();
					if (npgSqlDataReader.HasRows)
						if (npgSqlDataReader.Read())
							windowNumber = npgSqlDataReader.GetInt32(0);
					npgSqlDataReader.Close();
					//get path show or not
					bool showClient = true;
					sql = "SELECT show FROM paths WHERE id IN (SELECT path_id FROM clients WHERE id = " + ClientId + ");";
					npgSqlCommand = new NpgsqlCommand(sql, conn);
					npgSqlDataReader = npgSqlCommand.ExecuteReader();
					if (npgSqlDataReader.HasRows)
						if (npgSqlDataReader.Read())
							showClient = npgSqlDataReader.GetBoolean(0);
					npgSqlDataReader.Close();
					if (showClient && WindowShow)
					{
						ClientsToSoundAdd(Prefix, clientNumber.ToString(), windowNumber.ToString());
						if (SoundThread.ThreadState == System.Threading.ThreadState.Unstarted)
							SoundThread.Start();
					}
				}
				else
					result = "errorКлиенты для данного окна отсутствуют.";
				conn.Close();
			}
			catch (Exception ex)
			{
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}
		public void WindowSetClientStatus(string Input)
		{
			//SELECT fu_set_client_status( "WindowId" , "ClientId" , "StatusId" );
			try
			{
				string WindowId = Input.Substring(0, Input.IndexOf("/;"));
				Input = Input.Substring(Input.IndexOf("/;") + 2);
				string ClientId = Input.Substring(0, Input.IndexOf("/;"));
				Input = Input.Substring(Input.IndexOf("/;") + 2);
				string StatusId = Input.Substring(0, Input.IndexOf("/;"));
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT fu_set_client_status(" + WindowId + ", " + ClientId + ", " + StatusId + ");";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				npgSqlCommand.ExecuteReader();
				conn.Close();
			}
			catch (Exception ex)
			{
				LogAdd(ex.Message);
			}
		} // "window_id/;client_id/;status_id/;"
		public void WindowGetBackWaitClient(string Input)
		{
			//SELECT fu_get_back_wait_client( "WindowId" , "ClientId" );
			try
			{
				string WindowId = Input.Substring(0, Input.IndexOf("/;"));
				Input = Input.Substring(Input.IndexOf("/;") + 2);
				string ClientId = Input.Substring(0, Input.IndexOf("/;"));
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT fu_get_back_wait_client(" + WindowId + ", " + ClientId + ");";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				npgSqlCommand.ExecuteReader();
				conn.Close();
			}
			catch (Exception ex)
			{
				LogAdd(ex.Message);
			}
		} // "window_id/;client_id/;status_id/;"
		public bool WindowGetCountInfo(ref string result, string WindowId)
		{
			//SELECT fu_get_wait_client_count( "WindowId" );
			//SELECT fu_get_client_in_window_count( "WindowId" );
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT fu_get_wait_client_count(" + WindowId + ");";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					if (npgSqlDataReader.Read())
						if (npgSqlDataReader.GetValue(0).ToString() != "")
							result = npgSqlDataReader.GetInt32(0).ToString() + "/;";
				}
				else
					result = "0/;";
				npgSqlDataReader.Close();
				sql = "SELECT fu_get_client_in_window_count(" + WindowId + ");";
				npgSqlCommand = new NpgsqlCommand(sql, conn);
				npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					if (npgSqlDataReader.Read())
						if (npgSqlDataReader.GetValue(0).ToString() != "")
							result += npgSqlDataReader.GetInt32(0).ToString() + "/;";
				}
				else
					result += "0/;";
				npgSqlDataReader.Close();
				conn.Close();
			}
			catch (Exception ex)
			{
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}
		public bool WindowGetWaitClients(ref string result, string WindowId)
		{
			//SELECT fu_get_wait_clients( "WindowId" );
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT fu_get_wait_clients(" + WindowId + ");";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					result = "";
					while (npgSqlDataReader.Read())
						if (npgSqlDataReader.GetValue(0).ToString() != "")
							result += npgSqlDataReader.GetValue(0).ToString().Substring(1).Replace(",", "/,").Replace(")", "/;");
				}
				else
					result = "0";
				npgSqlDataReader.Close();
				conn.Close();
			}
			catch (Exception ex)
			{
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}
		public void WindowRePlayClient(string Input)
		{
			//SELECT prefix FROM turns WHERE id= (SELECT turn_id FROM clients, paths WHERE clients.id= "ClientId" AND clients.path_id = paths.id);
			//SELECT index FROM clients WHERE id= "ClientId" ;
			//SELECT index FROM windows WHERE id IN (SELECT window_id FROM clients WHERE id= "ClientId" );
			try
			{
				string ClientId = Input;
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
				    DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				// get turn_prefix
				string Prefix = "A";
				string sql = "SELECT prefix FROM turns WHERE id = (SELECT turn_id FROM clients, paths " +
						"WHERE clients.id = " + ClientId + " AND clients.path_id = paths.id);";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
					if (npgSqlDataReader.Read())
						Prefix = npgSqlDataReader.GetString(0);
				npgSqlDataReader.Close();
				// get client index
				string ClientIndex = "0";
				sql = "SELECT index FROM clients WHERE id = " + ClientId + ";";
				npgSqlCommand = new NpgsqlCommand(sql, conn);
				npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
					if (npgSqlDataReader.Read())
						ClientIndex = npgSqlDataReader.GetInt32(0).ToString();
				npgSqlDataReader.Close();
				//get window index
				string WindowIndex = "0";
				sql = "SELECT index FROM windows WHERE id IN (SELECT window_id FROM clients WHERE id = " + ClientId + ");";
				npgSqlCommand = new NpgsqlCommand(sql, conn);
				npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
					if (npgSqlDataReader.Read())
						WindowIndex = npgSqlDataReader.GetInt32(0).ToString();
				npgSqlDataReader.Close();
				conn.Close();
				ClientsToSoundAdd(Prefix, ClientIndex, WindowIndex);
			}
			catch (Exception ex)
			{
				LogAdd(ex.Message);
			}
		} // "window_id/;client_id/;status_id/;"
		//table
		public bool TableGetClients(ref string result, string TableId)
		{
			//SELECT fu_get_table_clients( "TableId" );
			//// returning (id,turnPrefix,Index,statusId,statusTime,isLife)
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT fu_get_table_clients(" + TableId + ");";//(id,turnPrefix,Index,statusId,statusTime,isLife)
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					result = "";
					while (npgSqlDataReader.Read())
						result += npgSqlDataReader.GetValue(0).ToString().Substring(1).Replace(",", "/,").Replace(")", "/;");
				}
				npgSqlDataReader.Close();
				conn.Close();
			}
			catch (Exception ex)
			{
				result = "error" + ex.Message;
				return false;
			}
			return true;
		}
		//tools
		public bool ToolRemoveClient(ref string result, string ClientId)
		{
			//SELECT status_id FROM clients WHERE id= "ClientId" ;
			//SELECT index FROM paths WHERE id IN (SELECT path_id FROM clients WHERE id= "ClientId" );
			//DELETE FROM clients WHERE id= "ClientId" ;
			//DELETE FROM client_info WHERE client_id = "ClientId" ;
			try
			{
				bool cont = true;
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();

				int StatusId = 0;
				string sql = "SELECT status_id FROM clients WHERE id = " + ClientId + ";";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					if (npgSqlDataReader.Read())
					{
						StatusId = npgSqlDataReader.GetInt32(0);
					}
				}
				else
				{
					result = "Клиент не найден";
					cont = false;
				}
				npgSqlDataReader.Close();
				if (cont)
					if (StatusId > 1)
					{
						result = "Невозможно удалить клиента, так как его уже обслуживают.";
						cont = false;
					}
				//
				int PathIndex = 0;
				if (cont)
				{
					sql = "SELECT index FROM paths WHERE id IN (SELECT path_id FROM clients WHERE id = " + ClientId + ");";
					npgSqlCommand = new NpgsqlCommand(sql, conn);
					npgSqlDataReader = npgSqlCommand.ExecuteReader();
					if (npgSqlDataReader.HasRows)
					{
						if (npgSqlDataReader.Read())
						{
							PathIndex = npgSqlDataReader.GetInt32(0);
						}
					}
					else
					{
						result = "Путь не найден. ОБРАТИТЕСЬ К АДМИНИСТРАТОРУ!";
						cont = false;
					}
					npgSqlDataReader.Close();
				}

				if (cont)
					if (PathIndex > 1)
					{
						result = "Невозможно удалить клиента, так как его уже обслуживают (прошел первый кабинет).";
						cont = false;
					}
				// удаление
				if (cont)
				{
					sql = "DELETE FROM clients WHERE id = " + ClientId + ";";
					npgSqlCommand = new NpgsqlCommand(sql, conn);
					npgSqlCommand.ExecuteReader();
					sql = "DELETE FROM client_info WHERE client_id = " + ClientId + ";";
					npgSqlCommand = new NpgsqlCommand(sql, conn);
					npgSqlCommand.ExecuteReader();
					result = "Клиент удален.";
				}
				conn.Close();
				return true;
			}
			catch (Exception ex)
			{
				result = "error" + ex.Message;
				return false;
			}
		}
		
		public void OpenServer()
		{
			try
			{
				serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
				serverSocket.Bind(ipEndPoint);
				serverSocket.Listen(40);
				_opened = true;
				serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);
				LogAdd(">>> Сервер работает (" + Port.ToString() + ")");
				SaveSettings();
			}
			catch (Exception ex)
			{
				LogAdd("!OpenServer: " + ex.Message);
			}
		}
		public void CloseServer()
		{
			try
			{
				_opened = false;
				while (Clients.Count > 0)
				{
					Clients[0].socket.Close();
					Clients.RemoveAt(0);
				}
				serverSocket.Close();
				LogAdd(">>> Сервер не работает");
			}
			catch (Exception ex)
			{
				LogAdd("!CloseServer: " + ex.Message);
			}
		}
		private void OnAccept(IAsyncResult ar)
		{
			try
			{
				if (!Opened) return;
				ClientData cd = new ClientData(serverSocket.EndAccept(ar));
				Clients.Add(cd);
				// продолжаем слушать сокет на наличие новых подключений
				serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);
				// начинаем прослушивать конкретного клиента
				cd.socket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), cd.socket);
				LogAdd(cd.ToString() + " >> Клиент присоединился");
			}
			catch (Exception ex)
			{
				LogAdd("!OnAccept: " + ex.Message);
			}
		}
		private void OnDisconnect(IAsyncResult ar)
		{
			ClientData cd = Clients[Clients.IndexOf(new ClientData((Socket)ar.AsyncState))];
			Clients.Remove(cd);
			LogAdd(cd.ToString() + " >> Клиент отсоединился");
			cd.socket.Close();
		}
		private void OnReceive(IAsyncResult ar)
		{
			try
			{
				if (!Opened) return;
				ClientData cd = Clients[Clients.IndexOf(new ClientData((Socket)ar.AsyncState))];
				cd.socket.EndReceive(ar);

				cd.messageReceived = new Message(byteData);
				cd.messageToSend = new Message();

				LogAdd(cd.ToString() + " > Получено: " + cd.messageReceived.ToString());

				cd.messageToSend.Cmd = cd.messageReceived.Cmd;

				switch (cd.messageReceived.Cmd)
				{
					case "Disconnect": cd.socket.BeginDisconnect(true, new AsyncCallback(OnDisconnect), cd.socket); break;

					case "TableGetClients": TableGetClients(ref cd.messageToSend.Data, cd.messageReceived.Data); break;

					case "TerminalGetTurns": TerminalGetTurns(ref cd.messageToSend.Data); break;
					case "TerminalGetTurnsVisualization": TerminalGetTurnsVisualization(ref cd.messageToSend.Data); break;
					case "TerminalGetInfos": TerminalGetInfos(ref cd.messageToSend.Data); break;
					case "TerminalGetDates": TerminalGetDates(ref cd.messageToSend.Data, cd.messageReceived.Data); break;
					case "TerminalGetDays": TerminalGetDays(ref cd.messageToSend.Data); break;
					case "TerminalAddClient": TerminalAddClient(ref cd.messageToSend.Data, cd.messageReceived.Data); break;

					case "WindowGetClientStatuses": WindowGetClientStatuses(ref cd.messageToSend.Data); break;
					case "WindowGetClient": WindowGetClient(ref cd.messageToSend.Data, cd.messageReceived.Data); break;
					case "WindowSetClientStatus": WindowSetClientStatus(cd.messageReceived.Data); break;
					case "WindowSetBackWaitClient": WindowGetBackWaitClient(cd.messageReceived.Data); break;
					case "WindowGetCountInfo": WindowGetCountInfo(ref cd.messageToSend.Data, cd.messageReceived.Data); break;
					case "WindowGetWaitClients": WindowGetWaitClients(ref cd.messageToSend.Data, cd.messageReceived.Data); break;
					case "WindowRePlayClient": WindowRePlayClient(cd.messageReceived.Data); break;

					case "ToolRemoveClient": ToolRemoveClient(ref cd.messageToSend.Data, cd.messageReceived.Data); break;
				}

				if (cd.messageReceived.Cmd != "Disconnect")
				{
					byte[] message = cd.messageToSend.ToByte();
					cd.socket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), cd.socket);
					cd.socket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), cd.socket);
				}
			}
			catch (Exception ex)
			{
				LogAdd("!OnReceive: " + ex.Message);
			}
		}
		public void OnSend(IAsyncResult ar)
		{
			try
			{
				ClientData cd = Clients[Clients.IndexOf(new ClientData((Socket)ar.AsyncState))];
				cd.socket.EndSend(ar);
				LogAdd(cd.ToString() + " > Отправлено: " + cd.messageToSend.ToString());
			}
			catch (Exception ex)
			{
				LogAdd("!OnSend: " + ex.Message);
			}
		}
	}
}
