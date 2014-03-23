HostedNet&Internet Connection Sharing Manager
===================================

Created for sharing my internet connection from my notebook to my phone over wifi.
I just hate ConeXXXXXXy steps to free users.

Requirements
------------

* Windows 7 or higher.
* .NET Framework 4.0.


Usage
-----

All commands require administrative privileges.

---
For HostedNet:
	Open in VisualStudio and build HostedNetWPF project.

For ICSManager:	
	icsmanager info

Display information about currently available connections:

* name
* guid
* status
* address
* gateway
* sharing status

---

    icsmanager enable {GUID-OF-CONNECTION-TO-SHARE} {GUID-OF-HOME-CONNECTION} [force]
    icsmanager enable "Name of connection to share" "Name of home connection" [force]

Enable connection sharing. Use the `force` argument if you want to automatically disable existing connection sharing.

---

    icsmanager disable

Disable connection sharing.

---

Powershell
----------

0. Import module:

    Import-Module IcsManager.dll

0. List network connections:

    Get-NetworkConnections

0. Start Internet Connection Sharing:

    Enable-ICS "Connection to share" "Home connection"

0. Stop Internet Connection Sharing:

    Disable-ICS

