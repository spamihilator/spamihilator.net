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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Spamihilator {
  /// <summary>
  /// A message node
  /// </summary>
  public class MessageNode {
    private String node;

    /// <summary>
    /// The node's header
    /// </summary>
    public MessageHeader Header { get; private set; }

    /// <summary>
    /// The node's body (or null if the node has children)
    /// </summary>
    public String Body { get; private set; }

    /// <summary>
    /// The node's children (empty if the node has a body)
    /// </summary>
    public IReadOnlyList<MessageNode> Children { get; private set; }

    /// <summary>
    /// Parses a message node
    /// </summary>
    /// <param name="node">the node to parse</param>
    public MessageNode(String node) {
      this.node = node;

      using (StringReader reader = new StringReader(node)) {
        Header = ParseHeader(reader);
        String boundary = GetBoundary(GetFields(
          MessageHeaderField.FieldType.ContentType));
        if (boundary != null) {
          Children = ParseBody(reader, boundary);
          Body = null;
        } else {
          Children = new List<MessageNode>();
          Body = ParseBody(reader);
        }
      }
    }

    /// <summary>
    /// Parses the node header
    /// </summary>
    /// <param name="reader">reads lines from the node</param>
    /// <returns>the header</returns>
    private static MessageHeader ParseHeader(StringReader reader) {
      List<MessageHeaderField> headerFields = new List<MessageHeaderField>();

      String currentHeaderLine = "";
      String line;
      while ((line = reader.ReadLine()) != null) {
        if (line.Length == 0) {
          //end of header
          if (currentHeaderLine.Length != 0) {
            headerFields.Add(new MessageHeaderField(currentHeaderLine));
          }
          break;
        }

        if (line.StartsWith("\t") || line.StartsWith(" ")) {
          //multi-line header field
          currentHeaderLine += " " + line.Trim();
        } else {
          if (currentHeaderLine.Length != 0) {
            headerFields.Add(new MessageHeaderField(currentHeaderLine));
          }
          currentHeaderLine = line.TrimEnd();
        }
      }

      return new MessageHeader(headerFields);
    }

    /// <summary>
    /// Parses the Content-Type header fields and returns the
    /// boundary for the parts in a multi-part message. Returns
    /// the first boundary found.
    /// </summary>
    /// <param name="contentTypes">the Content-Type header fields</param>
    /// <returns>the boundary or null if the message is not a
    /// multi-part message</returns>
    private static String GetBoundary(
        IEnumerable<MessageHeaderField> contentTypes) {
      String boundary = null;
      foreach (MessageHeaderField f in contentTypes) {
        //check if this is a multi-part message
        if (f.Body.IndexOf("multipart",
          StringComparison.OrdinalIgnoreCase) >= 0) {
          //look for boundary
          int bound = f.Body.IndexOf("boundary",
            StringComparison.OrdinalIgnoreCase);
          if (bound >= 0) {
            bound += 8; //"boundary".Length
            int start = f.Body.IndexOf('=', bound);
            if (start < 0)
              start = bound;
            else
              start++;

            //skip to beginning of boundary
            while (start < f.Body.Length &&
              Char.IsWhiteSpace(f.Body[start])) start++;

            int end;
            if (f.Body[start] == '"') {
              //handle quoted boundary
              ++start;
              end = start;
              while (end < f.Body.Length) {
                if (f.Body[end] == '\\' && end < f.Body.Length - 1 &&
                  f.Body[end + 1] == '"') {
                  //skip escaped " character
                  ++end;
                } else if (f.Body[end] == '"') {
                  break;
                }
                ++end;
              }
            } else {
              //skip to end of parameter or end of field
              end = start;
              while (end < f.Body.Length && f.Body[end] != ';')
                ++end;
            }

            //extract boundary
            int len = end - start;
            boundary = f.Body.Substring(start, len);
            break;
          }
        }
      }

      //return null if boundary is empty
      if (boundary != null && boundary.Length == 0) {
        boundary = null;
      }

      return boundary;
    }

    /// <summary>
    /// Parses the body of a message that is not a multi-part message
    /// </summary>
    /// <param name="reader">reads the message's contents</param>
    /// <returns>the message body</returns>
    private static String ParseBody(StringReader reader) {
      StringBuilder body = new StringBuilder();
      String line;
      while ((line = reader.ReadLine()) != null) {
        if (body.Length > 0)
          body.Append("\r\n");
        body.Append(line);
      }

      return body.ToString();
    }

    /// <summary>
    /// Parses the body of a multi-part message
    /// </summary>
    /// <param name="reader">returns the message's contents</param>
    /// <param name="boundary">the boundary that separates multi-part
    /// nodes</param>
    /// <returns>a list of parsed multi-part nodes</returns>
    private static IReadOnlyList<MessageNode> ParseBody(
        StringReader reader, String boundary) {
      //skip everything until first boundary
      String line;
      while ((line = reader.ReadLine()) != null) {
        if (line.Equals("--" + boundary))
          break;
      }

      //read nodes
      List<MessageNode> nodes = new List<MessageNode>();
      StringBuilder body = new StringBuilder();
      while ((line = reader.ReadLine()) != null) {
        if (line.Equals("--" + boundary)) {
          //new node
          if (body.Length > 0)
            nodes.Add(new MessageNode(body.ToString()));
          body.Clear();
        } else if (line.Equals("--" + boundary + "--")) {
          //last node
          break;
        } else {
          //save line for current node
          if (body.Length > 0)
            body.Append("\r\n");
          body.Append(line);
        }
      }

      //create a new node for remaining lines (happens if there
      //is no final boundary)
      if (body.Length > 0)
        nodes.Add(new MessageNode(body.ToString()));

      return nodes;
    }

    /// <summary>
    /// Returns a list of header fields having the given name
    /// </summary>
    /// <param name="name">the header field name</param>
    /// <returns>a list of header fields having the given name (may be
    /// empty)</returns>
    public IEnumerable<MessageHeaderField> GetFields(String name) {
      return Header.GetFields(name);
    }

    /// <summary>
    /// Returns the body of the first header field having the given name
    /// </summary>
    /// <param name="name">the header field name</param>
    /// <returns>the body of the header field or null if there
    /// is no such field</returns>
    public String GetFieldBody(String name) {
      return Header.GetFieldBody(name);
    }

    /// <summary>
    /// Returns a list of header fields having the given type
    /// </summary>
    /// <param name="type">the header field type</param>
    /// <returns>a list of header fields having the given type (may be
    /// empty</returns>
    public IEnumerable<MessageHeaderField> GetFields(
        MessageHeaderField.FieldType type) {
      return Header.GetFields(type);
    }

    /// <summary>
    /// Returns the body of the first header field having the given type
    /// </summary>
    /// <param name="type">the header field type</param>
    /// <returns>the body of the header field or null if there
    /// is no such field</returns>
    public String GetFieldBody(MessageHeaderField.FieldType type) {
      return Header.GetFieldBody(type);
    }
  }
}
