/**************************************************************************
 *                                                                        *
 *  Description: Actress is a C# port of the F# MailboxProcessor          *
 *  Website:     https://github.com/kthompson/Actress                     *
 *  Copyright:   (c) 2017, Kevin Thompson                                 *
 *                                                                        *
 **************************************************************************/

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ActressMas
{
    internal static class WaitHandleEx
    {
        public static Task<bool> ToTask(this WaitHandle waitHandle, TimeSpan? maxValue = null)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Registering callback to wait till WaitHandle changes its state
            WaitOrTimerCallback callBack = (o, timeout) =>
            {
                tcs.SetResult(!timeout);
            };

            ThreadPool.RegisterWaitForSingleObject(waitHandle, callBack, null,
                maxValue ?? TimeSpan.MaxValue, true);

            return tcs.Task;
        }
    }
}