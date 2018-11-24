using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RLNET;
using UIConsole;
using SadRex;
using System.IO;

namespace TerminalUI
{
    class Game
    {
        private static readonly int _width = 120;
        private static readonly int _height = 80;

        public static WorldMap world { get; private set; }

        private static RLRootConsole RootConsole;
        private static TerminalCollection terminals;

        public static RLColor PlayerColor = RLColor.White;
        public static Player Player { get; private set; }

        private static CommandSystem commands;

        public static int oldMouseX;
        public static int oldMouseY;

        public static void MessageBox(string message, string title, TerminalControls controls)
        {
            terminals.Add(new Terminal(_width / 2 - 15, _height / 2 - 14, 30, 28, UIColours.CoolDark, RootConsole));
            string guid = terminals.Get(terminals.Count - 1).guid;
            terminals.Get(guid).Text = message;
            terminals.Get(guid).Title = title;
            terminals.Get(guid).Border = TerminalBorder.Single;
            terminals.Get(guid).Resizable = false;
            terminals.Get(guid).PaddingY = 2;
            terminals.Get(guid).PaddingX = 2;
            terminals.Get(guid).Controls = controls;

            Terminal btn = Button("OK", 2, 23, 26, 3);
            btn.Border = TerminalBorder.Single;
            btn.onClick += (object sender, EventArgs e) =>
                {
                    terminals.Get(guid).Hide(2);
                };

            terminals.Get(guid).AddChild(btn);
            
        }
        public static Terminal Button(string caption, int x, int y, int w, int h)
        {
            Terminal btn = new Terminal(x, y, w, h, UIColours.CoolDark, RootConsole);
            btn.Type = TerminalType.Button;
            btn.Text = caption;
            btn.Border = TerminalBorder.Single;
            
            terminals.Add(btn);
            return btn;
        }

        static void Main(string[] args)
        {

            string fontFile = "terminal8x8.png";
            string consoleTitle = "TerminalUI";

            RootConsole = new RLRootConsole(fontFile, _width, _height, 8, 8, 1f, consoleTitle);
            terminals = new TerminalCollection(RootConsole);

            commands = new CommandSystem();

            RootConsole.Update += OnRootConsoleUpdate;
            RootConsole.Render += OnRootConsoleRender;

            Player = new Player();

            MapGenerator mapGenerator = new MapGenerator(_width, _height);
            world = mapGenerator.CreateMap();
            world.UpdatePlayerFieldOfView();

            MessageBox("This is a message box.", "Message Box", TerminalControls.Close);

            RootConsole.Run();
            
        }

        private static void OnExit(object sender, EventArgs e)
        {
            RootConsole.Close();
        }
        
        private static void OnMapUpdate(object sender, EventArgs e)
        {
            bool didPlayerAct = false;
            RLKeyPress keypress = null;// = RootConsole.Keyboard.GetKeyPress();
            if (keypress != null)
            {
                if (keypress.Key == RLKey.Up)
                {
                    didPlayerAct = CommandSystem.MovePlayer(Direction.Up);
                }
                if (keypress.Key == RLKey.Down)
                {
                    didPlayerAct = CommandSystem.MovePlayer(Direction.Down);
                }
                if (keypress.Key == RLKey.Left)
                {
                    didPlayerAct = CommandSystem.MovePlayer(Direction.Left);
                }
                if (keypress.Key == RLKey.Right)
                {
                    didPlayerAct = CommandSystem.MovePlayer(Direction.Right);
                }
                if (keypress.Key == RLKey.Escape)
                {
                    RootConsole.Close();
                }
            }

        }

        private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e)
        {
            // Update Terminals
            terminals.Update();
        }

        // Event handler for RLNET's Render event
        private static void OnRootConsoleRender(object sender, UpdateEventArgs e)
        {

            RootConsole.Clear();

            //world.Draw(MapConsole.Console);

            terminals.Draw();

            // Do mouse stuff last so it appears on top.
            int mouseX = RootConsole.Mouse.X;
            int mouseY = RootConsole.Mouse.Y;

            oldMouseX = mouseX;
            oldMouseY = mouseY;

            RootConsole.Draw();
        }
    }
}
