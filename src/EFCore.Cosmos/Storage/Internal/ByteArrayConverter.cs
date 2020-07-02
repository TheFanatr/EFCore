// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Utilities;
using Newtonsoft.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CollectionConverter<TElement, TCollection> : JsonConverter where TCollection : ICollection<TElement>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public delegate bool Extractor(JsonToken token, object value, out TElement data);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private Func<List<TElement>, TCollection> Materializer { get; } = list => (TCollection)(ICollection<TElement>)list;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private Func<TElement, object> Renderer { get; } = element => element;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private Extractor Translator { get; } = (JsonToken token, object value, out TElement data) => token switch
        {
            JsonToken.StartObject => (data = default, Result: false).Result,
            JsonToken.StartArray => (data = default, Result: false).Result,
            _ => (data = (TElement)value, Result: true).Result
        };

        // Parameter materializer should only be null if TDictionary is a base type of Dictionary.

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CollectionConverter(Func<List<TElement>, TCollection> materializer = default, Func<TElement, object> renderer = default, Extractor translator = default)
        {
            Translator = translator ?? Translator;
            Renderer = renderer ?? Renderer;
            Materializer = materializer ?? Materializer;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            var data = (TCollection)value;

            writer.WriteStartArray();

            foreach (var element in data)
            {
                writer.WriteValue(Renderer?.Invoke(element) ?? element);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (Translator is null)
            {
                throw new Exception { };
            }

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new Exception(reader.TokenType.ToString());
            }

            var data = new List<TElement> { };

            while (reader.Read())
            {
                if (Translator.Invoke(reader.TokenType, reader.Value, out var value))
                {
                    data.Add(value);
                }
                else
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.EndArray:
                            return Materializer.Invoke(data);
                        case JsonToken.Comment:
                            break;
                        default:
                            throw new Exception(reader.TokenType.ToString());
                    }
                }
            }

            throw new Exception { };
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool CanConvert(Type objectType) => objectType == typeof(TCollection);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public delegate bool JsonDataExtractor<TElement>(JsonToken token, object value, out TElement data);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DictionaryConverter<TElementA, TElementB, TDictionary> : JsonConverter where TDictionary : IDictionary<TElementA, TElementB>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private Func<Dictionary<TElementA, TElementB>, TDictionary> Materializer { get; } = dictionary => (TDictionary)(IDictionary<TElementA, TElementB>)dictionary;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private JsonDataExtractor<TElementA> LabelTranslator { get; } = (JsonToken token, object value, out TElementA data) => token switch
        {
            JsonToken.StartObject => (data = default, Result: false).Result,
            JsonToken.StartArray => (data = default, Result: false).Result,
            _ => (data = (TElementA)value, Result: true).Result
        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private JsonDataExtractor<TElementB> ValueTranslator { get; } = (JsonToken token, object value, out TElementB data) => token switch
        {
            JsonToken.StartObject => (data = default, Result: false).Result,
            JsonToken.StartArray => (data = default, Result: false).Result,
            _ => (data = (TElementB)value, Result: true).Result
        };

        private Func<TElementA, string> LabelRenderer { get; } = element => element.ToString();

        private Func<TElementB, object> ValueRenderer { get; } = element => element;

        // Parameter materializer should only be null if TDictionary is a base type of Dictionary.

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DictionaryConverter(Func<Dictionary<TElementA, TElementB>, TDictionary> materializer, Func<TElementA, string> labelRenderer = default, Func<TElementB, object> valueRenderer = default, JsonDataExtractor<TElementA> labelTranslator = default, JsonDataExtractor<TElementB> valueTranslator = default)
        {
            LabelTranslator = labelTranslator ?? LabelTranslator;
            ValueTranslator = valueTranslator ?? ValueTranslator;

            LabelRenderer = labelRenderer ?? LabelRenderer;
            ValueRenderer = valueRenderer ?? ValueRenderer;

            Materializer = materializer ?? Materializer;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (LabelRenderer is null)
            {
                throw new InvalidOperationException("The provided DictionaryConverter LabelRenderer is null. Cannot convert arbitrary key element TElementA to string.");
            }

            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            var data = (TDictionary)value;

            writer.WriteStartObject();

            foreach (var element in data)
            {
                writer.WritePropertyName(LabelRenderer.Invoke(element.Key));
                writer.WriteValue(ValueRenderer?.Invoke(element.Value) ?? element.Value);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (LabelTranslator is null || ValueTranslator is null)
            {
                throw new Exception { };
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new Exception(reader.TokenType.ToString());
            }

            var data = new Dictionary<TElementA, TElementB> { };
            var labelSet = false;

            TElementA currentLabel = default;

            while (reader.Read())
            {
                if (reader.TokenType is JsonToken.PropertyName && LabelTranslator.Invoke(reader.TokenType, reader.Value, out var label))
                {
                    currentLabel = label;
                    labelSet = true;
                }
                else if (labelSet && ValueTranslator.Invoke(reader.TokenType, reader.Value, out var value))
                {
                    data.Add(currentLabel, value);
                    currentLabel = default;
                    labelSet = false;
                }
                else
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.EndObject:
                            return Materializer.Invoke(data);
                        case JsonToken.Comment:
                            break;
                        default:
                            throw new Exception(reader.TokenType.ToString());
                    }
                }
            }

            throw new Exception { };
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool CanConvert(Type objectType) => objectType == typeof(TDictionary);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ByteArrayConverter : JsonConverter
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var data = (byte[])value;

            writer.WriteStartArray();

            for (var i = 0; i < data.Length; i++)
            {
                writer.WriteValue(data[i]);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new Exception(reader.TokenType.ToString());
            }

            var byteList = new List<byte>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Integer:
                        byteList.Add(Convert.ToByte(reader.Value));
                        break;
                    case JsonToken.EndArray:
                        return byteList.ToArray();
                    case JsonToken.Comment:
                        break;
                    default:
                        throw new Exception(reader.TokenType.ToString());
                }
            }

            throw new Exception();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool CanConvert(Type objectType)
            => objectType == typeof(byte[]);
    }
}
