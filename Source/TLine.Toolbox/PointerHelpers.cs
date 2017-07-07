using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Toolbox
{
    public static unsafe class PointerHelpers
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        public static void MemCpy(byte* destination, byte* source, int size)
        {
            memcpy((IntPtr)destination, (IntPtr)(source), (UIntPtr)size);
        }

        public static void MemCpy(IntPtr destination, IntPtr source, int size)
        {
            memcpy(destination, source, (UIntPtr)size);
        }
    }
}
