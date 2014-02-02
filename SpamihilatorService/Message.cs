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
  /// A message
  /// </summary>
  public class Message {
    /// <summary>
    /// The message's original text
    /// </summary>
    public String Text { get; private set; }

    /// <summary>
    /// The message's root node
    /// </summary>
    public MessageNode Root { get; private set; }

    /// <summary>
    /// Parses a message text
    /// </summary>
    /// <param name="text">the text to parse</param>
    public Message(String text) {
      Text = text;
      Root = new MessageNode(text);
    }
  }
}
