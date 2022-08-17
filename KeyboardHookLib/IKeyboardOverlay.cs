using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardHookLib
{
    public interface IKeyboardOverlay
    {
        bool RunKeyUp(int keyCode);
        bool RunKeyDown(int keyCode);
        int GetTrigger();
        bool GetToggle();
    }
}
