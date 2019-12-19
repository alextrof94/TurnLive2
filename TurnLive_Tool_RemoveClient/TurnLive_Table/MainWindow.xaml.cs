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
using System.IO;

namespace TurnLive_Tool_RemoveClient
{
	public partial class MainWindow : Window
	{
		const string PName = "TurnLive Tool RemoveClient";
		const string PVersion = "v2.2";
		private string ProgrammName { get { return PName + " " + PVersion; } }
		// forms
		ServerSettings foServerSettings;
		// client
		Client_Tool_RemoveClient client;
		// args
		bool ShowLog = false;

		public MainWindow()
		{
			InitializeComponent();
			client = new Client_Tool_RemoveClient();
			foreach (string s in App.Arguments)
			{
				if (s == "-SL")
					ShowLog = true;
			}
			foServerSettings = new ServerSettings();
			lbLog.ItemsSource = client.Log;
			client.Log.Add(ProgrammName + " Клиент включен.");
			if (ShowLog)
				lbLog.Visibility = Visibility.Visible;
			client.cbShowMessage += ShowMessage;
			client.cbShowResponse += ShowResponse;
		}

		private void ShowMessage(string str)
		{
			App.Current.Dispatcher.Invoke((Action)delegate { laInform.Content = str; });
		}
		private void ShowResponse(string str)
		{
			App.Current.Dispatcher.Invoke((Action)delegate 
			{ 
				tbResponse.Text = str;
				buSend.IsEnabled = true;
			});
		}

		private void ConnectError()
		{
			tbClientId.IsEnabled = false;
			buSend.IsEnabled = false;
			MessageBoxResult result;
			result = MessageBox.Show("Ошибка подключения к " + client.Host + ":" + client.Port +
					"\nИзменить адрес сервера? (отмена - выход)", "Ошибка", MessageBoxButton.YesNoCancel);
			if (result == MessageBoxResult.Yes)
			{
				foServerSettings.Host = client.Host;
				foServerSettings.Port = client.Port;
				foServerSettings.ShowDialog();
				client.Host = foServerSettings.Host;
				client.Port = foServerSettings.Port;
				Connect();
				return;
			}
			else if (result == MessageBoxResult.No)
			{
				Connect();
				return;
			}
			else
			{
				MessageBox.Show("Приложение будет закрыто", ProgrammName, MessageBoxButton.OK, MessageBoxImage.Information);
				CloseProgramm();
				return;
			}
		}
		private void Start()
		{
			tbClientId.IsEnabled = true;
			buSend.IsEnabled = true;
		}
		private void Connect()
		{
			client.cbConnect = Start;
			client.cbConnectError = ConnectError;
			client.OpenClient();
		}

		private void CloseProgramm()
		{
			client.Disconnect();
			client.Log.Add("Клиент выключен.");
			client.LogSave();
			client.SaveSettings();
			Application.Current.Shutdown();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			CloseProgramm();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.Title = ProgrammName;
			Connect();
		}

		private void buSend_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int.Parse(tbClientId.Text);
				buSend.IsEnabled = false;
				client.RemoveClient(tbClientId.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, ProgrammName, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
