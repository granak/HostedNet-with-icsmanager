using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HostedNetWPF;

namespace HostedNet
{
	/// <summary>
	/// Interaction logic for LaunchButton.xaml
	/// </summary>
	public partial class LaunchButton : UserControl
	{
		#region Fields
		private HostedNetWPF_BP.MainWindow backProcess;
		private IcsManagerGUI.IcsManagerForm icsForm;
		private MainWindow parentWindow;
		private UserSettings userSettings;
		#endregion

		#region Constructors
		public LaunchButton()
		{
			InitializeComponent();
			userSettings = SettingsHelper.LoadUserSettings(true);
			parentWindow = System.Windows.Application.Current.MainWindow as MainWindow;

		}
		#endregion

		#region Events
		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			LaunchHostedNetworkBatchFile();
		}
		void icsForm_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			BackgroundWizardDetailsManager("ICS Manager: opened");
			BackgroundWizardClose();
		}
		void icsForm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Infos")
				BackgroundWizardDetailsManager(icsForm.Infos);
			if (e.PropertyName == "ICSApplied")
				BackgroundWizardClose();
		}
		void icsForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			BackgroundWizardClose();
		}
		#endregion

		#region Methods
		private void LaunchHostedNetworkBatchFile()
		{
			try
			{
				string batchFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "WifiLaunch.bat");
				Process batchFileProcess = new Process();
				ProcessStartInfo processInfo = new ProcessStartInfo(batchFilePath);
				processInfo.Arguments = String.Format("-id {0} -pass {1}", userSettings.SSID, userSettings.Password);
				processInfo.Verb = "runas";
				processInfo.CreateNoWindow = false;
				batchFileProcess.StartInfo = processInfo;
				batchFileProcess.Start();
				BackgroundWizardDetailsManager("Script: start");
				batchFileProcess.WaitForExit();
				batchFileProcess.Close();
				BackgroundWizardDetailsManager("Script: done");

				BackgroundWizardDetailsManager("ICS Manager: wait for check");
				if (!IcsManagerLibrary.IcsManager.GetCurrentlySharedConnections().Exists)
				{
					BackgroundWizardDetailsManager("ICS Manager: opening");
					icsForm = new IcsManagerGUI.IcsManagerForm();
					icsForm.Paint += icsForm_Paint;
					icsForm.PropertyChanged += icsForm_PropertyChanged;
					icsForm.FormClosed += icsForm_FormClosed;
					icsForm.Show();
				}
				else
				{
					BackgroundWizardDetailsManager("ICS Manager: done");
					BackgroundWizardClose();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("BatchError: " + ex.Message, "HostedNet batch error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private void BackgroundWizardDetailsManager(string value)
		{
			if (backProcess == null)
			{
				backProcess = new HostedNetWPF_BP.MainWindow();
				backProcess.Topmost = true;
				backProcess.Show();
			}
			backProcess.DetailsContent = value;
		}
		private void BackgroundWizardClose()
		{
			if (backProcess == null)
				return;

			backProcess.Close();
			backProcess = null;
		}
		#endregion
	}
}
