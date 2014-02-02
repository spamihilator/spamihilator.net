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

namespace Spamihilator
{
  /// <summary>
  /// A message header's field
  /// </summary>
  public class MessageHeaderField
  {
    /// <summary>
    /// Well known field types
    /// </summary>
    public enum FieldType
    {
      To,
      From,
      ContentType,
      BCC,
      CC,
      Subject,
      Date,
      Other
    }

    /// <summary>
    /// The field's name
    /// </summary>
    public String Name { get; private set; }

    /// <summary>
    /// The field's body
    /// </summary>
    public String Body { get; private set; }

    /// <summary>
    /// The field's type
    /// </summary>
    public FieldType Type { get; private set; }

    /// <summary>
    /// Parses a field
    /// </summary>
    /// <param name="line">the line to parse</param>
    public MessageHeaderField(String line)
    {
      //get field name and body
      int colon = line.IndexOf(':');
      if (colon >= 0)
      {
        Name = line.Substring(0, colon).Trim();
        Body = line.Substring(colon + 1).Trim();
      }
      else
      {
        Name = line.Trim();
      }

      //determine type
      if (Name.Equals("To", StringComparison.OrdinalIgnoreCase))
      {
        Type = FieldType.To;
      }
      else if (Name.Equals("From", StringComparison.OrdinalIgnoreCase))
      {
        Type = FieldType.From;
      }
      else if (Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
      {
        Type = FieldType.ContentType;
      }
      else if (Name.Equals("BCC", StringComparison.OrdinalIgnoreCase))
      {
        Type = FieldType.BCC;
      }
      else if (Name.Equals("CC", StringComparison.OrdinalIgnoreCase))
      {
        Type = FieldType.CC;
      }
      else if (Name.Equals("Subject", StringComparison.OrdinalIgnoreCase))
      {
        Type = FieldType.Subject;
      }
      else if (Name.Equals("Date", StringComparison.OrdinalIgnoreCase))
      {
        Type = FieldType.Date;
      }
      else
      {
        Type = FieldType.Other;
      }
    }
  }
}
