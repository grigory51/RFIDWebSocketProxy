using CommandLine;
using ZPort;

namespace RFIDWSProxy {
    class Settings {
        private static Settings settingsObject = null;

        public ZP_PORT_TYPE ReaderPortType = ZP_PORT_TYPE.ZP_PORT_COM;
        public string ReaderPortName { get; set; }
        public int WebsocketListenPort { get; set; }

        [Option('l', "logfile")]
        public string LogFile { get; set; }

        private Settings() { }

        public static Settings instance() {
            if (Settings.settingsObject == null) {
                Settings.settingsObject = new Settings();

                Settings.settingsObject.ReaderPortName = "COM8";
                Settings.settingsObject.LogFile = "rfid_daemon.log";
                Settings.settingsObject.WebsocketListenPort = 8000;
            }
            return Settings.settingsObject;
        }


    }
}
