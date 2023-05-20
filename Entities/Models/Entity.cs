using Entities.LinkModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Entities.Models
{
    /// <summary>
    /// Represents an entity with dynamic properties that can be serialized/deserialized to/from XML.
    /// </summary>
    public class Entity : DynamicObject, IXmlSerializable, IDictionary<string, object>
    {
        private readonly string _root = "Entity";
        private readonly IDictionary<string, object> _expando = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        public Entity()
        {
            _expando = new ExpandoObject();
        }

        /// <summary>
        /// Provides the implementation for operations that get member values.
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_expando.TryGetValue(binder.Name, out object value))
            {
                result = value;
                return true;
            }

            return base.TryGetMember(binder, out result);
        }

        /// <summary>
        /// Provides the implementation for operations that set member values.
        /// </summary>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _expando[binder.Name] = value;

            return true;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads XML data and populates the entity's dynamic properties.
        /// </summary>
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement(_root);

            while (!reader.Name.Equals(_root))
            {
                string typeContent;
                Type underlyingType;
                var name = reader.Name;

                reader.MoveToAttribute("type");
                typeContent = reader.ReadContentAsString();
                underlyingType = Type.GetType(typeContent);
                reader.MoveToContent();
                _expando[name] = reader.ReadElementContentAs(underlyingType, null);
            }
        }

        /// <summary>
        /// Writes the entity's dynamic properties to XML.
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            foreach (var key in _expando.Keys)
            {
                var value = _expando[key];
                WriteLinksToXml(key, value, writer);
            }
        }

        /// <summary>
        /// Writes the property and its value to XML.
        /// </summary>
        private void WriteLinksToXml(string key, object value, XmlWriter writer)
        {
            writer.WriteStartElement(key);
            if (value.GetType() == typeof(List<Link>))
            {
                foreach (var val in value as List<Link>)
                {
                    writer.WriteStartElement(nameof(Link));
                    WriteLinksToXml(nameof(val.Href), val.Href, writer);
                    WriteLinksToXml(nameof(val.Method), val.Method, writer);
                    WriteLinksToXml(nameof(val.Rel), val.Rel, writer);
                    writer.WriteEndElement();
                }
            }
            else
            { 
                writer.WriteString(value.ToString());
            }
        writer.WriteEndElement();
        }

        // IDictionary<string, object> implementation

        /// <summary>
        /// Adds a new dynamic property to the entity.
        /// </summary>
        public void Add(string key, object value)
        {
            _expando.Add(key, value);
        }

        /// <summary>
        /// Determines whether the entity contains the specified dynamic property.
        /// </summary>
        public bool ContainsKey(string key)
        {
            return _expando.ContainsKey(key);
        }

        /// <summary>
        /// Gets a collection containing the keys of the dynamic properties.
        /// </summary>
        public ICollection<string> Keys => _expando.Keys;

        /// <summary>
        /// Removes the specified dynamic property from the entity.
        /// </summary>
        public bool Remove(string key)
        {
            return _expando.Remove(key);
        }

        /// <summary>
        /// Tries to get the value associated with the specified key.
        /// </summary>
        public bool TryGetValue(string key, out object value)
        {
            return _expando.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets a collection containing the values of the dynamic properties.
        /// </summary>
        public ICollection<object> Values => _expando.Values;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        public object this[string key]
        {
            get => _expando[key];
            set => _expando[key] = value;
        }

        /// <summary>
        /// Adds a new dynamic property to the entity.
        /// </summary>
        public void Add(KeyValuePair<string, object> item)
        {
            _expando.Add(item);
        }

        /// <summary>
        /// Removes all dynamic properties from the entity.
        /// </summary>
        public void Clear()
        {
            _expando.Clear();
        }

        /// <summary>
        /// Determines whether the entity contains a specific dynamic property.
        /// </summary>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return _expando.Contains(item);
        }

        /// <summary>
        /// Copies the dynamic properties to an array, starting at a particular index.
        /// </summary>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _expando.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of dynamic properties in the entity.
        /// </summary>
        public int Count => _expando.Count;

        /// <summary>
        /// Gets a value indicating whether the entity is read-only.
        /// </summary>
        public bool IsReadOnly => _expando.IsReadOnly;

        /// <summary>
        /// Removes the first occurrence of a specific dynamic property from the entity.
        /// </summary>
        public bool Remove(KeyValuePair<string, object> item)
        {
            return _expando.Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dynamic properties.
        /// </summary>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _expando.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dynamic properties.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
