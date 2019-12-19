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

namespace TurnLive_Table
{
	public partial class MainWindow : Window
	{
		const string ProgrammName = "TurnLive Table v2.2";
		// forms
		ServerSettings foServerSettings;
		Settings foSettings;
		// client
		Client_Table client;
		// args
		bool FullScreen = true;
		bool Silent = false;
		bool ShowLog = false;
		int FontSize = 72;
		int HourChange = 0;

		int InfoNow = 0;
		string[] Information;
        double laInfoStartLength = 0;
        double laInfoStartLeft = 0;

        public MainWindow()
		{
			InitializeComponent();
            laInfoStartLength = laInfo.Width;
            laInfoStartLeft = laInfo.Margin.Left;

            if (System.IO.File.Exists(Environment.CurrentDirectory + "\\Information.txt"))
				Information = File.ReadAllLines(Environment.CurrentDirectory + "\\Information.txt");
			else
			{
				MessageBox.Show(Environment.CurrentDirectory + "\\Information.txt");
				Information = new string[1];
				Information[0] = "Файл Information.txt не найден";
			}
			TextChange();
			client = new Client_Table();
			FontSize = client.FontSize;
			HourChange = client.HourChange;
			foreach (string s in App.Arguments)
			{
				if (s == "-FS")
					FullScreen = false;
				if (s == "-SI")
					Silent = true;
				if (s == "-SL")
					ShowLog = true;
				if (s.IndexOf("-F") > -1)
					try { FontSize = int.Parse(s.Substring(2)); }
					catch { FontSize = 72; }
				if (s.IndexOf("-h") > -1)
					try { HourChange = int.Parse(s.Substring(2)); }
					catch { HourChange = 0; }
			}
			client.FontSize = FontSize;
			foSettings = new Settings();
			this.dgList.FontSize = FontSize;
			foServerSettings = new ServerSettings();
			lbLog.ItemsSource = client.Log;
			client.Log.Add("Клиент включен.");
			if (ShowLog)
				lbLog.Visibility = Visibility.Visible;
			dgList.ItemsSource = client.UserList;
			client.cbShowMessage += ShowMessage;
			if (!FullScreen)
			{
				this.WindowState = System.Windows.WindowState.Normal;
				this.ResizeMode = System.Windows.ResizeMode.CanResize;
				this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
			}
			tiReload = new System.Timers.Timer();
			tiReload.Elapsed += new System.Timers.ElapsedEventHandler(tiReloadTick);
			tiReload.Interval = client.ReloadInterval;
            tiTextChange = new System.Timers.Timer();
            tiTextChange.Elapsed += new System.Timers.ElapsedEventHandler(tiTextChangeTick);
            tiTextChange.Interval = 15000;
            tiTextRoll = new System.Timers.Timer();
            tiTextRoll.Elapsed += new System.Timers.ElapsedEventHandler(tiTextRollTick);
            tiTextRoll.Interval = 60000;
            laTime.Content = DateTime.Now.AddHours(HourChange).ToString("HH:mm");
		}

		private void ShowMessage(string str)
		{
			App.Current.Dispatcher.Invoke((Action)delegate { laInform.Content = str; });
		}

		System.Timers.Timer tiReload;
		private void tiReloadTick(object sender, EventArgs e)
		{
			Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate { Reload(); }));
		}
		private void Reload()
		{
			laTime.Content = DateTime.Now.AddHours(HourChange).ToString("HH:mm");
			if (client.Opened)
				client.GetFromServerClients();
		}

        System.Timers.Timer tiTextChange;
        private void tiTextChangeTick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate { TextChange(); }));
        }
        private void TextChange()
        {
            if (InfoNow < Information.Length - 1)
                InfoNow++;
            else
                InfoNow = 0;

            if (laInfoStartLength < laInfo.Width)
            {
                tiTextRoll.Interval = tiTextChange.Interval / (laInfo.Width - laInfoStartLength);
                tiTextRoll.Enabled = true;
            }
            else
            {
                tiTextRoll.Enabled = false;
            }
            laInfo.Margin = new Thickness(laInfoStartLeft, 12, 155, 0);
            laInfo.Content = Information[InfoNow];
        }

        System.Timers.Timer tiTextRoll;
        private void tiTextRollTick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate { TextRoll(); }));
        }
        private void TextRoll()
        {
            laInfo.Margin = new Thickness(laInfo.Margin.Left - 1, 12, 155, 0);
        }

        private void StartTimers()
		{
			client.SaveSettings();
			tiReload.Start();
			tiTextChange.Start();
			Reload();
		}

		private void ConnectError()
		{
			if (tiReload.Enabled)
				tiReload.Stop();
			if (tiTextChange.Enabled)
				tiTextChange.Stop();
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
			client.cbConnect = StartTimers;
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

		private void laTime_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (!Silent)
			{
				foSettings.tbTableId.Text = client.TableId.ToString();
				foSettings.tbInterval.Text = client.ReloadInterval.ToString();
				foSettings.tbFontSize.Text = FontSize.ToString();
				foSettings.ShowDialog();
				client.TableId = int.Parse(foSettings.tbTableId.Text);
				client.ReloadInterval = int.Parse(foSettings.tbInterval.Text);
				client.FontSize = int.Parse(foSettings.tbFontSize.Text);
				FontSize = client.FontSize;
				client.SaveSettings();
			}
		}
	}
}
