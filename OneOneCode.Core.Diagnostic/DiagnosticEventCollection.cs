using Microsoft.Extensions.DiagnosticAdapter;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OneOneCode.Core.Diagnostic
{
    internal class DiagnosticEventCollection
    {
        private readonly Dictionary<string, DiagnosticEvent> _eventDict = new Dictionary<string, DiagnosticEvent>();

        public DiagnosticEventCollection(IDiagnosticProcessor diagnosticProcessor)
        {
            foreach (var method in diagnosticProcessor.GetType().GetMethods())
            {
                var diagnosticName = method.GetCustomAttribute<DiagnosticNameAttribute>();
                if (diagnosticName == null)
                    continue;
                _eventDict.Add(diagnosticName.Name, new DiagnosticEvent(diagnosticProcessor, method));
            }
        }

        public DiagnosticEvent GetDiagnosticEvent(string name)
        {
            if (_eventDict.ContainsKey(name))
            {
                return _eventDict[name];
            }
            return null;
        }
    }
}
