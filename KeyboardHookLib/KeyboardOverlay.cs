using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardHookLib
{
    /// <summary>
    /// Represents a keyboard overlay that the Hook will use to invoke key actions
    /// </summary>
    public class KeyboardOverlay : IKeyboardOverlay
    {
        private Key[] keys;
        private int trigger;
        private bool toggle;
        
        public int Trigger
        {
            get { return trigger; }
            set { trigger = value; }
        }

        public bool Toggle
        {
            get { return toggle; }
            set { toggle = value; }
        }
        public int GetTrigger()
        {
            return trigger;
        }

        public bool GetToggle()
        {
            return toggle;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="trigger">what key triggers the overlay to be active</param>
        /// <param name="toggle">should overlay toggle rather than hold to use default: true</param>
        public KeyboardOverlay(int trigger, bool toggle=true)
        {
            this.trigger = trigger;
            this.toggle= toggle;
            keys = new Key[256];
        }

        /// <summary>
        /// Invokes action that's attached to the key if the key is not null on key down event
        /// </summary>
        /// <param name="keyCode">Key code of the physical key pressed</param>
        /// <returns>wheter to prevent the default implementation of the key</returns>
        public bool RunKeyDown(int keyCode)
        {
            if(keys[keyCode] == null) { return false; }
            keys[keyCode].KeyDown.Invoke();
            return keys[keyCode].PreventDefault;
        }

        /// <summary>
        /// Invokes action that's attached to the key if the key is not null on key up event
        /// </summary>
        /// <param name="keyCode">Key code of the physical key pressed</param>
        /// <returns>wheter to prevent the default implementation of the key</returns>
        public bool RunKeyUp (int keyCode)
        {
            if (keys[keyCode] == null) { return false; }
            keys[keyCode].KeyUp.Invoke();
            return keys[keyCode].PreventDefault;
        }

        /// <summary>
        /// Creates a new Key and places into key array
        /// </summary>
        /// <param name="keyCode">Key code of the physical key to which the key is mapped</param>
        /// <param name="actionDown">Action to invoke on keyDown event</param>
        /// <param name="actionUp">Action to invoke on keyUp event</param>
        /// <param name="preventDefault">wheter the default action of the key should be prevented</param>
        public void AssignAction(int keyCode, Action actionDown, Action actionUp, bool preventDefault=true)
        {
            keys[keyCode] = new Key(keyCode, actionDown, actionUp, preventDefault);
        }

        /// <summary>
        /// Creates a new Key and places into key array
        /// </summary>
        /// <param name="keyCode">Key code of the physical key to which the key is mapped</param>
        /// <param name="actionDown">Action to invoke on keyDown event</param>
        /// <param name="actionUp">Action to invoke on keyUp event</param>
        public void AssignAction(int keyCode, Action actionDown, Action actionUp)
        {
            keys[keyCode] = new Key(keyCode, actionDown, actionUp);
        }
    }
}
