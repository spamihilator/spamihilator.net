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
using System.Collections.Generic;
using System.Text;

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
    /// <param name="status">the operation's result message</param>
    public delegate void ClientCallback(bool success, String status);

    /// <summary>
    /// A callback that will be invoked when a list of unique IDs
    /// has been received from the server
    /// </summary>
    /// <param name="success">true if the list of unique IDs was
    /// received successfully</param>
    /// <param name="status">the operation's result message (an
    /// error message in case <code>success</code> is false)</param>
    /// <param name="uniqueIds">a dictionary mapping message IDs to unique
    /// IDs (empty in case <code>success</code> is false)</param>
    public delegate void ListUniqueIdsCallback(bool success, String status,
      Dictionary<long, String> uniqueIds);

    /// <summary>
    /// A callback that will be invoked when a list of messages
    /// has been received from the server
    /// </summary>
    /// <param name="success">true if the list of messages was
    /// received successfully</param>
    /// <param name="status">the operation's result message (an
    /// error message in case <code>success</code> is false)</param>
    /// <param name="messages">a dictionary mapping message IDs to message
    /// contents (empty in case <code>success</code> is false)</param>
    public delegate void ListCallback(bool success, String status,
      Dictionary<long, long> messages);

    /// <summary>
    /// A callback that will be invoked when a message has been
    /// retrieved from the server
    /// </summary>
    /// <param name="success">true if the message was received
    /// successfully</param>
    /// <param name="status">the operation's result message (an
    /// error message in case <code>success</code> is false)</param>
    /// <param name="message">the message contents (empty in case
    /// <code>success</code> is false)</param>
    public delegate void RetrieveCallback(bool success, String status,
      String message);

    /// <summary>
    /// Logs communication with the peer to a file
    /// </summary>
    private static readonly ILog log = LogManager.GetLogger(
      System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

    /// <summary>
    /// Deletes a message from the server
    /// </summary>
    /// <param name="id">the message's ID</param>
    /// <param name="callback">a method that will be called after the
    /// message has been deleted and the response has been received</param>
    public void Delete(long id, ClientCallback callback) {
      SendAndReceive("DELE " + id, callback);
    }

    /// <summary>
    /// Lists unique IDs of messages
    /// </summary>
    /// <param name="callback">a method that will be called after
    /// the list of unique IDs has been received from the server</param>
    public void ListUniqueIds(ListUniqueIdsCallback callback) {
      ListInternal<long, String>("UIDL", long.Parse, s => s,
        (s, m, d) => callback(s, m, d), "unique-id");
    }

    /// <summary>
    /// Lists messages
    /// </summary>
    /// <param name="callback">a method that will be called after
    /// the list of messages has been received from the server</param>
    public void List(ListCallback callback) {
      ListInternal<long, long>("LIST", long.Parse, long.Parse,
        (s, m, d) => callback(s, m, d), "scan");
    }

    /// <summary>
    /// Send a list command to the server and reads its multi-line response
    /// consisting of key-value listings.
    /// </summary>
    /// <typeparam name="K">the type of keys to read</typeparam>
    /// <typeparam name="V">the type of values to read</typeparam>
    /// <param name="command">the command to send to the server</param>
    /// <param name="mk">a function that parses a string to a key</param>
    /// <param name="mv">a function that parses a string to a value</param>
    /// <param name="callback">a callback to call after the whole
    /// multi-line response has been read and after all key-value pairs
    /// have been converted.</param>
    /// <param name="listingName">a listing's name (used to produce a
    /// human-readable error message if a listing is invalid)</param>
    private void ListInternal<K, V>(String command, Func<String, K> mk,
        Func<String, V> mv, Action<bool, String, Dictionary<K, V>> callback,
        String listingName) {
      SendLine(command, () => {
        Receive(ur => {
          var listings = new Dictionary<K, V>();
          if (!IsOK(ur)) {
            callback(false, ur, listings);
          } else {
            ReceiveCallback handleLine = null;
            handleLine = line => {
              if (line == ".") {
                callback(true, ur, listings);
              } else {
                int sp = line.IndexOf(' ');
                if (sp < 0) {
                  sp = line.IndexOf('\t');
                }
                if (sp <= 0) {
                  log.Error("POP3 server returned invalid " + listingName +
                    " listing");
                } else {
                  K key = mk(line.Substring(0, sp));
                  V value = mv(line.Substring(sp + 1));
                  listings.Add(key, value);
                }
                Receive(handleLine);
              }
            };
            Receive(handleLine);
          }
        });
      });
    }

    /// <summary>
    /// Retrieve a message from the server
    /// </summary>
    /// <param name="id">the message's ID</param>
    /// <param name="callback">a callback that will be invoked after
    /// the message was received successfully</param>
    public void Retrieve(long id, RetrieveCallback callback) {
      SendLine("RETR " + id, () => {
        Receive(ur => {
          var message = new StringBuilder();
          if (!IsOK(ur)) {
            callback(false, ur, message.ToString());
          } else {
            ReceiveCallback handleLine = null;
            handleLine = line => {
              if (line == ".") {
                callback(true, ur, message.ToString());
              } else {
                if (line.StartsWith(".")) {
                  line = line.Substring(1);
                }
                message.AppendLine(line);
                Receive(handleLine);
              }
            };
            Receive(handleLine);
          }
        });
      });
    }
  }
}
