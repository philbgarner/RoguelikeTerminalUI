using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;
using UIConsole;

namespace TerminalUI
{
    public interface IActor
    {
        string Name { get; set; }
        int Awareness { get; set; }
    }

    public interface IDrawable
    {
        RLColor Color { get; set; }
        char Symbol { get; set; }
        int X { get; set; }
        int Y { get; set; }

        void Draw(RLConsole console, IMap map);
    }
    public class Actor : IActor, IDrawable
    {
        // IActor
        public string Name { get; set; }
        public int Awareness { get; set; }

        // IDrawable
        public RLColor Color { get; set; }
        public char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public void Draw(RLConsole console, IMap map)
        {
            // Don't draw actors in cells that haven't been explored
            if (!map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            // Only draw the actor with the color and symbol when they are in field-of-view
            if (map.IsInFov(X, Y))
            {
                console.Set(X, Y, Color, Palette.FloorBackgroundFOV, Symbol);
            }
            else
            {
                // When not in field-of-view just draw a normal floor
                console.Set(X, Y, Palette.Floor, Palette.FloorBackground, '.');
            }
        }
    }

    public class Player : Actor
    {
        public Player()
        {
            Awareness = 15;
            Name = "Rogue";
            Color = Game.PlayerColor;
            Symbol = '@';
            X = 10;
            Y = 10;
        }
    }

}
