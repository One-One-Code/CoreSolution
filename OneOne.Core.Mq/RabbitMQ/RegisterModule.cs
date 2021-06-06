using OneOne.Core.IOC.AutoFac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneOne.Core.Mq.RabbitMQ
{
    public class RegisterModule
    {
        public static void Register(IAutoFacRegistration reg)
        {
            reg.Map<IEventPublisher, EventPublisher>();
        }
    }
}
