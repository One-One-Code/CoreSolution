using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.IO;

namespace OneOne.Core.Logger
{
    public class Log
    {
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="messageType">日志类型</param>
        public static void Write(string message, MessageType messageType)
        {
            Write(message, messageType, Type.GetType("System.Object"), null);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="messageType">日志类型</param>
        /// <param name="type">配置类型</param>
        public static void Write(string message, MessageType messageType, Type type)
        {
            Write(message, messageType, type, null);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="messageType">日志类型</param>
        /// <param name="ex">异常</param>
        /// <param name="type">配置类型</param>
        public static void Write(string message, MessageType messageType, Type type, Exception ex)
        {
            ILog log = GetLog(type);
            switch (messageType)
            {
                case MessageType.Debug: if (log.IsDebugEnabled) { log.Debug(message, ex); } break;
                case MessageType.Info: if (log.IsInfoEnabled) { log.Info(message, ex); } break;
                case MessageType.Warn: if (log.IsWarnEnabled) { log.Warn(message, ex); } break;
                case MessageType.Error:
                    if (log.IsErrorEnabled)
                    {
                        log.Error(message, ex);
                    }
                    break;
                case MessageType.Fatal:
                    if (log.IsFatalEnabled)
                    {
                        log.Fatal(message, ex);
                    }
                    break;
            }
        }

        /// <summary>
        /// 断言
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="message">日志信息</param>
        public static void Assert(bool condition, string message)
        {
            Assert(condition, message, Type.GetType("System.Object"));
        }

        /// <summary>
        /// 断言
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="message">日志信息</param>
        /// <param name="type">日志类型</param>
        public static void Assert(bool condition, string message, Type type)
        {
            if (!condition)
            {
                Write(message, MessageType.Info, type, null);
            }
        }

        private static ILog GetLog(Type type)
        {
            ILoggerRepository repository = LogManager.GetRepository("NETCoreRepository");
            //// 默认简单配置，输出至控制台
            //BasicConfigurator.Configure(repository);
            ILog log = LogManager.GetLogger(repository.Name, type);
            return log;
        }

        /// <summary>
        /// 日志的注册
        /// 在程序启动的时候调用该方法进行日志注册
        /// </summary>
        public static void LogRegister(FileInfo configFile)
        {
            ILoggerRepository repository = LogManager.CreateRepository("NETCoreRepository");
            XmlConfigurator.Configure(repository, configFile);
        }
    }

    /// <summary>
    /// 日志类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 调试
        /// </summary>
        Debug,
        /// <summary>
        /// 信息
        /// </summary>
        Info,
        /// <summary>
        /// 警告
        /// </summary>
        Warn,
        /// <summary>
        /// 错误
        /// </summary>
        Error,
        /// <summary>
        /// 致命错误
        /// </summary>
        Fatal
    }
}
