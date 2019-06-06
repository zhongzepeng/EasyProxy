using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EasyProxy.Core
{
    public class TimeoutSocketWrapper
    {
        private readonly Socket socket;
        private readonly IPEndPoint endPoint;
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private bool connectSuccess;

        public TimeoutSocketWrapper(IPEndPoint endPoint)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.endPoint = endPoint;
        }

        public Socket Connect(int timeout)
        {
            manualResetEvent.Reset();
            socket.BeginConnect(endPoint, new AsyncCallback(OnConnect), socket);
            if (manualResetEvent.WaitOne(timeout, false))
            {
                if (connectSuccess)
                {
                    return socket;
                }
                return null;
            }
            else
            {
                socket.Close();
                return null;
            }
        }

        private void OnConnect(IAsyncResult asyncResult)
        {
            var tsocket = asyncResult.AsyncState as Socket;
            connectSuccess = true;
            try
            {
                tsocket.EndConnect(asyncResult);
            }
            catch (Exception)
            {
                connectSuccess = false;
            }
            manualResetEvent.Set();
        }
    }
}
