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

namespace TurnLive_AdminTools
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		const string PName = "TurnLive AdminTools";
		const string PVersion = "v2.2";
		private string ProgrammName { get { return PName + " " + PVersion; } }

		private string DBHost { get; set; }
		private string DBPort { get; set; }
		private string DBUser { get; set; }
		private string DBPass { get; set; }
		private string DBName { get; set; }

		private ObservableCollection<CTurn> turns;
		private ObservableCollection<CPath> paths;
		private ObservableCollection<CTurnVisualization> turnVisualizations;
		private ObservableCollection<CDay> days;
		private ObservableCollection<CDayWorktime> dayWorktimes;
		private ObservableCollection<CTurnLength> turnLengths;
		private ObservableCollection<CWindow> windows;
		private ObservableCollection<CWindowPath> windowPaths;
		private ObservableCollection<CTable> tables;
		private ObservableCollection<CTableWindow> tableWindows;
		private ObservableCollection<CInfo> infos;

		public MainWindow()
		{
			InitializeComponent();
			turns = new ObservableCollection<CTurn>();
			lbTurnTurns.ItemsSource = turns;
			paths = new ObservableCollection<CPath>();
			turnVisualizations = new ObservableCollection<CTurnVisualization>();
			days = new ObservableCollection<CDay>();
			dayWorktimes = new ObservableCollection<CDayWorktime>();
			turnLengths = new ObservableCollection<CTurnLength>();
			windows = new ObservableCollection<CWindow>();
			windowPaths = new ObservableCollection<CWindowPath>();
			tables = new ObservableCollection<CTable>();
			tableWindows = new ObservableCollection<CTableWindow>();
			infos = new ObservableCollection<CInfo>();
			PropertiesFile props = new PropertiesFile(AppDomain.CurrentDomain.BaseDirectory + "properties.properties");
			DBHost = props.get("DBHost", "127.0.0.1");
			DBPort = props.get("DBPort", "5432");
			DBUser = props.get("DBUser", "postgres");
			DBPass = props.get("DBPass", "post");
			DBName = props.get("DBName", "turnlive2");
			Refresh();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			double w = 100;
			w = (grMain.ActualWidth - 26) / 2;

			grTurnTurns.Width = w;
			grPathPaths.Width = w;
			grTurnAdd.Width = w;
			grPathAdd.Width = w;
			grButtonsAll.Width = w;
			grButtonsAdd.Width = w;
			grWindowsAll.Width = w;
			grWindowsAdd.Width = w;
			grWindowsPaths.Width = w;
			grWindowsPathsAdd.Width = w;
			grDaysAll.Width = w;
			grDaysParams.Width = w;
			grDaysIntervals.Width = w;
			grDaysIntervalsAdd.Width = w;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.Title = ProgrammName;
		}

		private void Refresh()
		{
			DBGetTurns();
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
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "DBGetTurns");
			}
		}

		private void buRefresh_Click(object sender, RoutedEventArgs e)
		{
			Refresh();
		}
	}
}
