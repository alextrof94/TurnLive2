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
	/// Логика взаимодействия для Wait.xaml
	/// </summary>
	public partial class Wait : Window
	{
		public Wait()
		{
			InitializeComponent();
		}

		//static UI
		private void buClose_Click(object sender, RoutedEventArgs e) { this.Hide(); }

		private void buGetBack_Click(object sender, RoutedEventArgs e)
		{
			//this.Hide();
		}

		private void Moving(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed) { this.DragMove(); }
		}

		private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
		}
	}
}
