using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Drawing;

namespace CircleCrossWinApp.GameModel.CircleCross
{
    public enum ChessType
    {
        None = -1,
        Circle = 1,
        Cross = 2
    };

    public enum WinSide
    {
        None = -1,
        Draw = 0,
        Circle = 1,
        Cross = 2,
    };

    public class CircleCrossBoard : IDisposable
    {
        //static readonly ConcurrentDictionary<Guid, CircleCrossBoard> Boards = new ConcurrentDictionary<Guid, CircleCrossBoard>();
        private Guid Id = Guid.NewGuid();
        private List<CircleCrossChess> ChessOnBoards = new List<CircleCrossChess>();
        private static Lazy<CircleCrossBoard> LazyInstance;
        private static object _lock = new object();
        private readonly static ChessType DefaultType = ChessType.Circle;
        public DateTime? CreateTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        private Size BoardSize = new Size(3, 3);
        public bool IsEnd { get; private set; }
        public bool IsStarted { get; private set; }
        public WinSide WinType { get; private set; }

        public Size GetBoardSize
        {
            get
            {
                return BoardSize;
            }
        }

        public ChessType CurrentChessType
        {
            get
            {
                if (IsStarted && !IsEnd)
                {
                    return ChessCount > 0 ? Enum.GetValues(typeof(ChessType)).Cast<ChessType>().First(v => !v.HasFlag(ChessOnBoards[ChessOnBoards.Count - 1].Type) && (int)v > -1) : DefaultType;
                }
                return ChessType.None;
            }
        }


        public int ChessCount
        {
            get
            {
                return ChessOnBoards.Count;
            }
        }

        private CircleCrossBoard()
        {
        }

        private CircleCrossBoard(Size size)
            : this()
        {
            if (BoardSize.IsEmpty || BoardSize.Height != BoardSize.Width && BoardSize.Width < 3)
            {
                throw new Exception("Board size a least 3 x 3");
            }
            BoardSize = size;
        }

        /*
          public static readonly ConcurrentDictionary<Guid, CircleCrossBoard> AllChessBoards {
              get { 
                  return Boards; 
              }
          }
       */

        public static CircleCrossBoard Current
        {
            get
            {
                lock (_lock)
                {
                    if (LazyInstance == null)
                    {
                        LazyInstance = new Lazy<CircleCrossBoard>(() => new CircleCrossBoard());
                    }
                    return LazyInstance.Value;
                }
            }
        }

        public void Start()
        {
            if (IsStarted)
            {
                throw new Exception("The game is started");
            }
            IsStarted = true;
            IsEnd = false;
            CreateTime = DateTime.Now;
        }

        public void Start(CircleCrossChess Chess)
        {
            Start();
            Chess.Step = 1;
            ChessOnBoards.Add(Chess);
        }

        public void Stop()
        {
            Dispose();
        }

        public bool TryAddChess(Point Position)
        {
            if (!IsEnd && IsStarted)
            {
                ChessType Type = ChessOnBoards.Count == 0 ? DefaultType : Enum.GetValues(typeof(ChessType)).Cast<ChessType>().First(v => !v.HasFlag(ChessOnBoards[ChessOnBoards.Count - 1].Type) && (int)v > -1);
                CircleCrossChess Chess = new CircleCrossChess(Position, Type, ChessOnBoards.Count + 1);
                if (ChessOnBoards.Count(v => Point.Equals(v.Position, Position)) == 0)
                {
                    ChessOnBoards.Add(Chess);
                    Calculate();
                    return true;
                }
            }
            return false;
        }

        private void Calculate()
        {
            List<CircleCrossChess> CircleChess = ChessOnBoards.Where(v => v.Type == ChessType.Circle).ToList();
            List<CircleCrossChess> CrossChess = ChessOnBoards.Where(v => v.Type == ChessType.Cross).ToList();

            WinType = GameWin(CircleChess) ? WinSide.Circle : GameWin(CrossChess) ? WinSide.Cross : Math.Pow(BoardSize.Height, 2) == ChessCount ? WinSide.Draw : WinSide.None;
            IsEnd = WinType != WinSide.None;
            if (IsEnd)
            {
                IsStarted = false;
                EndTime = DateTime.Now;
            }
        }

        public bool GameWin(List<CircleCrossChess> Chess)
        {
            for (int i = 1; i <= BoardSize.Width; i++)
            {
                if (Chess.Count(v => v.Position.X == i) == BoardSize.Height)
                {
                    return true;
                }
            }

            for (int i = 1; i <= BoardSize.Height; i++)
            {
                if (Chess.Count(v => v.Position.Y == i) == BoardSize.Width)
                {
                    return true;
                }
            }

            return Chess.Count(v => v.IsValidPlusObliquePoint) == BoardSize.Width || Chess.Count(v => v.IsValidMinusObliquePoint) == BoardSize.Width;
        }

        public void Dispose()
        {
            LazyInstance = null;
            GC.SuppressFinalize(this);
        }
    }

    public class CircleCrossChess
    {
        public Point Position { get; private set; }
        public ChessType Type { get; private set; }
        public int Step { get; internal set; }
        /// <summary>
        /// Left Bottom Corner is 1 , 1,  Right Top Corner is 3, 3
        /// (For 3 x 3)
        /// </summary>
        public bool IsValidPlusObliquePoint
        {
            get
            {
                return Position.X == Position.Y;
            }
        }

        public bool IsValidMinusObliquePoint
        {
            get
            {
                return Position.Y + Position.X == CircleCrossBoard.Current.GetBoardSize.Width + 1;
            }
        }

        private CircleCrossChess()
        {
            Position = new Point();
        }

        public CircleCrossChess(Point Position, ChessType Type)
            : this()
        {
            if (!ValidatChessPosition(Position))
            {
                throw new Exception("Position cannot be negative");
            }
            this.Position = Position;
            this.Type = Type;
        }

        internal CircleCrossChess(Point Position, ChessType Type, int Step)
            : this(Position, Type)
        {
            this.Step = Step;
        }

        private bool ValidatChessPosition(Point Position)
        {
            return Position.X > 0 && Position.Y > 0;
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.Type.HasFlag(((CircleCrossChess)obj).Type);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
