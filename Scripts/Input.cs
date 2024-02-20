using Godot;
using System;
using System.Collections.Generic;

#nullable enable

namespace Base
{
    public enum InputName
    {
        LEFT_CLICK,
        RIGHT_CLICK,
        MIDDLE_CLICK,
        SCROLL_UP,
        SCROLL_DOWN,
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,
        TILDE,
        ONE,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        ZERO,
        MINUS,
        EQUALS,
        BACKSPACE,
        TAB,
        CAPS_LOCK,
        LEFT_SHIFT,
        LEFT_CONTROL,
        LEFT_ALT,
        LEFT_FUNCTION,
        SPACEBAR,
        RIGHT_SHIFT,
        RIGHT_CONTROL,
        RIGHT_ALT,
        RIGHT_FUNCTION,
        ENTER,
        ESCAPE,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,
        PRINT_SCREEN,
        SCROLL_LOCK,
        PAUSE,
        INSERT,
        DELETE,
        HOME,
        END,
        PAGE_UP,
        PAGE_DOWN,
        LEFT,
        RIGHT,
        UP,
        DOWN,
    }

    public class Input
    {
        public const int NOTHING = -1;

        public static int elementHovered = NOTHING;
        public static int elementHeld = NOTHING;

        public static void GuiInput(InputEvent inputEvent, int elementId)
        {
            
        }
    }
}