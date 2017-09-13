using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsTest
{
    public class TetrisGame
    {
        private Random Generator = new Random((int)DateTime.UtcNow.Ticks);
        public Tetris Data { get; private set; }

        public TetrisGame(int width, int height)
        {
            Data = new Tetris(width, height);
        }

        public void Reset()
        {
            Data = new Tetris(Data.TableWidth, Data.TableHeight);
            GenerateNextRandomBlock();
        }

        public void Tick()
        {
            var currentBlock = Data.CurrentBlock;
            if (currentBlock != null)
            {
                // Move block down by 1.
                var newBlock = new Tetris.PositionedBlock(currentBlock.Item, currentBlock.X, currentBlock.Y + 1);

                // Detect if the current block would collide with anything.
                bool collision = CheckForCollision(newBlock);
                if (collision)
                {
                    // collision, flatten the current block to table and remove it.
                    for (int j = 0; j < Data.CurrentBlock.Item.Height; ++j)
                        for (int i = 0; i < Data.CurrentBlock.Item.Width; ++i)
                        {
                            Data.Table[Data.CurrentBlock.Y + j, Data.CurrentBlock.X + i] = Data.Table[Data.CurrentBlock.Y + j, Data.CurrentBlock.X + i] == Tetris.ESquare.EMPTY 
                                ? Data.CurrentBlock.Item.Table[j, i]
                                : Tetris.ESquare.FILLED;
                        }

                    Data.CurrentBlock = null;
                    
                    // Check if any rows are now filled.
                    for (int r = 0; r < Data.TableHeight; ++r)
                    {
                        bool rowFilled = true;
                        for (int c = 0; c < Data.TableWidth; ++c)
                        {
                            if (Data.Table[r, c] == Tetris.ESquare.EMPTY)
                            {
                                rowFilled = false;
                                break;
                            }
                        }

                        if (rowFilled)
                        {

                            if (r == 0) // clear first row.
                            {
                                var zero = new Tetris.ESquare[Data.TableWidth];
                                for (int i = 0; i < Data.TableWidth; ++i)
                                    zero[i] = Tetris.ESquare.EMPTY;
                                Array.Copy(Data.Table, r * Data.TableWidth, zero, 0, Data.TableWidth);
                            }
                            else // move all rows above this row down by 1.
                                for (int r2 = r - 1; r2 >= 0; --r2)
                                    Array.Copy(Data.Table, r2 * Data.TableWidth, Data.Table, (r2 + 1) * Data.TableWidth, Data.TableWidth);
                        }
                    }
                }
                else
                {
                    Data.CurrentBlock = newBlock;
                }
            }
            else
            {
                // no current block, so spawn a new one.
                var b = new Tetris.Block(Data.NextBlock);
                Data.CurrentBlock = new Tetris.PositionedBlock(b, Data.TableWidth / 2 - b.Table.GetLength(0) / 2, 0);

                // check if the new block collides.
                bool collision = CheckForCollision(Data.CurrentBlock);
                if (collision)
                {
                    // game over.
                    Reset();
                }

                GenerateNextRandomBlock();
            }
        }

        public enum EInputs
        {
            ACCELERATE,
            SHIFT_LEFT,
            SHIFT_RIGHT,
            ROTATE_CW,
            ROTATE_ACW,
        }
        public void Input(EInputs input)
        {
            var currentBlock = Data.CurrentBlock;
            switch (input)
            {
                case EInputs.ACCELERATE:
                    if (currentBlock != null)
                    {
                        var newBlock = new Tetris.PositionedBlock(Data.CurrentBlock.Item, currentBlock.X, currentBlock.Y + 1);
                        // check if the new block would collide with anything.
                        bool collision = CheckForCollision(newBlock);
                        if (collision)
                        {
                            // collision, do nothing.
                        }
                        else
                        {
                            Data.CurrentBlock = newBlock;
                        }
                    }
                    break;

                case EInputs.SHIFT_LEFT:
                    if (currentBlock != null)
                    {
                        var newBlock = new Tetris.PositionedBlock(Data.CurrentBlock.Item, currentBlock.X - 1, currentBlock.Y);
                        // check if the new block would collide with anything.
                        bool collision = CheckForCollision(newBlock);
                        if (collision)
                        {
                            // collision, do nothing.
                        }
                        else
                        {
                            Data.CurrentBlock = newBlock;
                        }
                    }
                    break;

                case EInputs.SHIFT_RIGHT:
                    if (currentBlock != null)
                    {
                        var newBlock = new Tetris.PositionedBlock(Data.CurrentBlock.Item, currentBlock.X + 1, currentBlock.Y);
                        // check if the new block would collide with anything.
                        bool collision = CheckForCollision(newBlock);
                        if (collision)
                        {
                            // collision, do nothing.
                        }
                        else
                        {
                            Data.CurrentBlock = newBlock;
                        }
                    }
                    break;

                case EInputs.ROTATE_CW:
                    if (currentBlock != null)
                    {
                        var newBlock = new Tetris.PositionedBlock(Data.CurrentBlock.Item.Rotate(clockwise: true), currentBlock.X, currentBlock.Y);
                        // check if the new block would collide with anything.
                        bool collision = CheckForCollision(newBlock);
                        if (collision)
                        {
                            // collision, do nothing.
                        }
                        else
                        {
                            Data.CurrentBlock = newBlock;
                        }
                    }
                    break;
                case EInputs.ROTATE_ACW:
                    break;
            }
        }

        private bool CheckForCollision(Tetris.PositionedBlock block)
        {
            // first check if it is out of bounds.
            if (block.X < 0 || block.X + block.Item.Width > Data.TableWidth)
                return true;
            if (block.Y + block.Item.Height > Data.TableHeight)
                return true;

            // Logically flatten the gametable and block.
            for (int i = 0; i < block.Item.Width; ++i)
                for (int j = 0; j < block.Item.Height; ++j)
                {
                    if (Data.Table[block.Y + j, block.X + i] == Tetris.ESquare.FILLED
                        && block.Item.Table[j, i] == Tetris.ESquare.FILLED)
                        return true;
                }

            return false;
        }

        private void GenerateNextRandomBlock()
        {
            int rand = Generator.Next((int)Tetris.Block.EType.NUMBER_OF_BLOCKS);
            Data.NextBlock = (Tetris.Block.EType)rand;
        }
    }

    public class Tetris
    {
        public enum ESquare
        {
            EMPTY,
            FILLED
        }

        public class PositionedBlock
        {
            public Block Item { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public PositionedBlock(Block item, int x, int y)
            {
                Item = item;
                X = x;
                Y = y;
            }
        }

        public class Block
        {
            public enum EType
            {
                LINE,
                L,
                L_2,
                SQUARE,
                WIGGLY,
                WIGGLY_2,
                THREE,
                NUMBER_OF_BLOCKS
            }

            public ESquare[,] Table { get; private set; }
            public int Width { get { return Table.GetLength(1); } }
            public int Height { get { return Table.GetLength(0); } }

            public Block(ESquare[,] table)
            {
                Table = table;
            }

            public Block(EType type)
            {
                switch (type)
                {
                    default:
                        throw new ArgumentException();

                    case EType.LINE:
                        Table = new ESquare[,] 
                        { 
                            { ESquare.FILLED }, 
                            { ESquare.FILLED }, 
                            { ESquare.FILLED }, 
                            { ESquare.FILLED }, 
                        };
                        break;
                    case EType.L:
                        Table = new ESquare[,] 
                        { 
                            { ESquare.FILLED, ESquare.EMPTY }, 
                            { ESquare.FILLED, ESquare.EMPTY }, 
                            { ESquare.FILLED, ESquare.FILLED },
                        };
                        break;
                    case EType.L_2:
                        Table = new ESquare[,] 
                        { 
                            { ESquare.EMPTY, ESquare.FILLED }, 
                            { ESquare.EMPTY, ESquare.FILLED }, 
                            { ESquare.FILLED, ESquare.FILLED },
                        };
                        break;
                    case EType.SQUARE:
                        Table = new ESquare[,] 
                        { 
                            { ESquare.FILLED, ESquare.FILLED },
                            { ESquare.FILLED, ESquare.FILLED },
                        };
                        break;
                    case EType.WIGGLY:
                        Table = new ESquare[,] 
                        { 
                            { ESquare.FILLED, ESquare.EMPTY }, 
                            { ESquare.FILLED, ESquare.FILLED }, 
                            { ESquare.EMPTY, ESquare.FILLED },
                        };
                        break;
                    case EType.WIGGLY_2:
                        Table = new ESquare[,] 
                        { 
                            { ESquare.EMPTY, ESquare.FILLED }, 
                            { ESquare.FILLED, ESquare.FILLED }, 
                            { ESquare.FILLED, ESquare.EMPTY },
                        };
                        break;
                    case EType.THREE:
                        Table = new ESquare[,] 
                        { 
                            { ESquare.EMPTY, ESquare.FILLED, ESquare.EMPTY }, 
                            { ESquare.FILLED, ESquare.FILLED, ESquare.FILLED }, 
                        };
                        break;
                }

                Table = RotateTableImpl(Table, clockwise: true);
            }

            public Block Rotate(bool clockwise)
            {
                return new Block(RotateTableImpl(Table, clockwise));
            }

            private static ESquare[,] RotateTableImpl(ESquare[,] src, bool clockwise)
            {
                int w = src.GetLength(0);
                int h = src.GetLength(1);

                var table = new ESquare[h, w];

                // CW -> leftmost column becomes top row
                for (int x = 0; x < w; ++x)
                {
                    for (int y = 0; y < h; ++y)
                    {
                        table[y, x] = clockwise
                            ? src[w - 1 - x, y]
                            : src[x, h - 1 - y];
                    }
                }

                return table;
            }
        }

        public ESquare[,] Table { get; private set; }
        public Block.EType NextBlock { get; set; }
        public PositionedBlock CurrentBlock { get; set; }

        public int TableWidth { get; private set; }
        public int TableHeight { get; private set; }
        
        public Tetris(int tableWidth, int tableHeight)
        {
            TableWidth = tableWidth;
            TableHeight = tableHeight;

            Table = new ESquare[tableHeight, tableWidth];
            for (int i = 0; i < tableWidth; ++i)
                for (int j = 0; j < tableHeight; ++j)
                    Table[j, i] = ESquare.EMPTY;
        }
    }
}
