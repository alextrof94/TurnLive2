﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace TurnLive_Tool_RemoveClient
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static string[] Arguments;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			Arguments = e.Args;
		}
	}
}
