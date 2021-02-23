using AspectCore.Extensions.Reflection;
using OneOneCode.Core.Diagnostic.Resolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OneOneCode.Core.Diagnostic
{
    internal class DiagnosticEvent
    {
        private readonly IDiagnosticProcessor _diagnosticProcessor;
        private readonly IParameterResolver[] _parameterResolvers;
        private readonly MethodReflector _reflector;

        public DiagnosticEvent(IDiagnosticProcessor diagnosticProcessor, MethodInfo method)
        {
            _diagnosticProcessor = diagnosticProcessor;
            _reflector = method.GetReflector();
            _parameterResolvers = GetParameterResolvers(method).ToArray();
        }

        public void Invoke(object value)
        {
            var args = new object[_parameterResolvers.Length];
            for (var i = 0; i < _parameterResolvers.Length; i++)
            {
                args[i] = _parameterResolvers[i].Resolve(value);
            }

            _reflector.Invoke(_diagnosticProcessor, args);
        }

        private static IEnumerable<IParameterResolver> GetParameterResolvers(MethodInfo methodInfo)
        {
            foreach (var parameter in methodInfo.GetParameters())
            {
                var binder = parameter.GetCustomAttribute<ParameterResolverAttribute>();
                if (binder != null)
                {
                    if (binder is ObjectAttribute objectBinder)
                    {
                        if (objectBinder.TargetType == null)
                        {
                            objectBinder.TargetType = parameter.ParameterType;
                        }
                    }
                  
                    yield return binder;
                }
                else
                {
                    yield return new NullParameterResolver();
                }
            }
        }
    }
}
