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

namespace TurnLive_Window
{
	public partial class MainWindow : Window
	{
		const string ProgrammName = "TurnLive Window v2.2";
		// forms
		Wait foWait;
		Password foPassword;
		ServerSettings foServerSettings;
		Settings foSettings;
		// client
		Client_Window client;
		// args
		bool FullScreen = false;
		bool Silent = false;
		bool ShowLog = false;
		// other
		System.Timers.Timer tiWait;
		System.Timers.Timer tiButtonsEnabled;
		private bool ButtonsEnabled = false;

		private bool _reGetClient = false;

		public MainWindow()
		{
			foreach (string s in App.Arguments)
			{
				if (s == "-FS")
					FullScreen = true;
				if (s == "-SI")
					Silent = true;
				if (s == "-SL")
					ShowLog = true;
				if (s == "-NC")
					cbNext.IsChecked = false;
			}
			InitializeComponent();
			foPassword = new Password();
			foServerSettings = new ServerSettings();
			foSettings = new Settings();
			client = new Client_Window();
			foWait = new Wait();
			lbLog.ItemsSource = client.Log;
			client.Log.Add("Клиент включен.");
			if (ShowLog)
				lbLog.Visibility = Visibility.Visible;
			client.cbShowMessage = ShowMessage;
			client.cbUpdateInfo = UpdateInfo;
			foWait.dgWait.ItemsSource = client.WaitClients;
			foWait.buGetBack.Click += buGetBack_Click;
			if (Silent)
			{
				buSettings.Visibility = Visibility.Hidden;
			}
			if (FullScreen)
			{
				this.WindowState = System.Windows.WindowState.Maximized;
				this.ResizeMode = System.Windows.ResizeMode.NoResize;
				buClose.IsEnabled = false;
				buClose.Visibility = Visibility.Hidden;
				buWait.Visibility = Visibility.Hidden;
				buInfo.Visibility = Visibility.Hidden;
			}
			tiWait = new System.Timers.Timer();
			tiWait.Interval = 30000;
			tiWait.Elapsed += tiWait_Tick;
			tiButtonsEnabled = new System.Timers.Timer();
			tiButtonsEnabled.Interval = 30000;
			tiButtonsEnabled.Elapsed += tiButtonsEnabled_Tick;
		}

		private void tiWait_Tick(object Sender, System.Timers.ElapsedEventArgs args)
		{
			client.GetFromServerCountInfo();
		}
		private void tiButtonsEnabled_Tick(object Sender, System.Timers.ElapsedEventArgs args)
		{
			tiButtonsEnabled.Stop();
			SetButtonEnable();
		}

		private void ShowMessage(string str, bool ClientActionEnable)
		{
			App.Current.Dispatcher.Invoke( (Action)delegate 
					{
						tbInfo.Text = str;
						buClientAction.IsEnabled = ClientActionEnable;
						if (!ClientActionEnable)
						{
							ButtonsEnabled = false;
							SetButtonEnable();
						}
						else
						{
							client.GetFromServerCountInfo();
						}
					});
		}
		private void UpdateInfo(string str)
		{
			if (str.Length > 0)
			{
				string strWait = str.Substring(0, str.IndexOf("/;"));
				str = str.Substring(str.IndexOf("/;") + 2);
				string strWindowClients = str.Substring(0, str.IndexOf("/;"));
				if (int.Parse(strWait) > 0)
					buWait.IsEnabled = true;
				buWait.Content = "Отложенная очередь (" + strWait + ")";
				laWindowClients.Content = strWindowClients;
			}
		}

		private void ShowClient()
		{
			tbInfo.Text = "";
			for (int i = 0; i < client.ClientStatuses.Count; i++)
				if (client.ClientStatuses[i].Id == client.user.StatusId)
					if (client.ClientStatuses[i].Name.ToUpper() == "ОПОЗДАЛ")
						tbInfo.Text += "Данный клиент уже опоздал на прием\n";
			tbInfo.Text += "ID: " + client.user.Id + "\n";
			tbInfo.Text += "Номер: " + client.user.TurnPrefix + client.user.Index + "\n";
			for (int i = 0; i < client.user.Infos.Count; i++)
				tbInfo.Text += client.user.Infos[i].ToString();
			buClientAction.Content = "Присвоить статус";
			ButtonsEnabled = true;
			SetButtonEnable();
			client.GetFromServerCountInfo();
		}
		private void StartProgramm()
		{
			cbStatus.ItemsSource = client.ClientStatuses;
			buClientAction.IsEnabled = true;
			client.cbShowClient = ShowClient;
			if (_reGetClient)
			{
				client.GetFromServerClient(false);
				buClientAction.IsEnabled = false;
				_reGetClient = false;
			}
			tbInfo.Text = "Соединение установлено.\nДля начала работы нажмите \"Вызвать клиента\"\n";
		}
		private void GetStatuses()
		{
			client.cbGetStatuses = StartProgramm;
			client.SaveSettings();
			client.GetFromServerClientStatuses();
			tiWait.Start();
		}
		private void ConnectError()
		{
			if (client.user != null)
				_reGetClient = true;
			client.user = null;
			buClientAction.Content = "Вызвать клиента";
			tiWait.Stop();
			ButtonsEnabled = false;
			SetButtonEnable();
			MessageBoxResult result;
			if (Silent)
				result = MessageBoxResult.No;
			else
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
		private void Connect()
		{
			client.cbConnect = GetStatuses;
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

		//static UI
		private void buClose_Click(object sender, RoutedEventArgs e) { this.Close(); }
		private void buWait_Click(object sender, RoutedEventArgs e) 
		{
			client.GetFromServerWaitClients();
			foWait.Show();
		}

		private void Moving(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed) { this.DragMove(); }
		}
		
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.Title = ProgrammName;
			ButtonsEnabled = false;
			SetButtonEnable();
			Connect();
		}

		private void buInfo_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(
					"Программа: " + ProgrammName + "\n" +
					"составляющая комплекса программ TurnLive (Server, Terminal, Window, Table)\n" +
					"Автор: Трофимов Александр\n" +
					"e-mail: alextrof94@gmail.com"
					, "Информация о программе и авторе", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void buSettings_Click(object sender, RoutedEventArgs e)
		{
			foSettings.tbWindowId.Text = client.WindowId.ToString();
			foSettings.ShowDialog();
			client.WindowId = int.Parse(foSettings.tbWindowId.Text);
			client.SaveSettings();
		}

		private void buClientAction_Click(object sender, RoutedEventArgs e)
		{
			if (client.user == null)
			{
				client.GetFromServerClient();
				buClientAction.IsEnabled = false;
			}
			else
			{
				if (cbNext.IsChecked.Value)
				{
					//set status
					client.SendToServerClientStatus((cbStatus.SelectedItem as Status).Id);
					buClientAction.Content = "Вызвать клиента";
					ButtonsEnabled = false;
					SetButtonEnable();
					tbInfo.Text = "";
					//get client
					client.GetFromServerClient();
				}
				else
				{
					//set status
					client.SendToServerClientStatus((cbStatus.SelectedItem as Status).Id);
					buClientAction.Content = "Вызвать клиента";
					ButtonsEnabled = false;
					SetButtonEnable();
					buClientAction.IsEnabled = true;
					tbInfo.Text = "Ожидание клиента";
				}
				client.user = null;
			}
			cbStatus.SelectedIndex = 0;
		}

		private void buGetBack_Click(object sender, RoutedEventArgs e)
		{
			//возврат клиента в очередь
			if (foWait.dgWait.SelectedItem != null)
			{
				if ((foWait.dgWait.SelectedItem as User).Id > 0)
					client.SendToServerSetBackWaitClient((foWait.dgWait.SelectedItem as User).Id);
			}
			foWait.Hide();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			CloseProgramm();
		}

		private void buRePlay_Click(object sender, RoutedEventArgs e)
		{
			client.RePlayClient();
		}

		private void SetButtonEnable()
		{
			cbNext.IsEnabled = ButtonsEnabled;
			cbStatus.IsEnabled = ButtonsEnabled;
			buClientAction.IsEnabled = ButtonsEnabled;
			buWait.IsEnabled = ButtonsEnabled;
			buRePlay.IsEnabled = ButtonsEnabled;
		}
	}
}
