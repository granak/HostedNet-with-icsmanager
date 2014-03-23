using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using NETCONLib;
using IcsManagerLibrary;
using System.ComponentModel;

namespace IcsManagerGUI
{
    public partial class IcsManagerForm : Form, INotifyPropertyChanged
	{
		#region HostedNetInfo

		#region Fields
		private string _infos = string.Empty;
		private bool _icsApplied = false;
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		#region Properties
		public string Infos
		{
			get
			{
				return _infos;
			}
			set
			{
				_infos += String.Format("{0}{1}", value, Environment.NewLine);
				OnPropertyChanged("Infos");
			}
		}
		public bool ICSApplied
		{
			get { return _icsApplied; }
			set
			{
				_icsApplied = value;
				OnPropertyChanged("ICSApplied");
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
		#endregion

		public IcsManagerForm()
        {
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormSharingManager_Load(object sender, EventArgs e)
        {
            try
            {
                RefreshConnections();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Please restart this program with administrative priviliges.");
                Close();
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("This program is not supported on your operating system.");
                Close();
            }
        }

        private void AddNic(NetworkInterface nic)
        {
            var connItem = new ConnectionItem(nic);
            cbSharedConnection.Items.Add(connItem);
            cbHomeConnection.Items.Add(connItem);
            var netShareConnection = connItem.Connection;
            if (netShareConnection != null)
            {
                var sc = IcsManager.GetConfiguration(netShareConnection);
                if (sc.SharingEnabled)
                {
                    switch (sc.SharingConnectionType)
                    {
                        case tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC:
                            cbSharedConnection.SelectedIndex = cbSharedConnection.Items.Count - 1;
                            break;
                        case tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE:
                            cbHomeConnection.SelectedIndex = cbSharedConnection.Items.Count - 1;
                            break;
                    }
                }
            }
        }

        private void RefreshConnections()
        {
            cbSharedConnection.Items.Clear();
            cbHomeConnection.Items.Clear();
            foreach (var nic in IcsManager.GetIPv4EthernetAndWirelessInterfaces())
            {
                AddNic(nic);
            }
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
			Infos = "ICSManager: applying settings";
            var sharedConnectionItem = cbSharedConnection.SelectedItem as ConnectionItem;
            var homeConnectionItem = cbHomeConnection.SelectedItem as ConnectionItem;
            if ((sharedConnectionItem == null) || (homeConnectionItem == null))
            {
                MessageBox.Show(@"Please select both connections.");
                return;
            }
            if (sharedConnectionItem.Connection == homeConnectionItem.Connection)
            {
                MessageBox.Show(@"Please select different connections.");
                return;
            }
            IcsManager.ShareConnection(sharedConnectionItem.Connection, homeConnectionItem.Connection);
			RefreshConnections();
			Infos = "ICSManager: refresh connections";
			Infos = "ICSManager: done";
			ICSApplied = true;
        }

        private void cbHomeConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void buttonStopSharing_Click(object sender, EventArgs e)
        {
            IcsManager.ShareConnection(null, null);
            RefreshConnections();
        }
    }
}
