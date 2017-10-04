using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd
{
    /// <summary>
    /// Internal trace helper (wrapper for TraceTool).
    /// </summary>
    public class Trace
    {
        public static void Info(string message)
        {
            Info(message, new object[] { });
        }

        public static void Info(string message, params object[] parameters)
        {
            #if DEBUG
            TraceTool.TTrace.Debug.Send(String.Format("Dewd: " + message, parameters));
            #endif
        }

        public static void Warn(string message)
        {
            Warn(message, new object[] { });
        }

        public static void Warn(string message, params object[] parameters)
        {
            #if DEBUG
            TraceTool.TTrace.Warning.Send(String.Format("Dewd: " + message, parameters));
            #endif
        }

        public static void Error(Exception ex)
        {
            #if DEBUG
            TraceTool.TTrace.Error.Send(String.Format("Dewd: {0}", ex.ToString()));
            #endif
        }
    }
}