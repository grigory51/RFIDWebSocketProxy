using Alchemy.Classes;
using RFIDWSProxy.Listeners;
using RFIDWSProxy.Utils;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using ZREADER;


namespace RFIDWSProxy {
    class RFIDService : ServiceBase {
        protected ConcurrentDictionary<EndPoint, UserContext> clientsPool;
        protected WebSocketListener websocketListener;
        protected RFIDListener rfidListener;

        public RFIDService() {
            this.clientsPool = new ConcurrentDictionary<EndPoint, UserContext>();
            this.ServiceName = "RFIDWSProxy";
        }

        protected override void OnStart(string[] args) {
            AppDomain.CurrentDomain.UnhandledException += RFIDService.UnhandledException;
            CommandLine.Parser.Default.ParseArguments(args, Settings.instance());

            try {
                this.websocketListener = new WebSocketListener(ref clientsPool);
                this.rfidListener = new RFIDListener(Settings.instance().ReaderPortType, Settings.instance().ReaderPortName, this.CardDetectCallback);

                this.websocketListener.StartListen();
                this.rfidListener.StartListen();
            } catch (Exception e) {
                Log.Write(e);
                this.OnStop();
            }
        }

        protected override void OnStop() {
            try {
                this.websocketListener.StopListen();
                this.rfidListener.StopListen();
            } catch (Exception e) {
                Log.Write(e);
            }
        }

        static void Main(string[] args) {
            RFIDService service = new RFIDService();
            if (Environment.UserInteractive) {
                service.OnStart(args);
                Console.WriteLine("Press any key to stop program");
                Console.ReadKey();
                service.OnStop();
            } else {
                ServiceBase.Run(service);
            }
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Log.Write((Exception)e.ExceptionObject);
            Environment.Exit(1);
        }

        bool CardDetectCallback(UInt32 nMsg, IntPtr nMsgParam, IntPtr pUserData) {
            if (nMsg == ZRIntf.ZR_RN_CARD_INSERT) {
                ZR_CARD_INFO pInfo = (ZR_CARD_INFO)Marshal.PtrToStructure(nMsgParam, typeof(ZR_CARD_INFO));
                string cardNum = ZRIntf.CardNumToStr(pInfo.nNum, pInfo.nType);

                Log.Write("Card was readed: " + cardNum);
                foreach (UserContext client in this.clientsPool.Values) {
                    client.Send(cardNum);
                }
            }
            return true;
        }
    }
}
