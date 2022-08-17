using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardHookLib
{
    /// <summary>
    /// Represents a Key what should happen on key down and key up events and what physical key it is mapped to.
    /// </summary>
    public class Key
    {
        private Action keyDown;
        private Action keyUp;
        private int keyCode;
        private bool preventDefault;

        public Action KeyDown {
                get { return keyDown; }
                set { keyDown = value; } 
        }
        public Action KeyUp { 
            get { return keyUp; } 
            set { keyUp = value; } 
        }
        public int KeyCode { 
            get { return keyCode; } 
            set { keyCode = value; } 
        }

        public bool PreventDefault
        {
            get { return preventDefault; }
            set { preventDefault = value; }
        }

        /// <summary>
        /// Constructor for Key object
        /// </summary>
        /// <param name="keyCode">What key is the action mapped to</param>
        /// <param name="keyDown">Action to invoke on keyDown event</param>
        /// <param name="keyUp">Action to invoke on keyUp event</param>
        /// <param name="preventDefault">Should default mapping of the key be prevented default: true</param>
        public Key(int keyCode, Action keyDown, Action keyUp, bool preventDefault = true)
        {
            this.keyCode = keyCode;
            this.keyDown = keyDown;
            this.keyUp = keyUp;
            this.preventDefault = preventDefault;
        }
    }
}
