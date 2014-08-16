using Alchemy;
using Alchemy.Classes;
using RFIDWSProxy.Utils;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace RFIDWSProxy {
    class WebSocketListener {
        private ConcurrentDictionary<EndPoint, UserContext> connectionPull;
        private WebSocketServer listener;

        public WebSocketListener(ref ConcurrentDictionary<EndPoint, UserContext> connectionPull) {
            this.connectionPull = connectionPull;

            this.listener = new WebSocketServer(Settings.instance().WebsocketListenPort, IPAddress.Any) {
                OnConnected = this.OnConnected,
                OnDisconnect = this.OnDisconnect,
                TimeOut = new TimeSpan(0, 5, 0)
            };
        }

        public void Start() {
            listener.Start();
        }

        public void Stop() {
            listener.Stop();
        }

        void OnConnected(UserContext context) {
            connectionPull.TryAdd(context.ClientAddress, context);

            Log.Write("On connected : " + context.ClientAddress.ToString());
            Log.Write("Clients Online: " + connectionPull.Count);
        }

        void OnDisconnect(UserContext context) {
            UserContext trash;
            connectionPull.TryRemove(context.ClientAddress, out trash);

            Log.Write("Client Disconnected " + context.ClientAddress);
            Log.Write("Clients Online: " + connectionPull.Count);
        }
    }
}
