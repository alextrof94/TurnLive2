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
	/// <summary>
	/// Логика взаимодействия для Settings.xaml
	/// </summary>
	public partial class Settings : Window
	{
		public Settings()
		{
			InitializeComponent();
		}

		private void tbWindowId_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if ("1234567890".IndexOf(e.Text) < 0)
				e.Handled = true;
		}

		private void buSave_Click(object sender, RoutedEventArgs e)
		{
			this.Hide();
		}
	}
}
