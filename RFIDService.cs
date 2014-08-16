using Alchemy.Classes;
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
            CommandLine.Parser.Default.ParseArguments(args, Settings.instance());

            this.websocketListener = new WebSocketListener(ref clientsPool);
            this.rfidListener = new RFIDListener(Settings.instance().ReaderPortType, Settings.instance().ReaderPortName);

            this.websocketListener.Start();

            this.rfidListener.PlaceCardCallback = this.CardDetectCallback;
            this.rfidListener.Open();
            this.rfidListener.StartListen();
        }

        protected override void OnStop() {
            this.websocketListener.Stop();
            this.rfidListener.Destroy();
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
