using System.Runtime.InteropServices;

namespace SimpleSoundtrackManager.MVVM.Model.Utils
{
    /// <summary>
    /// <c>Class</c> Used to access SystemContextMenu.
    /// </summary>
    internal static partial class NativeUtils
    {
        internal static uint TPM_LEFTALIGN;

        internal static uint TPM_RETURNCMD;

        /// <summary>
        /// <c>Constructor</c> Constructs an instance of the NativeUtils class.
        /// </summary>
        static NativeUtils()
        {
            TPM_LEFTALIGN = 0;
            TPM_RETURNCMD = 256;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll")]
        public static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);
    }
}
