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

namespace Spamihilator
{
  /// <summary>
  /// Base class for clients that send and receive data line by line
  /// </summary>
  class Client : Peer
  {
    /// <summary>
    /// Connect to a remote host
    /// </summary>
    /// <param name="host">the remote host</param>
    /// <param name="port">the port to connect</param>
    public void Connect(String host, int port)
    {
      IPAddress addr = IPAddress.Parse(host);
      IPEndPoint remoteEP = new IPEndPoint(addr, port);

      socket = new Socket(AddressFamily.InterNetwork,
        SocketType.Stream, ProtocolType.Tcp);
      socket.BeginConnect(remoteEP, ConnectCallback, this);
    }

    /// <summary>
    /// Will be called after a connection has been established successfully.
    /// </summary>
    /// <param name="ar">the result of the asynchronous operation</param>
    private static void ConnectCallback(IAsyncResult ar)
    {
      Client client = (Client)ar.AsyncState;
      client.socket.EndConnect(ar);
      client.OnConnect();
    }

    /// <summary>
    /// Will be called after a connection has been established
    /// </summary>
    virtual protected void OnConnect()
    {
      //nothing to do here
    }
  }
}
