using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace TurnLive_Terminal
{
	class User
	{
		public int Id { get; set; }
		public int TurnId { get; set; }
		public string TurnPrefix { get; set; }
		public int Index { get; set; }
		public int StatusId { get; set; }
		public string StatusTime { get; set; }
		public string TurnLine { get; set; }
		public bool IsLive { get; set; }
		public bool turnEnabled { get; set; }
		public ObservableCollection<UserInfo> Infos { get; set; }
		
		public User() : this(0, "", 0, 0, "", true) {}
		public User(int Id, string TurnPrefix, int Index, int StatusId, string StatusTime, bool IsLive)
		{
			this.Id = Id;
			this.TurnPrefix = TurnPrefix;
			this.Index = Index;
			this.StatusId = StatusId;
			this.StatusTime = StatusTime;
			this.IsLive = IsLive;
			Infos = new ObservableCollection<UserInfo>();
			TurnId = 0;
			turnEnabled = false;
			TurnLine = "";
		}
	}
	class UserInfo
	{
		public string Name { get; set; }
		public string Value { get; set; }

		public UserInfo() : this("", "") {}
		public UserInfo(string Name, string Value)
		{
			this.Name = Name;
			this.Value = Value;
		}
		public override string ToString()
		{
			return Name + ": " + Value;
		}
	}
	class Info
	{
		public int Id { get; set; }
		public bool Required { get; set; }
		public string Name { get; set; }
		public string Pattern { get; set; }

		public Info(int Id, bool Required, string Name, string Pattern)
		{
			this.Id = Id;
			this.Required = Required;
			this.Name = Name;
			this.Pattern = Pattern;
		}
		public override string ToString()
		{
			return Name;
		}
	}
	class Turn
	{
		public int Id { get; set; }
		public string Prefix { get; set; }
		public string Name { get; set; }
		public bool IsLive { get; set; }
		public bool Enabled { get; set; }
		public Turn() : this(0, "", "", false) { }
		public Turn(int Id, string Prefix, string Name, bool IsLive)
		{
			this.Id = Id;
			this.Prefix = Prefix;
			this.Name = Name;
			this.IsLive = IsLive;
			Enabled = true;
		}
	}
	class TurnVisualization
	{
		public int Id { get; set; }
		public int TurnId { get; set; }
		public string Name { get; set; }
		public int TvMainId { get; set; }
		public bool Fms { get; set; }
		public TurnVisualization() : this(0, 0, "", 0, false) { }
		public TurnVisualization(int Id, int TurnId, string Name, int TvMainId, bool Fms)
		{
			this.Id = Id;
			this.TurnId = TurnId;
			this.Name = Name;
			this.TvMainId = TvMainId;
			this.Fms = Fms;
		}
	}
	class Day
	{
		public int Id { get; set; }
		public bool Work { get; set; }
		public bool Terminal { get; set; }
		public Day() : this(0, false, false) { }
		public Day(int Id, bool Work, bool Terminal)
		{
			this.Id = Id;
			this.Work = Work;
			this.Terminal = Terminal;
		}
	}
	class Date
	{
		public int DayId { get; set; }
		public bool Working { get; set; }
		public ObservableCollection<myTime> times { get; set; }
		public Date() : this(0, false) { }
		public Date(int DayId, bool Working)
		{
			this.DayId = DayId;
			this.Working = Working;
			times = new ObservableCollection<myTime>();
		}
		public override string ToString()
		{
			string result = "";
			int offset = DayId - Convert.ToInt32(DateTime.Now.DayOfWeek);
			result += DateTime.Now.AddDays(offset).Date.ToShortDateString();
			switch (Convert.ToInt32(DateTime.Now.AddDays(offset).DayOfWeek))
			{
				case 0: result += " Воскресенье"; break;
				case 1: result += " Понедельник"; break;
				case 2: result += " Вторник";     break;
				case 3: result += " Среда";       break;
				case 4: result += " Четверг";     break;
				case 5: result += " Пятница";     break;
				case 6: result += " Суббота";     break;
				default: break; 
			}
			return result;
		}
		public string ToShortString()
		{
			string result = "";
			int offset = DayId - Convert.ToInt32(DateTime.Now.DayOfWeek);
			result += DateTime.Now.AddDays(offset).Date.ToString("yyyy-MM-dd");
			return result;
		}
	}
	class myTime
	{
		public int Main { get; set; }
		public int Minute { get { return Main % 60; } }
		public int Hour { get { return Main / 60; } }
		public myTime() : this(0) { }
		public myTime(int Main) { this.Main = Main; }
		public myTime(string HM)
		{
			try
			{
				int h = int.Parse(HM.Substring(0, HM.IndexOf(":")));
				int m = int.Parse(HM.Substring(HM.IndexOf(":") + 1));
				this.Main = h * 60 + m;
			}
			catch
			{
				this.Main = 0;
			}
		}
		public override string ToString()
		{
			int h = Main / 60;
			int m = Main % 60;
			return ((h < 10) ? ("0" + h.ToString()) : h.ToString()) + ":" + ((m < 10) ? ("0" + m.ToString()) : m.ToString());
		}
	}

	public class Message
	{
		public int Id;
		public string Cmd;
		public string Data;

		public Message()
		{
			this.Id = 0;
			this.Cmd = "";
			this.Data = "";
		}

		public Message(byte[] data)
		{
			this.Id = BitConverter.ToInt32(data, 0);
			int cmdLen = BitConverter.ToInt32(data, 4);
			if (cmdLen > 0)
				this.Cmd = Encoding.Unicode.GetString(data, 8, cmdLen);
			else
				this.Cmd = "";
			int dataLen = BitConverter.ToInt32(data, 8 + cmdLen);
			if (dataLen > 0)
				this.Data = Encoding.Unicode.GetString(data, 12 + cmdLen, dataLen);
			else
				this.Data = "";
		}

		public byte[] ToByte()
		{
			List<byte> result = new List<byte>();
			result.AddRange(BitConverter.GetBytes(Id));
			result.AddRange(BitConverter.GetBytes(Cmd.Length * 2));
			result.AddRange(Encoding.Unicode.GetBytes(Cmd));
			result.AddRange(BitConverter.GetBytes(Data.Length * 2));
			result.AddRange(Encoding.Unicode.GetBytes(Data));
			return result.ToArray();
		}

		public override string ToString()
		{
			return Id.ToString() + "|" + Cmd + "|" + Data;
		}
	}

	public class PropertiesFile
	{
		private Dictionary<String, String> list;
		private String filename;

		public PropertiesFile(String file)
		{
			reload(file);
		}

		public String get(String field, String defValue)
		{
			return (get(field) == null) ? (defValue) : (get(field));
		}
		public String get(String field)
		{
			return (list.ContainsKey(field)) ? (list[field]) : (null);
		}

		public void set(String field, Object value)
		{
			if (!list.ContainsKey(field))
				list.Add(field, value.ToString());
			else
				list[field] = value.ToString();
		}

		public void Save()
		{
			Save(this.filename);
		}

		public void Save(String filename)
		{
			this.filename = filename;

			if (!System.IO.File.Exists(filename))
				System.IO.File.Create(filename);

			System.IO.StreamWriter file = new System.IO.StreamWriter(filename);

			foreach (String prop in list.Keys.ToArray())
				if (!(list[prop] == null || list[prop] == ""))
					file.WriteLine(prop + "=" + list[prop]);

			file.Close();
		}

		public void reload()
		{
			reload(this.filename);
		}

		public void reload(String filename)
		{
			this.filename = filename;
			list = new Dictionary<String, String>();

			if (System.IO.File.Exists(filename))
				loadFromFile(filename);
			else
				System.IO.File.Create(filename);
		}

		private void loadFromFile(String file)
		{
			foreach (String line in System.IO.File.ReadAllLines(file))
			{
				if ((!String.IsNullOrEmpty(line)) &&
					(!line.StartsWith(";")) &&
					(!line.StartsWith("#")) &&
					(!line.StartsWith("'")) &&
					(line.Contains('=')))
				{
					int index = line.IndexOf('=');
					String key = line.Substring(0, index).Trim();
					String value = line.Substring(index + 1).Trim();

					if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
						(value.StartsWith("'") && value.EndsWith("'")))
					{
						value = value.Substring(1, value.Length - 2);
					}

					try
					{
						//ignore dublicates
						list.Add(key, value);
					}
					catch { }
				}
			}
		}
	}
}