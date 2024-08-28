using PNGTuberManager.APIs;
using PNGTuberManager.EventArgs;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PNGTuberManager.Service
{
    internal static class LowLevelMouseTouchHook
    {
        public static EventHandler<MouseArgs> OnMouseInput;
        public static EventHandler<TouchPenArgs> OnTouchInput;

        private static IntPtr _hookID = IntPtr.Zero;
        private static Win32.LowLevelMouseProc _proc = HookCallback;

        private const int WH_MOUSE_LL = 14;

        public static void SetHook()
        {
            _hookID = SetHook(_proc);
        }
        public static void Unhook()
        {
            Win32.UnhookWindowsHookEx(_hookID);
        }

        /// <summary>
        /// The callback function for the hook.
        /// </summary>
        /// <param name="nCode">The hook code.</param>
        /// <param name="wParam">The wParam parameter.</param>
        /// <param name="lParam">The lParam parameter.</param>
        /// <returns>The result of the next hook procedure.</returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            return Task.Run(async () => await ProcessHookAsync(nCode, wParam, lParam)).Result;
        }

        private static async Task<IntPtr> ProcessHookAsync(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Win32Structs.MSLLHOOKSTRUCT hookStruct = (Win32Structs.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32Structs.MSLLHOOKSTRUCT));

            if (hookStruct.dwExtraInfo == IntPtr.Zero)
            {
                //Console.WriteLine($"Mouse event detected at ({hookStruct.pt.x}, {hookStruct.pt.y})");
                OnMouseInput?.Invoke(null, new MouseArgs { X = hookStruct.pt.x, Y = hookStruct.pt.y });
            }
            if (hookStruct.dwExtraInfo != IntPtr.Zero)
            {
                //Console.WriteLine($"Pen or Touch recognized at ({hookStruct.pt.x}, {hookStruct.pt.y})");
                OnTouchInput?.Invoke(null, new TouchPenArgs { X = hookStruct.pt.x, Y = hookStruct.pt.y });
            }
            return Win32.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Sets the hook for low-level mouse events.
        /// </summary>
        /// <param name="proc">The callback function for the hook.</param>
        /// <returns>The hook ID.</returns>
        private static IntPtr SetHook(Win32.LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return Win32.SetWindowsHookEx(WH_MOUSE_LL, proc, Win32.GetModuleHandle(curModule.ModuleName), 0);
            }
        }
    }
}
