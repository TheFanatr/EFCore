// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Specifies value snapshotting and comparison for arrays where each element is compared
    ///         a new array is constructed when snapshotting.
    ///     </para>
    /// </summary>
    /// <typeparam name="TElement"> The array element type. </typeparam>
    public class ArrayStructuralComparer<TElement> : ValueComparer<TElement[]>
    {
        /// <summary>
        ///     Creates a comparer instance.
        /// </summary>
        public ArrayStructuralComparer()
            : base(
                CreateDefaultEqualsExpression(),
                CreateDefaultHashCodeExpression(favorStructuralComparisons: true),
                v => v == null ? null : v.ToArray())
        {
        }
    }

    /// <summary>
    ///     <para>
    ///         Specifies value snapshotting and comparison for arrays where each element is compared
    ///         a new array is constructed when snapshotting.
    ///     </para>
    /// </summary>
    /// <typeparam name="TValue"> The value that will be compared. </typeparam>
    public class GeneralStructuralComparer<TValue> : ValueComparer<TValue>
    {
        /// <summary>
        ///     Creates a comparer instance.
        /// </summary>
        public GeneralStructuralComparer(Expression<Func<TValue, TValue>> snapshotExpression)
            : base(
                CreateDefaultEqualsExpression(),
                CreateDefaultHashCodeExpression(favorStructuralComparisons: true),
                snapshotExpression)
        {
        }
    }
}
