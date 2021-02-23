using System;
using System.Collections.Generic;
using System.Text;

namespace OneOneCode.Core.Diagnostic.Resolver
{
    public class NullParameterResolver : IParameterResolver
    {
        public object Resolve(object value)
        {
            return null;
        }
    }
}
