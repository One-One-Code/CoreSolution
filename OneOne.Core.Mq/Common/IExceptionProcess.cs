using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneOne.Core.Mq.Common
{
    /// <summary>
    /// 达到重试次数后的异常处理接口
    /// </summary>
    public interface IExceptionProcess
    {
        /// <summary>
        /// 业务逻辑方法
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="message">消息对象</param>
        /// <param name="queueUri">队列的uri</param>
        /// <returns>true表示成功</returns>
        bool Process<T>(T message,Uri queueUri);
    }
}
