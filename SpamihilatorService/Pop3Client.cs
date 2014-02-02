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

namespace Spamihilator {
  /// <summary>
  /// A POP3 mail client
  /// </summary>
  public class Pop3Client : Client {
    /// <summary>
    /// A generic callback method
    /// </summary>
    /// <param name="success">true if the asynchronous operation was
    /// successful</param>
    /// <param name="msg">the operation's result message</param>
    public delegate void ClientCallback(bool success, String msg);

    /// <summary>
    /// Checks if the given message is an error message
    /// </summary>
    /// <param name="line">the message</param>
    /// <returns>true if the message is an error</returns>
    private bool IsError(String line) {
      return line.StartsWith("-ERR");
    }

    /// <summary>
    /// Checks if the given message is a success message
    /// </summary>
    /// <param name="line">the message</param>
    /// <returns>true if the message is a success message</returns>
    private bool IsOK(String line) {
      return line.StartsWith("+OK");
    }

    /// <summary>
    /// Converts the given generic callback to a receive callback
    /// </summary>
    /// <param name="callback">the generic callback</param>
    /// <returns>the receive callback</returns>
    private ReceiveCallback To(ClientCallback callback) {
      if (callback != null) {
        return result => callback(IsOK(result), result);
      }
      return null;
    }

    /// <summary>
    /// Sends the given line, reads the response from the server
    /// and invokes the given generic callback
    /// </summary>
    /// <param name="line">the line to send</param>
    /// <param name="callback">the method to invoke after the response
    /// has been received from the server</param>
    private void SendAndReceive(String line, ClientCallback callback) {
      SendLine(line, () => Receive(To(callback)));
    }

    /// <summary>
    /// Connects to the given POP3 server
    /// </summary>
    /// <param name="host">the server's host name</param>
    /// <param name="port">the port to connect to</param>
    /// <param name="callback">a method that will be called when the
    /// connection was successful</param>
    override public void Connect(String host, int port,
      ConnectCallback callback) {
      //connect, read and discard welcome message, and then invoke callback
      base.Connect(host, port, () => Receive(_ => callback()));
    }

    /// <summary>
    /// Login to the POP3 server using the given credentials
    /// </summary>
    /// <param name="user">the user name</param>
    /// <param name="password">the password</param>
    /// <param name="callback">a callback method that will be invoked after
    /// the login has been performed</param>
    public void Login(String user, String password, ClientCallback callback) {
      SendLine("USER " + user, () => {
        Receive(ur => {
          if (!IsOK(ur)) {
            callback(false, ur);
          } else {
            SendAndReceive("PASS " + password, callback);
          }
        });
      });
    }

    /// <summary>
    /// Logout from the POP3 server
    /// </summary>
    /// <param name="callback">a method that will be called after the
    /// logout has been performed</param>
    public void Logout(ClientCallback callback) {
      SendAndReceive("QUIT", callback);
    }

    /// <summary>
    /// Send a NOOP signal to the server
    /// </summary>
    /// <param name="callback">a method that will be called after the
    /// message has been sent and the response has been received</param>
    public void Noop(ClientCallback callback) {
      SendAndReceive("NOOP", callback);
    }
  }
}
