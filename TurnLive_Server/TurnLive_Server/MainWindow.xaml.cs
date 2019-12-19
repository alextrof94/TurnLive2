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

namespace TurnLive_Server
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		string ProgrammName = "TurnLive Server v2.2";
		Server server;
		HostSettings foHostSettings;

		public MainWindow()
		{
			InitializeComponent();
			server = new Server();
			server.OpenServer();
			if (server.Opened)
			{
				buServer.Content = "Остановить";
				buClose.IsEnabled = false;
				buSettings.IsEnabled = false;
			}
			lbLog.ItemsSource = server.Log;
			foHostSettings = new HostSettings();
		}

		private void Moving(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed) { this.DragMove(); }
		}

		private void buServer_Click(object sender, RoutedEventArgs e)
		{
			if (server.Opened)
				server.CloseServer();
			else
				server.OpenServer();
			if (!server.Opened)
			{
				buServer.Content = "Запустить";
				buClose.IsEnabled = true;
				buSettings.IsEnabled = true;
			}
			else
			{
				buServer.Content = "Остановить";
				buClose.IsEnabled = false;
				buSettings.IsEnabled = false;
			}
		}

		private void buClose_Click(object sender, RoutedEventArgs e) 
		{
			this.Close();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			if (server.SoundThread.ThreadState != System.Threading.ThreadState.Unstarted)
				server.SoundThread.Abort();
			if (server.Opened)
				server.CloseServer();
			server.LogSave();
			server.SaveSettings();
			App.Current.Shutdown();
		}

		private void buSettings_Click(object sender, RoutedEventArgs e)
		{
			foHostSettings.tbPort.Text = server.Port.ToString();
			foHostSettings.ShowDialog();
			server.Port = int.Parse(foHostSettings.tbPort.Text);
			server.SaveSettings();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.Title = ProgrammName;
		}

	}
}
