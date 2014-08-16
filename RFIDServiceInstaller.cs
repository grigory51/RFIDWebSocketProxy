using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;

namespace RFIDWSProxy {
    [RunInstaller(true)]
    public class RFIDServiceInstaller : Installer {
        public RFIDServiceInstaller() {
            ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.ServiceName = "RFIDWSProxy";
            serviceInstaller.DisplayName = "RFIDWSProxy";
         
            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
