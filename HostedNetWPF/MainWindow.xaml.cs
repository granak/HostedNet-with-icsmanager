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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Security.Permissions;
using System.Security.Principal;
using System.Xml.Serialization;

namespace HostedNetWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Fields
		private HostedNetWPF_BP.MainWindow backProcess;
		private IcsManagerGUI.IcsManagerForm icsForm;
		#endregion

		#region Constructors
		public MainWindow()
		{
			InitializeComponent();
			if (!IsUserAdministrator())
			{
				UACShieldIconToButton();
				LaunchButton_Content_Label.Margin = new Thickness { Left = 20 };
			}
			else
				LaunchButton_Content_Label.Margin = new Thickness { Left = 12 };

			LaunchButton.IsEnabled = false;
			MenuItem_Home_Launch.IsEnabled = false;
		}
		#endregion

		#region DllImports
		[DllImport("Shell32.dll", SetLastError = false)]
		public static extern Int32 SHGetStockIconInfo(HostedNetWPF.MainWindow.NestedClass.SHSTOCKICONID siid, HostedNetWPF.MainWindow.NestedClass.SHGSI uFlags, ref HostedNetWPF.MainWindow.NestedClass.SHSTOCKICONINFO psii);
		#endregion

		#region Events
		private void ResetButton_Click(object sender, RoutedEventArgs e)
		{
			SSID.Clear();
			Password.Clear();
		}
		private void Close_MenuItem_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private void Launch_MenuItem_Click(object sender, RoutedEventArgs e)
		{
			LaunchHostedNetworkBatchFile();
		}
		private void MenuItem_Home_SaveSettings_Click(object sender, RoutedEventArgs e)
		{
			SaveSettings();
		}
		private void MenuItem_Home_LoadSettings_Click(object sender, RoutedEventArgs e)
		{
			UserSettings settings = LoadSettings(true);
			if(settings != null)
			{
				SSID.Text = settings.SSID;
				Password.Password = settings.Password;
				
				LaunchButton.IsEnabled = true;
				MenuItem_Home_Launch.IsEnabled = true;
			} 
		}

		private void LaunchButton_Click(object sender, RoutedEventArgs e)
		{
			LaunchHostedNetworkBatchFile();
		}
		private void SSID_KeyUp(object sender, KeyEventArgs e)
		{
			if (SSID.Text.Length < 3 || Password.Password.Length < 8)
			{
				LaunchButton.IsEnabled = false;
				MenuItem_Home_Launch.IsEnabled = false;
			}
			else
			{
				LaunchButton.IsEnabled = true;
				MenuItem_Home_Launch.IsEnabled = true;
			}
		}
		private void Password_KeyUp(object sender, KeyEventArgs e)
		{
			if (SSID.Text.Length < 3 || Password.Password.Length < 8)
			{
				LaunchButton.IsEnabled = false;
				MenuItem_Home_Launch.IsEnabled = true;
			}
			else
			{
				LaunchButton.IsEnabled = true;
				MenuItem_Home_Launch.IsEnabled = true;
			}
		}

		void icsForm_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			BackgroundWizardDetailsManage("ICS Manager: opened");
			BackgroundWizardClose();
		}
		void icsForm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "Infos")
				BackgroundWizardDetailsManage(icsForm.Infos);
			if (e.PropertyName == "ICSApplied")
				BackgroundWizardClose();
		}
		void icsForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			BackgroundWizardClose();
		}

		#endregion

		#region Methods
		private bool IsUserAdministrator()
		{
			//bool value to hold our return value
			bool isAdmin;
			try
			{
				//get the currently logged in user
				WindowsIdentity user = WindowsIdentity.GetCurrent();
				WindowsPrincipal principal = new WindowsPrincipal(user);
				isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
			catch (UnauthorizedAccessException ex)
			{
				isAdmin = false;
			}
			catch (Exception ex)
			{
				isAdmin = false;
			}
			return isAdmin;
		}

		/// <summary>
		/// Set UAC icon to launch button. http://ithoughthecamewithyou.com/post/UAC-shield-icon-in-WPF.aspx
		/// </summary>
		private void UACShieldIconToButton()
		{
			BitmapSource shieldSource = null;

			if (Environment.OSVersion.Version.Major >= 6)
			{
				HostedNetWPF.MainWindow.NestedClass.SHSTOCKICONINFO sii = new HostedNetWPF.MainWindow.NestedClass.SHSTOCKICONINFO();
				sii.cbSize = (UInt32)Marshal.SizeOf(typeof(HostedNetWPF.MainWindow.NestedClass.SHSTOCKICONINFO));

				Marshal.ThrowExceptionForHR(SHGetStockIconInfo(HostedNetWPF.MainWindow.NestedClass.SHSTOCKICONID.SIID_SHIELD,
					HostedNetWPF.MainWindow.NestedClass.SHGSI.SHGSI_ICON | HostedNetWPF.MainWindow.NestedClass.SHGSI.SHGSI_SMALLICON,
					ref sii));

				shieldSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
					sii.hIcon,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());

				//DestroyIcon(sii.hIcon);
			}
			else
			{
				shieldSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
					System.Drawing.SystemIcons.Shield.Handle,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());
			}

			if (shieldSource != null)
			{
				UACImage.Source = shieldSource;
				Icon_MI_Launch.Source = shieldSource;
			}
		}

		private void LaunchHostedNetworkBatchFile()
		{
			if (string.IsNullOrEmpty(SSID.Text) || string.IsNullOrEmpty(Password.Password))
				return;

			if (Password.Password.Length < 8)
				MessageBox.Show("Password must have least 8 characters.");

			try
			{
				string batchFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "WifiLaunch.bat");
				Process batchFileProcess = new Process();
				ProcessStartInfo processInfo = new ProcessStartInfo(batchFilePath);
				processInfo.Arguments = String.Format("-id {0} -pass {1}", SSID.Text.ToString(), Password.Password.ToString()).ToString();
				processInfo.Verb = "runas";
				batchFileProcess.StartInfo = processInfo;
				batchFileProcess.Start();
				BackgroundWizardDetailsManage("Script: start");
				batchFileProcess.WaitForExit();
				batchFileProcess.Close();
				BackgroundWizardDetailsManage("Script: done");

				BackgroundWizardDetailsManage("ICS Manager: wait for check");
				if (!IcsManagerLibrary.IcsManager.GetCurrentlySharedConnections().Exists)
				{
					BackgroundWizardDetailsManage("ICS Manager: opening");
					icsForm = new IcsManagerGUI.IcsManagerForm();
					icsForm.Paint += icsForm_Paint;
					icsForm.PropertyChanged += icsForm_PropertyChanged;
					icsForm.FormClosed += icsForm_FormClosed;
					icsForm.Show();
				}
				else
				{
					BackgroundWizardDetailsManage("ICS Manager: done");
					BackgroundWizardClose();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("BatchError: " + ex.Message, "HostedNet batch error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void BackgroundWizardDetailsManage(string value)
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

		private void SaveSettings()
		{
			if (SSID.Text.Length < 3 || Password.Password.Length < 8)
			{
				MessageBox.Show("Verify constraints for SSID and password to lunch hosted network.", "Save settings error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			try
			{
				UserSettings settings = new UserSettings();
				settings.SSID = SSID.Text;
				settings.Password = Password.Password;
				XmlSerializer serializer = new XmlSerializer(settings.GetType());
				serializer.Serialize(new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserSettings.data").ToString(), FileMode.OpenOrCreate), settings);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Settings was not saved due to: " + ex.Message);
			}
		}
		private UserSettings LoadSettings(bool showErrors)
		{
			try
			{
				FileStream file = new FileStream(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserSettings.data").ToString(), FileMode.Open, FileAccess.Read);
				
				XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
				UserSettings settings = serializer.Deserialize(file) as UserSettings;
				return settings;
			}
			catch(FileNotFoundException)
			{
				if(showErrors)
					MessageBox.Show("Settings file was not found. First save some settings.");
				return null;
			}
			catch(Exception ex)
			{
				if (showErrors)
					MessageBox.Show("Settings was not loaded due to: " + ex.Message);
				return null;
			}
			return null;
		}

		#endregion

		#region NestedClasses
		public class NestedClass
		{
			public enum SHSTOCKICONID : uint
			{
				SIID_DOCNOASSOC = 0,
				SIID_DOCASSOC = 1,
				SIID_APPLICATION = 2,
				SIID_FOLDER = 3,
				SIID_FOLDEROPEN = 4,
				SIID_DRIVE525 = 5,
				SIID_DRIVE35 = 6,
				SIID_DRIVEREMOVE = 7,
				SIID_DRIVEFIXED = 8,
				SIID_DRIVENET = 9,
				SIID_DRIVENETDISABLED = 10,
				SIID_DRIVECD = 11,
				SIID_DRIVERAM = 12,
				SIID_WORLD = 13,
				SIID_SERVER = 15,
				SIID_PRINTER = 16,
				SIID_MYNETWORK = 17,
				SIID_FIND = 22,
				SIID_HELP = 23,
				SIID_SHARE = 28,
				SIID_LINK = 29,
				SIID_SLOWFILE = 30,
				SIID_RECYCLER = 31,
				SIID_RECYCLERFULL = 32,
				SIID_MEDIACDAUDIO = 40,
				SIID_LOCK = 47,
				SIID_AUTOLIST = 49,
				SIID_PRINTERNET = 50,
				SIID_SERVERSHARE = 51,
				SIID_PRINTERFAX = 52,
				SIID_PRINTERFAXNET = 53,
				SIID_PRINTERFILE = 54,
				SIID_STACK = 55,
				SIID_MEDIASVCD = 56,
				SIID_STUFFEDFOLDER = 57,
				SIID_DRIVEUNKNOWN = 58,
				SIID_DRIVEDVD = 59,
				SIID_MEDIADVD = 60,
				SIID_MEDIADVDRAM = 61,
				SIID_MEDIADVDRW = 62,
				SIID_MEDIADVDR = 63,
				SIID_MEDIADVDROM = 64,
				SIID_MEDIACDAUDIOPLUS = 65,
				SIID_MEDIACDRW = 66,
				SIID_MEDIACDR = 67,
				SIID_MEDIACDBURN = 68,
				SIID_MEDIABLANKCD = 69,
				SIID_MEDIACDROM = 70,
				SIID_AUDIOFILES = 71,
				SIID_IMAGEFILES = 72,
				SIID_VIDEOFILES = 73,
				SIID_MIXEDFILES = 74,
				SIID_FOLDERBACK = 75,
				SIID_FOLDERFRONT = 76,
				SIID_SHIELD = 77,
				SIID_WARNING = 78,
				SIID_INFO = 79,
				SIID_ERROR = 80,
				SIID_KEY = 81,
				SIID_SOFTWARE = 82,
				SIID_RENAME = 83,
				SIID_DELETE = 84,
				SIID_MEDIAAUDIODVD = 85,
				SIID_MEDIAMOVIEDVD = 86,
				SIID_MEDIAENHANCEDCD = 87,
				SIID_MEDIAENHANCEDDVD = 88,
				SIID_MEDIAHDDVD = 89,
				SIID_MEDIABLURAY = 90,
				SIID_MEDIAVCD = 91,
				SIID_MEDIADVDPLUSR = 92,
				SIID_MEDIADVDPLUSRW = 93,
				SIID_DESKTOPPC = 94,
				SIID_MOBILEPC = 95,
				SIID_USERS = 96,
				SIID_MEDIASMARTMEDIA = 97,
				SIID_MEDIACOMPACTFLASH = 98,
				SIID_DEVICECELLPHONE = 99,
				SIID_DEVICECAMERA = 100,
				SIID_DEVICEVIDEOCAMERA = 101,
				SIID_DEVICEAUDIOPLAYER = 102,
				SIID_NETWORKCONNECT = 103,
				SIID_INTERNET = 104,
				SIID_ZIPFILE = 105,
				SIID_SETTINGS = 106,
				SIID_DRIVEHDDVD = 132,
				SIID_DRIVEBD = 133,
				SIID_MEDIAHDDVDROM = 134,
				SIID_MEDIAHDDVDR = 135,
				SIID_MEDIAHDDVDRAM = 136,
				SIID_MEDIABDROM = 137,
				SIID_MEDIABDR = 138,
				SIID_MEDIABDRE = 139,
				SIID_CLUSTEREDDRIVE = 140,
				SIID_MAX_ICONS = 175
			}

			[Flags]
			public enum SHGSI : uint
			{
				SHGSI_ICONLOCATION = 0,
				SHGSI_ICON = 0x000000100,
				SHGSI_SYSICONINDEX = 0x000004000,
				SHGSI_LINKOVERLAY = 0x000008000,
				SHGSI_SELECTED = 0x000010000,
				SHGSI_LARGEICON = 0x000000000,
				SHGSI_SMALLICON = 0x000000001,
				SHGSI_SHELLICONSIZE = 0x000000004
			}

			[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			public struct SHSTOCKICONINFO
			{
				public UInt32 cbSize;
				public IntPtr hIcon;
				public Int32 iSysIconIndex;
				public Int32 iIcon;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH = 260 used by http://social.msdn.microsoft.com/Forums/vstudio/en-US/4ac1e170-bd21-4877-ace2-361b955ee9ef/maxpath-limitation?forum=vsx
				public string szPath;
			}
		}
		#endregion

		private void About_MenuItem_Click(object sender, RoutedEventArgs e)
		{
			new About().Show();
		}

		private void HTU_MenuItem_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("http://www.granak.sk");
		}

	}
}
