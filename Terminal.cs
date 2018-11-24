using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RLNET;
using SadRex;

namespace UIConsole
{
    public class Palette
    {
        public static RLColor FloorBackground = RLColor.Black;
        public static RLColor Floor = RLColor.Gray;
        public static RLColor FloorBackgroundFOV = RLColor.Gray;
        public static RLColor FloorFOV = RLColor.White;

        public static RLColor WallBackground = RLColor.Black;
        public static RLColor Wall = RLColor.Gray;
        public static RLColor WallBackgroundFOV = RLColor.Gray;
        public static RLColor WallFOV = RLColor.White;

        public static RLColor TextHeading = RLColor.White;
    }
    public class UIPalette
    {
        public RLColor Background;
        public RLColor TextColour;
        public RLColor HoverBackground;
        public RLColor HoverTextColour;
        public RLColor PressedBackground;
        public RLColor PressedTextColour;

        public RLColor ButtonBackground;
        public RLColor ButtonTextColour;
        public RLColor ButtonBorderTextColour;
        public RLColor ButtonBorderBackground;
        public RLColor ButtonHoverTextColour;
        public RLColor ButtonHoverBackground;
        public RLColor ButtonPressedBackground;
        public RLColor ButtonPressedTextColour;

        public RLColor BorderTextColour;
        public RLColor BorderBackground;
        public RLColor TitleTextColour;
        public RLColor TitleBackground;
    }

    public static class UIColours
    {
        public static UIPalette CoolDark { get
            {
                UIPalette p = new UIPalette();
                p.Background = new RLColor(47, 46, 109);
                p.TextColour = new RLColor(175, 175, 175);

                p.ButtonBorderBackground = new RLColor(97, 96, 109);
                p.ButtonBorderTextColour = new RLColor(250, 250, 250);
                p.ButtonHoverBackground = new RLColor(47, 46, 109);
                p.ButtonHoverTextColour = new RLColor(225, 225, 225);
                p.ButtonPressedBackground = new RLColor(175, 175, 175);
                p.ButtonPressedTextColour = new RLColor(47, 46, 109);

                return p;
            }
        }
    }

    public static class ListExtensions
    {
        public static void MoveItemAtIndexToFront<T>(this List<T> list, int index)
        {
            T item = list[index];
            for (int i = index; i > 0; i--)
                list[i] = list[i - 1];
            list[0] = item;
        }
    }


    public class TerminalEvent : EventArgs
    {
        public string key = "";
        public int keyCode = 0;
        public RLKeyPress keyPress;
        public int mousex = 0;
        public int mousey = 0;
        public int mouseDeltaX { get { return mousex - mouseOldX; } }
        public int mouseDeltaY { get { return mousey - mouseOldY; } }
        public int mouseOldX = 0;
        public int mouseOldY = 0;
        public bool mouseLeft = false;
        public bool mouseRight = false;
    }

    public class TerminalCollection
    {
        private List<Terminal> terminals;
        private int mouseX = 0;
        private int mouseY = 0;
        private bool mouseLeft = false;
        private bool mouseRight = false;
        private RLRootConsole RootConsole;

        public int Count { get { return terminals.Count; } }

        public Terminal Get(int c)
        {
            return terminals[c];
        }
        public Terminal Get(string guid)
        {
            foreach (Terminal term in terminals)
            {
                if (term.guid == guid)
                    return term;
            }
            return new Terminal(0,0,0,0,RootConsole);
        }

        public TerminalCollection(RLRootConsole rootConsole)
        {
            terminals = new List<Terminal>();
            RootConsole = rootConsole;
        }
        public void Add(Terminal term)
        {
            terminals.Add(term);
        }
        public void Remove(Terminal term)
        {
            terminals.Remove(term);
        }
        public void Remove(int index)
        {
            terminals.RemoveAt(index);
        }

        public void Update()
        {
            RLKeyPress kp = RootConsole.Keyboard.GetKeyPress();
            if (kp != null)
                Console.Write(kp.Key);
            KeyDown(kp);

            MouseMove();
            MouseDown();
            MouseUp();

            foreach (Terminal term in terminals)
            {
                term.Update();
            }
        }
        public void Draw()
        {
            for (int i = terminals.Count - 1; i >= 0; i--)
            {
                if (terminals[i].W > 0)
                    terminals[i].Render();
            }
        }
        public void KeyDown(RLKeyPress keyPress)
        {
            if (keyPress != null)
            {
                TerminalEvent e = new TerminalEvent();
                if (keyPress.Char != null)
                {
                    char k = (char)keyPress.Char;
                    e.key = k.ToString();
                    e.keyCode = (int)keyPress.Char;
                }
                e.keyPress = keyPress;
                foreach (Terminal term in terminals)
                {
                    if (term.OnKeyDown(e))
                    {
                        break;
                    }
                }
            }
        }
        public void MouseMove()
        {
            if (mouseX != RootConsole.Mouse.X || mouseY != RootConsole.Mouse.Y)
            {
                TerminalEvent e = new TerminalEvent();
                e.mouseLeft = RootConsole.Mouse.LeftPressed;
                e.mouseRight = RootConsole.Mouse.RightPressed;
                e.mouseOldX = mouseX;
                e.mouseOldY = mouseY;
                e.mousex = RootConsole.Mouse.X;
                e.mousey = RootConsole.Mouse.Y;

                foreach (Terminal terminal in terminals)
                {
                    if (
                        (e.mousex >= terminal.X && e.mousex < terminal.X + terminal.W)
                        && (e.mousey >= terminal.Y && e.mousey < terminal.Y + terminal.H)
                    )
                    {
                        if (terminal.OnMouseMove(e))
                        {
                            break;
                        }
                    }
                }
                mouseX = RootConsole.Mouse.X; mouseY = RootConsole.Mouse.Y;
            }
        }
        public void MouseDown()
        {
            if (
                (RootConsole.Mouse.LeftPressed && !mouseLeft)
                || (RootConsole.Mouse.RightPressed && !mouseRight)
            )
            {
                TerminalEvent e = new TerminalEvent();
                e.mouseLeft = RootConsole.Mouse.LeftPressed;
                e.mouseRight = RootConsole.Mouse.RightPressed;
                e.mousex = RootConsole.Mouse.X;
                e.mousey = RootConsole.Mouse.Y;

                mouseX = RootConsole.Mouse.X;
                mouseY = RootConsole.Mouse.Y;
                mouseLeft = RootConsole.Mouse.LeftPressed;
                mouseRight = RootConsole.Mouse.RightPressed;

                foreach (Terminal terminal in terminals)
                {
                    if (
                        (e.mousex >= terminal.X && e.mousex < terminal.X + terminal.W)
                        && (e.mousey >= terminal.Y && e.mousey < terminal.Y + terminal.H)
                    )
                    {
                        if (terminal.OnMouseDown(e))
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void ToFront(Terminal term)
        {
            terminals.Remove(term);
            terminals.Insert(0, term);
        }

        public void MouseUp()
        {
            if (
                (!RootConsole.Mouse.LeftPressed && mouseLeft)
                || (!RootConsole.Mouse.RightPressed && mouseRight)
            )
            {
                TerminalEvent e = new TerminalEvent();
                e.mouseLeft = RootConsole.Mouse.LeftPressed;
                e.mouseRight = RootConsole.Mouse.RightPressed;
                e.mousex = RootConsole.Mouse.X;
                e.mousey = RootConsole.Mouse.Y;

                mouseX = RootConsole.Mouse.X;
                mouseY = RootConsole.Mouse.Y;
                mouseLeft = false;
                mouseRight = false;
                foreach (Terminal terminal in terminals)
                {
                    if (
                        (e.mousex >= terminal.X && e.mousex < terminal.X + terminal.W)
                        && (e.mousey >= terminal.Y && e.mousey < terminal.Y + terminal.H)
                    )
                    {
                        if (terminal.OnMouseUp(e))
                        {
                            ToFront(terminal);
                            break;
                        }
                    }
                }
            }
        }

    }

    public enum TerminalType
    {
        Window = 0, Button = 1
    }

    public enum TerminalAlign
    {
        Left = 0, Right = 1, Centre = 2, Top = 3, Bottom = 4
    }

    /**********************************************************************************************************************************************************************************
     * 
     *  Terminal Border Enum Types
     * 
     * ********************************************************************************************************************************************************************************/
    public enum TerminalBorder
    {
        None = 0, Single = 1, Double = 2
    }

    /**********************************************************************************************************************************************************************************
     * 
     *  Terminal Border Enum Types
     * 
     * ********************************************************************************************************************************************************************************/
    [Flags]
    public enum TerminalControls
    {
        None = 1, Close = 2, Minimize = 4, Restore = 8, Maximize = 16
    }

    /**********************************************************************************************************************************************************************************
     * 
     *  Terminal Class
     * 
     * ********************************************************************************************************************************************************************************/
    public class Terminal
    {
        public TerminalBorder Border { get; set; }
        public TerminalControls Controls { get; set; }
        public TerminalType Type { get; set; }

        private string _guid;
        public string guid { get { return _guid; } }

        public List<Terminal> Children = new List<Terminal>();
        public Terminal Parent;
        private bool _Hidden = false;
        public bool Hidden { get { return _Hidden; } }
        private bool _Minimized = false;
        public bool Minimized { get { return _Minimized; } }
        private bool _Maximized = false;
        public bool Maximized { get { return _Maximized; } }
        public bool Resizable { get; set; }
        public bool Moveable { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        private int _XRestore;
        private int _YRestore;

        public int W { get; set; }
        public int H { get; set; }
        private int _WRestore;
        private int _HRestore;
        private bool _resizingTerminal = false;

        public int PaddingX { get; set; }
        public int PaddingY { get; set; }

        private int _movingX = 0;
        private int _movingY = 0;
        private bool _movingTitle = false;
        private bool _hasMouseDown = false;
        private bool _hasMouseOver = false;

        public RLColor TextColour
        {
            get
            {
                return ConsoleStyle.TextColour;
            }
        }
        public RLColor Background {
            get
            {
                return ConsoleStyle.Background;
            }
        }

        public string Text { get; set; }
        public TerminalAlign VAlignment { get; set; }
        public TerminalAlign HAlignment { get; set; }
        public string Title { get; set; }

        private UIPalette _consoleStyle;
        public UIPalette ConsoleStyle {
            get { return _consoleStyle; }
            set { _consoleStyle = value; }
        }

        private RLConsole _console;
        private RLRootConsole _rootConsole;

        public RLConsole Console { get { return _console; } }

        public void _Terminal(int x, int y, int w, int h, UIPalette palette, RLRootConsole rootConsole)
        {
            _guid = System.Guid.NewGuid().ToString();
            Type = TerminalType.Window;
            _Hidden = false;
            Text = "";
            Title = "";
            ConsoleStyle = palette;
            Resizable = false;

            VAlignment = TerminalAlign.Top;
            HAlignment = TerminalAlign.Left;
            Border = TerminalBorder.None;
            Controls = TerminalControls.None;
            _console = new RLConsole(w, h);
            _rootConsole = rootConsole;

            _console.SetBackColor(0, 0, w, h, Background);

            X = x; Y = y;
            W = w; H = h;
            PaddingX = 1;
            PaddingY = 1;
            _XRestore = X; _YRestore = Y;
            _WRestore = W; _HRestore = H;
        }

        public Terminal(int x, int y, int w, int h, UIPalette style, RLRootConsole rootConsole)
        {
            _Terminal(x, y, w, h, style, rootConsole);
        }
        public Terminal(int x, int y, int w, int h, RLRootConsole rootConsole)
        {
            UIPalette c = UIColours.CoolDark;
            _Terminal(x, y, w, h, c, rootConsole);
        }

        public void AddChild(Terminal term)
        {
            term.Parent = this;
            Children.Add(term);
        }

        public long MilisecondsSinceEpoch()
        {
            return (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        private int TitleLength()
        {
            return Title.Length + 10;
        }

        public async Task Minimize(long timeout)
        {
            long time = timeout;
            time = MilisecondsSinceEpoch() + timeout;
            await Task.Run(() =>
            {
                while (true)
                {
                    if (time < MilisecondsSinceEpoch())
                    {
                        time = MilisecondsSinceEpoch() + timeout;
                        W--;
                        H--;

                        X -= 2;
                        Y -= 2;
                        if (X < 0)
                            X = 0;
                        if (Y < 0)
                            Y = 0;

                        if (W < TitleLength())
                            W = TitleLength();
                        if (H < 2)
                            H = 2;

                        if (H == 2 && X == 0 && Y == 0 && W == TitleLength())
                        {
                            _Minimized = true;
                            break;
                        }
                    }
                }
            });
        }

        public static IEnumerable<T> MaskToList<T>(Enum mask)
        {
            if (typeof(T).IsSubclassOf(typeof(Enum)) == false)
                throw new ArgumentException();

            return Enum.GetValues(typeof(T))
                                 .Cast<Enum>()
                                 .Where(m => mask.HasFlag(m))
                                 .Cast<T>();
        }

        public async Task Maximize(long timeout)
        {
            long time = timeout;
            time = MilisecondsSinceEpoch() + timeout;
            Console.Resize(_rootConsole.Width, _rootConsole.Height);
            await Task.Run(() =>
            {
                while (true)
                {
                    if (time < MilisecondsSinceEpoch())
                    {
                        time = MilisecondsSinceEpoch() + timeout;
                        W += 2;
                        H += 2;
                        X--;
                        Y--;

                        if (X < 0)
                            X = 0;
                        if (Y < 0)
                            Y = 0;
                        if (W > _rootConsole.Width)
                            W = _rootConsole.Width;
                        if (H > _rootConsole.Height)
                            H = _rootConsole.Height;

                        if (
                            H == _rootConsole.Height && W == _rootConsole.Width
                            && X == 0 && Y == 0
                        )
                        {
                            _Maximized = true;
                            break;
                        }
                    }
                }
            });
        }

        public async Task Restore(long timeout)
        {
            long time = timeout;
            time = MilisecondsSinceEpoch() + timeout;
            await Task.Run(() =>
            {
                while (true)
                {
                    if (time < MilisecondsSinceEpoch())
                    {
                        time = MilisecondsSinceEpoch() + timeout;

                        int distY = Math.Abs(_HRestore - Y);
                        int distX = Math.Abs(_WRestore - X);

                        if (X < _XRestore)
                        {
                            X++;
                        }
                        else if (X > _XRestore)
                        {
                            X--;
                        }
                        if (Y < _YRestore)
                        {
                            Y++;
                        }
                        else if (Y > _YRestore)
                        {
                            Y--;
                        }

                        if (W < _WRestore)
                        {
                            if (distX > 1)
                            {
                                W += 2;
                            }
                            else if (distX == 1)
                            {
                                W++;
                            }
                        }
                        else if (W > _WRestore)
                        {
                            if (distX > 1)
                            {
                                W -= 2;
                            }
                            else if (distX == 1)
                            {
                                W--;
                            }
                        }
                        if (H < _HRestore)
                        {
                            if (distY > 1)
                            {
                                H += 2;
                            }
                            else if (distY == 1)
                            {
                                H++;
                            }
                        }
                        else if (H > _HRestore)
                        {
                            if (distY > 1)
                            {
                                H -= 2;
                            }
                            else if (distY == 1)
                            {
                                H--;
                            }

                        }

                        if (H == _HRestore && W == _WRestore && X == _XRestore && Y == _YRestore)
                        {
                            _Maximized = false;
                            _Minimized = false;
                            break;
                        }
                    }
                }
            });
        }

        public async Task Hide(long timeout)
        {
            long time = timeout;
            time = MilisecondsSinceEpoch() + timeout;
            await Task.Run(() =>
            {
                while (true)
                {
                    if (time < MilisecondsSinceEpoch())
                    {
                        time = MilisecondsSinceEpoch() + timeout;
                        W--;
                        H--;

                        if (W < 0)
                            W = 0;
                        if (H < 0)
                            H = 0;

                        if (W == 0 && H == 0)
                        {
                            _Hidden = true;
                            break;
                        }
                    }
                }
            });
        }

        public async Task Show(long timeout)
        {
            long time = timeout;
            time = MilisecondsSinceEpoch() + timeout;
            await Task.Run(() =>
            {
                while (true)
                {
                    if (time < MilisecondsSinceEpoch())
                    {
                        time = MilisecondsSinceEpoch() + timeout;
                        W++;
                        H++;

                        if (W > 0)
                            W = _WRestore;
                        if (H < 0)
                            H = _HRestore;

                        if (W == _WRestore && H == _HRestore)
                        {
                            break;
                        }
                    }
                }
            });
        }

        public event EventHandler<TerminalEvent> onKeyDown;
        public event EventHandler onUpdate;
        public event EventHandler onClick;
        public event EventHandler onMouseDown;
        public event EventHandler onMouseUp;
        public event EventHandler onMouseMove;
        public event EventHandler onMouseEnter;
        public event EventHandler onMouseLeave;

        public void Update()
        {
            if (_resizingTerminal && _hasMouseDown)
            {
                int dw = _rootConsole.Mouse.X - _movingX;
                int dh = _rootConsole.Mouse.Y - _movingY;
                _movingX = _rootConsole.Mouse.X;
                _movingY = _rootConsole.Mouse.Y;
                W += dw;
                H += dh;

                if (W < TitleLength())
                    W = TitleLength();

                if (H < 2)
                    H = 2;

                Console.Resize(W, H);

            }
            else if (_resizingTerminal && !_hasMouseDown)
            {
                _resizingTerminal = false;
            }

            if (_movingTitle && _hasMouseDown)
            {
                int dx = _rootConsole.Mouse.X - _movingX;
                int dy = _rootConsole.Mouse.Y - _movingY;
                X += dx;
                Y += dy;
                _movingX = _rootConsole.Mouse.X;
                _movingY = _rootConsole.Mouse.Y;
            }
            if (_movingTitle && !_hasMouseDown)
            {
                _movingTitle = false;
            }

            if (_hasMouseDown && !_rootConsole.Mouse.LeftPressed && !_rootConsole.Mouse.LeftPressed)
            {
                _hasMouseDown = false;
            }

            if (Parent == null)
            {
                if (_hasMouseOver &&
                    !(
                        (_rootConsole.Mouse.X >= X && _rootConsole.Mouse.X < X + W)
                        && (_rootConsole.Mouse.Y >= Y && _rootConsole.Mouse.Y < Y + H)
                    )
                )
                {
                    _hasMouseOver = false;
                    OnMouseLeave();
                }
            }
            else {
                if (_hasMouseOver &&
                    !(
                        (_rootConsole.Mouse.X >= Parent.X + X && _rootConsole.Mouse.X < Parent.X + X + W)
                        && (_rootConsole.Mouse.Y >= Parent.Y + Y && _rootConsole.Mouse.Y < Parent.Y + Y + H)
                    )
                )
                {
                    _hasMouseOver = false;
                    OnMouseLeave();
                }
            }
            OnUpdate();

            foreach(Terminal child in Children)
            {
                child.Update();
            }
        }

        private void OnUpdate()
        {
            if (onUpdate != null)
            {
                onUpdate(this, EventArgs.Empty);
            }
        }
        public bool OnKeyDown(TerminalEvent e)
        {
            if (onKeyDown != null)
            {
                onKeyDown(this, e);
                return true;
            }
            return false;
        }
        public void OnClick(TerminalEvent e)
        {
            if (onClick != null)
            {
                onClick(this, EventArgs.Empty);
            }
        }
        public bool OnMouseDown(TerminalEvent e)
        {
            _hasMouseDown = true;
            if (onMouseDown != null)
            {
                onMouseDown(this, EventArgs.Empty);
                return true;
            }
            foreach (Terminal child in Children)
            {
                if (e.mousex >= X + child.X && e.mousex < X + child.X + child.W && e.mousey >= Y + child.Y && e.mousey < Y + child.Y + child.H)
                    child.OnMouseDown(e);
            }

            if (Title.Length > 0 && (e.mousex >= X + 2 && e.mousex < X + 6 + Title.Length && e.mousey == Y) && e.mouseLeft && Moveable)
            {
                _movingTitle = true;
                _movingX = e.mousex;
                _movingY = e.mousey;
                return true;
            }
            if (e.mousex == X + W - 1 && e.mousey == Y + H - 1)
            {
                _resizingTerminal = true;
                _movingX = e.mousex;
                _movingY = e.mousey;
                return true;
            }

            return true;
        }
        public bool OnMouseUp(TerminalEvent e)
        {

            string cs = ControlString();
            int clickPos = W - (e.mousex - X) - 1;
            if (e.mousex < X + W - 2 && e.mousex > X + W - cs.Length - 1)
            {
                char clickGlyph = cs[cs.Length - clickPos];
                switch((int)clickGlyph)
                {
                    case 88:
                        Hide(2);
                        break;
                    case 30:
                        if (!_Maximized && !_Minimized)
                        {
                            Maximize(2);
                        }
                        if (_Minimized)
                        {
                            Restore(2);
                        }
                        break;
                    case 31:
                        if (!_Minimized && !_Maximized)
                        {
                            Minimize(2);
                        }
                        if (_Maximized)
                        {
                            Restore(2);
                        }
                        break;
                }
            }

            foreach (Terminal child in Children)
            {
                if (e.mousex >= X + child.X && e.mousex < X + child.X + child.W && e.mousey >= Y + child.Y && e.mousey < Y + child.Y + child.H)
                    child.OnMouseUp(e);
            }

            if (onMouseUp != null && onClick != null && _hasMouseDown)
            {
                onMouseUp(this, EventArgs.Empty);
                onClick(this, EventArgs.Empty);
                _hasMouseDown = false;
            }
            else if (onClick != null && _hasMouseDown)
            {
                onClick(this, EventArgs.Empty);
                _hasMouseDown = false;
            }
            else if (onMouseUp != null)
            {
                onMouseUp(this, EventArgs.Empty);
                _hasMouseDown = false;
            }
            return true;
        }
        public bool OnMouseMove(TerminalEvent e)
        {
            if (!_hasMouseOver)
            {
                _hasMouseOver = true;
                OnMouseEnter(e);
            }

            foreach (Terminal child in Children)
            {
                if (e.mousex >= child.Parent.X + child.X && e.mousex < child.Parent.X + child.X + child.W
                    && e.mousey >= child.Parent.Y + child.Y && e.mousey < child.Parent.Y + child.Y + child.H)
                {
                    child.OnMouseMove(e);
                }
            }

            if (onMouseMove != null)
            {
                onMouseMove(this, EventArgs.Empty);
                return true;
            }
            return false;
        }
        public void OnMouseEnter(TerminalEvent e)
        {
            if (onMouseEnter != null)
            {
                onMouseEnter(this, EventArgs.Empty);
            }
        }
        public void OnMouseLeave()
        {
            if (onMouseLeave != null)
            {
                onMouseLeave(this, EventArgs.Empty);
            }
        }

        public void DrawImage(Image img, int x, int y)
        {
            int imgW = img.Width;
            int imgH = img.Height;

            foreach (Layer l in img.Layers)
            {
                int dx = x;
                int dy = y;

                foreach (SadRex.Cell c in l.Cells)
                {
                    char g = (char)c.Character;
                    Color bg = c.Background;
                    Color fg = c.Foreground;

                    RLColor f = new RLColor(fg.R, fg.G, fg.B);
                    RLColor b = new RLColor(bg.R, bg.G, bg.B);

                    if (!c.IsTransparent())
                        Print(dx, dy, g.ToString(), f, b);

                    dx++;
                    if (dx >= imgW + x)
                    {
                        dx = x;
                        dy++;
                    }
                }
            }
        }

        public void Print(int x, int y, string msg)
        {
            //_console.Print(x, y, msg, TextColour);
            Print(x, y, msg, TextColour, Background);
        }
        public void Print(int x, int y, char msg)
        {
            //_console.Print(x, y, msg.ToString(), TextColour);
            Print(x, y, msg.ToString(), TextColour, Background);
        }
        public void Print(int x, int y, string msg, RLColor colour)
        {
            //_console.Print(x, y, msg, colour);
            Print(x, y, msg, colour, Background);
        }
        public void Print(int x, int y, char msg, RLColor colour, RLColor bgcolour)
        {
            Print(x, y, msg.ToString(), colour, bgcolour);
        }
        public void Print(int x, int y, string msg, RLColor colour, RLColor bgcolour)
        {
            try
            {
                if (msg.Length < W - PaddingX)
                {
                    _console.Print(x, y, msg, colour, bgcolour);
                }
                else
                {
                    string p = msg;
                    int dy = y;
                    while (p.Length > 0)
                    {
                        if (p.Length > W - 1 - PaddingX)
                        {
                            string s = p.Substring(0, W - 1 - PaddingX);
                            _console.Print(x, dy, s, colour, bgcolour);
                            p = p.Substring(W - 1 - PaddingX);
                            dy++;
                        }
                        else
                        {
                            _console.Print(x, dy, p, colour, bgcolour);
                            p = "";
                        }
                    }
                }
            }
            catch { }

        }

        public void RenderSolidBackground(RLColor color, RLColor bg)
        {
            try
            {
                Console.Set(X, Y, W, H, color, bg, 32);
            }
            catch { }
        }
        public void RenderBorder(RLColor color, RLColor bg)
        {
            if (Border == TerminalBorder.Single)
            {
                // Horizontal Borders
                for (int i = 1; i < W - 1; i++)
                {
                    Print(i, 0, (char)196, color, bg);
                    Print(i, H - 1, (char)196, color, bg);
                }
                // Vertical Borders
                for (int i = 1; i < H - 1; i++)
                {
                    Print(0, i, (char)179, color, bg);
                    Print(W - 1, i, (char)179, color, bg);
                }
                // Top Left
                Print(0, 0, (char)218, color, bg);
                // Top Right
                Print(W - 1, 0, (char)191, color, bg);
                // Bottom Left
                Print(0, H - 1, (char)192, color, bg);
                // Bottom Right
                Print(W - 1, H - 1, (char)217, color, bg);
            }
        }
        private string ControlString()
        {
            char _maxGlyph = (char)30;
            char _minGlyph = (char)31;
            string minGlyph = _minGlyph.ToString();
            string maxGlyph = _maxGlyph.ToString();

            string ret = "";

            if (Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Minimize) && Controls.HasFlag(TerminalControls.Maximize) && _Minimized)
            {
                ret = "[" + maxGlyph + "X]";
            }
            else if (Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Minimize) && Controls.HasFlag(TerminalControls.Maximize) && _Maximized)
            {
                ret = "[" + minGlyph + "X]";
            }
            else if (Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Minimize) && Controls.HasFlag(TerminalControls.Maximize))
            {
                ret = "[" + minGlyph + maxGlyph + "X]";
            }

            else if (Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Minimize))
            {
                ret = "[" + minGlyph + "X]";
            }
            else if (Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Minimize) && _Minimized)
            {
                ret = "[" + maxGlyph + "X]";
            }

            else if (Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Maximize))
            {
                ret = "[" + maxGlyph + "X]";
            }
            else if (Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Maximize) && _Maximized)
            {
                ret = "[" + minGlyph + "X]";
            }

            else if (!Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Minimize))
            {
                ret = "[" + minGlyph + "]";
            }
            else if (!Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Minimize) && _Minimized)
            {
                ret = "[" + maxGlyph + "]";
            }

            else if (!Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Maximize))
            {
                ret = "[" + maxGlyph + "]";
            }
            else if (!Controls.HasFlag(TerminalControls.Close) && Controls.HasFlag(TerminalControls.Maximize) && _Maximized)
            {
                ret = "[" + minGlyph + "]";
            }
            else if (Controls.HasFlag(TerminalControls.Close))
            {
                ret = "[X]";
            }

                return ret;
        }
        public void RenderTitle(RLColor color, RLColor bg)
        {
            
            if (Title.Length > 0)
            {
                Print(2, 0, "[ " + Title + " ]", color, bg);
            }

            string controlString = ControlString();
            if (controlString.Length > 0)
                Print(W - controlString.Length - 1, 0, controlString);

        }

        public event EventHandler onRender;
        public void Render()
        {
            _console.Clear();

            if (W > 0 && H > 0)
            {
                int mx = 0; int my = 0;
                if (HAlignment == TerminalAlign.Centre)
                {
                    mx = (W / 2) - (Text.Length / 2) - PaddingX;
                }
                else if (HAlignment == TerminalAlign.Left)
                {
                    mx = PaddingX;
                }
                else if (HAlignment == TerminalAlign.Right)
                {
                    mx = W - 1 - Text.Length - PaddingX;
                }

                if (VAlignment == TerminalAlign.Centre)
                {
                    my = (H / 2) - PaddingY;
                }
                else if (VAlignment == TerminalAlign.Top)
                {
                    my = PaddingY;
                }
                else if (VAlignment == TerminalAlign.Bottom)
                {
                    my = H - 1 - PaddingY;
                }

                if (onRender != null)
                    onRender(this, EventArgs.Empty);

                if (_hasMouseDown && Type == TerminalType.Button)
                {
                    _console.SetBackColor(0, 0, W, H, ConsoleStyle.ButtonPressedBackground);
                    Print(mx, my, Text, ConsoleStyle.ButtonPressedTextColour, ConsoleStyle.ButtonPressedBackground);
                    RenderBorder(ConsoleStyle.ButtonPressedTextColour, ConsoleStyle.ButtonPressedBackground);
                }
                else if (_hasMouseOver && Type == TerminalType.Button)
                {
                    _console.SetBackColor(0, 0, W, H, ConsoleStyle.ButtonHoverBackground);
                    Print(mx, my, Text, ConsoleStyle.ButtonHoverTextColour, ConsoleStyle.ButtonHoverBackground);
                    RenderSolidBackground(ConsoleStyle.ButtonHoverTextColour, ConsoleStyle.ButtonHoverBackground);
                    RenderBorder(ConsoleStyle.ButtonHoverTextColour, ConsoleStyle.ButtonHoverBackground);
                }
                else
                {
                    _console.SetBackColor(0, 0, W, H, ConsoleStyle.Background);
                    Print(mx, my, Text, ConsoleStyle.TextColour, ConsoleStyle.Background);
                    RenderBorder(ConsoleStyle.TextColour, ConsoleStyle.Background);
                    RenderTitle(ConsoleStyle.TextColour, ConsoleStyle.Background);
                }

                if (Resizable)
                {
                    Print(W - 1, H - 1, (char)243);
                }

                foreach (Terminal child in Children)
                {
                    child.Render();
                }
                try
                {
                    if (Parent != null)
                    {
                        RLConsole.Blit(_console, 0, 0, W, H, Parent.Console, X, Y);
                    }
                    else
                    {
                        RLConsole.Blit(_console, 0, 0, W, H, _rootConsole, X, Y);
                    }
                }
                catch { }
            }
        }



    }
}
