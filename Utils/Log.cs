using System;
using System.Diagnostics;


namespace RFIDWSProxy.Utils {
    class Log {
        protected static EventLog eventLog = new System.Diagnostics.EventLog();

        public Log() {
            ((System.ComponentModel.ISupportInitialize)(eventLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(eventLog)).EndInit();
        }

        public static void Write(string msg) {
            if (Environment.UserInteractive) {
                Console.WriteLine(msg);
            } else {
                try {
                    string serviceName = "RFIDWSProxy";
                    if (!EventLog.SourceExists(serviceName)) {
                        EventLog.CreateEventSource(serviceName, serviceName);
                    }
                    eventLog.Source = serviceName;
                    eventLog.WriteEntry(msg);
                } catch { }
            }
        }

        public static void Write(Exception e) {
            Log.Write(e.Message);
        }
    }
}
