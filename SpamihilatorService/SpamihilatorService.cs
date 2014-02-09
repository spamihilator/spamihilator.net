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
using System.ServiceProcess;
using System.Threading;

namespace Spamihilator {
  /// <summary>
  /// Spamihilator's background service
  /// </summary>
  public class SpamihilatorService : ServiceBase {
    public static ManualResetEvent accepted = new ManualResetEvent(false);

    /// <summary>
    /// The service's main method
    /// </summary>
    public void Run() {
      //listen to ::1, port 115
      TcpListener listener = new TcpListener(IPAddress.IPv6Loopback, 115);
      listener.Start();

      //accept incoming connections
      while (true) {
        accepted.Reset();
        listener.BeginAcceptSocket(AcceptCallback, listener);
        accepted.WaitOne();
      }
    }

    /// <summary>
    /// Asynchronously accepts an incoming connection
    /// </summary>
    /// <param name="ar">the result of the asynchronous operation</param>
    private static void AcceptCallback(IAsyncResult ar) {
      accepted.Set();
      Pop3Server ps = new Pop3Server();
      ps.Accept(ar);
    }
  }
}
