using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace CircleCrossWinApp.Model.CircleCross
{
    public enum ChessType
    {
        Circle,
        Cross
    };

    public enum Position
    {
        None = -1,
        BottomLeft = 4,
        Bottom = 5,
        BottomRight = 6,
        Left = 7,
        Center = 8,
        Right = 9,
        TopLeft = 10,
        Top = 11,
        TopRight = 12
    };

    public class CircleCrossBoard
    {
        //static readonly ConcurrentDictionary<Guid, CircleCrossBoard> Boards = new ConcurrentDictionary<Guid, CircleCrossBoard>();
        List<CircleCrossChess> ChessOnBoards = new List<CircleCrossChess>();
        static Lazy<CircleCrossBoard> LazyInstance;
        static object _lock = new object();
        readonly static ChessType DefaultType = ChessType.Circle;
        Guid Id = Guid.NewGuid();
        DateTime CreateTime = DateTime.Now;
        DateTime EndTime;
        bool IsEnded;

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

        public bool Started
        {
            get
            {
                return IsEnded;
            }
        }

        public void Start()
        {
            if (IsEnded)
            {
                IsEnded = false;
                throw new Exception("The game is started");
            }
        }

        public void Start(CircleCrossChess Chess)
        {
            this.Start();
            Chess.Step = 0;
            ChessOnBoards.Add(Chess);
        }

        public bool TryAddChess(int xPos, int yPos)
        {
            if (this.IsEnded)
            {
                ChessType Type = ChessOnBoards.Count == 0 ? DefaultType : Enum.GetValues(typeof(ChessType)).Cast<ChessType>().First(v => !v.HasFlag(ChessOnBoards[ChessOnBoards.Count - 1].Type));
                CircleCrossChess Chess = new CircleCrossChess(xPos, yPos, Type);
                Chess.Step = ChessOnBoards.Count + 1;
                ChessOnBoards.Add(Chess);
                return true;
            }
            return false;
        }

        public int ChessCount
        {
            get
            {
                return ChessOnBoards.Count;
            }
        }
    }

    public class CircleCrossChess
    {
        public int xPos { get; internal set; }
        public int yPos { get; internal set; }
        public ChessType Type { get; internal set; }
        public int Step { get; internal set; }

        public CircleCrossChess(int xPosition, int yPosition, ChessType chessType)
        {
            this.xPos = xPosition;
            this.yPos = yPosition;
            this.Type = chessType;
        }

        public Position PositionOf(CircleCrossChess chess)
        {
            Position resultPosition = Position.None;
            Array positions = Enum.GetValues(typeof(Position));
            foreach (Position pos in positions)
            {
                if ((int)pos == (this.xPos.CompareTo(chess.xPos) + 2) * 1 + (this.yPos.CompareTo(chess.yPos) + 2) * 3)
                {
                    resultPosition = pos;
                    break;
                }
            }
            return resultPosition;
        }

        public override string ToString()
        {
            return this.Type.ToString();
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
