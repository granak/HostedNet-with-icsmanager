using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace HostedNetWPF_BP
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		#region Fields
		private string _detailsContent = string.Empty;
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		#region Properties
		public string DetailsContent
		{
			get
			{
				return _detailsContent;
			}
			set
			{
				_detailsContent += String.Format("{0}{1}", value, Environment.NewLine);
				OnPropertyChanged("DetailsContent");
			}
		}
		#endregion

		#region Constructors
		public MainWindow()
		{
			InitializeComponent();
			//ExpDetails.IsExpanded = false;
			//ExpDetails.Height = 25;
			//this.Height = 160;
			ExpDetails.IsExpanded = true;
			ExpDetails.Height = 75;
			this.Height = 210;
			this.PropertyChanged += BackgroundProcess_PropertyChanged;
		}
		#endregion

		#region Events
		private void Expander_Collapsed(object sender, RoutedEventArgs e)
		{
			ExpDetails.Height = 25;
			this.Height = 160;
		}

		private void Expander_Expanded(object sender, RoutedEventArgs e)
		{
			ExpDetails.Height = 75;
			this.Height = 210;
		}

		void BackgroundProcess_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "DetailsContent")
			{
				TextBlockDetails.Text = DetailsContent;
			}
		}
		#endregion

		#region Methods
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
		#endregion
	}
}
