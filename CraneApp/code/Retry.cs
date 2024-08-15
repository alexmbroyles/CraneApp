using System;
using System.Collections.Generic;
using System.Threading;

namespace CraneApp.code
{
    /// <summary>
    /// This is a very handy function that allows you to retry any method on error.
    /// </summary>
    /// 
    #region Retry Method On Error
    public static class Retry
    {
        public static void Do(Action action, TimeSpan retryInterval, int retryCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount);
        }

        public static T Do<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                    {
                        Thread.Sleep(retryInterval);
                    }
                    //Thread.SpinWait(retryInterval);
                    //Thread.
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
    #endregion 