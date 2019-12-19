using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace TurnLive_AdminTools
{
	class CTime
	{
		public int Main { get; set; }
		public int Minute { get { return Main % 60; } }
		public int Hour { get { return Main / 60; } }
		public CTime() : this(0) { }
		public CTime(int Main) { this.Main = Main; }
		public CTime(string HM)
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

	class CTurn
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Prefix { get; set; }
		public bool IsLive { get; set; }

		public override string ToString()
		{
			return Id.ToString() + " | " + Prefix + " | " + Name + " | " + IsLive.ToString();
		}
	}
	class CPath
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Index { get; set; }
		public bool Show { get; set; }
	}
	class CTurnVisualization
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int TurnId { get; set; }
		public int tvMainId { get; set; }
		public bool IsFms { get; set; }
	}
	class CDay
	{
		public int Id { get; set; }
		public bool Work { get; set; }
		public bool Record { get; set; }	
	}
	class CDayWorktime
	{
		public int DayId { get; set; }
		public CTime Start { get; set; }
		public CTime End { get; set; }
	}
	class CTurnLength
	{
		public int DayId { get; set; }
		public int TurnId { get; set; }
		public int Length { get; set; }
	}
	class CWindow
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Index { get; set; }
	}
	class CWindowPath
	{
		public int WindowId { get; set; }
		public int PathId { get; set; }
	}
	class CTable
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int RowCount { get; set; }
	}
	class CTableWindow
	{
		public int TableId { get; set; }
		public int WindowId { get; set; }
	}
	class CInfo
	{
		public int Id { get; set; }
		public bool Required { get; set; }
		public string Name { get; set; }
		public string Pattern { get; set; }
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