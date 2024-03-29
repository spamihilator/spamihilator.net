﻿// Copyright 2002-2014 Michel Kraemer
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
  /// A POP3 server
  /// </summary>
  class Pop3Server : Server {
    /// <summary>
    /// Sends a success message
    /// </summary>
    /// <param name="line">the message to send</param>
    private void SendOK(String line) {
      SendLine("+OK " + line, () => Receive(Translate));
    }

    /// <summary>
    /// Sends an error message
    /// </summary>
    /// <param name="line">the message to send</param>
    private void SendERR(String line) {
      SendLine("-ERR " + line, () => Receive(Translate));
    }

    override protected void OnConnect() {
      SendOK("Spamihilator ready.");
    }

    private void Translate(String line) {
      String up = line.ToUpper();
      if (up == "QUIT") {
        SendLine("+OK Everything done.", Shutdown);
      } else {
        SendOK(line);
      }
    }
  }
}
