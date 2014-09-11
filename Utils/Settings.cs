using CommandLine;
using ZPort;

namespace RFIDWSProxy {
    class Settings {
        private static Settings settingsObject = null;

        public ZP_PORT_TYPE ReaderPortType = ZP_PORT_TYPE.ZP_PORT_COM;
        [Option('r', "readerport")]
        public string ReaderPortName { get; set; }

        [Option('w', "wsport")]
        public int WebsocketListenPort { get; set; }

        private Settings() { }

        public static Settings instance() {
            if (Settings.settingsObject == null) {
                Settings.settingsObject = new Settings();

                Settings.settingsObject.ReaderPortName = "COM8";
                Settings.settingsObject.WebsocketListenPort = 8000;
            }
            return Settings.settingsObject;
        }
    }
}
