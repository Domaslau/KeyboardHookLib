using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KeyboardHookLib
{
    public class Hook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        public static IKeyboardOverlay[]? _keyboardOverlays;
        private static IKeyboardOverlay? _activeOverlay;


        /// <summary>
        /// Set a hook to listen for keyboard inputs and execute calback when specific input is detected
        /// </summary>
        /// <returns>Hook pointer that needs to be used when removing the hook</returns>
        public static IntPtr UseHook()
        {
            _hookID = SetHook(_proc);
            return _hookID;
        }

        /// <summary>
        /// Remove the set hook
        /// </summary>
        /// <param name="hookID">pointer received when running UseHook()</param>
        /// <returns>If the function succeeds, the return value is nonzero. 
        /// If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
        public static bool ClearHook(IntPtr hookID)
        {
           return UnhookWindowsHookEx(hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        
        /// <summary>
        /// An application-defined or library-defined callback function used with the SetWindowsHookEx function
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);
        
        /// <summary>
        /// Hook callback function
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //As per microsoft specification if nCode is less than 0 return immediately without processing
            if (nCode < 0) { return CallNextHookEx(_hookID, nCode, wParam, lParam); }

            //read virtual key code to a variable
            int vkCode = Marshal.ReadInt32(lParam);
            if(_activeOverlay != null)
            {  
                   
                if (!_activeOverlay.IsKeySet(vkCode) && vkCode != _activeOverlay.GetTrigger()) { return CallNextHookEx(_hookID, nCode, wParam, lParam); };
                if (_activeOverlay.IsToggle())
                {
                    if (GetKeyState(_activeOverlay.GetTrigger()) == -128) { _activeOverlay = null; return CallNextHookEx(_hookID, nCode, wParam, lParam); }

                    //Depending on keyup or key down run associated action
                    if (wParam == (IntPtr)WM_KEYDOWN)
                    {
                        if (_activeOverlay.RunKeyDown(vkCode))
                        {
                            return _hookID;
                        }
                        else return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }
                    if (wParam == (IntPtr)WM_KEYUP)
                    {
                        if (_activeOverlay.RunKeyUp(vkCode))
                        {
                            return _hookID;
                        }
                        else return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }

                } else
                {
                    if (GetKeyState(_activeOverlay.GetTrigger()) > -1) { _activeOverlay = null; return CallNextHookEx(_hookID, nCode, wParam, lParam); };
                    if(GetKeyState(_activeOverlay.GetTrigger()) == vkCode) { return CallNextHookEx(_hookID, nCode, wParam, lParam); };

                    //Depending on keyup or key down run associated action
                    if (wParam == (IntPtr)WM_KEYDOWN)
                    {
                        if (_activeOverlay.RunKeyDown(vkCode))
                        {
                            return _hookID;
                        }
                        else return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }
                    if (wParam == (IntPtr)WM_KEYUP)
                    {
                        if (_activeOverlay.RunKeyUp(vkCode))
                        {
                            return _hookID;
                        }
                        else return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }
                }

                //Any other key state proceed to next hook
                return CallNextHookEx(_hookID, nCode, wParam, lParam); 

            }
            else
            {
                //If keyboard overlays has an entry associated with virtual key code set that as active overlay.
                if (_keyboardOverlays[vkCode] != null)
                {
                    _activeOverlay = _keyboardOverlays[vkCode];
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
                } else return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
        }


       /**
        * Imports from dlls
        */

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern short GetKeyState(int nVirtKey);
    }
}
