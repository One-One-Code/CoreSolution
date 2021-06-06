using log4net;
using log4net.Config;
using log4net.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace OneOne.Core.Logger
{
    public class Log
    {
        private static Dictionary<string, bool> IsEntity = new Dictionary<string, bool>();
        private static ILoggerRepository LoggerRepository = null;

        /// <summary>
        /// 使用配置文件进行注册
        /// </summary>
        /// <param name="configFile"></param>
        public static void LogRegister(FileInfo configFile)
        {
            LoggerRepository = LogManager.CreateRepository("NETCoreRepository");
            XmlConfigurator.Configure(LoggerRepository, configFile);
        }

        /// <summary>
        /// 设置日志message使用LogEntity对象的json格式存储
        /// </summary>
        public static void SetEntity()
        {
            IsEntity.Clear();
            foreach (var item in Enum.GetNames(typeof(MessageType)))
            {
                IsEntity.Add(item, true);
            }
        }

        public static void SetEntity(string type)
        {
            if (IsEntity.ContainsKey(type))
            {
                IsEntity[type] = true;
            }
            else
            {
                IsEntity.Add(type, true);
            }
        }

        /// <summary>
        /// 取消设置日志message使用LogEntity对象的json格式存储
        /// </summary>
        public static void UnSetEntity()
        {
            IsEntity.Clear();
            foreach (var item in Enum.GetNames(typeof(MessageType)))
            {
                IsEntity.Add(item, false);
            }
        }

        public static void UnSetEntity(string type)
        {
            if (IsEntity.ContainsKey(type))
            {
                IsEntity[type] = false;
            }
            else
            {
                IsEntity.Add(type, false);
            }
        }

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
            try
            {
                var isEntity = IsEntity[messageType.ToString()];
                ILog log = LogManager.GetLogger(LoggerRepository.Name, type);
                if (isEntity)
                {
                    var entity = new LogEntity() { Exception = ex, Message = message, Type = messageType, ThreadId = Thread.CurrentThread.ManagedThreadId, Time = DateTime.Now };
                    message = JsonConvert.SerializeObject(entity);
                }

                switch (messageType)
                {
                    case MessageType.Debug: if (log.IsDebugEnabled) { log.Debug(message); } break;
                    case MessageType.Info: if (log.IsInfoEnabled) { log.Info(message); } break;
                    case MessageType.Warn: if (log.IsWarnEnabled) { log.Warn(message); } break;
                    case MessageType.Error:
                        if (isEntity)
                        {
                            log.Error(message);
                        }
                        else
                        {
                            log.Error(message, ex);
                        }

                        break;
                    case MessageType.Fatal:
                        if (isEntity)
                        {
                            log.Fatal(message);
                        }
                        else
                        {
                            log.Fatal(message, ex);
                        }
                        break;
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// 自定义日志记录
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void Write(string message, string type = null, Exception ex = null)
        {
            try
            {
                type = type ?? "CustomAction";
                var isEntity = IsEntity.ContainsKey(type) ? IsEntity[type] : IsEntity[MessageType.Info.ToString()];
                var log = LogManager.GetLogger(LoggerRepository.Name, type);
                if (isEntity)
                {
                    var entity = new LogEntity() { Exception = ex, Message = message, Type = MessageType.Custom, ThreadId = Thread.CurrentThread.ManagedThreadId, Time = DateTime.Now };
                    message = JsonConvert.SerializeObject(entity);
                    log.Logger.Log(null, new log4net.Core.Level(50000, type), message, null);
                    return;
                }
                log.Logger.Log(null, new log4net.Core.Level(50000, type), message, ex);
            }
            catch (Exception e)
            {

            }

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
        Fatal,

        /// <summary>
        /// 自定义
        /// </summary>
        Custom
    }

    public class LogEntity
    {
        /// <summary>
        /// 线程ID
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Exception { get; set; }
    }
}
