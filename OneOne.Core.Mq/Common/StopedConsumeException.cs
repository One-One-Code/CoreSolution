using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneOne.Core.Mq.Common
{
    /// <summary>
    /// 已停止消费异常
    /// </summary>
    public class StopedConsumeException : ApplicationException
    {
        public StopedConsumeException()
        {

        }

        public StopedConsumeException(string message)
            : base(message)
        {

        }
        public StopedConsumeException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
