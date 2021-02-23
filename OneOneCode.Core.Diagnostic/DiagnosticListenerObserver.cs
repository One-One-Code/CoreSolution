using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OneOneCode.Core.Diagnostic
{
    /// <summary>
    /// 监听观察类
    /// </summary>
    public class DiagnosticListenerObserver : IObserver<DiagnosticListener>
    {
        private readonly IEnumerable<IDiagnosticProcessor> _diagnosticProcessors;

        public DiagnosticListenerObserver(IEnumerable<IDiagnosticProcessor> diagnosticProcessors)
        {
            _diagnosticProcessors = diagnosticProcessors;
        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            //throw new NotImplementedException();
        }

        public void OnNext(DiagnosticListener value)
        {
            var diagnosticProcessor = _diagnosticProcessors?.FirstOrDefault(_ => _.ListenerName == value.Name);
            if (diagnosticProcessor == null) return;

            value.Subscribe(new DiagnosticEventObserver(diagnosticProcessor));
        }
    }
}
