// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosTypeMappingSource : TypeMappingSource
    {
        private readonly Dictionary<Type, CosmosTypeMapping> _clrTypeMappings;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosTypeMappingSource([NotNull] TypeMappingSourceDependencies dependencies)
            : base(dependencies)
        {
            _clrTypeMappings
                = new Dictionary<Type, CosmosTypeMapping>
                {
                    { typeof(JObject), new CosmosTypeMapping(typeof(JObject)) },
                };

            // Adds 360 entries containing maps for every major collection type and interface of every primitive, and every major dictionary type and interface of every combination of two primitives.

            AddValueTMSetsExhaustive<bool>();
            AddValueTMSetsExhaustive<byte>();
            AddValueTMSetsExhaustive<sbyte>();
            AddValueTMSetsExhaustive<short>();
            AddValueTMSetsExhaustive<int>();
            AddValueTMSetsExhaustive<long>();
            AddValueTMSetsExhaustive<float>();
            AddValueTMSetsExhaustive<double>();
            AddValueTMSetsExhaustive<decimal>();
            AddValueTMSetsExhaustive<DateTime>();
            AddTMSetsExhaustive<string>();
            AddValueTMSetsExhaustive<Guid>();

            void AddValueTMSetsExhaustive<TElement>() where TElement : struct
            {
                AddTMSetsExhaustive<TElement>();
                AddTMSetsExhaustive<TElement?>(addPairedTMs: false);
            }

            void AddTMSetsExhaustive<TElement>(bool addPairedTMs = true)
            {
                AddTMs<TElement>();

                if (addPairedTMs)
                {
                    AddValuePairedTMs<TElement, bool>();
                    AddValuePairedTMs<TElement, byte>();
                    AddValuePairedTMs<TElement, sbyte>();
                    AddValuePairedTMs<TElement, short>();
                    AddValuePairedTMs<TElement, int>();
                    AddValuePairedTMs<TElement, long>();
                    AddValuePairedTMs<TElement, float>();
                    AddValuePairedTMs<TElement, double>();
                    AddValuePairedTMs<TElement, decimal>();
                    AddValuePairedTMs<TElement, DateTime>();
                    AddPairedTMs<TElement, string>();
                    AddValuePairedTMs<TElement, Guid>();
                }

                void AddValuePairedTMs<TElementA, TElementB>() where TElementB : struct
                {
                    AddPairedTMs<TElementA, TElementB>();
                    AddPairedTMs<TElementA, TElementB?>();
                }
            }

            void AddTMs<TElement>()
            {
                AddBuiltTM<TElement[]>(elements => elements == null ? null : elements == null ? null : elements.ToArray());

                AddBuiltTM<HashSet<TElement>>(elements => elements == null ? null : elements.ToHashSet());
                AddBuiltTM<List<TElement>>(elements => elements == null ? null : elements.ToList());

                AddBuiltTM<ISet<TElement>>(elements => elements == null ? null : elements.ToHashSet());
                AddBuiltTM<IList<TElement>>(elements => elements == null ? null : elements.ToList());
                AddBuiltTM<ICollection<TElement>>(elements => elements == null ? null : elements.ToArray());

                void AddBuiltTM<TCollection>(Expression<Func<TCollection, TCollection>> snapshotCapturer) where TCollection : ICollection<TElement> => _clrTypeMappings.Add(typeof(TCollection), new CosmosTypeMapping(typeof(TCollection), keyComparer: new GeneralStructuralComparer<TCollection>(snapshotCapturer)));
            }

            void AddPairedTMs<TElementA, TElementB>()
            {
                // TODO: Add lookups. Will require change to generic constraints.

                AddBuiltTM<Dictionary<TElementA, TElementB>>(dictionary => dictionary == null ? null : dictionary.ToDictionary(element => element.Key, element => element.Value));
                AddBuiltTM<IDictionary<TElementA, TElementB>>(dictionary => dictionary == null ? null : dictionary.ToDictionary(element => element.Key, element => element.Value));

                void AddBuiltTM<TDictionary>(Expression<Func<TDictionary, TDictionary>> snapshotCapturer) where TDictionary : IDictionary<TElementA, TElementB> => _clrTypeMappings.Add(typeof(TDictionary), new CosmosTypeMapping(typeof(TDictionary), keyComparer: new GeneralStructuralComparer<TDictionary>(snapshotCapturer)));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            Check.DebugAssert(clrType != null, "ClrType is null");

            if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
            {
                return mapping;
            }

            if ((clrType.IsValueType
                 && !clrType.IsEnum)
                || clrType == typeof(string))
            {
                return new CosmosTypeMapping(clrType);
            }

            return base.FindMapping(mappingInfo);
        }
    }
}
