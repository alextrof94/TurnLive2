using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurnLive_Window
{
	class User
	{
		public int Id { get; set; }
		public string TurnPrefix { get; set; }
		public int Index { get; set; }
		public int StatusId { get; set; }
		public string StatusTime { get; set; }
		public bool IsLive { get; set; }
		public string Time { get { return StatusTime.Substring(0, StatusTime.IndexOf(".") - 3); } }
		public string Number { get { return (IsLive ? "Ж" : "З") + TurnPrefix + Index; } }
		public List<UserInfo> Infos { get; set; }
		
		public User() : this(0, "", 0, 0, "", true) {}
		public User(int Id, string TurnPrefix, int Index, int StatusId, string StatusTime, bool IsLive)
		{
			this.Id = Id;
			this.TurnPrefix = TurnPrefix;
			this.Index = Index;
			this.StatusId = StatusId;
			this.StatusTime = StatusTime;
			this.IsLive = IsLive;
			Infos = new List<UserInfo>();
		}

		public override string ToString()
		{
			return Id + " | " + TurnPrefix + Index + " | " + StatusTime + " | " + Infos[0].ToString();
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
			return Name + ":" + Value + "\n";
		}
	}
	class Status
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string NameInWindow { get; set; }
		public Status() : this(0, "", "") { }
		public Status(int Id, string Name, string NameInWindow) { this.Id = Id; this.Name = Name; this.NameInWindow = NameInWindow; }
		public override string ToString()
		{
			return NameInWindow;
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
				if (!String.IsNullOrWhiteSpace(list[prop]))
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