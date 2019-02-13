using System;

namespace Lockstep.Common.Logging
{
    public static class Log
    {                                                                                                                                 
        public static LogSeverity LogSeverityLevel = LogSeverity.Info | LogSeverity.Warn | LogSeverity.Error | LogSeverity.Exception;

        public static event EventHandler<LogEventArgs> OnMessage;

        public static void SetLogAllSeverities()
        {
            LogSeverityLevel = LogSeverity.Trace | LogSeverity.Info | LogSeverity.Warn | LogSeverity.Error | LogSeverity.Exception;
        }

        public static void Warn(object sender, string message, params object[] args)
        {
            LogMessage(sender, LogSeverity.Warn, message, args);
        }

        public static void Info(object sender, string message, params object[] args)
        {
            LogMessage(sender, LogSeverity.Info, message, args);
        }

        public static void Trace(object sender, string message, params object[] args)
        {
            LogMessage(sender, LogSeverity.Trace, message, args);
        }

        private static void LogMessage(object sender, LogSeverity sev, string format, params object[] args)
        {
            if (OnMessage != null && (LogSeverityLevel & sev) != 0)
            {
                var message = (args != null && args.Length > 0) ? string.Format(format, args) : format;
                OnMessage.Invoke(sender, new LogEventArgs(sev, message));
            }
        }
    }

}
