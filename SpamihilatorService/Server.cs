// Copyright 2002-2014 Michel Kraemer
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using log4net;
using System;
using System.Net.Sockets;
using System.Text;

namespace Spamihilator
{
    /// <summary>
    /// Base class for servers that handle received data line by line
    /// </summary>
    abstract class Server
    {
        /// <summary>
        /// Internal buffer size for asynchronously received data
        /// </summary>
        private const int BUFFER_SIZE = 1024;

        /// <summary>
        /// Client socket
        /// </summary>
        private Socket socket;

        /// <summary>
        /// Internal buffer for asynchronously received data
        /// </summary>
        private byte[] buffer = new byte[BUFFER_SIZE];

        /// <summary>
        /// Internal buffer for asynchronously received lines
        /// </summary>
        private StringBuilder line = new StringBuilder();

        /// <summary>
        /// Logs communication with the server to a file
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Asynchronously accepts an incoming connection
        /// </summary>
        /// <param name="ar">the result of the asynchronous operation</param>
        public void Accept(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            socket = s.EndAccept(ar);

            OnConnect();

            //receive data asynchronously
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None,
                new AsyncCallback(ReceiveCallback), this);
        }

        /// <summary>
        /// Will be called when data is received asynchronously
        /// </summary>
        /// <param name="ar">the result of the asynchronous operation</param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            Server s = (Server)ar.AsyncState;
            int read = s.socket.EndReceive(ar);

            if (read > 0)
            {
                //look for end of line
                int l = read;
                for (int i = 0; i < read; ++i)
                {
                    if (s.buffer[i] == '\n')
                    {
                        l = i;
                        break;
                    }
                }

                //trim '\r' if there is any
                int l2 = l;
                if (l2 > 0 && s.buffer[l2 - 1] == '\r')
                {
                    --l2;
                }

                //append data to line buffer
                s.line.Append(Encoding.ASCII.GetString(s.buffer, 0, l2));

                if (l < read)
                {
                    //we received a full line. translate it...
                    if (!s.TranslateInternal(s.line.ToString()))
                    {
                        //quit signal received
                        s.socket.Shutdown(SocketShutdown.Both);
                        s.socket.Close();
                        return;
                    }

                    //clear line buffer and append rest of byte buffer
                    s.line.Clear();
                    s.line.Append(Encoding.ASCII.GetString(s.buffer, l + 1,
                        read - l - 1));
                }

                //receive more data
                s.socket.BeginReceive(s.buffer, 0, BUFFER_SIZE,
                    SocketFlags.None, new AsyncCallback(ReceiveCallback), s);
            }
            else
            {
                //socket was closed by peer
                s.socket.Close();
            }
        }

        private bool TranslateInternal(String line)
        {
            log.Info(line);
            return Translate(line);
        }

        /// <summary>
        /// Will be called after a connection has been established
        /// </summary>
        virtual protected void OnConnect()
        {
            //nothing to do here
        }

        /// <summary>
        /// Translates a line
        /// </summary>
        /// <param name="line">the line to translate</param>
        /// <returns><code>true</code> if the process should continue,
        /// <code>false</code> if the connection should be closed.</returns>
        abstract protected bool Translate(String line);

        /// <summary>
        /// Sends a string to the client
        /// </summary>
        /// <param name="str">the string to send</param>
        protected void Send(String str)
        {
            log.Info(str.TrimEnd());
            byte[] byteData = Encoding.ASCII.GetBytes(str);
            socket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), this);
        }

        /// <summary>
        /// Sends a line to the client
        /// </summary>
        /// <param name="line">the line to send</param>
        protected void SendLine(String line)
        {
            Send(line + "\r\n");
        }

        /// <summary>
        /// Will be called when data is to be sent asychronously
        /// </summary>
        /// <param name="ar">the result of the asynchronous operation</param>
        private static void SendCallback(IAsyncResult ar)
        {
            Server s = (Server)ar.AsyncState;
            int bytesSent = s.socket.EndSend(ar);
        }
    }
}
