using System;
using System.Runtime.InteropServices;

namespace ArkaControls.Drawing
{
    internal static class NativeMethods
    {
        public const int EM_SETCUEBANNER = 0x1501;
        public const int WM_PAINT = 0x000F;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SendMessageW")]
        private static extern IntPtr SendMessageString(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;

        /// <summary>Sets the grey hint text of an edit control. No-op (returns false) off Windows or on failure.</summary>
        public static bool SetCueBanner(IntPtr editHandle, string text, bool showWhenFocused)
        {
            if (!IsWindows || editHandle == IntPtr.Zero) return false;
            try
            {
                SendMessageString(editHandle, EM_SETCUEBANNER, showWhenFocused ? (IntPtr)1 : IntPtr.Zero, text ?? string.Empty);
                return true;
            }
            catch (Exception) { return false; }
        }
    }
}
