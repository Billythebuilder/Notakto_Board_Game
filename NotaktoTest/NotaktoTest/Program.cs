/*
Requirements of Game Design 
To make this as simple as possible for grading purposes, I have broken down the requirements into numbers. These numbers can be used via ctrl+f command within Visual Studio Code (VS) to assist in the grading process. I have also included a test case for each requirement and a screenshot of the corresponding result in VS. You will find this is larger than requirement (1/2 page) but I feel it important to discuss. These screenshots are attached in the end of the document as an appendix. 

The following requirements have been completed and tested:
R1) Your system should cater for different modes of play including: 
    R1A) Human vs. Human
    R1B) Human vs Computer
    R1C) With human players, system must check validity of moves
    R1D) With computer players, system can randomly select a valid move

R2) Games can be played from start to finish 
    R2A) …and should be able to be saved and restored from any state of play (save file)
    R2B) A game should be re-playable from any position after being loaded from a save  file

R3) During a game, all moves made by both (Human) players should be
	R3A) … undo-able
	R3B) … and redo-able

R4) Provide in game help system to assist users with available commands
	R4A) Provide examples for users

The following requirements have been tested and failed (not included in design):
None

*/	


//User Interface Class

// R1C & R3A & R3B
public class GameBoard
{
    public List<List<GamePiece>> Boards { get; private set; }
    public List<bool> ActiveBoards { get; private set; }
    private const int BoardSize = 3;
    public static int NumBoards = 3;

    public GameBoard()
    {
        Boards = new List<List<GamePiece>>();
        ActiveBoards = new List<bool>();

        for (int b = 0; b < NumBoards; b++)
        {
            Boards.Add(new List<GamePiece>());
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                Boards[b].Add(new GamePiece());
            }
            ActiveBoards.Add(true);
        }
    }

    public bool IsValidMove(int boardIndex, int row, int col)
    {
        return ActiveBoards[boardIndex] && Boards[boardIndex][row * BoardSize + col].IsEmpty;
    }

    public void MakeMove(int boardIndex, int row, int col)
    {
        if (IsValidMove(boardIndex, row, col))
        {
            Boards[boardIndex][row * BoardSize + col].Mark();
            CheckBoardState(boardIndex);
        }
    }

    public void UndoMove(int boardIndex, int row, int col)
    {
        Boards[boardIndex][row * BoardSize + col].Clear();
        ActiveBoards[boardIndex] = true; // Reactivate the board
    }

    private void CheckBoardState(int boardIndex)
    {
        bool isThreeInARow = false;
        for (int i = 0; i < BoardSize; i++)
        {
            if (CheckLine(boardIndex, i * BoardSize, 1) || // Check rows
                CheckLine(boardIndex, i, BoardSize))       // Check columns
            {
                isThreeInARow = true;
                break;
            }
        }

        if (!isThreeInARow)
        {
            if (CheckLine(boardIndex, 0, BoardSize + 1) || // Top-left to bottom-right diagonal
                CheckLine(boardIndex, BoardSize - 1, BoardSize - 1)) // Top-right to bottom-left diagonal
            {
                isThreeInARow = true;
            }
        }

        if (isThreeInARow)
        {
            ActiveBoards[boardIndex] = false;
            Console.WriteLine($"Board {boardIndex} is deactivated due to three in a row.");
        }
        else if (Boards[boardIndex].All(p => !p.IsEmpty))
        {
            ActiveBoards[boardIndex] = false;
            Console.WriteLine($"Board {boardIndex} is a tie.");
        }
    }

    private bool CheckLine(int boardIndex, int start, int step)
    {
        if (Boards[boardIndex][start].IsEmpty)
        {
            return false;
        }

        for (int i = 1; i < BoardSize; i++)
        {
            if (Boards[boardIndex][start + i * step].IsEmpty)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsGameOver()
    {
        return ActiveBoards.All(b => !b) || Boards.All(b => b.All(p => !p.IsEmpty));
    }

    public override string ToString()
    {
        string result = "";
        for (int b = 0; b < NumBoards; b++)
        {
            Console.WriteLine($"\n Board {b} " + (ActiveBoards[b] ? "(Active)" : "(Inactive)"));
            Console.WriteLine("╔═════════════════╗");
            for (int i = 0; i < BoardSize; i++)
            {
                Console.Write("║ ");
                for (int j = 0; j < BoardSize; j++)
                {
                    string symbol = Boards[b][i * BoardSize + j].ToString();
                    Console.Write($" {symbol} ");
                    if (j < BoardSize - 1)
                        Console.Write(" │ ");
                }
                Console.WriteLine(" ║");
                if (i < BoardSize - 1)
                    Console.WriteLine("║─────┼─────┼─────║");
            }
            Console.WriteLine("╚═════════════════╝");
        }
        return result;
    }
}
//R1A & R1B
public class GameMode
{
    //Choose game mode, Human vs Human || Human vs Comp
    // public enum Mode { HumanVsHuman, HumanVsComp }
    public bool IsHumanVsHuman { get; private set; }

    public GameMode()
    {
        Console.WriteLine("Welcome To Notakto!");
        Console.WriteLine("Written by William Stark");
        Console.WriteLine("N11947446");
        Console.WriteLine("Choose which game mode you would like to play:");
        Console.WriteLine("1. Human vs Human: Go head to head against a friend");
        Console.WriteLine("2. Human vs Computer: Have a go against an AI opponent");

        int choice;
        //invalid input
        while (!int.TryParse(Console.ReadLine(), out choice) || (choice != 1 && choice != 2))
        {
            Console.WriteLine("Invalid input. Please enter 1 or 2.");
        }

        IsHumanVsHuman = choice == 1;
    }
}

// R1A & R1B
public abstract class Player
{
    public string Name { get; protected set; }

    protected Player(string name)
    {
        Name = name;
    }

    public abstract (int boardIndex, int row, int col) GetMove(GameBoard board);
}
// R1A
public class HumanPlayer : Player
{
    public HumanPlayer(string name) : base(name) { }

    public override (int boardIndex, int row, int col) GetMove(GameBoard board)
    {
        return (-1, -1, -1);
    }
}
// R1B
public class ComputerPlayer : Player
{
    public ComputerPlayer(string name) : base(name) { }

    public override (int boardIndex, int row, int col) GetMove(GameBoard board)
    {
        Random rand = new Random();
        List<(int boardIndex, int row, int col)> validMoves = new List<(int boardIndex, int row, int col)>();

        for (int b = 0; b < GameBoard.NumBoards; b++)
        {
            if (!board.ActiveBoards[b]) continue;

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (board.IsValidMove(b, r, c))
                    {
                        validMoves.Add((b, r, c));
                    }
                }
            }
        }

        if (validMoves.Count > 0)
        {
            return validMoves[rand.Next(validMoves.Count)];
        }

        return (-1, -1, -1);
    }
}

public class GamePiece
{
    public bool IsEmpty { get; private set; }
    public string Symbol { get; private set; }
    public int BoardIndex { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }

    // Both players input 'X' as game piece. 
    // Show different state of board
    // Mark() for piece placed, Clear() for empty

    public GamePiece()
    {
        IsEmpty = true;
        Symbol = "-";
    }

    public void Mark()
    {
        IsEmpty = false;
        Symbol = "X";
    }

    public void Clear()
    {
        IsEmpty = true;
        Symbol = "-";
    }

    public override string ToString()
    {
        return Symbol;
    }
}
//R3A & R3B
public class MoveHistory
{
    private List<GamePiece> moves;
    private int currentMoveIndex;

    public MoveHistory()
    {
        moves = new List<GamePiece>();
        currentMoveIndex = -1;
    }

    // add moves to current index
    // allows undo/redo ops
    public void AddMove(GamePiece move)
    {
        if (currentMoveIndex < moves.Count - 1)
        {
            moves.RemoveRange(currentMoveIndex + 1, moves.Count - currentMoveIndex - 1);
        }
        moves.Add(move);
        currentMoveIndex++;
    }

    public List<GamePiece> GetMoves()
    {
        return new List<GamePiece>(moves);
    }

    //must place move to undo
    //when undo index --
    public GamePiece Undo()
    {
        if (currentMoveIndex >= 0)
        {
            return moves[currentMoveIndex--];
        }
        return null;
    }

    public GamePiece Redo()
    {
        if (currentMoveIndex < moves.Count - 1)
        {
            return moves[++currentMoveIndex];
        }
        return null;
    }

    public bool CanUndo() => currentMoveIndex >= 0;
    public bool CanRedo() => currentMoveIndex < moves.Count - 1;
}
//R3A & R3B & R2A & R2B
public class Game
{
    private GameBoard board;
    private List<Player> players;
    private MoveHistory moveHistory;
    private HelpSystem helpSystem;
    private int currentPlayerIndex;

    public Game()
    {
        board = new GameBoard();
        moveHistory = new MoveHistory();
        helpSystem = new HelpSystem();
        currentPlayerIndex = 0;

        GameMode gameMode = new GameMode();
        players = new List<Player>
        {
            new HumanPlayer("Player 1"),
            gameMode.IsHumanVsHuman ? (Player)new HumanPlayer("Player 2") : new ComputerPlayer("Computer")
        };
    }
    //Serialize (Save) and Deserialize (Load)
    //Save board state, current player index, move history, and save as .txt file
    public string SerializeGameState()
        {
        string gameState = $"{currentPlayerIndex}\n";

        // Save (Serialize)
        for (int b = 0; b < GameBoard.NumBoards; b++)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    gameState += board.Boards[b][i * 3 + j].ToString() + " ";
                }
                gameState = gameState.TrimEnd() + "\n";
            }
        }

        // Move history save
        gameState += "\n";
        foreach (var move in moveHistory.GetMoves())
        {
            gameState += $"{move.BoardIndex} {move.Row} {move.Col}\n";
        }

        return gameState;
        }
    //Convert txt to game objects
    //Player index, board states, and move history need to be loaded
    //Parse txt input to objects
    public void DeserializeGameState(string gameState)
        {
        string[] lines = gameState.Split('\n');

        if (lines.Length < 1 + 4 * GameBoard.NumBoards)
        {
            throw new InvalidOperationException("Invalid game state data.");
        }

        currentPlayerIndex = int.Parse(lines[0]);

        board = new GameBoard();

        int lineIndex = 1;
        for (int b = 0; b < GameBoard.NumBoards; b++)
        {
            for (int i = 0; i < 3; i++)
            {
                string[] cells = lines[lineIndex++].Split(' ');

                for (int j = 0; j < 3; j++)
                {
                    if (cells[j] != "-")
                    {
                        board.MakeMove(b, i, j);
                    }
                }
            }
        }

        moveHistory = new MoveHistory();
        for (int i = lineIndex; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] moveParts = lines[i].Split(' ');

            if (moveParts.Length != 3)
            {
                throw new InvalidOperationException($"Invalid move data at line {i}.");
            }

            GamePiece move = new GamePiece
            {
                BoardIndex = int.Parse(moveParts[0]),
                Row = int.Parse(moveParts[1]),
                Col = int.Parse(moveParts[2])
            };
            move.Mark();
            moveHistory.AddMove(move);
        }
        }

    public void Start()
    {
        helpSystem.DisplayRules();
        helpSystem.DisplayCommands();

        while (true)
        {
            Console.WriteLine(board.ToString());
            Player currentPlayer = players[currentPlayerIndex];

            Console.WriteLine($"{currentPlayer.Name}'s turn");

            string input = GetPlayerInput(currentPlayer);
            if (input.ToLower() == "s")
            {
                string gameState = SerializeGameState();
                File.WriteAllText("saved_game.txt", gameState);
                Console.WriteLine("Game saved.");
                continue;
            }
            if (input.ToLower() == "l")
            {
                if (File.Exists("saved_game.txt"))
                {
                    string gameState = File.ReadAllText("saved_game.txt");
                    DeserializeGameState(gameState);
                    Console.WriteLine("Game loaded.");
                }
                else
                {
                    Console.WriteLine("No saved game found.");
                }
                continue;
            }

            if (input.ToLower() == "u")
            {
                if (moveHistory.CanUndo())
                {
                    GamePiece undoneMove = moveHistory.Undo();
                    board.UndoMove(undoneMove.BoardIndex, undoneMove.Row, undoneMove.Col);
                    currentPlayerIndex = (currentPlayerIndex - 1 + players.Count) % players.Count;
                    Console.WriteLine("Move undone.");
                }
                else
                {
                    Console.WriteLine("Cannot undo. No more moves to undo.");
                }
                continue;
            }

            if (input.ToLower() == "r")
            {
                if (moveHistory.CanRedo())
                {
                    GamePiece redoneMove = moveHistory.Redo();
                    if (redoneMove != null)
                    {
                        board.MakeMove(redoneMove.BoardIndex, redoneMove.Row, redoneMove.Col);
                        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                        Console.WriteLine("Move redone.");
                    }
                    else
                    {
                        Console.WriteLine("No move to redo");
                    }
                }
                else
                {
                    Console.WriteLine("No move to redo");
                }
                continue;
            }

            if (input.ToLower() == "q")
            {
                Console.WriteLine("Thanks for playing!");
                break;
            }

            if (input.ToLower() == "h")
            {
                helpSystem.DisplayHelp();
                helpSystem.DisplayCommands();
                continue;
            }

            string[] moveInput = input.Split(' ');
            if (moveInput.Length == 3 &&
                int.TryParse(moveInput[0], out int boardIndex) &&
                int.TryParse(moveInput[1], out int row) &&
                int.TryParse(moveInput[2], out int col))
            {
                if (board.IsValidMove(boardIndex, row, col))
                {
                    board.MakeMove(boardIndex, row, col);
                    GamePiece move = new GamePiece { BoardIndex = boardIndex, Row = row, Col = col };
                    move.Mark();
                    moveHistory.AddMove(move);

                    if (board.IsGameOver())
                    {
                        Console.WriteLine(board.ToString());
                        Console.WriteLine($"{currentPlayer.Name} loses!");
                        break;
                    }

                    currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                }
                else
                {
                    Console.WriteLine("Invalid move. Try again.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Try again.");
            }
        }
    }

    private string GetPlayerInput(Player player)
    {
        if (player is HumanPlayer)
        {
            Console.Write($"{player.Name}, enter your move (board row col) or command: ");
            return Console.ReadLine();
        }
        else
        {
            (int boardIndex, int row, int col) = player.GetMove(board);
            Console.WriteLine($"Computer plays: {boardIndex} {row} {col}");
            return $"{boardIndex} {row} {col}";
        }
    }
}

//R4A
public class HelpSystem
{
    //1. Game Rules
    //2. Game Commands
    //3. Help w/ examples
    public void DisplayRules()
    {
        Console.WriteLine("\n╔═══ Notakto Rules ═════╗");
        Console.WriteLine("║ • Play on 3 boards    ║");
        Console.WriteLine("║ • Both use 'X' symbol ║");
        Console.WriteLine("║ • Avoid 3 in a row    ║");
        Console.WriteLine("║ • Last to play loses  ║");
        Console.WriteLine("╚═══════════════════════╝\n");
    }

    public void DisplayCommands()
    {
        Console.WriteLine("\n╔═══ Commands ═══╗");
        Console.WriteLine("║ Move: 'b r c'  ║");
        Console.WriteLine("║ Undo:    'u'   ║");
        Console.WriteLine("║ Redo:    'r'   ║");
        Console.WriteLine("║ Help:    'h'   ║");
        Console.WriteLine("║ Save:    's'   ║");
        Console.WriteLine("║ Load:    'l'   ║");
        Console.WriteLine("║ Quit:    'q'   ║");
        Console.WriteLine("╚════════════════╝\n");
    }

    public void DisplayHelp()
    {
        Console.WriteLine("\n╔═══ How to Play ═══╗");
        Console.WriteLine("║ Enter moves as:   ║");
        Console.WriteLine("║ 'board row column'║");
        Console.WriteLine("║ Example: '0 0 0'  ║");
        Console.WriteLine("║ for top-left of   ║");
        Console.WriteLine("║ the first board   ║");
        Console.WriteLine("║ do not enter      ║");
        Console.WriteLine("║ commas please 8^) ║");
        Console.WriteLine("╚═══════════════════╝\n");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Game game = new Game();
        game.Start();
    }
}
