// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerShapedQueryOptimizerFactory : IShapedQueryOptimizerFactory
    {
        private readonly QuerySqlGeneratorDependencies _sqlGeneratorDependencies;
        private readonly ShapedQueryOptimizerDependencies _dependencies;
        private readonly RelationalShapedQueryOptimizerDependencies _relationalDependencies;

        public SqlServerShapedQueryOptimizerFactory(
            QuerySqlGeneratorDependencies sqlGeneratorDependencies,
            ShapedQueryOptimizerDependencies dependencies,
            RelationalShapedQueryOptimizerDependencies relationalDependencies)
        {
            _sqlGeneratorDependencies = sqlGeneratorDependencies;
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        public virtual ShapedQueryOptimizer Create(QueryCompilationContext queryCompilationContext)
            => new SqlServerShapedQueryOptimizer(
                _dependencies,
                _relationalDependencies,
                _sqlGeneratorDependencies,
                queryCompilationContext);
    }
}
