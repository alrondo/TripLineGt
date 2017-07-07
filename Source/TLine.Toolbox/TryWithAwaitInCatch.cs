using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Toolbox
{
    //
    // Source: http://stackoverflow.com/questions/16626161/a-good-solution-for-await-in-try-catch-finally
    //
    public static class TryWithAwaitInCatch
    {        
        public static async Task ExecuteAndHandleErrorAsync(Func<Task> actionAsync, Func<Exception, Task<bool>> errorHandlerAsync)
        {
            ExceptionDispatchInfo capturedException = null;
            try
            {
                await actionAsync();
            }
            catch (Exception ex)
            {
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }

            if (capturedException != null)
            {
                bool needsThrow = await errorHandlerAsync(capturedException.SourceException);
                if (needsThrow)
                {
                    capturedException.Throw();
                }
            }
        }
    }
}
