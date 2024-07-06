using System;
using System.Collections.Generic;
using System.Linq;

public class PopIt
{
    public const int FIRST = 1;
    public const int SECOND = 2;
    public const int NONE = 0;
    public const int POPPED = 1;
    public const int NORESULT = 0;
    public const int FIRSTWIN = 1;
    public const int SECONDWIN = 2;

    public int[][] board;
    public int turn;

    public PopIt(int[][] board = null, int turn = FIRST)
    {
        if (board == null)
        {
            this.board = new int[6][];
            for (int i = 0; i < 6; i++)
            {
                this.board[i] = new int[6];
            }
        }
        else
        {
            this.board = board;
        }
        this.turn = turn;
    }

    public PopIt MakeMove(int moveRow, int numberOfPops)
    {
        int[][] newBoard = new int[6][];
        for (int i = 0; i < 6; i++)
        {
            newBoard[i] = (int[])this.board[i].Clone();
        }

        for (int i = 0; i < numberOfPops; i++)
        {
            for (int col = 0; col < 6; col++)
            {
                if (newBoard[moveRow][col] == NONE)
                {
                    newBoard[moveRow][col] = POPPED;
                    break;
                }
            }
        }

        int newTurn = 3 - this.turn;
        return new PopIt(newBoard, newTurn);
    }

    public static (int[][], int) StringToArray(string str)
    {
        if (str.Length != 37)
        {
            throw new ArgumentException("Input string must have exactly 37 characters.");
        }

        int[][] array = new int[6][];
        for (int i = 0; i < 6; i++)
        {
            array[i] = new int[6];
            for (int j = 0; j < 6; j++)
            {
                array[i][j] = int.Parse(str[i * 6 + j].ToString());
            }
        }

        int turn = int.Parse(str[36].ToString());
        return (array, turn);
    }

    public static void PrintPopIt(PopIt popIt, bool fancy = false)
    {
        for (int row = 0; row < 6; row++)
        {
            Console.Write($"{row + 1} ");
            for (int col = 0; col < 6; col++)
            {
                if (fancy)
                {
                    if (popIt.board[row][col] == NONE)
                    {
                        Console.Write("🔲");
                    }
                    else
                    {
                        Console.Write("⬛");
                    }
                }
                else
                {
                    if (popIt.board[row][col] == NONE)
                    {
                        Console.Write("- ");
                    }
                    else
                    {
                        Console.Write("X ");
                    }
                }
            }
            Console.WriteLine();
        }

        if (fancy)
        {
            Console.WriteLine("   1 2 3 4 5 6");
        }
        else
        {
            Console.WriteLine("  1 2 3 4 5 6");
        }
        Console.WriteLine();
    }

    public static List<int> MoveGen(PopIt popIt)
    {
        List<int> moves = new List<int>();
        foreach (int[] row in popIt.board)
        {
            moves.Add(row.Count(col => col == NONE));
        }
        return moves;
    }

    public static bool BoardFull(PopIt popIt)
    {
        foreach (int[] row in popIt.board)
        {
            if (row.Any(col => col == NONE))
            {
                return false;
            }
        }
        return true;
    }

    public static int GetResult(PopIt popIt)
    {
        if (BoardFull(popIt))
        {
            return popIt.turn;
        }
        else if (IsCheckMate(popIt))
        {
            return 3 - popIt.turn;
        }
        return NORESULT;
    }

    public static bool IsCheckMate(PopIt popIt)
    {
        int numPops = 0;
        foreach (int[] row in popIt.board)
        {
            foreach (int col in row)
            {
                if (col == NONE)
                {
                    numPops++;
                    if (numPops > 1)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public static int PerfD(PopIt popIt, int depth)
    {
        if (depth == 0)
        {
            return 1;
        }

        int nodes = 0;
        int row = 0;
        foreach (int totalPopsAvail in MoveGen(popIt))
        {
            for (int numberOfPops = 1; numberOfPops <= totalPopsAvail; numberOfPops++)
            {
                PopIt newPopIt = popIt.MakeMove(row, numberOfPops);
                nodes += PerfD(newPopIt, depth - 1);
            }
            row++;
        }
        return nodes;
    }

    public static void PerfT(PopIt popIt, int maxDepth)
    {
        var startTime = DateTime.UtcNow;
        int totalNodes = 0;
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            totalNodes += PerfD(popIt, depth);
            var elapsed = DateTime.UtcNow - startTime;
            Console.WriteLine(
                $"info string perft depth {depth} nodes {totalNodes} time {(int)(elapsed.TotalMilliseconds)} nps {(int)(totalNodes / (elapsed.TotalSeconds + 0.00000001))}"
            );
        }
    }
}

public class PopItEngine
{
    private static int remainingTime = 60000;
    private static int hbtm = 10;
    private static int sbtm = 40;
    private static ulong nodes = 0;
    private static (int, int) rootBestMove = (0, 0);
    private static (int, int)[] TT = new (int, int)[8388608];
    private static DateTime startTime;

    public static int StmEval(PopIt popIt)
    {
        int rows = 0;
        int numRowsWithAtLeast2EmptySpaces = 0;

        for (int row = 0; row < 6; row++)
        {
            int numEmptySpaces = 0;
            int emptyRow = 0;
            for (int col = 0; col < 6; col++)
            {
                if (popIt.board[row][col] == PopIt.NONE && emptyRow == 0)
                {
                    emptyRow = 1;
                    numEmptySpaces += 1;
                }
            }
            rows += emptyRow;
            if (numEmptySpaces >= 2)
            {
                numRowsWithAtLeast2EmptySpaces += 1;
            }
        }

        if (numRowsWithAtLeast2EmptySpaces != 0 && numRowsWithAtLeast2EmptySpaces % 2 == 0)
        {
            return -1000;
        }
        else if (numRowsWithAtLeast2EmptySpaces % 2 != 0)
        {
            return 1000;
        }
        else
        {
            if (rows % 2 == 0)
            {
                return 1000;
            }
            else
            {
                return -1000;
            }
        }
    }

    public static int[] Flatten2DArray(int[][] array)
    {
        return array.SelectMany(row => row).ToArray();
    }

    public static int NegaMax(PopIt popIt, int depth, int ply, int alpha, int beta)
    {
        if (depth > 1 && (DateTime.UtcNow - startTime).TotalMilliseconds > (remainingTime / hbtm))
        {
            throw new TimeoutException();
        }
        if (PopIt.IsCheckMate(popIt) && ply != 0)
        {
            return -30000 + ply;
        }
        if (depth == 0)
        {
            return StmEval(popIt);
        }

        int TTHash = Flatten2DArray(popIt.board).GetHashCode() % 8388608;
        var (TT_row, TT_pops) = TT[TTHash];
        int maxScore = -100000;
        int row = 0;
        bool breakOut = false;
        List<(int, int)> moves = new List<(int, int)>();

        foreach (int totalPopsAvail in PopIt.MoveGen(popIt))
        {
            for (int numberOfPops = 1; numberOfPops <= totalPopsAvail; numberOfPops++)
            {
                moves.Add((row, numberOfPops));
            }
            row++;
        }

        moves.Sort((move1, move2) => ((move1.Item1 != TT_row ? 1 : 0) + (move1.Item2 != TT_pops ? 1 : 0)).CompareTo((move2.Item1 != TT_row ? 1 : 0) + (move2.Item2 != TT_pops ? 1 : 0)));

        foreach (var move in moves)
        {
            (row, int numberOfPops

) = move;
            nodes++;
            PopIt newPopIt = popIt.MakeMove(row, numberOfPops);
            int score = -NegaMax(newPopIt, depth - 1, ply + 1, -beta, -alpha);

            if (score > maxScore)
            {
                maxScore = score;
                TT[TTHash] = (row, numberOfPops);
                if (score > alpha)
                {
                    alpha = score;
                    if (score >= beta)
                    {
                        breakOut = true;
                        break;
                    }
                }
                if (ply == 0)
                {
                    rootBestMove = (row, numberOfPops);
                }
            }

            if (breakOut)
            {
                break;
            }
        }

        return maxScore;
    }

    public static void GetBestMove(PopIt popIt)
    {
        nodes = 0;
        rootBestMove = (0, 0);
        startTime = DateTime.UtcNow;

        try
        {
            for (int i = 1; i < 256; i++)
            {
                int score = NegaMax(popIt, i, 0, -100000, 100000);
                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;

                var (bestRow, bestNumPops) = rootBestMove;

                Console.WriteLine($"info depth {i} score {score} nodes {nodes} time {(int)elapsed} nps {(int)(1000 * nodes / (elapsed + 0.0000000001))} pv {bestRow} {bestNumPops}");

                if (elapsed > (remainingTime / sbtm))
                {
                    break;
                }
            }
        }
        catch (TimeoutException)
        {
            return;
        }
    }

    public static void Bench(PopIt popIt, int maxDepth)
    {
        nodes = 0;
        var startTime = DateTime.UtcNow;
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            int score = NegaMax(popIt, depth, 0, -100000, 100000);
            var elapsed = DateTime.UtcNow - startTime;
            var (bestRow, bestNumPops) = rootBestMove;
            Console.WriteLine($"info depth {depth} score {score} nodes {nodes} time {(int)(1000 * elapsed.TotalMilliseconds)} nps {(int)(nodes / (elapsed.TotalSeconds + 0.0000000001))} pv {bestRow} {bestNumPops}");
        }
    }

    public static (double, double) ParseParameters(string line)
    {
        const double DEFAULT_TIME1 = 60000;
        const double DEFAULT_TIME2 = 60000;
        string[] parameters = line.Split(' ').Skip(1).ToArray();
        double time1 = DEFAULT_TIME1, time2 = DEFAULT_TIME2;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i] == "infinite")
            {
                time1 = 1000000000D;
                time2 = 1000000000D;
            }
            else if (parameters[i] == "time1" && i + 1 < parameters.Length)
            {
                time1 = double.Parse(parameters[i + 1]);
            }
            else if (parameters[i] == "time2" && i + 1 < parameters.Length)
            {
                time2 = double.Parse(parameters[i + 1]);
            }
        }

        return (time1, time2);
    }

    public static void Uci()
    {
        PopIt pop = new PopIt();
        while (true)
        {
            string line = Console.ReadLine();
            if (line == "upi")
            {
                Console.WriteLine("id name Marburg");
                Console.WriteLine("id author Dragjon");
                Console.WriteLine("upiok");
            }
            else if (line == "isready")
            {
                Console.WriteLine("readyok");
            }
            else if (line.StartsWith("position"))
            {
                string posStr = line.Split(' ')[1];
                var (pos, turn) = PopIt.StringToArray(posStr);
                pop = new PopIt(pos, turn);
            }
            else if (line.StartsWith("go"))
            {
                var (time1, time2) = ParseParameters(line);
                remainingTime = (pop.turn == PopIt.FIRST) ? (int)time1 : (int)time2;
                startTime = DateTime.UtcNow;
                GetBestMove(pop);
                var (bestRow, bestNumPops) = rootBestMove;
                Console.WriteLine($"bestmove {bestRow} {bestNumPops}");
            }
        }
    }

    public static void Main(string[] args)
    {
        Uci();
    }
}
