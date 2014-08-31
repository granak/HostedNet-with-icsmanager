using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Security.Permissions;
using System.Security.Principal;
using System.Xml.Serialization;
using HostedNet;
using MahApps.Metro.Controls;
using System.Drawing;
using MahApps.Metro.Controls.Dialogs;
using WpfPageTransitions;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace HostedNetWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region Fields

		private AppSettings appSettings;

		#endregion

		#region Constructors
		public MainWindow()
		{
			InitializeComponent();
			appSettings = SettingsHelper.LoadAppSettings(false);

			if (appSettings != null && SettingsHelper.LoadUserSettings(false) != null)
				if(appSettings.SaveConnectionSettings)
					PageTransition.ShowPage(new LaunchButton());
				else
					PageTransition.ShowPage(new Login());
			else
				PageTransition.ShowPage(new Login());
		}
		#endregion

		#region Properties
		public string SSID { get; set; }
		public string Password { get; set; }
		#endregion

		private void LoginWindowButton_OnClick(object sender, RoutedEventArgs e)
		{
			PageTransition.ShowPage(new Login());
		}

		private void SettingsWindowButton_OnClick(object sender, RoutedEventArgs e)
		{
			PageTransition.ShowPage(new Settings());
		}
	}
}
