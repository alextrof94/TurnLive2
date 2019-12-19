using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace TurnLive_Server
{

	delegate void Dcb(Message m);

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
			return (Convert.ToInt32((h < 10) ? ("0" + h.ToString()) : h.ToString())).ToString() + ":" + (Convert.ToInt32((m < 10) ? ("0" + m.ToString()) : m.ToString())).ToString();
		}
	}
	class myTimeInterval 
	{
		public myTime Start { get; set; }
		public myTime End { get; set; }

		public myTimeInterval() : this(new myTime(0), new myTime(0)) { }
		public myTimeInterval(myTime Start, myTime End)
		{
			this.Start = Start;
			this.End = End;
		}
	}
	
	public class ClientData
	{
		public Socket socket { get; set; }
		public Message messageReceived { get; set; }
		public Message messageToSend { get; set; }

		public ClientData(Socket socket)
		{
			this.socket = socket;
		}

		public override int GetHashCode()
		{
			return socket.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			return ((obj as ClientData).socket == socket);
		}
		public override string ToString()
		{
			return socket.RemoteEndPoint.ToString();
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