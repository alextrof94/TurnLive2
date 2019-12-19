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
	/// Логика взаимодействия для PrintTemplate.xaml
	/// </summary>
	public partial class PrintTemplate : Window
	{
		public bool th = false;
		public PrintTemplate()
		{
			InitializeComponent();
		}

		public void RefreshSize()
		{
			this.Height = 320 + 30 * lbInfos.Items.Count;
		}

		private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{

		}
	}
}
