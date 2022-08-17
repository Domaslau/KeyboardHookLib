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
        public static bool _preventDefault = true;


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
        /// Helper function to find an overlay.
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        private static IKeyboardOverlay? GetOverlay(int keyCode)
        {
            if( == null)
            {
                return null;
            }
            else { return _keyboardOverlays[keyCode]; }
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
        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) { return CallNextHookEx(_hookID, nCode, wParam, lParam); };
            int vkCode = Marshal.ReadInt32(lParam);

            //Active overlay can only be set when key is down.
            if (wParam == (IntPtr)WM_KEYDOWN)
            {
                //if activeOverlay is not set and keyboarOverlays has an entry for the key set that to active overlay.
                if(_activeOverlay == null && _keyboardOverlays[vkCode]!=null)
                {
                    _activeOverlay = _keyboardOverlays[vkCode];
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
                }
                //If overlay is not null
                else
                {
                   //Check if overlay should be toggled if it's toggle it will run anything available from active overlay
                    if (_activeOverlay.GetToggle())
                    {
                        if (_activeOverlay.RunKeyDown(vkCode))
                        {
                            return _hookID;
                        }
                    }
                    //Else only run if GetKeyState returns negative value.
                    else
                    {
                        if(GetKeyState(_activeOverlay.GetTrigger()) < 0)
                        {
                            if (_activeOverlay.RunKeyDown(vkCode)) { 
                                return _hookID;
                            }

                        }
                    }
                }
            }

            //Active overlay should be cleared only on key up event.
            if (wParam == (IntPtr)WM_KEYUP)
            {
                if (_activeOverlay == null) { return CallNextHookEx(_hookID, vkCode, wParam, lParam); }
                //if current key up event trigger is the trigger key for active overlay
                if (_activeOverlay.GetTrigger() == vkCode)
                {
                    if (_activeOverlay.GetToggle())
                    {
                        //Only clear if key state is in 0
                        if(GetKeyState(_activeOverlay.GetTrigger()) == -128 || GetKeyState(_activeOverlay.GetTrigger()) == 0) {
                            _activeOverlay = null;
                        }
                       
                    } 
                    //Clear if trigger needs to be held
                    else
                    {
                        _activeOverlay = null;
                    }
                } 
                //else it will run overlay key up 
                else
                {
                    //if it's toggle run always
                    if (_activeOverlay.GetToggle())
                    {
                        if (_activeOverlay.RunKeyUp(vkCode))
                        {
                            return _hookID;
                        }
                    }
                    //else only run if GetKeyState returns negative value.
                    else
                    {
                        if (GetKeyState(_activeOverlay.GetTrigger()) < 0)
                        {
                            if (_activeOverlay.RunKeyUp(vkCode))
                            {
                                return _hookID;
                            }
                        }
                    }
                }
            };

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
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
