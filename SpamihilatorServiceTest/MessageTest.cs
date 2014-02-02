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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spamihilator;

namespace SpamihilatorServiceTest {
  /// <summary>
  /// Tests the Message class
  /// </summary>
  [TestClass]
  public class MessageTest {
    /// <summary>
    /// Tests if a simple message can be parsed
    /// </summary>
    [TestMethod]
    public void ParseSimpleMessage() {
      String text = @"From: Alice <alice@foo.com>
To: Bob <bob@foo.com>
Subject: Greetings

Hello Bob!
";
      Message msg = new Message(text);
      Assert.AreEqual(text, msg.Text);
      Assert.AreEqual("Hello Bob!", msg.Root.Body);
      Assert.AreEqual("Greetings", msg.Root.GetFieldBody("Subject"));
      Assert.AreEqual("Alice <alice@foo.com>", msg.Root.GetFieldBody(
          MessageHeaderField.FieldType.From));
      Assert.AreEqual("Bob <bob@foo.com>", msg.Root.GetFieldBody(
          MessageHeaderField.FieldType.To));
      Assert.AreEqual("Greetings", msg.Root.GetFieldBody(
          MessageHeaderField.FieldType.Subject));
    }

    /// <summary>
    /// Tests if a multi-part message can be parsed
    /// </summary>
    [TestMethod]
    public void ParseMultipartMessage() {
      String text = @"From: Alice <alice@foo.com>
To: Bob <bob@foo.com>
Subject: Greetings
Content-Type: multipart/alternative; boundary=boundary42

This is a multipart message
--boundary42
Content-Type: text/plain; charset=us-ascii

Hello Bob!
--boundary42
Content-Type: text/plain; charset=us-ascii

Dear Bob!
--boundary42
Content-Type: text/plain; charset=us-ascii

Merry Christmas!
--boundary42--
";

      Message msg = new Message(text);
      Assert.AreEqual("Greetings", msg.Root.GetFieldBody("Subject"));
      Assert.AreEqual(3, msg.Root.Children.Count);
      Assert.IsNull(msg.Root.Body);
      Assert.AreEqual("Hello Bob!", msg.Root.Children[0].Body);
      Assert.AreEqual("Dear Bob!", msg.Root.Children[1].Body);
      Assert.AreEqual("Merry Christmas!", msg.Root.Children[2].Body);
    }

    /// <summary>
    /// Tests if a multi-part message with nested nodes can be parsed
    /// </summary>
    [TestMethod]
    public void ParseNestedMultipartMessage() {
      String text = @"From: Alice <alice@foo.com>
To: Bob <bob@foo.com>
Subject: Greetings
Content-Type: multipart/alternative; boundary=boundary42

This is a multipart message
--boundary42
Content-Type: multipart/alternative; boundary=""in\\""ner""

Ignore
--in\\""ner
Content-Type: text/plain; charset=us-ascii

Hello Bob!
--in\\""ner
Content-Type: text/plain; charset=us-ascii

To whom it may concern!
--in\\""ner--
Ignore2
--boundary42
Content-Type: text/plain; charset=us-ascii

Dear Bob!
--boundary42
Content-Type: text/plain; charset=us-ascii

Merry Christmas!
--boundary42--
Remaining ignore.
";

      Message msg = new Message(text);
      Assert.AreEqual("Greetings", msg.Root.GetFieldBody("Subject"));
      Assert.AreEqual(3, msg.Root.Children.Count);
      Assert.IsNull(msg.Root.Body);
      Assert.IsNull(msg.Root.Children[0].Body);
      Assert.AreEqual(2, msg.Root.Children[0].Children.Count);
      Assert.AreEqual("Hello Bob!", msg.Root.Children[0].Children[0].Body);
      Assert.AreEqual("To whom it may concern!",
        msg.Root.Children[0].Children[1].Body);
      Assert.AreEqual("Dear Bob!", msg.Root.Children[1].Body);
      Assert.AreEqual("Merry Christmas!", msg.Root.Children[2].Body);
    }
  }
}
