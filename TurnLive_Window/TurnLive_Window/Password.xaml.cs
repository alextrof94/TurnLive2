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
	public partial class Password : Window
	{
		public bool Entered = false;

		public Password()
		{
			InitializeComponent();
		}

		private void buEnter_Click(object sender, RoutedEventArgs e) { Entered = true; this.Hide(); }
		private void buCancel_Click(object sender, RoutedEventArgs e) { Entered = false; this.Hide(); }

		private void Window_Loaded(object sender, RoutedEventArgs e) { tbPassword.Focus(); }
	}
}
