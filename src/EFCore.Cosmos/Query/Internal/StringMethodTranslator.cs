// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class StringMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _containsMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });

        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        private static readonly MethodInfo _firstOrDefaultMethodInfoWithoutArgs
           = typeof(Enumerable).GetRuntimeMethods().Single(
               m => m.Name == nameof(Enumerable.FirstOrDefault)
               && m.GetParameters().Length == 1).MakeGenericMethod(new[] { typeof(char) });

        private static readonly MethodInfo _lastOrDefaultMethodInfoWithoutArgs
             = typeof(Enumerable).GetRuntimeMethods().Single(
                m => m.Name == nameof(Enumerable.LastOrDefault)
                && m.GetParameters().Length == 1).MakeGenericMethod(new[] { typeof(char) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public StringMethodTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Translate(
            SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            if (_containsMethodInfo.Equals(method))
            {
                return TranslateSystemFunction("CONTAINS", instance, arguments[0], typeof(bool));
            }

            if (_firstOrDefaultMethodInfoWithoutArgs.Equals(method))
            {
                return TranslateSystemFunction("LEFT", arguments[0], _sqlExpressionFactory.Constant(1), typeof(char));
            }


            if (_lastOrDefaultMethodInfoWithoutArgs.Equals(method))
            {
                return TranslateSystemFunction("RIGHT", arguments[0], _sqlExpressionFactory.Constant(1), typeof(char));
            }

            if (_startsWithMethodInfo.Equals(method))
            {
                return TranslateSystemFunction("STARTSWITH", instance, arguments[0], typeof(bool));
            }

            if (_endsWithMethodInfo.Equals(method))
            {
                return TranslateSystemFunction("ENDSWITH", instance, arguments[0], typeof(bool));
            }

            return null;
        }

        private SqlExpression TranslateSystemFunction(string function, SqlExpression instance, SqlExpression pattern, Type returnType)
            => _sqlExpressionFactory.Function(function, new[] { instance, pattern }, returnType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class IndexerUseTranslator : IMethodCallTranslator
    {
        //private static readonly MethodInfo _indexerGetterMethodInfo = 

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IndexerUseTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (method.DeclaringType.GetRuntimeProperty("Item") is { } indexer && /*indexer.IsIndexerProperty() && */indexer.GetIndexParameters().Length == 1 && method == indexer.GetGetMethod() && arguments is { } && arguments[0] is SqlConstantExpression indexerParameter)
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                // TODO: Run conversions to string through custom JSON serializer or convert manually.
                // TODO: Consider running conversions (from SqlConstantExpression) to string through Expression.Convert.
                // TODO: Create and use IndexerAccessExpression/IndexerUseExpression : SqlAccessExpression, IAccessExpression.

                //var indexerMetadata = new Property("__indexer", indexer.PropertyType, indexer, default, new EntityType() /*Not the declared*/, ConfigurationSource.Convention, ConfigurationSource.Convention);
                //indexerMetadata.SetJsonPropertyName(Convert.ToString(indexerParameter.Value));
                
                //return new KeyAccessExpression(indexerMetadata, instance);

                return new KeyAccessExpression(method.ReturnType, instance, Convert.ToString(indexerParameter.Value));
#pragma warning restore EF1001 // Internal EF Core API usage.
            }

            return null;
        }
    }
}
