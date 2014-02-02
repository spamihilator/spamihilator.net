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
  abstract class Peer
  {
    /// <summary>
    /// A delegate that will be called when an asynchronous send
    /// operation has completed successfully
    /// </summary>
    protected delegate void SendCallback();

    /// <summary>
    /// A callback that will be called when an asynchronous receive
    /// operation has completed successfully
    /// </summary>
    /// <param name="line">the received line</param>
    protected delegate void ReceiveCallback(String line);

    /// <summary>
    /// Logs communication with the peer to a file
    /// </summary>
    private static readonly ILog log = LogManager.GetLogger(
      System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// Internal buffer size for asynchronously received data
    /// </summary>
    private const int BUFFER_SIZE = 1024;

    /// <summary>
    /// Peer socket
    /// </summary>
    protected Socket socket;

    /// <summary>
    /// Internal buffer for asynchronously received data
    /// </summary>
    private byte[] buffer = new byte[BUFFER_SIZE];

    /// <summary>
    /// Internal buffer for asynchronously received lines
    /// </summary>
    private StringBuilder line = new StringBuilder();

    /// <summary>
    /// Gracefully shuts the connection down and releases all resources
    /// </summary>
    protected void Shutdown()
    {
      socket.Shutdown(SocketShutdown.Both);
      socket.Close();
    }

    /// <summary>
    /// Asynchronously receives a line from the peer
    /// </summary>
    /// <param name="callback">a method to call after the asynchronous
    /// receive operation has been completed successfully</param>
    protected void Receive(ReceiveCallback callback)
    {
      //receive more data
      socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None,
        ar => ReceiveCallbackInternal(ar, callback), this);
    }

    /// <summary>
    /// Will be called when data is received asynchronously
    /// </summary>
    /// <param name="ar">the result of the asynchronous operation</param>
    /// <param name="callback">a method to call after the asynchronous
    /// receive operation has been completed successfully</param>
    private static void ReceiveCallbackInternal(IAsyncResult ar,
      ReceiveCallback callback)
    {
      Peer p = (Peer)ar.AsyncState;
      int read = p.socket.EndReceive(ar);

      if (read > 0)
      {
        //look for end of line
        int l = read;
        for (int i = 0; i < read; ++i)
        {
          if (p.buffer[i] == '\n')
          {
            l = i;
            break;
          }
        }

        //trim '\r' if there is any
        int l2 = l;
        if (l2 > 0 && p.buffer[l2 - 1] == '\r')
        {
          --l2;
        }

        //append data to line buffer
        p.line.Append(Encoding.ASCII.GetString(p.buffer, 0, l2));

        if (l < read)
        {
          //we received a full line
          String line = p.line.ToString();
          log.Info(line);

          //clear line buffer and append rest of byte buffer
          //do this before calling the callback as it may
          //start another asynchronous receive operation
          p.line.Clear();
          p.line.Append(Encoding.ASCII.GetString(p.buffer, l + 1,
            read - l - 1));

          if (callback != null)
            callback(line);
        }
        else
        {
          //receive more data
          p.Receive(callback);
        }
      }
      else
      {
        //socket was closed by peer
        p.socket.Close();
      }
    }

    /// <summary>
    /// Sends a string to the peer
    /// </summary>
    /// <param name="str">the string to send</param>
    /// <param name="callback">a method to call after the string
    /// has been successfully sent</param>
    protected void Send(String str, SendCallback callback)
    {
      log.Info(str.TrimEnd());
      byte[] byteData = Encoding.ASCII.GetBytes(str);
      socket.BeginSend(byteData, 0, byteData.Length, 0,
        ar => SendCallbackInternal(ar, callback), this);
    }

    /// <summary>
    /// Sends a line to the peer
    /// </summary>
    /// <param name="line">the line to send</param>
    /// <param name="callback">a method to call after the line
    /// has been successfully sent</param>
    protected void SendLine(String line, SendCallback callback)
    {
      Send(line + "\r\n", callback);
    }

    /// <summary>
    /// Will be called when data is to be sent asychronously
    /// </summary>
    /// <param name="ar">the result of the asynchronous operation</param>
    /// <param name="callback">a method to call after the asynchronous
    /// send operation has been completed successfully</param>
    private static void SendCallbackInternal(IAsyncResult ar,
      SendCallback callback)
    {
      Peer p = (Peer)ar.AsyncState;
      int bytesSent = p.socket.EndSend(ar);

      if (callback != null)
      {
        callback();
      }
    }
  }
}
