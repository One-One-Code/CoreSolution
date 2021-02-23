using System;
using System.Collections.Generic;
using System.Text;

namespace OneOneCode.Core.Diagnostic
{
    /// <summary>
    /// 诊断事件的观察类
    /// </summary>
    public class DiagnosticEventObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly DiagnosticEventCollection _eventCollection;

        public DiagnosticEventObserver(IDiagnosticProcessor diagnosticProcessor)
        {
            _eventCollection = new DiagnosticEventCollection(diagnosticProcessor);
        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            //throw new NotImplementedException();
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            var diagnosticEvent = _eventCollection.GetDiagnosticEvent(value.Key);
            if (diagnosticEvent == null) return;

            try
            {
                diagnosticEvent.Invoke(value.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
