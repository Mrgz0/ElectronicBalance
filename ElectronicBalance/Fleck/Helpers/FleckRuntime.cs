using System;
using System.Runtime.InteropServices;

namespace Fleck.Helpers
{
    internal static class FleckRuntime
    {
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public static bool IsRunningOnWindows()
        {
            return true;
        }
    }
}