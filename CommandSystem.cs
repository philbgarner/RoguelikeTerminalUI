using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalUI
{
    public enum Direction
    {
        None = 0, DownLeft = 1, Down = 2, DownRight = 3, Right = 4, UpRight = 5, Up = 6, UpLeft = 7, Left = 8, Centre = 9
    }

    public class CommandSystem
    {
        public static bool MovePlayer(Direction direction)
        {
            int x = Game.Player.X;
            int y = Game.Player.Y;
            switch (direction)
            {
                case Direction.Up:
                    y--;
                    break;
                case Direction.UpRight:
                    y--;
                    x++;
                    break;
                case Direction.Right:
                    x++;
                    break;
                case Direction.DownRight:
                    y++;
                    x++;
                    break;
                case Direction.Down:
                    y++;
                    break;
                case Direction.DownLeft:
                    y++;
                    x--;
                    break;
                case Direction.Left:
                    x--;
                    break;
                case Direction.UpLeft:
                    x--;
                    y--;
                    break;
                default:
                    return false;
            }

            if (Game.world.SetActorPosition(Game.Player, x, y))
            {
                return true;
            }

            return false;
        }
    }
}
