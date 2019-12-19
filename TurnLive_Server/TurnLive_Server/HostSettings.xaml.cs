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

namespace TurnLive_Server
{
	/// <summary>
	/// Логика взаимодействия для HostSettings.xaml
	/// </summary>
	public partial class HostSettings : Window
	{
		public HostSettings()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			this.Hide();
		}
	}
}
