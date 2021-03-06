﻿/********************************************************
 * 
 *  WndProc.cs
 *  
 *  (C) Copyright 2009 Jeff Boulanger. All rights reserved. 
 *  Used in UltimaXNA with permission.
 *  
 ********************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UltimaXNA.Input;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;

namespace UltimaXNA.Input.Core
{
    /// <summary>
    /// Provides an asyncronous Input Event system that can be used to monitor Keyboard and Mouse events.
    /// </summary>
    public class WndProc : MessageHook
    {
        const bool WP_PASSTHROUGH = true;
        const bool WP_NOPASSTHROUGH = false;

        public override int HookType
        {
            get { return NativeConstants.WH_CALLWNDPROC; }
        }

        protected WndProc(IntPtr hWnd)
            : base(hWnd)
        {
            
        }

        protected MouseState getMouseState()
        {
            return Mouse.GetState();
        }

        protected KeyboardState getKeyState()
        {
            return Keyboard.GetState();
        }

        /// <summary>
        /// Gets the currently pressed Modifier keys, Control, Alt, Shift
        /// </summary>
        protected WinKeys getModifierKeys()
        {
            WinKeys none = WinKeys.None;

            if (NativeMethods.GetKeyState(0x10) < 0)
            {
                none |= WinKeys.Shift;
            }

            if (NativeMethods.GetKeyState(0x11) < 0)
            {
                none |= WinKeys.Control;
            }

            if (NativeMethods.GetKeyState(0x12) < 0)
            {
                none |= WinKeys.Alt;
            }

            return none;
        }

        /// <summary>
        /// Gets the current pressed Mouse Buttons
        /// </summary>
        protected MouseButtonInternal getMouseButtons(MouseState state)
        {
            MouseButtonInternal none = MouseButtonInternal.None;

            if (state.LeftButton == ButtonState.Pressed)
                none |= MouseButtonInternal.Left;
            if (state.RightButton == ButtonState.Pressed)
                none |= MouseButtonInternal.Right;
            if (state.MiddleButton == ButtonState.Pressed)
                none |= MouseButtonInternal.Middle;
            if (state.XButton1 == ButtonState.Pressed)
                none |= MouseButtonInternal.XButton1;
            if (state.XButton2 == ButtonState.Pressed)
                none |= MouseButtonInternal.XButton2;

            return none;
        }

        protected override IntPtr WndProcHook(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            Message message = new Message(msg, wParam, lParam);
            if (WndPrc(ref message) == WP_NOPASSTHROUGH)
                return IntPtr.Zero;
            return base.WndProcHook(hWnd, msg, wParam, lParam);
        }

        private bool WndPrc(ref Message message)
        {
            try
            {
                switch (message.Id)
                {
                    case NativeConstants.WM_DEADCHAR:
                        {
                            break;
                        }
                    case NativeConstants.WM_KEYDOWN:
                    case NativeConstants.WM_KEYUP:
                    case NativeConstants.WM_CHAR:
                        {
                            
                            WmKeyEvent(ref message);
                            
                            break;
                        }
                    case NativeConstants.WM_SYSKEYDOWN:
                    case NativeConstants.WM_SYSKEYUP:
                    case NativeConstants.WM_SYSCHAR:
                        {
                            NativeMethods.TranslateMessage(ref message);
                            WmKeyEvent(ref message);
                            return WP_NOPASSTHROUGH;
                        }
                    case NativeConstants.WM_SYSCOMMAND:
                        {
                            break;
                        }
                    case NativeConstants.WM_MOUSEMOVE:
                        {
                            WmMouseMove(ref message);
                            break;
                        }
                    case NativeConstants.WM_LBUTTONDOWN:
                        {
                            WmMouseDown(ref message, MouseButtonInternal.Left, 1);
                            break;
                        }
                    case NativeConstants.WM_RBUTTONDOWN:
                        {
                            WmMouseDown(ref message, MouseButtonInternal.Right, 1);
                            break;
                        }
                    case NativeConstants.WM_MBUTTONDOWN:
                        {
                            WmMouseDown(ref message, MouseButtonInternal.Middle, 1);
                            break;
                        }
                    case NativeConstants.WM_LBUTTONUP:
                        {
                            WmMouseUp(ref message, MouseButtonInternal.Left, 1);
                            return WP_PASSTHROUGH;
                        }
                    case NativeConstants.WM_RBUTTONUP:
                        {
                            WmMouseUp(ref message, MouseButtonInternal.Right, 1);
                            return WP_PASSTHROUGH;
                        }
                    case NativeConstants.WM_MBUTTONUP:
                        {
                            WmMouseUp(ref message, MouseButtonInternal.Middle, 1);
                            return WP_PASSTHROUGH;
                        }
                    case NativeConstants.WM_LBUTTONDBLCLK:
                        {
                            WmMouseDown(ref message, MouseButtonInternal.Left, 2);
                            return WP_PASSTHROUGH;
                        }
                    case NativeConstants.WM_RBUTTONDBLCLK:
                        {
                            WmMouseDown(ref message, MouseButtonInternal.Right, 2);
                            return WP_PASSTHROUGH;
                        }
                    case NativeConstants.WM_MBUTTONDBLCLK:
                        {
                            WmMouseDown(ref message, MouseButtonInternal.Middle, 2);
                            return WP_PASSTHROUGH;
                        }
                    case NativeConstants.WM_MOUSEWHEEL:
                        {
                            WmMouseWheel(ref message);
                            return WP_PASSTHROUGH;
                        }
                    case NativeConstants.WM_XBUTTONDOWN:
                        {
                            WmMouseDown(ref message, GetXButton(Message.HighWord(message.WParam)), 1);
                            return WP_PASSTHROUGH;
                        }
                    case NativeConstants.WM_XBUTTONUP:
                        {
                            WmMouseUp(ref message, GetXButton(Message.HighWord(message.WParam)), 1);
                            return WP_PASSTHROUGH;
                        }
                    case NativeConstants.WM_XBUTTONDBLCLK:
                        {
                            WmMouseDown(ref message, GetXButton(Message.HighWord(message.WParam)), 2);
                            return WP_PASSTHROUGH;
                        }
                }
            }
            catch
            {
                //TODO: log...crash...what?
            }

            return WP_PASSTHROUGH;
        }

        private MouseButtonInternal translateWParamIntoMouseButtons(int wParam)
        {
            MouseButtonInternal mb = MouseButtonInternal.None;
            if ((wParam & 0x0001) == 0x0001)
                mb |= MouseButtonInternal.Left;
            if ((wParam & 0x0002) == 0x0002)
                mb |= MouseButtonInternal.Right;
            if ((wParam & 0x0002) == 0x0010)
                mb |= MouseButtonInternal.Middle;
            if ((wParam & 0x0002) == 0x0020)
                mb |= MouseButtonInternal.XButton1;
            if ((wParam & 0x0002) == 0x0040)
                mb |= MouseButtonInternal.XButton2;
            return mb;
        }

        /// <summary>
        /// Gets the Mouse XButton deciphered from the wparam argument of a Message
        /// </summary>
        /// <param name="wparam"></param>
        /// <returns></returns>
        private MouseButtonInternal GetXButton(int wparam)
        {
            switch (wparam)
            {
                case 1: return MouseButtonInternal.XButton1;
                case 2: return MouseButtonInternal.XButton2;
            }

            return MouseButtonInternal.None;
        }

        /// <summary>
        /// Reads the supplied message and executes any Mouse Wheel events required.
        /// </summary>
        /// <param name="message">The Message to parse</param>
        private void WmMouseWheel(ref Message message)
        {
            OnMouseWheel(new EventArgsMouse(
                translateWParamIntoMouseButtons(Message.SignedLowWord(message.WParam)),
                Message.SignedHighWord(message.WParam), 
                Message.SignedLowWord(message.LParam), 
                Message.SignedHighWord(message.LParam),
                (int)(long)message.WParam,
                getModifierKeys()
                ));
        }

        /// <summary>
        /// Reads the supplied message and executes any Mouse Move events required.
        /// </summary>
        /// <param name="message">The Message to parse</param>
        private void WmMouseMove(ref Message message)
        {
            OnMouseMove(new EventArgsMouse(
                translateWParamIntoMouseButtons(Message.SignedLowWord(message.WParam)),
                0, 
                Message.SignedLowWord(message.LParam), 
                Message.SignedHighWord(message.LParam),
                (int)(long)message.WParam,
                getModifierKeys()
                ));
        }

        /// <summary>
        /// Reads the supplied message and executes any Mouse Down events required.
        /// </summary>
        /// <param name="message">The Message to parse</param>
        /// <param name="button">The Mouse Button the Message is for</param>
        /// <param name="clicks">The number of clicks for the Message</param>
        private void WmMouseDown(ref Message message, MouseButtonInternal button, int clicks)
        {
            // HandleMouseBindings();
            OnMouseDown(new EventArgsMouse(
                button, 
                clicks, 
                Message.SignedLowWord(message.LParam), 
                Message.SignedHighWord(message.LParam),
                (int)(long)message.WParam,
                getModifierKeys()
                ));
        }

        /// <summary>
        /// Reads the supplied message and executes any Mouse Up events required.
        /// </summary>
        /// <param name="message">The Message to parse</param>
        /// <param name="button">The Mouse Button the Message is for</param>
        /// <param name="clicks">The number of clicks for the Message</param>
        private void WmMouseUp(ref Message message, MouseButtonInternal button, int clicks)
        {
            // HandleMouseBindings();
            OnMouseUp(new EventArgsMouse(
                button, 
                clicks, 
                Message.SignedLowWord(message.LParam), 
                Message.SignedHighWord(message.LParam),
                (int)(long)message.WParam,
                getModifierKeys()
                ));
        }

        /// <summary>
        /// Reads the supplied message and executes any Keyboard events required.
        /// </summary>
        /// <param name="message">The Message to parse</param>
        /// <returns>A Boolean value indicating wether the Key events were handled or not</returns>
        private void WmKeyEvent(ref Message message)
        {
            // HandleKeyBindings();
            // KeyPressEventArgs keyPressEventArgs = null;
            EventArgsKeyboard EventArgsKeyboard = null;

            if ((message.Id == NativeConstants.WM_CHAR) || (message.Id == NativeConstants.WM_SYSCHAR))
            {
                // Is this extra information necessary?
                // wm_(sys)char: http://msdn.microsoft.com/en-us/library/ms646276(VS.85).aspx

                EventArgsKeyboard = new EventArgsKeyboard(
                    (WinKeys)(int)(long)message.WParam,
                    (int)(long)message.LParam,
                    getModifierKeys()
                    );
                IntPtr zero = (IntPtr)0;// (char)((ushort)((long)message.WParam));
                OnChar(EventArgsKeyboard);
            }
            else
            {
                // wm_(sys)keydown: http://msdn.microsoft.com/en-us/library/ms912654.aspx
                // wm_(sys)keyup: http://msdn.microsoft.com/en-us/library/ms646281(VS.85).aspx
                EventArgsKeyboard = new EventArgsKeyboard(
                    (WinKeys)(int)(long)message.WParam,
                    (int)(long)message.LParam,
                    getModifierKeys()
                    );

                if ((message.Id == NativeConstants.WM_KEYDOWN) || (message.Id == NativeConstants.WM_SYSKEYDOWN))
                {
                    OnKeyDown(EventArgsKeyboard);
                }
                else if ((message.Id == NativeConstants.WM_KEYUP) || (message.Id == NativeConstants.WM_SYSKEYUP))
                {
                    OnKeyUp(EventArgsKeyboard);
                }
            }
        }

        /// <summary>
        /// Raises the MouseWheel event. Override this method to add code to handle when a mouse wheel is turned
        /// </summary>
        /// <param name="e">EventArgsMouse for the MouseWheel event</param>
        protected virtual void OnMouseWheel(EventArgsMouse e)
        {

        }

        /// <summary>
        /// Raises the MouseMove event. Override this method to add code to handle when the mouse is moved
        /// </summary>
        /// <param name="e">EventArgsMouse for the MouseMove event</param>
        protected virtual void OnMouseMove(EventArgsMouse e)
        {

        }

        /// <summary>
        /// Raises the MouseDown event. Override this method to add code to handle when a mouse button is pressed
        /// </summary>
        /// <param name="e">EventArgsMouse for the MouseDown event</param>
        protected virtual void OnMouseDown(EventArgsMouse e)
        {

        }

        /// <summary>
        /// Raises the MouseUp event. Override this method to add code to handle when a mouse button is released
        /// </summary>
        /// <param name="e">EventArgsMouse for the MouseUp event</param>
        protected virtual void OnMouseUp(EventArgsMouse e)
        {

        }

        /// <summary>
        /// Raises the KeyUp event. Override this method to add code to handle when a key is released
        /// </summary>
        /// <param name="e">KeyboardPressEventArgs for the KeyUp event</param>
        protected virtual void OnKeyUp(EventArgsKeyboard e)
        {

        }

        /// <summary>
        /// Raises the KeyDown event. Override this method to add code to handle when a key is pressed
        /// </summary>
        /// <param name="e">EventArgsKeyboard for the KeyDown event</param>
        protected virtual void OnKeyDown(EventArgsKeyboard e)
        {

        }
        
        /// <summary>
        /// Raises the OnChar event. Override this method to add code to handle when a WM_CHAR message is received
        /// </summary>
        /// <param name="e">EventArgsKeyboard for the OnChar event</param>
        protected virtual void OnChar(EventArgsKeyboard e)
        {

        }
    }
}
