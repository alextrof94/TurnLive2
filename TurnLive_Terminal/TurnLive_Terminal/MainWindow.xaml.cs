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

namespace TurnLive_Terminal
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
    /* COMPORT UPDATE 12.04.18 */
        private static char ESC = (char)27;
        private static char LF = (char)13;
        /* new string in Citizen */
        private static char CR = (char)10;
        private static char SP = (char)32;
        /* Шрифт типа А (12*24) выделенный, двойной ширины, двойной высоты */
        private static String FONT_A_DH_DW_B = new String(new char[] { ESC, (char)33, (char)56 });
        /* Шрифт типа А (12*24) двойной ширины, двойной высоты */
        private static String FONT_A_DH_DW = new String(new char[] { ESC, (char)33, (char)48 });
        /* Шрифт типа А (12*24) */
        private static String FONT_A = new String(new char[] { ESC, (char)33, (char)0 });
        /* Шрифт типа B (9*17) */
        private static String FONT_B = new String(new char[] { ESC, (char)33, (char)1 });
        /* Шрифт типа B (9*17) выделенный, двойной ширины, двойной высоты */
        private static String FONT_B_DH_DW_B = new String(new char[] { ESC, (char)33, (char)57 });
        /* Шрифт типа B (9*17) двойной ширины, двойной высоты */
        private static String FONT_B_DH_DW = new String(new char[] { ESC, (char)33, (char)49 });
        /* Выбор кодовой страницы CP866 */
        private static String CP866 = new String(new char[] { ESC, (char)116, (char)7 });
        /* Отрезка ленты (команда должна следовать за CR) */
        private static String CUT = new String(new char[] { CR, ESC, (char)105 });
    /* END COMPORT UPDATE */
		const string ProgrammName = "TurnLive Terminal v2.3 (12.04.18)";
		// forms
		InputText foInputText;
		ServerSettings foServerSettings;
		Days foDays;
		TurnsVizualization foTurnsVizualization;
		PrintTemplate foPrintTemplate;
		WhiteScreen foWhiteScreen;
		// client
		Client_Terminal client;
		// args
		bool FullScreen = true;
		bool Silent = false;
		bool ShowLog = false;

		public MainWindow()
		{
			foreach (string s in App.Arguments)
			{
				if (s == "-FS")
					FullScreen = false;
				if (s == "-SI")
					Silent = true;
				if (s == "-SL")
					ShowLog = true;
			}
			InitializeComponent();
			foInputText = new InputText();
			foServerSettings = new ServerSettings();
			foWhiteScreen = new WhiteScreen();
			foPrintTemplate = new PrintTemplate();
			foTurnsVizualization = new TurnsVizualization();
			foDays = new Days();
			client = new Client_Terminal();
			lbLog.ItemsSource = client.Log;
			client.Log.Add("Клиент включен.");
			if (ShowLog)
				lbLog.Visibility = Visibility.Visible;
			client.cbClientPrint = ClientPrint;
			client.cbShowMessage = ShowMessage;
			client.cbConnectErrorHideScreen = HideScreen;
			if (!FullScreen)
			{
				this.WindowState = System.Windows.WindowState.Normal;
				this.ResizeMode = System.Windows.ResizeMode.CanResize;
				this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
				foTurnsVizualization.WindowState = System.Windows.WindowState.Normal;
				foTurnsVizualization.ResizeMode = System.Windows.ResizeMode.NoResize;
				foInputText.WindowState = System.Windows.WindowState.Normal;
				foInputText.ResizeMode = System.Windows.ResizeMode.NoResize;
				foWhiteScreen.WindowState = System.Windows.WindowState.Normal;
				foWhiteScreen.ResizeMode = System.Windows.ResizeMode.NoResize;
				foDays.WindowState = System.Windows.WindowState.Normal;
				foDays.ResizeMode = System.Windows.ResizeMode.NoResize;
			}
		}

		private void ShowMessage(string str)
		{
			App.Current.Dispatcher.Invoke((Action)delegate { laInform.Content = str; });
		}

		private void ClientPrint()
		{
			foreach (Turn t in client.turns)
				if (t.Id == client.user.TurnId)
				{
					t.Enabled = client.user.turnEnabled;
				}
            string timeToPrint = "00:00:00";
            if (client.user.StatusTime.IndexOf(".") > -1)
                timeToPrint = client.user.StatusTime.Substring(0, client.user.StatusTime.IndexOf(".") - 3);
            else
                timeToPrint = client.user.StatusTime.Substring(0, client.user.StatusTime.Length - 3);

            if (client.IsComPrinter)
            {
                // PRINT TO SERIAL (COM-PORT)
                try
                {
                    // generate string
                    string dataToPrint = CP866 + FONT_A + "------------------------" + CR +
                    CR +
                    CR +
                    FONT_B_DH_DW_B + "Ваш номер в очереди:" + CR +
                    CR +
                    FONT_A_DH_DW_B + " " + client.user.TurnPrefix + " " + client.user.Index + CR +
                    CR +
                    CR +
                    FONT_B_DH_DW_B + client.user.TurnLine + CR +
                    CR +
                    CR +
                    FONT_A_DH_DW + "КЛИЕНТ-ID: " + client.user.Id + CR +
                    CR +
                    CR +
                    FONT_B_DH_DW_B + "ВРЕМЯ: " + timeToPrint + CR +
                    CR +
                    CR +
                    FONT_B_DH_DW_B + " СОХРАНЯЙТЕ ТАЛОН!" + CR +
                    CR + CR +
                    "------------------------" +
                    CR + CR +
                    CR + CR +
                    CUT;
                    // create serial if null
                    if (client.Serial == null)
                    {
                        client.Serial = new System.IO.Ports.SerialPort(client.ComPort, client.ComBaudRate);
                        client.Serial.WriteTimeout = 500;
                    }
                    // connect serial
                    client.Serial.Open();
                    // write to serial
                    client.Serial.Write(dataToPrint);
                    // close serial
                    client.Serial.Close();
                    System.Threading.Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    client.LogAdd(ex.Message);
                }
                finally
                {
                    if (client.Serial.IsOpen)
                        client.Serial.Close();
                }
            }
            else
            {
                // MANUAL PRINT
                foPrintTemplate = new PrintTemplate();
                foPrintTemplate.laId.Content = client.user.Id;
                foPrintTemplate.laNumber.Content = client.user.TurnPrefix + client.user.Index;
                foPrintTemplate.laTime.Content = timeToPrint;
                foPrintTemplate.lbInfos.ItemsSource = client.user.Infos;
                foPrintTemplate.tbTurnName.Text = client.user.TurnLine;
                foPrintTemplate.RefreshSize();
                foPrintTemplate.Show();
                System.Threading.Thread.Sleep(2000);
                // Увеличить вывод в 5 раз
                PrintDialog printDialog = new PrintDialog();
                foPrintTemplate.grMain.LayoutTransform = new ScaleTransform(5, 5);
                // Напечатать элемент
                printDialog.PrintVisual(foPrintTemplate.grMain, "Печать чека" + client.user.TurnPrefix + client.user.Index);
                // Удалить трансформацию и снова сделать элемент видимым
                foPrintTemplate.grMain.LayoutTransform = null;
                foPrintTemplate.Hide();
            }
            ResetButtons();
            client.user = new User();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			grFmsMain.Width = (grMain.ActualWidth - 30) / 2;
			grPvsMain.Width = (grMain.ActualWidth - 30) / 2;
			grPvsMain.Margin = new Thickness(grMain.ActualWidth / 2 - 12, grPvsMain.Margin.Top, 12, grPvsMain.Margin.Bottom);
			Change_Windows_Sizes();
		}
		private void Change_Windows_Sizes()
		{
			foTurnsVizualization.Left = this.Left;
			foTurnsVizualization.Top = this.Top;
			foTurnsVizualization.Margin = this.Margin;
			foWhiteScreen.Left = this.Left;
			foWhiteScreen.Top = this.Top;
			foWhiteScreen.Margin = this.Margin;
			foDays.Left = this.Left;
			foDays.Top = this.Top;
			foDays.Margin = this.Margin;
			foInputText.Left = this.Left;
			foInputText.Top = this.Top;
			foInputText.Margin = this.Margin;

			foTurnsVizualization.Height = this.Height;
			foTurnsVizualization.Width = this.Width;
			foWhiteScreen.Height = this.Height;
			foWhiteScreen.Width = this.Width;
			foDays.Height = this.Height;
			foDays.Width = this.Width;
			foInputText.Height = this.Height;
			foInputText.Width = this.Width;
		}
		private void Window_LocationChanged(object sender, EventArgs e)
		{
			Change_Windows_Sizes();
		}
		private void Window_StateChanged(object sender, EventArgs e)
		{
			Change_Windows_Sizes();
		}

		private void DatesAddedTrue()
		{
			App.Current.Dispatcher.Invoke( (Action)delegate { foDays.cbDay.ItemsSource = client.dates; } );
		}
		private void ResetButtons()
		{
			bool TerminalW = true;
			foreach (Day d in client.days)
				if (Convert.ToInt32(DateTime.Now.DayOfWeek) == d.Id % 7)
					TerminalW = d.Terminal;
			foreach (Turn tu in client.turns)
				if (!tu.IsLive && !TerminalW)
					tu.Enabled = false;
			foreach (UIElement b in grFmsButtons.Children)
				if (b is Button)
					foreach (TurnVisualization tv in client.turnsVisualization)
						if (tv.Id == Convert.ToInt32((b as Button).Tag))
						{
							(b as Button).Content = tv.Name;
							foreach (Turn tu in client.turns)
								if (tu.Id == tv.TurnId)
								{
									b.IsEnabled = tu.Enabled;
								}
						}
			foreach (UIElement b in grPvsButtons.Children)
				if (b is Button)
					foreach (TurnVisualization tv in client.turnsVisualization)
						if (tv.Id == Convert.ToInt32((b as Button).Tag))
						{
							(b as Button).Content = tv.Name;
							foreach (Turn tu in client.turns)
								if (tu.Id == tv.TurnId)
								{
									b.IsEnabled = tu.Enabled;
								}
						}
		}

		private void buTurn_Click(object sender, RoutedEventArgs e)
		{
			int selectedTurnId = 0;
			if (foTurnsVizualization.reset)
				client.user.TurnLine = "";
			if (foTurnsVizualization.Visibility == System.Windows.Visibility.Visible)
			{
				foTurnsVizualization.Close();
			}
			foTurnsVizualization = new TurnsVizualization();
			TurnVisualization tv = new TurnVisualization();
			foreach (TurnVisualization t in client.turnsVisualization)
				if (t.Id == Convert.ToInt32((sender as Button).Tag)) 
				{
					tv = t;
					client.user.TurnLine += t.Name;
					if (tv.TurnId == 0)
						client.user.TurnLine += ": ";
				}
			if (tv != null)
			{
				if (tv.TurnId != 0)
				{
					selectedTurnId = tv.TurnId;
				}
				else
				{
					//создание доп формы выбора, tc оправдано!
					int BCount = 0;
					foreach (TurnVisualization tc in client.turnsVisualization)
						if (tc.TvMainId == tv.Id)
							BCount++;
					int Bi = 0;
					int Bh = Convert.ToInt32((this.ActualHeight - 250) / ((BCount+1) / 2));
					foreach (TurnVisualization tc in client.turnsVisualization)
						if (tc.TvMainId == tv.Id)
						{
							bool bEnabled = true;
							foreach (Turn tu in client.turns)
								if (tu.Id == tc.TurnId)
								{
									bEnabled = tu.Enabled;
								}
							Button b = new Button();
							b.IsEnabled = bEnabled;
							b.Name = "buTurnIn" + BCount.ToString();
							b.Tag = tc.Id;
							b.Content = tc.Name;
							b.FontSize = 36;
							b.HorizontalAlignment = HorizontalAlignment.Stretch;
							b.VerticalAlignment = VerticalAlignment.Top;
							b.Height = Bh - 10;
							b.Click += this.buTurn_Click;
							//b.Click += foTurnsVizualization.buTurnIn_Click;
							int mt = Convert.ToInt32(Bi/2) * Bh + 10;
							b.Margin = new Thickness(10, mt, 10, 0);
							if (Bi % 2 == 0)
							{
								foTurnsVizualization.grBL.Children.Add(b);
							}
							else
							{
								foTurnsVizualization.grBR.Children.Add(b);
							}
							Bi++;
						}
					foTurnsVizualization.label1.Content = tv.Name + ": выберите направление";
					foTurnsVizualization.result = 0;
					this.Hide();
					foTurnsVizualization.ShowDialog();
					this.Show();
					foreach (TurnVisualization t in client.turnsVisualization)
						if (foTurnsVizualization.result == t.Id)
							selectedTurnId = t.TurnId;
					foTurnsVizualization.grBL.Children.Clear();
					foTurnsVizualization.grBR.Children.Clear();
				}
				if (selectedTurnId != 0)
				{
					foWhiteScreen.Show();
					this.Hide();
					bool cont = true;
					bool IsLive = true;
					string SelectedDate = "'2000-01-01 01:01:01.123'"; 
					foreach (Turn t in client.turns)
						if (t.Id == selectedTurnId)
						{
							client.user.TurnPrefix = t.Prefix;
							client.user.TurnId = selectedTurnId;
							IsLive = t.IsLive;
						}
					if (!IsLive)
					{
						foDays = new Days();
						client.cbDates = DatesAddedTrue;
						client.GetFromServerDates(selectedTurnId);
						foDays.ShowDialog();
						if (!client.Opened) return;
						if (foDays.Result == "/!")
						{
							cont = false;
							client.user.TurnLine = "";
						}
						SelectedDate = foDays.Result;
					}

					string msg = selectedTurnId.ToString() + "/," + IsLive + "/," + SelectedDate + "/;";
					int i = 0;
					while (i < client.infos.Count && cont)
					{
						foInputText.InfoName = client.infos[i].Name;
						foInputText.Required = client.infos[i].Required;
						foInputText.Pattern = client.infos[i].Pattern;
						foInputText.Result = "/!";
						foInputText.ShowDialog();
						if (!client.Opened) return;
						if (foInputText.Result != "")
						{
							if (foInputText.Result == "/!")
							{
								cont = false;
								client.user.TurnLine = "";
							}
							else
							{
								client.user.Infos.Add(new UserInfo(client.infos[i].Name, foInputText.Result));
								msg += client.infos[i].Id.ToString() + "/," + foInputText.Result + "/;";
							}
						}
						i++;
					}
					if (cont)
					{
						client.SendClientToServer(msg);
					}
					this.Show();
					foWhiteScreen.Hide();
				}
			}
		}

		private void CreateButtons()
		{
			int FCount = 0;
			int PCount = 0;
			//buttons counts
			foreach (TurnVisualization t in client.turnsVisualization)
			{
				if (t.Fms && t.TvMainId == 0)
					FCount++;
				if (!t.Fms && t.TvMainId == 0)
					PCount++;
			}
			//creating
			int i = 0;
			int Fi = 0;
			int Pi = 0;
			double Fh = (grFmsButtons.ActualHeight - 10) / FCount;
			double Ph = (grPvsButtons.ActualHeight - 10) / PCount;
			foreach (TurnVisualization t in client.turnsVisualization)
			{
				if (t.TvMainId == 0)
				{
					bool bEnabled = true;
					foreach (Turn d in client.turns)
						if (d.Id == t.TurnId)
							bEnabled = d.Enabled;
					Button b = new Button();
					b.IsEnabled = bEnabled;
					b.Name = "buTurn" + i.ToString();
					b.Tag = t.Id;
					b.Content = t.Name;
					b.FontSize = 36;
					b.HorizontalAlignment = HorizontalAlignment.Stretch;
					b.VerticalAlignment = VerticalAlignment.Top;
					b.Click += buTurn_Click;
					if (t.Fms)
					{
						b.Height = Fh - 10;
						b.Margin = new Thickness(10, Fh * Fi + 10, 10, 10);
						grFmsButtons.Children.Add(b);
						Fi++;
					}
					else
					{
						b.Height = Ph - 10;
						b.Margin = new Thickness(10, Ph * Pi + 10, 10, 10);
						grPvsButtons.Children.Add(b);
						Pi++;
					}
				}
			}
			ResetButtons();
		}

		private void StartProgramm()
		{
			bool Working = false;
			int Today = Convert.ToInt32(DateTime.Now.DayOfWeek);
			foreach (Day d in client.days)
				if (Today == d.Id % 7)
					Working = d.Work;
			if (Working)
			{
				CreateButtons();
			}
			else	
			{
				if (!Silent)
					MessageBox.Show("Нерабочий день.\nПриложение будет закрыто");
				CloseProgramm();
			}		
		}
		private void GetInfos()
		{
			client.cbInfos = StartProgramm;
			client.GetFromServerInfos();
		}
		private void GetTurns()
		{
			client.cbTurns = GetInfos;
			client.GetFromServerTurns();
		}
		private void GetTurnsVisualization()
		{
			client.cbTurnsVisualization = GetTurns;
			client.GetFromServerTurnsVisualization();
		}
		private void GetDays()
		{
			foWhiteScreen.label1.Content = "Получение информации с сервера, ждите.";
			foWhiteScreen.Hide();
			this.Show();
			client.SaveSettings();
			client.cbDays = GetTurnsVisualization;
			client.GetFromServerDays();
		}
		private void HideScreen()
		{
			foWhiteScreen.label1.Content = "Отсутствует подключение к серверу :(";
			foWhiteScreen.Show();
			foDays.Hide();
			foInputText.Hide();
			foTurnsVizualization.Hide();
		}
		private void ConnectError()
		{
			MessageBoxResult result;
			if (Silent)
				result = MessageBoxResult.No;
			else
				result = MessageBox.Show("Соединение с " + client.Host + ":" + client.Port +
						" не установлено!\nИзменить адрес сервера?", "Ошибка", MessageBoxButton.YesNoCancel);
			if (result == MessageBoxResult.Yes)
			{
				foServerSettings.Host = client.Host;
				foServerSettings.Port = client.Port;
				foServerSettings.ShowDialog();
				client.Host = foServerSettings.Host;
				client.Port = foServerSettings.Port;
				Connect();
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
			client.cbConnect = GetDays;
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

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.Title = ProgrammName;
			Connect();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			CloseProgramm();
		}

	}
}
