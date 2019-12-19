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
	/// Логика взаимодействия для TurnsVizualization.xaml
	/// </summary>
	public partial class TurnsVizualization : Window
	{
		public int result;
		public bool reset = false;

		public TurnsVizualization()
		{
			InitializeComponent();
		}

		//public void buTurnIn_Click(object sender, RoutedEventArgs e)
		//{
		//    result = Convert.ToInt32((sender as Button).Tag);
		//    this.Hide();
		//}

		private void buBack_Click(object sender, RoutedEventArgs e)
		{
			reset = true;
			this.Hide();
		}

		private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			grBL.Width = (this.ActualWidth - 30) / 2;
			grBR.Width = grBL.Width;
		}
	}
}
