using System;
using ZPort;
using ZREADER;

namespace RFIDWSProxy.Listeners {
    class RFIDListener : IListener {
        IntPtr reader = new IntPtr();

        private ZP_PORT_TYPE portType;
        private string portName;

        private ZP_NOTIFYPROC placeCardCallback;

        public RFIDListener(ZP_PORT_TYPE portType, string portName, ZP_NOTIFYPROC placeCardCallback) {
            this.portType = portType;
            this.portName = portName;
            this.placeCardCallback = placeCardCallback;
        }

        public void StartListen() {
            int returnCode;

            returnCode = ZRIntf.ZR_Initialize(ZPIntf.ZP_IF_NO_MSG_LOOP);
            if (returnCode < 0) {
                throw new Exception("ZLib initialization error");
            }

            ZR_RD_OPEN_PARAMS openParams = new ZR_RD_OPEN_PARAMS(this.portType, this.portName);
            ZR_RD_INFO readerInfo = new ZR_RD_INFO();
            returnCode = ZRIntf.ZR_Rd_Open(ref reader, ref openParams, readerInfo);
            if (returnCode < 0) {
                throw new Exception("Reader open error");
            }

            ZR_RD_NOTIFY_SETTINGS notifySettings = new ZR_RD_NOTIFY_SETTINGS(ZRIntf.ZR_RNF_PLACE_CARD, this.placeCardCallback);
            returnCode = ZRIntf.ZR_Rd_SetNotification(reader, notifySettings);

            if (returnCode < 0) {
                throw new Exception("Callback setup error");
            }
        }

        public void StopListen() {
            if (reader != IntPtr.Zero) {
                ZRIntf.ZR_CloseHandle(reader);
            }
            ZRIntf.ZR_Finalyze();
        }

    }
}
