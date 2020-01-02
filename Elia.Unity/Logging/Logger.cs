using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Elia.Unity.Logging
{
    /// <summary>
    /// Basic implementation of <see cref="ILogger"/>.
    /// </summary>
	public class Logger : ILogger
	{
		#region Members

		#region ILogger

        /// <summary>
        /// 
        /// </summary>
		public ILogHandler logHandler { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public bool logEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public LogType filterLogType { get; set; }

		#endregion

		private readonly ILogger[] _loggers;

		#endregion

		#region Constructors

        /// <summary>
        /// Creates <see cref="ILogger"/> instances based on <see cref="LoggerFactory"/> log providers.
        /// </summary>
        /// <param name="factory"><see cref="LoggerFactory"/> instance</param>
		public Logger(LoggerFactory factory)
		{
			var providers = factory.GetProviders();
			_loggers = new ILogger[providers.Length];
			for (var i = 0; i < providers.Length; i++)
			{
				_loggers[i] = providers[i].CreateLogger();
			}
		}

        #endregion

        #region ILogger

        /// <summary>
        /// Logs exception message.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> instance</param>
        /// <param name="context">context of <see cref="Object"/> type</param>
		public void LogException(Exception exception, Object context)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].LogException(exception, context);
			}
		}

        /// <summary>
        /// Returns whether <paramref name="logType"/> is supported at all loggers provided by <see cref="LoggerFactory"/>.
        /// </summary>
        /// <param name="logType"><see cref="LogType"/> instance</param>
        /// <returns>True if <paramref name="logType"/> is supported at all loggers provided by <see cref="LoggerFactory"/></returns>
		public bool IsLogTypeAllowed(LogType logType)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				if (!_loggers[i].IsLogTypeAllowed(logType))
					return false;
			}

			return true;
		}

        /// <summary>
        /// Logs info <paramref name="message"/> of <paramref name="logType"/> type.
        /// </summary>
        /// <param name="logType"><see cref="LogType"/> instance</param>
        /// <param name="message">Message object</param>
		public void Log(LogType logType, object message)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].Log(logType, message);
			}
		}

        /// <summary>
        /// Logs info <paramref name="message"/> of <paramref name="logType"/> with <paramref name="context"/> object.
        /// </summary>
        /// <param name="logType"><see cref="LogType"/> instance</param>
        /// <param name="message">Message object</param>
        /// <param name="context">Context of <see cref="Object"/> type</param>
		public void Log(LogType logType, object message, Object context)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].Log(logType, message, context);
			}
		}

        /// <summary>
        /// Logs info <paramref name="message"/> of <paramref name="logType"/> type with <paramref name="tag"/>.
        /// </summary>
        /// <param name="logType"><see cref="LogType"/> instance</param>
        /// <param name="tag">Tag string</param>
        /// <param name="message">Message object</param>
		public void Log(LogType logType, string tag, object message)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].Log(logType, tag, message);
			}
		}

        /// <summary>
        /// Logs info <paramref name="message"/> of <paramref name="logType"/> type with <paramref name="tag"/> and <paramref name="context"/> object.
        /// </summary>
        /// <param name="logType"><see cref="LogType"/> instance</param>
        /// <param name="tag">Tag string</param>
        /// <param name="message">Message object</param>
        /// <param name="context">Context of <see cref="Object"/> type</param>
		public void Log(LogType logType, string tag, object message, Object context)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].Log(logType, tag, message, context);
			}
		}

        /// <summary>
        /// Logs info <paramref name="message"/> object.
        /// </summary>
        /// <param name="message">Message object</param>
		public void Log(object message)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].Log(message);
			}
		}

        /// <summary>
        /// Logs info <paramref name="message"/> object with <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">Tag string</param>
        /// <param name="message">Message object</param>
		public void Log(string tag, object message)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].Log(tag, message);
			}
		}

        /// <summary>
        /// Logs info <paramref name="message"/> object with <paramref name="tag"/> and <paramref name="context"/>.
        /// </summary>
        /// <param name="tag">Tag string</param>
        /// <param name="message">Message object</param>
        /// <param name="context">Context of <see cref="Object"/> type</param>
		public void Log(string tag, object message, Object context)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].Log(tag, message, context);
			}
		}

        /// <summary>
        /// Logs warning <paramref name="message"/> object with <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">Tag string</param>
        /// <param name="message">Message object</param>
		public void LogWarning(string tag, object message)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].LogWarning(tag, message);
			}
		}

        /// <summary>
        /// Logs warning <paramref name="message"/> object with <paramref name="tag"/> and <paramref name="context"/>.
        /// </summary>
        /// <param name="tag">Tag string</param>
        /// <param name="message">Message object</param>
        /// <param name="context">Context of <see cref="Object"/> type</param>
		public void LogWarning(string tag, object message, Object context)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].LogWarning(tag, message, context);
			}
		}

        /// <summary>
        /// Logs error <paramref name="message"/> object with <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">Tag string</param>
        /// <param name="message">Message object</param>
		public void LogError(string tag, object message)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].LogError(tag, message);
			}
		}

        /// <summary>
        /// Logs error <paramref name="message"/> object with <paramref name="tag"/> and <paramref name="context"/>.
        /// </summary>
        /// <param name="tag">Tag string</param>
        /// <param name="message">Message object</param>
        /// <param name="context">Context of <see cref="Object"/> type</param>
		public void LogError(string tag, object message, Object context)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].LogError(tag, message, context);
			}
		}

        /// <summary>
        /// Logs formatted info message.
        /// </summary>
        /// <param name="logType"><see cref="LogType"/> instance</param>
        /// <param name="context">context of <see cref="Object"/> type</param>
        /// <param name="format">Message format</param>
        /// <param name="args">Message arguments</param>
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            for (var i = 0; i < _loggers.Length; i++)
            {
                _loggers[i].LogFormat(logType, context, format, args);
            }
        }

        /// <summary>
        /// Logs formatted info message.
        /// </summary>
        /// <param name="logType"><see cref="LogType"/> instance</param>
        /// <param name="format">Message format</param>
        /// <param name="args">Message arguments</param>
		public void LogFormat(LogType logType, string format, params object[] args)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].LogFormat(logType, format, args);
			}
		}

        /// <summary>
        /// Logs exception message.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> instance</param>
		public void LogException(Exception exception)
		{
			for (var i = 0; i < _loggers.Length; i++)
			{
				_loggers[i].LogException(exception);
			}
		}

		#endregion
	}
}
