using System;
using System.Collections.Generic;
using System.Text;

namespace OneOneCode.Core.Diagnostic.Resolver
{
    public interface IParameterResolver
    {
        object Resolve(object value);
    }
}
