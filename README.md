# KeyboardHookLib

## About

This came to life just because Corsair is not allowing to have any other key as an FN key other than what they say is an FN key so software overide for my own purposes, but also able to run any other actions.
This library will add a hook to windows and listen for keypresses in all proceses and if conditions are met will execute Action attached to the key, until the app which installed this hook is closed.
It will not listen to processes that were started as an admin unless the app that installs this hook is started as an admin.
This will not run in a console application it requires standard event loop you should run this from WPF or WinForms application

## How to use it

```
using System.Windows.Forms;
using System.Runtime.InteropServices;
using KeyboardHookLib;



int KEYEVENTF_EXTENDEDKEY = 0x0001;
int KEYEVENTF_KEYUP = 0x0002;

[DllImport("user32.dll", SetLastError = true)]
static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
//Use caps-lock (20) as a toggle trigger
KeyboardOverlay kOverlay = new KeyboardOverlay(20,true);

/**
* Assign some actions to keys I - Up, K - Down, J - Left, L - Right
* Each key can have an action on key down and key up event.
* If you don't neet it set it as null
*/
kOverlay.AssignAction(
    (int)Keys.I,
    () =>
    {
        keybd_event((byte)Keys.Up, 0, KEYEVENTF_EXTENDEDKEY, 0);

    },
    () =>
    {
        keybd_event((byte)Keys.Up, 0, KEYEVENTF_KEYUP, 0);
    }
);
kOverlay.AssignAction(
    (int)Keys.J,
    () =>
    {
        keybd_event((byte)Keys.Left, 0, KEYEVENTF_EXTENDEDKEY, 0);

    },
    () =>
    {
        keybd_event((byte)Keys.Left, 0, KEYEVENTF_KEYUP, 0);
    }
);
kOverlay.AssignAction(
    (int)Keys.K,
    () =>
    {
        keybd_event((byte)Keys.Down, 0, KEYEVENTF_EXTENDEDKEY, 0);

    },
    () =>
    {
        keybd_event((byte)Keys.Down, 0, KEYEVENTF_KEYUP, 0);
    }
);
kOverlay.AssignAction(
    (int)Keys.L,
    () =>
    {
        keybd_event((byte)Keys.Right, 0, KEYEVENTF_EXTENDEDKEY, 0);

    },
    () =>
    {
        keybd_event((byte)Keys.Right, 0, KEYEVENTF_KEYUP, 0);
    }
);

/**
* Array of overlays allows for easy determination if trigger was pressed.
* Each index in the array could be mapped to a Virtual-Key code in Windows.
*/
KeyboardOverlay[] overlays = new KeyboardOverlay[256];
overlays[kOverlay.Trigger] = kOverlay;


Hook._keyboardOverlays = overlays;
IntPtr hook = Hook.UseHook();
Application.Run();
```
