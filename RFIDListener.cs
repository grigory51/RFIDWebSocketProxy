using System;
using ZPort;
using ZREADER;

namespace RFIDWSProxy {
    class RFIDListener {
        IntPtr reader = new IntPtr();

        public ZP_PORT_TYPE PortType { get; set; }
        public string PortName { get; set; }

        public ZP_NOTIFYPROC PlaceCardCallback { get; set; }

        public RFIDListener(ZP_PORT_TYPE portType, string portName) {
            this.PortType = portType;
            this.PortName = portName;
        }

        public bool Open() {
            int returnCode;

            returnCode = ZRIntf.ZR_Initialize(ZPIntf.ZP_IF_NO_MSG_LOOP);
            if (returnCode < 0) {
                return false;
            }

            ZR_RD_OPEN_PARAMS openParams = new ZR_RD_OPEN_PARAMS(this.PortType, this.PortName);
            ZR_RD_INFO readerInfo = new ZR_RD_INFO();
            returnCode = ZRIntf.ZR_Rd_Open(ref reader, ref openParams, readerInfo);
            if (returnCode < 0) {
                return false;
            }

            return true;
        }

        public bool StartListen() {
            ZR_RD_NOTIFY_SETTINGS notifySettings = new ZR_RD_NOTIFY_SETTINGS(ZRIntf.ZR_RNF_PLACE_CARD, this.PlaceCardCallback);
            int returnCode = ZRIntf.ZR_Rd_SetNotification(reader, notifySettings);

            if (returnCode < 0) {
                return false;
            }
            return true;
        }

        public void Destroy() {
            if (reader != IntPtr.Zero) {
                ZRIntf.ZR_CloseHandle(reader);
            }
            ZRIntf.ZR_Finalyze();
        }       
    }
}
