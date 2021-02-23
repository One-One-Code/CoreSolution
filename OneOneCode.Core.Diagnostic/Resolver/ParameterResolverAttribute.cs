using System;
using System.Collections.Generic;
using System.Text;

namespace OneOneCode.Core.Diagnostic.Resolver
{
    public abstract class ParameterResolverAttribute : Attribute, IParameterResolver
    {
        public abstract object Resolve(object value);
    }
}
