using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using log4net;
using SuperWebSocket;
using Tct.ActivityRecorderClient.ChromeCaptureServiceReference;
using System.Threading.Tasks;
using SuperSocket.SocketBase;

namespace Tct.ActivityRecorderClient.Communication
{
    class EdgeCaptureClientWrapper : IDisposable
    {
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static WebSocketServer socketServer;

        public EdgeCaptureClientWrapper()
        {
            socketServer = new WebSocketServer();
            if (!socketServer.Setup("127.215.40.10", 9696))
            {
                return;
            }

            socketServer.NewMessageReceived += WsOnNewMessageReceived;
            socketServer.NewSessionConnected += WebsocketOnNewSessionConnected;
            socketServer.SessionClosed += SocketServerOnSessionClosed;
            socketServer.Start();
        }

        private void SocketServerOnSessionClosed(WebSocketSession session, CloseReason value)
        {
            log.Debug("Edge client disconnected ({0}) with reason: {1}".FS(session.Origin, value.ToString()));            
        }

        private static void WebsocketOnNewSessionConnected(WebSocketSession session)
        {
            log.Debug("Edge client connected ({0})".FS(session.Origin));
        }

        private static void WsOnNewMessageReceived(WebSocketSession session, string value)
        {
            resString = value;
        }

        private static string resString;
        static object _lock;
        public static string Send(string cmd)
        {
            try
            {

                if (socketServer == null)
                {
                    socketServer = new WebSocketServer();
                    if (!socketServer.Setup("127.215.40.10", 9696))
                    {
                        return "";
                    }

                    socketServer.NewMessageReceived += WsOnNewMessageReceived;
                    socketServer.NewSessionConnected += WebsocketOnNewSessionConnected;
                    socketServer.Start();
                }

                var session = socketServer.GetAllSessions().ToList().FirstOrDefault();
                if (session != null)
                {
                    resString = "";
                    session.Send(cmd);
                    int cnt = 0;
                    while (string.IsNullOrEmpty(resString))
                    {
                        
                       Thread.Sleep(10);
                        if (cnt >= 20)
                        {
                            return "";
                        }
                        cnt++;
                    }
                    var ret = resString;
                    resString = "";
                    return ret;
                }
                return "";
            }
            catch (CommunicationException ex)
            {
                CloseIfUnusable(ex);
                throw;
            }
        }

        public static T Execute<T>(Func<WebSocketServer, T> command)
        {
            try
            {
                if (socketServer == null)
                {
                    socketServer = new WebSocketServer();
                    if (!socketServer.Setup("127.215.40.10", 9696))
                    {
                        return default(T);
                    }

                    socketServer.NewMessageReceived += WsOnNewMessageReceived;
                    socketServer.NewSessionConnected += WebsocketOnNewSessionConnected;
                    socketServer.Start();
                }

                return command(socketServer);
            }
            catch (CommunicationException ex)
            {
                CloseIfUnusable(ex);
                throw;
            }
        }

        private static void CloseIfUnusable(Exception ex)
        {
            if (!(ex is CommunicationException) || ex is FaultException) return;
            log.VerboseFormat("Closing shared client in state {0}", socketServer.State);
            socketServer.Dispose();
            socketServer = null;
        }

        public void Dispose()
        {
            socketServer.Dispose();
        }
    }
}
