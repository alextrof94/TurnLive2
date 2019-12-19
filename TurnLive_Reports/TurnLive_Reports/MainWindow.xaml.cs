using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Npgsql;

namespace TurnLive_Reports
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ObservableCollection<CTurn> turns;
		private string DBHost { get; set; }
		private string DBPort { get; set; }
		private string DBUser { get; set; }
		private string DBPass { get; set; }
		private string DBName { get; set; }
		private string InitialDirectory { get; set; }

		public MainWindow()
		{
			InitializeComponent();
			turns = new ObservableCollection<CTurn>();
			PropertiesFile props = new PropertiesFile(AppDomain.CurrentDomain.BaseDirectory + "properties.properties");
			DBHost = props.get("DBHost", "127.0.0.1");
			DBPort = props.get("DBPort", "5432");
			DBUser = props.get("DBUser", "postgres");
			DBPass = props.get("DBPass", "post"); 
			DBName = props.get("DBName", "turnlive2");
			InitialDirectory = props.get("InitialDirectory", "C:\\");
			DBGetTurns();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			dpFrom.SelectedDate = DateTime.Today;
			dpTo.SelectedDate = DateTime.Today;
			lbTurns.ItemsSource = turns;
		}

		private void cbReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.IsLoaded)
			{
				if (cbReportType.SelectedIndex == 0)
				{
					laType.Content = "Выберите направление:";
					tbWindowId.Visibility = Visibility.Hidden;
					buRefresh.Visibility = Visibility.Visible;
					lbTurns.Visibility = Visibility.Visible;
				}
				if (cbReportType.SelectedIndex == 1)
				{
					laType.Content = "Введите ID окна:";
					tbWindowId.Visibility = Visibility.Visible;
					buRefresh.Visibility = Visibility.Hidden;
					lbTurns.Visibility = Visibility.Hidden;
				}
			}
		}

		private void DBGetTurns()
		{
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				string sql = "SELECT t.id, t.name, t.prefix, t.is_live FROM turns t ORDER BY id ASC;";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				turns.Clear();
				if (npgSqlDataReader.HasRows)
				{
					while (npgSqlDataReader.Read())
					{
						CTurn s = new CTurn();
						s.Id = npgSqlDataReader.GetInt32(0);
						s.Name = npgSqlDataReader.GetString(1);
						s.Prefix = npgSqlDataReader.GetString(2);
						s.IsLive = npgSqlDataReader.GetBoolean(3);
						turns.Add(s);
					}
				}
				conn.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "DBGetTurns");
			}
		}

		private void buRefresh_Click(object sender, RoutedEventArgs e)
		{
			DBGetTurns();
		}

		private string CreateReport0()
		{
			string result = xmlFileHeaders.start;
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				result += xmlFileHeaders.RowS + xmlFileHeaders.CellS + "Отчет по направлению &quot;" + ((CTurn)lbTurns.SelectedItem).Name + "&quot; за период " + ((DateTime)dpFrom.SelectedDate).ToString("yyyy-MM-dd") + " - " + ((DateTime)dpTo.SelectedDate).ToString("yyyy-MM-dd") + xmlFileHeaders.CellE + xmlFileHeaders.RowE;
				string sql = "SELECT c.status_time, c.id, t.prefix, c.index, i.value FROM clients c, client_info i, turns t " +
						"WHERE c.status_time::date >= '" + ((DateTime)dpFrom.SelectedDate).ToString("yyyy-MM-dd") + "' " +
						"AND c.status_time::date <= '" + ((DateTime)dpTo.SelectedDate).ToString("yyyy-MM-dd") + "' " +
						"AND c.path_id IN (SELECT id FROM paths WHERE turn_id = " + ((CTurn)lbTurns.SelectedItem).Id + ") " +
						"AND c.id = i.client_id " +
						"AND t.id = (SELECT turn_id FROM paths WHERE id = c.path_id LIMIT 1) " +
						"ORDER BY c.status_time, c.id ASC;";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					while (npgSqlDataReader.Read())
					{
						//Значения [y - строка,x - столбец]
						result += xmlFileHeaders.RowS;
						result += xmlFileHeaders.CellS + npgSqlDataReader.GetDateTime(0) + xmlFileHeaders.CellE;
						result += xmlFileHeaders.CellS + npgSqlDataReader.GetInt32(1) + xmlFileHeaders.CellE;
						result += xmlFileHeaders.CellS + npgSqlDataReader.GetString(2) + npgSqlDataReader.GetInt32(3) + xmlFileHeaders.CellE;
						result += xmlFileHeaders.CellS + npgSqlDataReader.GetString(4) + xmlFileHeaders.CellE;
						result += xmlFileHeaders.RowE;
					}
				}
				else
					result += xmlFileHeaders.RowS + xmlFileHeaders.CellS + "Нет результатов" + xmlFileHeaders.CellE + xmlFileHeaders.RowE;
				conn.Close();
				result += xmlFileHeaders.end;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "CreateReport0");
			}
			return result;
		}
		private string CreateReport1()
		{
			string result = xmlFileHeaders.start;
			try
			{
				string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
					DBHost, DBPort, DBUser, DBPass, DBName);
				NpgsqlConnection conn = new NpgsqlConnection(connstring);
				conn.Open();
				result += xmlFileHeaders.RowS + xmlFileHeaders.CellS + "Отчет по окну " + tbWindowId.Text + " за период " + ((DateTime)dpFrom.SelectedDate).ToString("yyyy-MM-dd") + " - " + ((DateTime)dpTo.SelectedDate).ToString("yyyy-MM-dd") + xmlFileHeaders.CellE + xmlFileHeaders.RowE;
				string sql = "SELECT c.status_time, c.id, t.prefix, c.index, i.value FROM clients c, client_info i, turns t " +
						"WHERE c.status_time::date >= '" + ((DateTime)dpFrom.SelectedDate).ToString("yyyy-MM-dd") + "' " +
						"AND c.status_time::date <= '" + ((DateTime)dpTo.SelectedDate).ToString("yyyy-MM-dd") + "' " +
						"AND c.id IN (SELECT client_id FROM window_actions WHERE window_id = " + tbWindowId.Text + ") " +
						"AND c.id = i.client_id " +
						"AND t.id = (SELECT turn_id FROM paths WHERE id = c.path_id LIMIT 1) " +
						"AND c.status_id IN (5,6,7,8) " +
						"ORDER BY c.status_time, c.id ASC;";
				NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sql, conn);
				NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
				if (npgSqlDataReader.HasRows)
				{
					while (npgSqlDataReader.Read())
					{
						//Значения [y - строка,x - столбец]
						result += xmlFileHeaders.RowS;
						result += xmlFileHeaders.CellS + npgSqlDataReader.GetDateTime(0) + xmlFileHeaders.CellE;
						result += xmlFileHeaders.CellS + npgSqlDataReader.GetInt32(1) + xmlFileHeaders.CellE;
						result += xmlFileHeaders.CellS + npgSqlDataReader.GetString(2) + npgSqlDataReader.GetInt32(3) + xmlFileHeaders.CellE;
						result += xmlFileHeaders.CellS + npgSqlDataReader.GetString(4) + xmlFileHeaders.CellE;
						result += xmlFileHeaders.RowE;
					}
				}
				else
					result += xmlFileHeaders.RowS + xmlFileHeaders.CellS + "Нет результатов" + xmlFileHeaders.CellE + xmlFileHeaders.RowE;
				conn.Close();
				result += xmlFileHeaders.end;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "CreateReport0");
			}
			return result;
		}

		private void buCreate_Click(object sender, RoutedEventArgs e)
		{
			string result = "";
			switch(cbReportType.SelectedIndex)
			{
				case 0: if (lbTurns.SelectedItem != null) result = CreateReport0(); else MessageBox.Show("Выберите направление"); break;
				case 1: try { int.Parse(tbWindowId.Text); result = CreateReport1(); } catch (Exception ex) { MessageBox.Show(ex.Message); } break;
			}
			if (result != "")
			{
				System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
				sfd.InitialDirectory = InitialDirectory;
				sfd.Filter = "Report for Excel|*.xml";
				if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK && sfd.FileName != "")
				{
					System.IO.StreamWriter w = new System.IO.StreamWriter(sfd.OpenFile(), Encoding.UTF8);
					w.Write(result);
					w.Close();
					PropertiesFile props = new PropertiesFile(AppDomain.CurrentDomain.BaseDirectory + "properties.properties");
					props.set("InitialDirectory", System.IO.Path.GetFullPath(sfd.FileName));
					props.Save();
				}
			}
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
