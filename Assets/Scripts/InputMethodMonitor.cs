using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputMethodMonitor : MonoBehaviour
{

    public static InputMethodMonitor singleton;

    public enum InputMethod
    {
        Buttons, //Keyboard or gamepad
        Mouse
    }

    /// <summary>
    /// An event invoked when the input method is changed providing the old input method as a parameter
    /// </summary>
    public UnityEvent<InputMethod> InputMethodChanged;
    public InputMethod CurrentInputMethod { get; protected set; }

    private void Awake()
    {
        singleton = this;
    }

    private void Update()
    {

        InputMethod newInputMethod = GetCurrentInputMethod();

        if (newInputMethod != CurrentInputMethod)
        {
            InputMethod oldInputMethod = CurrentInputMethod;
            CurrentInputMethod = newInputMethod;
            InputMethodChanged.Invoke(oldInputMethod);
        }

    }

    private InputMethod GetCurrentInputMethod()
    {

        //The Unity input system being used makes it very hard to determine what I am trying to determine so this long code is required

        bool anyKeyDown = Input.anyKeyDown
            || Input.GetButtonDown("Submit")
            || Input.GetButtonDown("Run")
            || Input.GetButtonDown("Interact")
            || Input.GetButtonDown("Cancel")
            || Input.GetButtonDown("Menu")
            || Input.GetAxis("Horizontal") != 0
            || Input.GetAxis("Vertical") != 0
            || Input.GetButtonDown("Shortcut 1")
            || Input.GetButtonDown("Shortcut 2")
            || Input.GetButtonDown("Shortcut 3")
            || Input.GetButtonDown("Shortcut 4")
            || Input.GetButtonDown("Shortcut 5")
            || Input.GetButtonDown("Shortcut 6")
            || Input.GetButtonDown("Shortcut 7")
            || Input.GetButtonDown("Shortcut 8")
            || Input.GetButtonDown("Shortcut 9")
            || Input.GetButtonDown("Shortcut 10")
            || Input.GetButtonDown("Cheat Console");

        bool mouseButtonDown = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
        bool keyboardOrGamepadButtonDown = anyKeyDown && !mouseButtonDown;

        if (mouseButtonDown != keyboardOrGamepadButtonDown)
        {
            if (mouseButtonDown)
                return InputMethod.Mouse;
            else
                return InputMethod.Buttons;
        }

        return CurrentInputMethod;

    }

}
