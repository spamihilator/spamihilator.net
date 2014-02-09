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

using System;
using System.Net;
using System.Net.Sockets;

namespace Spamihilator {
  /// <summary>
  /// Base class for clients that send and receive data line by line
  /// </summary>
  public class Client : Peer {
    public delegate void ConnectCallback();

    /// <summary>
    /// Connect to a remote host
    /// </summary>
    /// <param name="host">the remote host</param>
    /// <param name="port">the port to connect</param>
    /// <param name="callback">a method that will be called when
    /// the connection has been established successfully</param>
    virtual public void Connect(String host, int port,
      ConnectCallback callback) {
      IPHostEntry entry = Dns.GetHostEntry(host);
      socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
      socket.BeginConnect(entry.AddressList, port,
        ar => ConnectCallbackInternal(ar, callback), this);
    }

    /// <summary>
    /// Will be called after a connection has been established successfully.
    /// </summary>
    /// <param name="ar">the result of the asynchronous operation</param>
    /// <param name="callback">a method that will be called when
    /// the connection has been established successfully</param>
    private static void ConnectCallbackInternal(IAsyncResult ar,
      ConnectCallback callback) {
      Client client = (Client)ar.AsyncState;
      client.socket.EndConnect(ar);
      if (callback != null) {
        callback();
      }
    }
  }
}
