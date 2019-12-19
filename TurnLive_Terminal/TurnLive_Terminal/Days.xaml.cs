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

namespace TurnLive_Terminal
{
	/// <summary>
	/// Логика взаимодействия для Days.xaml
	/// </summary>
	public partial class Days : Window
	{
		public string Result = "";
		public string ResultTime = "";

		public Days()
		{
			InitializeComponent();
		}

		private void buBack_Click(object sender, RoutedEventArgs e)
		{
			Result = "/!";
			this.Hide();
		}

		private void buNext_Click(object sender, RoutedEventArgs e)
		{
			Result = "'" + (cbDay.SelectedItem as Date).ToShortString() + " " + ResultTime + "'";
			this.Hide();
		}

		private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
		}

		private void cbDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			laTime.IsEnabled = true;
			grB.Children.Clear();
			if (cbDay.SelectedItem != null)
			{
				Date si = (cbDay.SelectedItem as Date);
				int rF = 8;
				int rC = si.times.Count / rF + 1;
				int bH = (int)grB.ActualHeight / rC;
				int bW = (int)grB.ActualWidth / rF;
				int s = 5;
				for (int i = 0; i < si.times.Count; i++)
				{
					Button b = new Button();
					b.Click += buTime_Click;
					b.Content = si.times[i].ToString();
					b.FontSize = 36;
					b.Height = bH - s;
					b.Width = bW - s;
					b.Margin = new Thickness(i%rF * bW + s, (int)(i/rF) * bH + s, 0, 0);
					b.VerticalAlignment = System.Windows.VerticalAlignment.Top;
					b.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					grB.Children.Add(b);
				}
			}
		}
		private void buTime_Click(object sender, RoutedEventArgs e)
		{
			foreach (UIElement b in grB.Children)
				b.IsEnabled = true;
			(sender as Button).IsEnabled = false;
			ResultTime = (sender as Button).Content.ToString();
			buNext.IsEnabled = true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			laTime.IsEnabled = false;
			buNext.IsEnabled = false;
		}
	}
}
