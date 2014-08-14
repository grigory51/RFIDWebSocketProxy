using Alchemy;
using Alchemy.Classes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using ZPort;
using ZREADER;


namespace RFIDDaemon {
    class Program {
        public const ZP_PORT_TYPE ReaderPortType = ZP_PORT_TYPE.ZP_PORT_COM;
        public const string ReaderPortName = "COM8";

        protected static ConcurrentDictionary<EndPoint, UserContext> Clients = new ConcurrentDictionary<EndPoint, UserContext>();

        static void OnConnected(UserContext context) {
            Clients.TryAdd(context.ClientAddress, context);
            Console.WriteLine("On connected : " + context.ClientAddress.ToString());
            Console.WriteLine("Clients Online: {0}", Program.Clients.Count);
        }

        static void OnDisconnect(UserContext context) {
            UserContext trash; // Concurrent dictionaries make things weird

            Program.Clients.TryRemove(context.ClientAddress, out trash);

            Console.WriteLine("Client Disconnected {0}", context.ClientAddress);
            Console.WriteLine("Clients Online: {0}", Program.Clients.Count);
        }

        static void Main(string[] args) {

            var aServer = new WebSocketServer(8000, IPAddress.Any) {
                OnConnected = OnConnected,
                OnDisconnect = OnDisconnect,
                TimeOut = new TimeSpan(0, 5, 0)
            };

            aServer.Start();


            int returnCode;
            IntPtr readerPtr = new IntPtr();

            Console.Write("Start reader...");
            returnCode = ZRIntf.ZR_Initialize(ZPIntf.ZP_IF_NO_MSG_LOOP);
            if (returnCode < 0) {
                Console.Write("Ошибка инициализации");
                System.Environment.Exit(1);
            }

            try {
                ZR_RD_OPEN_PARAMS openParams = new ZR_RD_OPEN_PARAMS(Program.ReaderPortType, Program.ReaderPortName);
                Console.WriteLine("Открытие считывателя ({0})...", Program.ReaderPortName);
                ZR_RD_INFO readerInfo = new ZR_RD_INFO();
                returnCode = ZRIntf.ZR_Rd_Open(ref readerPtr, ref openParams, readerInfo);
                if (returnCode < 0) {
                    Console.Write("Ошибка открытия считывателя");
                    System.Environment.Exit(1);
                }
                Console.WriteLine("с/н: {0}, v{1}.{2}", readerInfo.rBase.nSn, readerInfo.rBase.nVersion & 0xff, (readerInfo.rBase.nVersion >> 8) & 0xff);

                ZR_RD_NOTIFY_SETTINGS rNS = new ZR_RD_NOTIFY_SETTINGS(ZRIntf.ZR_RNF_PLACE_CARD, CardDetectCallback);
                returnCode = ZRIntf.ZR_Rd_SetNotification(readerPtr, rNS);

                if (returnCode < 0) {
                    Console.WriteLine("Ошибка ZR_Rd_SetNotification ({0}).", returnCode);
                    Console.ReadLine();
                    return;
                }


                var command = string.Empty;
                while (command != "exit") {
                    command = Console.ReadLine();
                }
                aServer.Stop();
            } finally {
                if (readerPtr != IntPtr.Zero) {
                    ZRIntf.ZR_CloseHandle(readerPtr);
                }
                ZRIntf.ZR_Finalyze();
            }


        }

        static bool CardDetectCallback(UInt32 nMsg, IntPtr nMsgParam, IntPtr pUserData) {
            ZR_CARD_INFO pInfo;
            switch (nMsg) {
                case ZRIntf.ZR_RN_CARD_INSERT:
                    pInfo = (ZR_CARD_INFO)Marshal.PtrToStructure(nMsgParam, typeof(ZR_CARD_INFO));
                    Console.WriteLine();
                    Console.WriteLine("Поднесена карта {0}", ZRIntf.CardNumToStr(pInfo.nNum, pInfo.nType));

                    foreach (var u in Program.Clients.Values) {
                        u.Send(ZRIntf.CardNumToStr(pInfo.nNum, pInfo.nType));
                    }

                    break;
                case ZRIntf.ZR_RN_CARD_REMOVE:
                    break;
            }
            return true;
        }
    }
}
