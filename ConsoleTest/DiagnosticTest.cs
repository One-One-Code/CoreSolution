using CommonServiceLocator;
using Microsoft.Extensions.DiagnosticAdapter;
using OneOne.Core.IOC.AutoFac;
using OneOneCode.Core.Diagnostic;
using OneOneCode.Core.Diagnostic.Resolver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ConsoleTest
{
    public static class DiagnosticTest
    {
        public static void Publish()
        {
            var locator = new AutoFacServiceLocator();
            locator.Map<IDiagnosticProcessor, StoreDiagnosticSubscription>(null, true);
            locator.Map<IObserver<DiagnosticListener>, DiagnosticListenerObserver>(null, true);
            locator.UseAsDefault();
            var observer = ServiceLocator.Current.GetInstance<IObserver<DiagnosticListener>>();
            DiagnosticListener.AllListeners.Subscribe(observer);
            var listener = new DiagnosticListener(EventName.ListenerName);
            if (listener.IsEnabled(EventName.AfterEventName))
            {
                listener.Write(EventName.AfterEventName, new EventData { Name="huang" });
            }
        }
    }

    public class StoreDiagnosticSubscription : IDiagnosticProcessor
    {
        public string ListenerName { get; } = EventName.ListenerName;

        [DiagnosticName(EventName.BeforeEventName)]
        public void BeforePublishMessageStore([Object] EventData name)
        {
            Console.WriteLine($"BeforePublishMessageStore:{name.Name}");
        }

        [DiagnosticName(EventName.AfterEventName)]
        public void AfterPublishMessageStore([Object] EventData name)
        {
            Console.WriteLine($"AfterPublishMessageStore:{name.Name}");
        }
    }

    public static class EventName
    {
        public const string ListenerName = "Test_Listener";
        public const string BeforeEventName = "Test_BeforeEventName";
        public const string AfterEventName = "Test_AfterEventName";
        public const string ErroreEventName = "Test_ErrorEventName";
    }

    public class EventData
    {
        public string Name { get; set; }
    }
}
