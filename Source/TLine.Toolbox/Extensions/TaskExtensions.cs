using System.Threading.Tasks;

namespace TripLine.Toolbox.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Consumes a task and doesn't do anything with it.  Useful for fire-and-forget calls to async methods within async methods.
        /// 
        /// </summary>
        /// <param name="task">The task whose result is to be ignored.</param>
        public static void Forget(this Task task)
        {
        }

    }
}