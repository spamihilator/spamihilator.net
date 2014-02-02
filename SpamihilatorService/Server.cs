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
using System.Net.Sockets;

namespace Spamihilator
{
  /// <summary>
  /// Base class for servers that handle received data line by line
  /// </summary>
  abstract class Server : Peer
  {
    /// <summary>
    /// Asynchronously accepts an incoming connection
    /// </summary>
    /// <param name="ar">the result of the asynchronous operation</param>
    public void Accept(IAsyncResult ar)
    {
      Socket s = (Socket)ar.AsyncState;
      socket = s.EndAccept(ar);
      OnConnect();
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
