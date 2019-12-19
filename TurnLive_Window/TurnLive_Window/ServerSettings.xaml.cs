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
using System.Windows.Shapes;

namespace TurnLive_Window
{
	public partial class ServerSettings : Window
	{
		public string Host { get; set; }
		public int Port { get; set; }

		public ServerSettings()
		{
			InitializeComponent();
		}

		private void buEnter_Click(object sender, RoutedEventArgs e)
		{
			Host = tbHost.Text;
			try { Port = int.Parse(tbPort.Text); }
			catch { }
			this.Hide();
		}
		private void buCancel_Click(object sender, RoutedEventArgs e) { this.Hide(); }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			tbHost.Text = Host;
			tbPort.Text = Port.ToString();
			tbHost.Focus();
		}
	}
}
