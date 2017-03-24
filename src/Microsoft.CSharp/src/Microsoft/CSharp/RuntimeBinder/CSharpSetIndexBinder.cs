// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder.Semantics;

namespace Microsoft.CSharp.RuntimeBinder
{
    /// <summary>
    /// Represents a dynamic indexer access in C#, providing the binding semantics and the details about the operation. 
    /// Instances of this class are generated by the C# compiler.
    /// </summary>
    internal sealed class CSharpSetIndexBinder : SetIndexBinder, ICSharpBinder
    {
        public BindingFlag BindingFlags => 0;

        public EXPR DispatchPayload(RuntimeBinder runtimeBinder, ArgumentObject[] arguments, Dictionary<int, LocalVariableSymbol> dictionary)
            => runtimeBinder.BindAssignment(this, arguments, dictionary);

        public void PopulateSymbolTableWithName(SymbolTable symbolTable, Type callingType, ArgumentObject[] arguments)
            => symbolTable.PopulateSymbolTableWithName(SpecialNames.Indexer, null, arguments[0].Type);

        public bool IsBinderThatCanHaveRefReceiver => true;

        internal bool IsCompoundAssignment { get; }

        public bool IsChecked { get; }

        public Type CallingContext { get; }

        private readonly List<CSharpArgumentInfo> _argumentInfo;

        CSharpArgumentInfo ICSharpBinder.GetArgumentInfo(int index) => _argumentInfo[index];

        private readonly RuntimeBinder _binder;

        //////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpSetIndexBinder" />.
        /// </summary>
        /// <param name="isCompoundAssignment">True if the assignment comes from a compound assignment in source.</param>
        /// <param name="callingContext">The <see cref="System.Type"/> that indicates where this operation is defined.</param>
        /// <param name="argumentInfo">The sequence of <see cref="CSharpArgumentInfo"/> instances for the arguments to this operation.</param>
        public CSharpSetIndexBinder(
            bool isCompoundAssignment,
            bool isChecked,
            Type callingContext,
            IEnumerable<CSharpArgumentInfo> argumentInfo) :
            base(BinderHelper.CreateCallInfo(argumentInfo, 2)) // discard 2 arguments: the target object and the value
        {
            IsCompoundAssignment = isCompoundAssignment;
            IsChecked = isChecked;
            CallingContext = callingContext;
            _argumentInfo = BinderHelper.ToList(argumentInfo);
            _binder = RuntimeBinder.GetInstance();
        }

        /// <summary>
        /// Performs the binding of the dynamic set index operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic set index operation.</param>
        /// <param name="indexes">The arguments of the dynamic set index operation.</param>
        /// <param name="value">The value to set to the collection.</param>
        /// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
#if ENABLECOMBINDER
            DynamicMetaObject com;
            if (!BinderHelper.IsWindowsRuntimeObject(target) && ComBinder.TryBindSetIndex(this, target, indexes, value, out com))
            {
                return com;
            }
#endif
            return BinderHelper.Bind(this, _binder, BinderHelper.Cons(target, indexes, value), _argumentInfo, errorSuggestion);
        }
    }
}
