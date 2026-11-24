using System;
using System.Threading.Tasks;

namespace IGOR.Common.Extensions
{
    /// <summary>
    /// Several extensions for Tasks
    /// </summary>
    public static class TaskExtensions
    {
        
        /// <summary>
        /// Awaits an Async Task
        /// </summary>
        /// <param name="errorCallback">Callback for Exceptions</param>
        /// <param name="completedCallback">Callback for Completed Task</param>
        public async static void Await(this Task task, Action<Exception> errorCallback = null, Action completedCallback = null)
        {
            try
            {
                await task;
                completedCallback?.Invoke();
            }
            catch (Exception ex)
            {
                errorCallback?.Invoke(ex);
            }
        }
    }
}
