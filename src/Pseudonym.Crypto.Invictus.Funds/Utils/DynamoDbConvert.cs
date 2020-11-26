using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.DynamoDBv2.Model;

namespace Pseudonym.Crypto.Invictus.Funds.Utils
{
    public static class DynamoDbConvert
    {
        private static readonly IReadOnlyList<Type> NumberTypes = new List<Type>()
        {
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal)
        };

        public static Dictionary<string, AttributeValue> Serialize<T>(T item)
            where T : class
        {
            return SerializeObject(typeof(T), item);
        }

        private static Dictionary<string, AttributeValue> SerializeObject(Type type, object item)
        {
            if (item == null)
            {
                return null;
            }

            var attributes = new Dictionary<string, AttributeValue>();

            foreach (var property in type.GetProperties())
            {
                attributes.Add(
                    property.Name,
                    SerializeItem(
                        property.PropertyType,
                        property.GetMethod.Invoke(item, null)));
            }

            return attributes;
        }

        private static AttributeValue SerializeItem(Type type, object value)
        {
            if (value == null)
            {
                return new AttributeValue()
                {
                    NULL = true
                };
            }
            else if (value is string s)
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    return new AttributeValue(s);
                }
                else
                {
                    return new AttributeValue()
                    {
                        NULL = true
                    };
                }
            }
            else if (value is Uri uri)
            {
                return new AttributeValue(uri.OriginalString);
            }
            else if (type.IsValueType)
            {
                return SerializeValue(type, value);
            }
            else if (type.IsArray ||
                (type.IsGenericType &&
                 typeof(IEnumerable).IsAssignableFrom(type)))
            {
                var arrayType = type.IsArray
                    ? type.MakeArrayType()
                    : type.GetGenericArguments().Single();

                return SerializeCollection(arrayType, value as IEnumerable);
            }
            else
            {
                return new AttributeValue()
                {
                    IsMSet = true,
                    M = SerializeObject(type, value)
                };
            }
        }

        private static AttributeValue SerializeCollection(Type type, IEnumerable collection)
        {
            if (collection == null)
            {
                return new AttributeValue()
                {
                    NULL = true
                };
            }

            if (type == typeof(string))
            {
                return new AttributeValue(
                    collection
                        .Cast<string>()
                        .ToList());
            }
            else if (type == typeof(byte))
            {
                return new AttributeValue()
                {
                    B = new MemoryStream(collection.Cast<byte>().ToArray())
                };
            }
            else if (typeof(IEnumerable<byte>).IsAssignableFrom(type))
            {
                return new AttributeValue()
                {
                    BS = collection
                        .Cast<IEnumerable<byte>>()
                        .Select(bs => new MemoryStream(bs.ToArray()))
                        .ToList()
                };
            }
            else if (NumberTypes.Contains(type))
            {
                var numberList = new List<string>();

                foreach (var child in collection)
                {
                    numberList.Add(child.ToString());
                }

                return new AttributeValue()
                {
                    NS = numberList
                };
            }
            else
            {
                var list = new List<AttributeValue>();

                foreach (var child in collection)
                {
                    list.Add(SerializeItem(type, child));
                }

                return new AttributeValue()
                {
                    IsLSet = true,
                    L = list
                };
            }
        }

        private static AttributeValue SerializeValue(Type type, object value)
        {
            if (value == null)
            {
                return new AttributeValue()
                {
                    NULL = true
                };
            }

            if (value is bool b)
            {
                return new AttributeValue()
                {
                    IsBOOLSet = true,
                    BOOL = b,
                };
            }
            else if (value is DateTime d)
            {
                return new AttributeValue(d.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"));
            }
            else if (NumberTypes.Contains(type))
            {
                return new AttributeValue()
                {
                    N = value.ToString()
                };
            }
            else
            {
                var stringValue = value.ToString();
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    return new AttributeValue(stringValue);
                }
                else
                {
                    return new AttributeValue()
                    {
                        NULL = true
                    };
                }
            }
        }
    }
}
