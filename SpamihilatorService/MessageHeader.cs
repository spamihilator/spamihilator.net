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
using System.Linq;

namespace Spamihilator
{
  using FieldType = MessageHeaderField.FieldType;
  using FieldsByType = ILookup<MessageHeaderField.FieldType, MessageHeaderField>;

  /// <summary>
  /// A message header
  /// </summary>
  public class MessageHeader
  {
    private FieldsByType fieldsByType;

    /// <summary>
    /// All header fields
    /// </summary>
    public IReadOnlyList<MessageHeaderField> Fields { get; private set; }

    /// <summary>
    /// Constructs a new header
    /// </summary>
    /// <param name="fields">the parsed header fields</param>
    public MessageHeader(IReadOnlyList<MessageHeaderField> fields)
    {
      Fields = fields;
      fieldsByType = (FieldsByType)fields.ToLookup(f => f.Type);
    }

    /// <summary>
    /// Returns a list of fields having the given name
    /// </summary>
    /// <param name="name">the field name</param>
    /// <returns>a list of fields having the given name (may be
    /// empty)</returns>
    public IEnumerable<MessageHeaderField> GetFields(String name)
    {
      return Fields.Where(f => f.Name.Equals(name,
          StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns the body of the first field having the given name
    /// </summary>
    /// <param name="name">the field name</param>
    /// <returns>the body of the field or null if there
    /// is no such field</returns>
    public String GetFieldBody(String name)
    {
      MessageHeaderField r = Fields.FirstOrDefault(f => f.Name.Equals(
          name, StringComparison.OrdinalIgnoreCase));
      return (r != null ? r.Body : null);
    }

    /// <summary>
    /// Returns a list of fields having the given type
    /// </summary>
    /// <param name="type">the field type</param>
    /// <returns>a list of fields having the given type (may be
    /// empty</returns>
    public IEnumerable<MessageHeaderField> GetFields(FieldType type)
    {
      return fieldsByType[type];
    }

    /// <summary>
    /// Returns the body of the first field having the given type
    /// </summary>
    /// <param name="type">the field type</param>
    /// <returns>the body of the field or null if there
    /// is no such field</returns>
    public String GetFieldBody(FieldType type)
    {
      MessageHeaderField r = fieldsByType[type].FirstOrDefault();
      return (r != null ? r.Body : null);
    }
  }
}
