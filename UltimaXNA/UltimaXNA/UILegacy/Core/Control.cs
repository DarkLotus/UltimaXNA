﻿/***************************************************************************
 *   Control.cs
 *   Part of UltimaXNA: http://code.google.com/p/ultimaxna
 *   
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 3 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UltimaXNA.Graphics;
using UltimaXNA.Input;

namespace UltimaXNA.UILegacy
{
    internal delegate void ControlMouseButtonEvent(int x, int y, MouseButton button);
    internal delegate void ControlMouseEvent(int x, int y);
    internal delegate void ControlEvent();

    public delegate void PublicControlEvent();

    public class Control : IControl
    {
        bool _enabled = false;
        bool _visible = false;
        bool _isInitialized = false;
        bool _isDisposed = false;
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }
        public bool Visible { get { return _visible; } set { _visible = value; } }
        public bool IsInitialized { get { return _isInitialized; } set { _isInitialized = value; } }
        public bool IsDisposed { get { return _isDisposed; } set { _isDisposed = value; } }
        public bool IsMovable = false;

        bool _handlesMouseInput = false;
        public bool HandlesMouseInput { get { return _handlesMouseInput; } set { _handlesMouseInput = value; } }
        bool _handlesKeyboardFocus = false;
        public bool HandlesKeyboardFocus { get { return _handlesKeyboardFocus; } set { _handlesKeyboardFocus = value; } }

        protected bool _renderFullScreen = false;

        internal ControlMouseButtonEvent OnMouseClick;
        internal ControlMouseButtonEvent OnMouseDoubleClick;
        internal ControlMouseButtonEvent OnMouseDown;
        internal ControlMouseButtonEvent OnMouseUp;
        internal ControlMouseEvent OnMouseOver;
        internal ControlMouseEvent OnMouseOut;

        float _inputMultiplier = 1.0f;
        public float InputMultiplier
        {
            set { _inputMultiplier = value; }
            get
            {
                if (_renderFullScreen)
                    return _inputMultiplier;
                else
                    return 1.0f;
            }
        }

        int _page = 0;
        public int Page
        {
            get
            {
                return _page;
            }
        }
        int _activePage = 0; // we always draw _activePage and Page 0.
        public int ActivePage
        {
            get { return _activePage; }
            set
            {
                _activePage = value;
                // Clear the current keyboardfocus if we own it and it's page != 0
                // If the page = 0, then it will still exist so it should maintain focus.
                if (_manager.KeyboardFocusControl != null)
                {
                    if (Controls.Contains(_manager.KeyboardFocusControl))
                    {
                        if (_manager.KeyboardFocusControl.Page == 0)
                            _manager.AnnounceNewKeyboardHandler(_manager.KeyboardFocusControl);
                        else
                            _manager.AnnounceNewKeyboardHandler(null);
                    }
                }
                // When you SET ActivePage to something, it announces to the inputmanager that there may be newly popped up
                // text boxes that want keyboard input.
                foreach (Control c in Controls)
                {
                    if (c.HandlesKeyboardFocus && (c.Page == 0 || c.Page == _activePage))
                    {
                        _manager.AnnounceNewKeyboardHandler(c);
                    }
                }
            }
        }

        Rectangle _area = Rectangle.Empty;
        Point2D _position;
        protected int OwnerX
        {
            get
            {
                if (_owner != null)
                    return _owner.X + _owner.OwnerX;
                else
                    return 0;
            }
        }
        protected int OwnerY
        {
            get
            {
                if (_owner != null)
                    return _owner.Y + _owner.OwnerY;
                else
                    return 0;
            }
        }
        public int X { get { return _position.X; } set { _position.X = value; } }
        public int Y { get { return _position.Y; } set { _position.Y = value; } }
        public virtual int Width
        {
            get { return _area.Width; }
            set
            {
                _area.Width = value;
            }
        }
        public virtual int Height
        {
            get { return _area.Height; }
            set
            {
                _area.Height = value;
            }
        }
        public Point2D Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }
        public Point2D Size
        {
            get { return new Point2D(_area.Width, _area.Height); }
            set
            {
                _area.Width = value.X;
                _area.Height = value.Y;
            }
        }
        public Rectangle Area
        {
            get { return _area; }
        }

        protected Control _owner = null;
        public Control Owner { get { return _owner; } }
        protected UIManager _manager = null;
        private List<Control> __controls = null;
        protected List<Control> Controls
        {
            get
            {
                if (__controls == null)
                    __controls = new List<Control>();
                return __controls;
            }
        }

        protected string getTextEntry(int entryID)
        {
            foreach (Control c in Controls)
            {
                if (c.GetType() == typeof(Gumplings.TextEntry))
                {
                    Gumplings.TextEntry g = (Gumplings.TextEntry)c;
                    if (g.EntryID == entryID)
                        return g.Text;
                }
            }
            return string.Empty;
        }

        static Texture2D _boundsTexture;
        protected static Texture2D BoundsTexture
        {
            get
            {
                if (_boundsTexture == null)
                {
                    _boundsTexture = new Texture2D(ClientVars._Support.Graphics, 1, 1);
                    _boundsTexture.SetData<Color>(new Color[] { Color.White });
                }
                return _boundsTexture;
            }
        }

        public Control(Control owner, int page)
        {
            _owner = owner;
            _page = page;
        }

        public virtual void Initialize(UIManager manager)
        {
            _manager = manager;
            _isInitialized = true;
            _isDisposed = false;
            Visible = true;
        }

        public Control AddControl(Control c)
        {
            Controls.Add(c);
            return LastControl;
        }

        public Control LastControl
        {
            get { return Controls[Controls.Count - 1]; }
        }

        public void ClearControls()
        {
            if (Controls != null)
                foreach (Control c in Controls)
                    c.Dispose();
        }

        public virtual void Dispose()
        {
            ClearControls();
            _isDisposed = true;
        }

        DragWidget _dragger;
        public void MakeDragger(Control toMove)
        {
            this.HandlesMouseInput = true;
            _dragger = new DragWidget(this, _owner);
        }

        Control _closeTarget;
        public void MakeCloseTarget(Control toClose)
        {
            _closeTarget = toClose;
            this.HandlesMouseInput = true;
            this.OnMouseClick += onCloseTargetClick;
        }
        void onCloseTargetClick(int x, int y, MouseButton button)
        {
            if (button == MouseButton.Right)
            {
                _closeTarget.Dispose();
            }
        }

        public Control[] HitTest(Point2D position, bool alwaysHandleMouseInput)
        {
            List<Control> focusedControls = new List<Control>();

            // offset the mouse position if we are rendering full screen...
            position.X = (int)((float)(position.X) / InputMultiplier);
            position.Y = (int)((float)(position.Y) / InputMultiplier);

            // If we're owned by something, make sure we increment our hitArea to show this.
            // position.X -= OwnerX;
            // position.Y -= OwnerY;

            bool inBounds = Area.Contains((int)position.X - OwnerX, (int)position.Y - OwnerY);
            if (inBounds)
            {
                if (_hitTest((int)position.X - X - OwnerX, (int)position.Y - Y - OwnerY))
                {
                    if (alwaysHandleMouseInput || this.HandlesMouseInput)
                        focusedControls.Insert(0, this);
                    foreach (Control c in Controls)
                    {
                        if ((c.Page == 0) || (c.Page == ActivePage))
                        {
                            Control[] c1 = c.HitTest(position, false);
                            if (c1 != null)
                            {
                                for (int i = c1.Length - 1; i >= 0; i--)
                                {
                                    focusedControls.Insert(0, c1[i]);
                                }
                            }
                        }
                    }
                }
            }

            if (focusedControls.Count == 0)
                return null;
            else
                return focusedControls.ToArray();
        }

        protected virtual bool _hitTest(int x, int y)
        {
            return true;
        }

        virtual public void Update(GameTime gameTime)
        {
            if (!_isInitialized)
                return;

            // update our area X and Y to reflect any movement.
            _area.X = X;
            _area.Y = Y;

            foreach (Control c in Controls)
            {
                if (!c.IsInitialized)
                    c.Initialize(_manager);
                c.Update(gameTime);
            }

            List<Control> disposedControls = new List<Control>();
            foreach (Control c in Controls)
            {
                if (c.IsDisposed)
                    disposedControls.Add(c);
            }
            foreach (Control c in disposedControls)
            {
                Controls.Remove(c);
            }
        }

        virtual public void Draw(SpriteBatchUI spriteBatch)
        {
            if (!_isInitialized)
                return;
            if (!Visible)
                return;

#if DEBUG
            // DrawBounds(spriteBatch);
#endif
        
            foreach (Control c in Controls)
            {
                if ((c.Page == 0) || (c.Page == ActivePage))
                {
                    if (c.IsInitialized)
                    {
                        c.Position += Position;
                        c.Draw(spriteBatch);
                        c.Position -= Position;
                    }
                }
            }
        }

#if DEBUG
        protected void DrawBounds(SpriteBatchUI spriteBatch, Color color)
        {
            int hue = Data.HuesXNA.GetWebSafeHue(color);

            Rectangle drawArea = _area;
            if (_owner == null)
            {
                _area.X -= X;
                _area.Y -= Y;
            }

            spriteBatch.Draw2D(BoundsTexture, new Rectangle(X, Y, Width, 1), hue, false, false);
            spriteBatch.Draw2D(BoundsTexture, new Rectangle(X, Y + Height - 1, Width, 1), hue, false, false);
            spriteBatch.Draw2D(BoundsTexture, new Rectangle(X, Y, 1, Height), hue, false, false);
            spriteBatch.Draw2D(BoundsTexture, new Rectangle(X + Width - 1, Y, 1, Height), hue, false, false);
        }
#endif

        public virtual void ActivateByButton(int buttonID)
        {
            if (_owner != null)
                _owner.ActivateByButton(buttonID);
        }

        public virtual void ActivateByHREF(string href)
        {
            if (_owner != null)
                _owner.ActivateByHREF(href);
        }

        public virtual void ActivateByKeyboardReturn(int textID, string text)
        {
            if (_owner != null)
                _owner.ActivateByKeyboardReturn(textID, text);
        }

        public virtual void ChangePage(int pageIndex)
        {
            if (_owner != null)
                _owner.ChangePage(pageIndex);
        }

        public void MouseDown(Point2D position, MouseButton button)
        {
            lastClickPosition = position;
            int x = (int)position.X - X - OwnerX;
            int y = (int)position.Y - Y - OwnerY;
            mouseDown(x, y, button);
            if (OnMouseDown != null)
                OnMouseDown(x, y, button);
        }

        public void MouseUp(Point2D position, MouseButton button)
        {
            int x = (int)position.X - X - OwnerX;
            int y = (int)position.Y - Y - OwnerY;
            mouseUp(x, y, button);
            if (OnMouseUp != null)
                OnMouseUp(x, y, button);
        }

        public void MouseOver(Point2D position)
        {
            // Does not double-click if you move your mouse more than x pixels from where you first clicked.
            if (Math.Abs(lastClickPosition.X - position.X) + Math.Abs(lastClickPosition.Y - position.Y) > 3)
                maxTimeForDoubleClick = 0.0f;

            int x = (int)position.X - X - OwnerX;
            int y = (int)position.Y - Y - OwnerY;
            mouseOver(x, y);
            if (OnMouseOver != null)
                OnMouseOver(x, y);
        }

        public void MouseOut(Point2D position)
        {
            int x = (int)position.X - X - OwnerX;
            int y = (int)position.Y - Y - OwnerY;
            mouseOut(x, y);
            if (OnMouseOut != null)
                OnMouseOut(x, y);
        }

        float maxTimeForDoubleClick = 0f;
        Point2D lastClickPosition;

        public void MouseClick(Point2D position, MouseButton button)
        {
            int x = (int)position.X - X - OwnerX;
            int y = (int)position.Y - Y - OwnerY;

            bool doubleClick = false;
            if (maxTimeForDoubleClick != 0f)
            {
                if (ClientVars.EngineVars.TheTime <= maxTimeForDoubleClick)
                {
                    maxTimeForDoubleClick = 0f;
                    doubleClick = true;
                }
            }
            else
            {
                maxTimeForDoubleClick = ClientVars.EngineVars.TheTime + ClientVars.EngineVars.SecondsForDoubleClick;
            }

            mouseClick(x, y, button);
            if (OnMouseClick != null)
                OnMouseClick(x, y, button);

            if (doubleClick)
            {
                mouseDoubleClick(x, y, button);
                if (OnMouseDoubleClick != null)
                    OnMouseDoubleClick(x, y, button);
            }
        }

        public void KeyboardInput(InputEventKB e)
        {
            keyboardInput(e);
        }

        public void ItemDrop(Entities.Item item, int x, int y)
        {
            itemDrop(item, x, y);
        }

        protected virtual void mouseDown(int x, int y, MouseButton button)
        {

        }

        protected virtual void mouseUp(int x, int y, MouseButton button)
        {

        }

        protected virtual void mouseOver(int x, int y)
        {

        }

        protected virtual void mouseOut(int x, int y)
        {

        }

        protected virtual void mouseClick(int x, int y, MouseButton button)
        {

        }

        protected virtual void mouseDoubleClick(int x, int y, MouseButton button)
        {

        }

        protected virtual void keyboardInput(InputEventKB e)
        {

        }

        protected virtual void itemDrop(Entities.Item item, int x, int y)
        {

        }

        internal void Center()
        {
            Position = new Point2D(
                (_manager.Width - Width) / 2,
                (_manager.Height - Height) / 2);
        }

        internal Color GumpColorHue(int hue, bool hueOnlyGreyPixels)
        {
            if (hue == 0)
                return Color.White;
            else
            {
                // max hue is 0xFFF, 12 bits. Pack these 12 bits into RG. B is the flag byte.
                Color c = new Color(0, 0, 0, 255);
                c.R = (byte)((hue & 0x003F) << 2);
                c.G = (byte)((hue & 0x0FC0) >> 4);
                if (hueOnlyGreyPixels)
                    c.B |= 0x1;
                return c;
            }
        }

        internal Color GumpColorReal(Color color)
        {
            if (color == Color.White)
                return Color.White;
            else
            {
                // pack the color into RGB565
                int packed = ((color.R & 0xE0) >> 3) + ((color.G & 0xF0) << 3) + ((color.B & 0xE0) << 8);

                Color c = new Color(0, 0, 0, 255);
                c.R = (byte)(packed & 0x000000FF);
                c.G = (byte)((packed & 0x0000FF00) >> 8);
                c.B |= 0x2; // flag for unpacking a color
                // if (hueOnlyGreyPixels)
                //     c.B |= 0x1;
                return c;
            }
        }

        internal void ReleaseKeyboardInput(Control c)
        {
            int startIndex = Controls.IndexOf(c);
            for (int i = startIndex + 1; i < Controls.Count; i++)
            {
                if (Controls[i].HandlesKeyboardFocus)
                {
                    _manager.KeyboardFocusControl = Controls[i];
                    return;
                }
            }
            for (int i = 0; i < startIndex; i++)
            {
                if (Controls[i].HandlesKeyboardFocus)
                {
                    _manager.KeyboardFocusControl = Controls[i];
                    return;
                }
            }
        }
    }
}
