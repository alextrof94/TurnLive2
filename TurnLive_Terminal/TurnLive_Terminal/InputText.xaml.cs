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
using System.Text.RegularExpressions;

namespace TurnLive_Terminal
{
	public partial class InputText : Window
	{
		public string InfoName { get; set; }
		public bool Required { get; set; }
		public string Pattern { get; set; }
		private Regex _pattern; // ранее
		private int _mincount; // замена паттерну
		public string Result { get; set; }

		Alphabet[] Languages;

		public InputText()
		{
			InitializeComponent();
			Languages = new Alphabet[3];
			Languages[0] = new Alphabet("ЙЦУКЕНГШЩЗХЪ", "ФЫВАПРОЛДЖЭ",  " ЯЧСМИТЬБЮ");
			Languages[1] = new Alphabet("QWERTYUIOP",   "ASDFGHJKL",    " ZXCVBNM");
			Languages[2] = new Alphabet("1234567890",   "():;?[]!@#$", " =+-:/*%");
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			double ButtonH = (grChars.ActualHeight - 6 * 5) / 4;
			double ButtonW = (grChars.ActualWidth - 6 * 13) / 12;
			foreach (Button a in grChars.Children)
			{
				a.Height = ButtonH;
				int W = 1;
				if (a.Name.IndexOf("W") > -1)
				{
					try { W = int.Parse(a.Name.Substring(a.Name.IndexOf("W") + 1, a.Name.IndexOf("R") - (a.Name.IndexOf("W") + 1))); }
					catch { W = 1; }
				}
				a.Width = ButtonW * W + 6 * (W - 1);
				int R = 0;
				try { R = int.Parse(a.Name.Substring(a.Name.IndexOf("R") + 1, a.Name.IndexOf("C") - (a.Name.IndexOf("R") + 1))); }
				catch { R = 0; }
				int C = 0;
				try { C = int.Parse(a.Name.Substring(a.Name.IndexOf("C") + 1)); }
				catch { C = 0; }
				if (R % 2 == 0)
					a.Margin = new Thickness(C * ButtonW + 6 * C, R * ButtonH + 6 * R, 0, 0);
				else
					a.Margin = new Thickness(C * ButtonW + 6 * C + ButtonW / 2 + 3, R * ButtonH + 6 * R, 0, 0);
			}
			foreach (Button a in grLanguages.Children)
			{
				a.Height = ButtonH;
				int R = 0;
				try { R = int.Parse(a.Name.Substring(a.Name.IndexOf("Lang") + 4)); }
				catch { R = 0; }
				a.Margin = new Thickness(6, R * ButtonH + 6 * R, 0, 0);
			}
		}

		private void buLang_Click(object sender, RoutedEventArgs e)
		{
			int Lang = 0;
			try { Lang = int.Parse(((sender as Button).Name.Substring((sender as Button).Name.IndexOf("Lang") + 4))); }
			catch { Lang = 0; }
			
			foreach (Button a in grChars.Children)
			{
				int R = 0;
				try { R = int.Parse(a.Name.Substring(a.Name.IndexOf("R") + 1, a.Name.IndexOf("C") - (a.Name.IndexOf("R") + 1))); }
				catch { R = 0; }
				int C = 0;
				try { C = int.Parse(a.Name.Substring(a.Name.IndexOf("C") + 1)); }
				catch { C = 0; }
				if (R < 3)
				{
					if (Languages[Lang].R[R].Length > C) 
					{
						a.Content = Languages[Lang].R[R][C];
						a.Visibility = Visibility.Visible;
					}
					else
						a.Visibility = Visibility.Hidden;
				}
			}
		}

		private void buChar_Click(object sender, RoutedEventArgs e)
		{
			//if (_pattern.IsMatch(tbInfoValue.Text + (sender as Button).Content))
			if (tbInfoValue.Text.Trim().Length < _mincount)
			{
				tbInfoValue.Text += (sender as Button).Content;
				buNext.IsEnabled = true;
			}
		}

		private void buLang3_Click(object sender, RoutedEventArgs e)
		{
			if (tbInfoValue.Text.Length > 0)
				tbInfoValue.Text = tbInfoValue.Text.Substring(0, tbInfoValue.Text.Length - 1);
		}

		private void buBack_Click(object sender, RoutedEventArgs e)
		{
			Result = "/!";
			this.Hide();
		}

		private void buNext_Click(object sender, RoutedEventArgs e)
		{
			Result = tbInfoValue.Text;
			this.Hide();
		}

		private void Window_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
		}

		private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_pattern = new Regex(Pattern);
			if (!int.TryParse(Pattern, out _mincount))
				MessageBox.Show("Ошибка паттерна");
			laInfoName.Content = InfoName;
			if (Required)
			{
				laInfoName.Content += " (обязательно)";
				buNext.IsEnabled = false;
			}
			tbInfoValue.Text = "";
		}

		private void tbInfoValue_TextChanged(object sender, TextChangedEventArgs e)
		{

		}
	}

	public class Alphabet
	{
		public string[] R { get; set; }

		public Alphabet(string R0, string R1, string R2)
		{
			R = new string[3];
			R[0] = R0;
			R[1] = R1;
			R[2] = R2;
		}
	}
}
