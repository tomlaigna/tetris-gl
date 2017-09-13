using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsTest
{
    public class TetrisConsoleView
    {
        public static void DrawTetrisToConsole(Tetris tetris)
        {
            Console.Clear();
            var table = (Tetris.ESquare[,])tetris.Table.Clone();
            if (tetris.CurrentBlock != null)
            {
                for (int j = 0; j < tetris.CurrentBlock.Item.Height; ++j)
                    for (int i = 0; i < tetris.CurrentBlock.Item.Width; ++i) 
                    {
                        table[tetris.CurrentBlock.Y + j, tetris.CurrentBlock.X + i] = table[tetris.CurrentBlock.Y + j, tetris.CurrentBlock.X + i] == Tetris.ESquare.EMPTY 
                            ? tetris.CurrentBlock.Item.Table[j,i]
                            : Tetris.ESquare.FILLED;
                    }
            }

            
            for (int j = 0; j < tetris.TableHeight; ++j)
            {
                for (int i = 0; i < tetris.TableWidth; ++i)
                {
                    if (table[j, i] == Tetris.ESquare.EMPTY)
                    {
                        Console.Write('.');
                    }
                    else
                    {
                        Console.Write('0');
                    }
                }

                Console.WriteLine("");
            }            
        }
    }
}
