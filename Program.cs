using System;

using System.Collections;
using System.Threading;
using System.IO;

using System.Drawing;

namespace Pictopog {
    class StringMatch {
        public String numberString;
        public int value;

        public StringMatch(String numberString, int value) {
            this.numberString = numberString;
            this.value = value;
        }
    }

    class Program {
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false) {
            if (!Directory.Exists("./Board/")) {
                Directory.CreateDirectory("./Board/");
            }
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create)) {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        public static T ReadFromBinaryFile<T>(string filePath) {
            using (Stream stream = File.Open(filePath, FileMode.Open)) {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }

        static int getInt(String prompt) {
            Console.Write(prompt);
            int value;
            while (!int.TryParse(Console.ReadLine(), out value)) {
                Console.WriteLine("Invalid Input, Try Again.");
                Console.Write(prompt);
            }
            return value;
        }

        static Constraint getConstraint(int n, int boardWidth) {
            int[] values;
            do {
                Console.Clear();
                if (n >= boardWidth) {
                    Console.WriteLine("Vertical Constraint " + ((n - boardWidth) + 1).ToString());
                } else {
                    Console.WriteLine("Horizontal Constraint " + (n + 1).ToString());
                }
                int constraintCount = getInt("Enter Constraint Count: ");
                values = new int[constraintCount];
                for (int i = 0; i < constraintCount; i++) {
                    values[i] = getInt((i + 1).ToString() + ": ");
                }

                // Confirmation
                Console.WriteLine("All Values Correct? (Enter Nothing if all good)");
            } while (Console.ReadLine() != "");
            Console.Clear();
            return new Constraint(values);
        }

        static Constraint[] getConstraints(int width, int height) {
            MouseOperations.MousePoint topLeft;
            MouseOperations.MousePoint bottomRight;
            do {
                Console.Clear();
                Console.WriteLine("{Enter} Set Top Left Position");
                Console.ReadLine();
                topLeft = MouseOperations.GetCursorPosition();

                Console.WriteLine("{Enter} Set Bottom Right Position");
                Console.ReadLine();
                bottomRight = MouseOperations.GetCursorPosition();

                // Confirmation
                Console.WriteLine("All Values Correct? (Enter Nothing if all good)");
            } while (Console.ReadLine() != "");
            Console.Clear();

            ////////////////////////
            float gridWidth = (float)(bottomRight.X - topLeft.X) / (float)width;
            float gridHeight = (float)(bottomRight.Y - topLeft.Y) / (float)height;

            int maxSize = 50;
            int sizeFactor = 1;
            while (gridWidth / sizeFactor > maxSize || gridHeight / sizeFactor > maxSize) {
                sizeFactor++;
            }
            ////////////////////////

            ArrayList matches = new ArrayList();

            Bitmap b;


            Constraint[] constraints = new Constraint[width + height];
            for (int n = 0; n < width + height; n++) {
                ArrayList constraintValues = new ArrayList();
                int offset = 1;
                while (true) {
                    if (n >= width) {
                        b = ScreenReading.getScreenRegion((int)(topLeft.X - (gridWidth * offset)), (int)(topLeft.Y + (gridHeight * (n - width))), (int)gridWidth, (int)gridHeight);
                    } else {
                        b = ScreenReading.getScreenRegion((int)(topLeft.X + (gridWidth * n)), (int)(topLeft.Y - (gridHeight * offset)), (int)gridWidth, (int)gridHeight);
                    }

                    String output = "";
                    for (int y = 0; y < b.Height; y += sizeFactor) {
                        for (int x = 0; x < b.Width; x += sizeFactor) {
                            String v = "  ";
                            if (b.GetPixel(x, y).GetBrightness() > 0.5) {
                                v = "██";
                            }
                            output += v;
                        }
                        output += "\n";
                    }
                    if (output.Contains("██")) {
                        int found = -1;
                        String trimmedOutput = output.Replace("\n", "").Trim();
                        for (int i = 0; i < matches.Count; i++) {
                            if (trimmedOutput.Contains(((StringMatch)matches[i]).numberString)) {
                                found = i;
                                break;
                            }
                        }
                        int value;
                        if (found == -1) {
                            Console.WriteLine(output);
                            value = getInt("Enter Value: ");
                            Console.Clear();
                            matches.Add(new StringMatch(trimmedOutput, value));
                        } else {
                            value = ((StringMatch)matches[found]).value;
                        }
                        // do the normie stuff here (adding to constraints ofc)
                        Console.WriteLine("Found: " + value.ToString());
                        constraintValues.Add(value);

                        offset++;
                    } else {
                        break;
                    }
                }
                int[] values = new int[constraintValues.Count];
                for (int i = 0; i < constraintValues.Count; i++) {
                    values[i] = (int)constraintValues[(constraintValues.Count - 1) - i];
                }
                constraints[n] = new Constraint(values);
            }
            return constraints;
        }

        static Board CreateBoard() {
            int boardWidth = getInt("Enter Board Width: ");
            int boardHeight = getInt("Enter Board Height: ");

            Constraint[] constraints = getConstraints(boardWidth, boardHeight);
            //Constraint[] constraints = new Constraint[boardWidth + boardHeight];
            //for (int n = 0; n < constraints.Length; n++) {
            //    constraints[n] = getConstraint(n, boardWidth);
            //}
            return new Board(boardWidth, boardHeight, constraints);
        }

        static void saveBoard(Board board, String path = "Default.board") {
            path = "./Board/" + path;
            board.clear();
            WriteToBinaryFile(path, board);
        }

        static Board loadBoard(String path = "Default.board") {
            path = "./Board/" + path;
            if (File.Exists(path)) {
                try {
                    return ReadFromBinaryFile<Board>(path);
                } catch (Exception e) {

                }
            }
            return null;
        }

        static void ListBoards() {
            String[] boards = Directory.GetFiles("./Board/");
            Console.WriteLine("Boards:");
            for (int i = 0; i < boards.Length; i++) {
                Console.WriteLine("\t- " + boards[i].Substring(8, boards[i].Length - 14));
            }
        }


        static void Main(string[] args) {
            Board board = loadBoard();
            if (board == null) {
                board = CreateBoard();
            }
            Console.Clear();

            while (true) {
                Console.WriteLine("1. Set Board Values");
                Console.WriteLine("2. Ammend Constraint");
                Console.WriteLine("3. Solve Board");
                Console.WriteLine("4. Save Board");
                Console.WriteLine("5. Load Board");

                // Get Choice
                int choice = getInt("Enter Option: ");
                Console.Clear();

                // Handle Thing
                switch (choice) {
                    case 1: {
                            board = CreateBoard();
                        } break;
                    case 2: {
                            Console.WriteLine("\nHorizontal:");
                            for (int n = 0; n < board.width; n++) {
                                Constraint c = board.constraints[n];
                                Console.Write((n + 1).ToString() + ": ");
                                for (int i = 0; i < c.c.Length; i++) {
                                    Console.Write(c.c[i].ToString() + ", ");
                                }
                                Console.WriteLine();
                            }
                            Console.WriteLine("\nVertical:");
                            for (int n = 0; n < board.height; n++) {
                                Constraint c = board.constraints[board.width + n];
                                Console.Write((n + 1 + board.width).ToString() + ": ");
                                for (int i = 0; i < c.c.Length; i++) {
                                    Console.Write(c.c[i].ToString() + ", ");
                                }
                                Console.WriteLine();
                            }

                            Console.WriteLine("Would you like to ammed a constraint? (Enter Nothing if all good)");
                            while (Console.ReadLine() != "") {
                                Console.Clear();

                                int index = getInt("Enter Index of constraint to mend: ") - 1;
                                if (index >= 0 && index < board.constraints.Length) {
                                    board.constraints[index] = getConstraint(index, board.width);
                                } else {
                                    Console.WriteLine("Failed to ammed.");
                                }

                                Console.WriteLine("Would you like to ammed another constraint? (Enter Nothing if all good)");
                            }

                        } break;
                    case 3: {
                            Console.WriteLine("Use Mouse To Output? (Yes/Any)");
                            if (Console.ReadLine().ToLower() == "yes") {
                                MouseOperations.MousePoint topLeft;
                                MouseOperations.MousePoint bottomRight;
                                do {
                                    Console.Clear();
                                    Console.WriteLine("{Enter} Set Top Left Position");
                                    Console.ReadLine();
                                    topLeft = MouseOperations.GetCursorPosition();

                                    Console.WriteLine("{Enter} Set Bottom Right Position");
                                    Console.ReadLine();
                                    bottomRight = MouseOperations.GetCursorPosition();

                                    // Confirmation
                                    Console.WriteLine("All Values Correct? (Enter Nothing if all good)");
                                } while (Console.ReadLine() != "");

                                Console.Clear();
                                board.solve(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
                            } else {
                                board.solve();
                            }
                            Console.ReadLine();
                       } break;
                    case 4: {
                            Console.Write("Enter File Name (Empty for default): ");
                            String path = Console.ReadLine();
                            if (path == "") {
                                path = "Default";
                            }
                            saveBoard(board, path + ".board");
                            Console.WriteLine("Board Saved");
                            Console.ReadLine();
                        } break;
                    case 5: {
                            ListBoards();
                            Console.Write("Enter File Name (Empty for default): ");
                            String path = Console.ReadLine();
                            if (path == "") {
                                path = "Default";
                            }
                            Board temp = loadBoard(path + ".board");
                            if (temp == null) {
                                Console.WriteLine("Board not found");
                                Console.ReadLine();
                                break;
                            }
                            board = temp;
                            Console.WriteLine("Board Loaded");
                            Console.ReadLine();
                        } break;
                    case 6: {
                            int lineIndex = getInt("Enter line index to check: ");
                            board.getPossibleLines(lineIndex - 1, true);
                            Console.ReadLine();
                        } break;
                    case 7: {
                            board.clear();
                        } break;
                }

                // End of cycle
                Console.Clear();
            }
        }
    }

    public enum Tile {
        Empty, Closed, Filled
    }

    [Serializable]
    class Constraint {
        public int[] c;
        public Constraint(int[] constraint) {
            this.c = constraint;
        }
    }

    [Serializable]
    class Line {
        public int[] sections;
        public Tile[] states; 
        public Line(Tile[] states) {
            this.states = states;
            CalculateSections();
        }

        public void Output() {
            for (int i = 0; i < states.Length; i++) {
                String v;
                switch (states[i]) {
                    default: v = "_"; break;
                    case Tile.Closed: v = "."; break;
                    case Tile.Filled: v = "O"; break;
                }
                Console.Write(v + " ");
            }
            Console.WriteLine();
        }

        public bool Overlay(Line l) {
            if (l.states.Length != states.Length) {
                return false;
            }
            for (int i = 0; i < states.Length; i++) {
                if ((states[i] == Tile.Closed && l.states[i] == Tile.Filled) ||
                    (states[i] == Tile.Filled && l.states[i] == Tile.Closed)) {
                    return false;
                }
            }
            return true;
        }

        public void CalculateSections() {
            int sectionCount = 1;
            Tile prevTile = Tile.Empty;
            Tile currentTile;
            for (int i = 0; i < states.Length; i++) {
                currentTile = states[i];
                if (currentTile != prevTile && (currentTile == Tile.Closed || prevTile == Tile.Closed)) {
                    sectionCount++;
                }
                prevTile = currentTile;
            }

            sections = new int[sectionCount];
            sections[0] = 0;
            sectionCount = 0;
            prevTile = Tile.Empty;
            for (int i = 0; i < states.Length; i++) {
                currentTile = states[i];
                if (currentTile != prevTile && (currentTile == Tile.Closed || prevTile == Tile.Closed)) {
                    sectionCount++;
                    sections[sectionCount] = 1;
                } else {
                    sections[sectionCount]++;
                }
                prevTile = currentTile;
            }
        }
    }

    [Serializable]
    class Board {
        public int width, height;
        public Constraint[] constraints;
        private Tile[,] board;
        public Board(int width, int height, Constraint[] constraints) {
            this.width = width;
            this.height = height;
            this.constraints = constraints;

            this.board = new Tile[width, height];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    this.board[x, y] = Tile.Empty;
                }
            }
        }

        public Line getLine(int index) {
            if (index >= width) { // horizontal
                index -= width;

                Tile[] tiles = new Tile[width];
                for (int i = 0; i < width; i++) {
                    tiles[i] = board[i, index];
                }

                return new Line(tiles);
            } else { // vertical
                Tile[] tiles = new Tile[height];
                for (int i = 0; i < height; i++) {
                    tiles[i] = board[index, i];
                }

                return new Line(tiles);
            }
        }
        public bool setLine(Line l, int index) {
            bool changes = false;
            if (index >= width) { // horizontal
                index -= width;
                for (int i = 0; i < width; i++) {
                    if (board[i, index] != l.states[i]) {
                        board[i, index] = l.states[i];
                        changes = true;
                    }
                }
            } else {
                for (int i = 0; i < height; i++) {
                    if (board[index, i] != l.states[i]) {
                        board[index, i] = l.states[i];
                        changes = true;
                    }
                }
            }
            return changes;
        }

        public void draw() {
            Console.WriteLine("Board:");
            String v;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    switch (board[x, y]) {
                        default: v = "__"; break;
                        case Tile.Closed: v = "░░"; break;
                        case Tile.Filled: v = "██"; break;
                    }
                    Console.Write(v);
                }
                Console.WriteLine();
            }
        }

        public void solve() {
            Console.Clear();
            draw();
            for (int i = 0; i < constraints.Length; i++) {
                if (!newBoolean(i)) {  
                    break;
                }
            }
            Console.CursorLeft = 0;
            Console.CursorTop = height + 1;
        }

        public void solve(int left, int top, int right, int bottom) {
            int tLeft = Math.Min(left, right);
            int tRight = Math.Max(left, right);
            int tTop = Math.Min(top, bottom);
            int tBottom = Math.Max(top, bottom);

            draw();
            for (int i = 0; i < constraints.Length; i++) {
                if (!newBoolean(i % constraints.Length, false, tLeft, tTop, tRight, tBottom)) {
                    break;
                }
            }
            Console.CursorLeft = 0;
            Console.CursorTop = height + 1;
        }


        public bool update() {
            bool changes = false;
            
            //newBoolean(new Line(new Tile[] {
            //    Tile.Empty, Tile.Empty, Tile.Filled, Tile.Empty, Tile.Empty,
            //    Tile.Empty, Tile.Empty, Tile.Filled, Tile.Empty, Tile.Empty,
            //    Tile.Empty, Tile.Empty, Tile.Filled, Tile.Empty, Tile.Empty
            //}), new Constraint(new int[] { 2, 5, 4 })).Output();

            return changes;
        }

        public bool check() {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (board[x, y] == Tile.Empty) {
                        return false;
                    }
                }
            }
            return true;
        }

        public void clear() {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    board[x, y] = Tile.Empty;
                }
            }
        }

        public Line[] getPossibleLines(int lineIndex, bool verbose = false) {
            Line l = getLine(lineIndex);
            Constraint c = constraints[lineIndex];

            if (verbose) {
                Console.Clear();
                Console.WriteLine("-----------Possibilities--------------");
            }

            ulong size = (ulong)l.states.Length;
            ulong constraintCount = (ulong)c.c.Length;
            ulong constraintTotal = 0;
            for (ulong i = 0; i < constraintCount; i++) {
                constraintTotal += (ulong)c.c[i];
            }
            ulong constraintBase = size - (constraintTotal + (constraintCount - 1));
            int[] offsets = new int[constraintCount];
            for (int i = 0; i < offsets.Length; i++) {
                offsets[i] = 0;
            }

            ArrayList lines = new ArrayList();

            ulong top = 1;
            for (ulong i = 1; i <= constraintCount; i++) {
                top *= (i + constraintBase);
            }
            ulong bottom = 1;
            for (ulong i = 2; i <= constraintCount; i++) {
                bottom *= i;
            }



            ulong degree = top / bottom;
            if (verbose) {
                Console.WriteLine("Data to look at:");

                Console.WriteLine("size:            " + size.ToString());
                Console.WriteLine("constraintCount: " + constraintCount.ToString());
                Console.WriteLine("constraintTotal: " + constraintTotal.ToString());
                Console.WriteLine("constraintBase:  " + constraintBase.ToString());
                Console.WriteLine("top:             " + top.ToString());
                Console.WriteLine("bottom:          " + bottom.ToString());
                Console.WriteLine("degree:          " + degree.ToString());

                Console.WriteLine("Stop looking, perv");
            }

            bool invalid;
            int left, max;
            for (ulong n = 0; n < degree; n++) {
                Line sout = new Line(new Tile[size]);
                for (int i = 0; i < sout.states.Length; i++) {
                    sout.states[i] = Tile.Closed;
                }

                invalid = false;
                left = 0;
                for (ulong i = 0; i < constraintCount && !invalid; i++) {
                    for (int k = 0; k < c.c[i]; k++) {
                        if (l.states[left + offsets[i] + k] == Tile.Closed) {
                            invalid = true;
                            break;
                        }
                        sout.states[left + offsets[i] + k] = Tile.Filled;
                    }
                    // the setting goes in here, im not too sure how to do it hmmmmmmmmm
                    left += c.c[i] + 1;
                }
                if (!invalid && sout.Overlay(l)) {
                    lines.Add(sout);
                    if (verbose) {
                        //sout.Output();
                    }
                }

                // increment offsets
                offsets[offsets.Length - 1]++;
                for (int o = offsets.Length - 1; o > 0; o--) {
                    if ((ulong)offsets[o] > constraintBase) {
                        offsets[o - 1]++;
                        offsets[o] = 0;
                    }
                }
                max = 0;
                for (int o = 0; o < offsets.Length; o++) {
                    max = Math.Max(max, offsets[o]);
                    offsets[o] = max;
                }
            }

            if (verbose) {
                Console.WriteLine("-------------Resultant----------------");
                Line[] ls = (Line[])lines.ToArray(typeof(Line));
                if (ls.Length > 0) {
                    for (int n = 0; n < l.states.Length; n++) {

                        Tile lastTile = ls[0].states[n];
                        for (int i = 1; i < ls.Length; i++) {
                            if (lastTile != ls[i].states[n]) {
                                lastTile = Tile.Empty;
                                break;
                            }
                        }
                        String v;
                        switch (lastTile) {
                            default: v = "_"; break;
                            case Tile.Closed: v = "."; break;
                            case Tile.Filled: v = "O"; break;
                        }
                        Console.Write(v + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("--------------Current-----------------");
                l.Output();
                Console.WriteLine("--------------------------------------");
            }

            return (Line[])lines.ToArray(typeof(Line));
        }

        public bool newBoolean(int lineIndex, bool verbose = false, int mLeft = -1, int mTop = -1, int mRight = -1, int mBottom = -1) {
            #region Math
            // 2 5
            // 0 _ x x _ _ _ _ _ _ _ _ _
            // 0 0 x x _ _ _ _ 0 _ _ _ _
            // 

            // minimu
            // 0 0 x x 0 0 0 0 0 _ _ _ _
            // max
            // _ _ x x _ 0 0 _ 0 0 0 0 0

            // working out every possible placement would be ineffiecent but so much better
            // hmmmmm
            // im not completely sure on how to do that tho
            // recursively? i think thats the strat
            // there needs to be the glue effect.

            /*
            sections -> 
               1 -> 2 spaces
               2 -> 2 filled
               3 -> 9 spaces

            c = 2

            2 ^ 2 + 1 -> 5 X
            2 ^ 1 + 1 -> 3 O
            2 ^ 0 + 1 -> 2 X

            

            0 0 _ 0 0 0 0 0 _ _ _ : 3 -> 10
            0 0 _ _ 0 0 0 0 0 _ _
            0 0 _ _ _ 0 0 0 0 0 _
            0 0 _ _ _ _ 0 0 0 0 0
            _ 0 0 _ 0 0 0 0 0 _ _
            _ 0 0 _ _ 0 0 0 0 0 _
            _ 0 0 _ _ _ 0 0 0 0 0
            _ _ 0 0 _ 0 0 0 0 0 _
            _ _ 0 0 _ _ 0 0 0 0 0
            _ _ _ 0 0 _ 0 0 0 0 0

            0 0 _ 0 0 0 0 0 _ _ : 2 -> 6
            0 0 _ _ 0 0 0 0 0 _
            0 0 _ _ _ 0 0 0 0 0
            _ 0 0 _ 0 0 0 0 0 _
            _ 0 0 _ _ 0 0 0 0 0
            _ _ 0 0 _ 0 0 0 0 0

            
            0 0 _ 0 0 0 0 0 _ : 1 -> 3
            0 0 _ _ 0 0 0 0 0 
            _ 0 0 _ 0 0 0 0 0 

            0 0 _ 0 0 0 0 0 : 0 -> 1
            

            c = 3

            3 ^ 3 - 1 / 2

            3 ^ 2 + 1 = 10  O
            3 ^ 1 + 1 = 4   O
            3 ^ 0 + 1 = 2   X

            0 _ 0 _ 0 0 0 0 _ _ : 2 -> 10
            0 _ 0 _ _ 0 0 0 0 _
            0 _ 0 _ _ _ 0 0 0 0
            0 _ _ 0 _ 0 0 0 0 _
            0 _ _ 0 _ _ 0 0 0 0
            0 _ _ _ 0 _ 0 0 0 0
            _ 0 _ 0 _ 0 0 0 0 _
            _ 0 _ 0 _ _ 0 0 0 0
            _ 0 _ _ 0 _ 0 0 0 0
            _ _ 0 _ 0 _ 0 0 0 0

            0 _ 0 _ 0 0 0 0 _ : 1 -> 4
            0 _ 0 _ _ 0 0 0 0 
            0 _ _ 0 _ 0 0 0 0 
            _ 0 _ 0 _ 0 0 0 0 

            0 _ 0 _ 0 0 0 0 : 0 -> 1


            c = 4
            d = 1(2)(3)(4) / 4(3)(2) = 

            0 _ 0 _ 0 _ 0 0 _ : 1 -> 5
            0 _ 0 _ 0 _ _ 0 0
            0 _ 0 _ _ 0 _ 0 0
            0 _ _ 0 _ 0 _ 0 0
            _ 0 _ 0 _ 0 _ 0 0



            (10)
            2 5
            2 + 5 + (N-1) = 8
            possible offsets = 10 - (2 + 5)

            10 - (8 - 2) = 4

            2 -> fits within 4 spacing.
            3 possible positions:
            0 0 _ _ => 8 - (0 + 1) => 7 -> 3 possible positions for 5 
            _ 0 0 _ => 8 - (1 + 1) => 6 -> 2 possible positions for 5
            _ _ 0 0 => 8 - (2 + 1) => 5 -> 1 possible positions for 5

            2, 5, 4

            2 + 5 + 4 = 11 / 15
            base should = 4? actually 2.
            0 0 _ 0 0 0 0 0 _ 0 0 0 0 _ _
            2 + 1 + 5 + 1 + 4 = 13 / 15
            15 - 13 = 2 -> all good

            2 + 5 = 7 / 10
            base should be 3? actually 2
            0 0 _ 0 0 0 0 0 _ _
            2 + 1 + 5 = 8 / 10
            10 - 8 = 2 -> all good

            2 + 5 = 7 / 15
            base should be 8? actually 7
            0 0 _ 0 0 0 0 0 _ _ _ _ _ _ _
            2 + 1 + 5  = 8 / 15
            15 - 8 = 7 -> all good
            
            base    : 2
            degree  : ((base + 2) * (base + 1)) / 2 : 6

            degree = b
            
            ////////////////////////////////////////////////

            // base = 1
            // constraints = 4
            // d = 1(2)(3)(4) / 4(3)(2)(1)


            // { 1, 14, 1, 3, 3 }),
            // base = 4 (5) (30 - 26)
            // contraints = 5

            // top      = 5(6(7(8(9)))) = 15120
            // bottom   = 5(4(3(2(1)))) = 120

            // degree = 126 = 210 / 84 || 262 / 126

            */
            #endregion

            Line l = getLine(lineIndex);
            Line[] ls = getPossibleLines(lineIndex, verbose);

            if (ls.Length > 0) {
                for (int n = 0; n < l.states.Length; n++) {
                    Tile lastTile = ls[0].states[n];
                    for (int i = 1; i < ls.Length; i++) {
                        if (lastTile != ls[i].states[n]) {
                            lastTile = Tile.Empty;
                            break;
                        }
                    }
                    if (lastTile != Tile.Empty) {
                        //l.states[n] = lastTile;
                        //setLine(l, lineIndex);
                        bool changed = false;
                        if (lineIndex >= width) {
                            if (board[n, lineIndex - width] != lastTile) {
                                board[n, lineIndex - width] = lastTile;
                                changed = true;
                            }
                        } else {
                            if (board[lineIndex, n] != lastTile) {
                                board[lineIndex, n] = lastTile;
                                changed = true;
                            }
                        }

                        if (changed) {
                            setConsoleSpot(lineIndex, n);
                            setMouseSpot(lineIndex, n, lastTile, mLeft, mTop, mRight, mBottom);
                            String v = "";
                            switch (lastTile) {
                                default: v = "__"; break;
                                case Tile.Closed: v = "--"; break;
                                case Tile.Filled: v = "[["; break;
                            }
                            Console.Write(v);
                            Thread.Sleep(10);

                            // check here if placing that last tile was a bad idea or not

                            int newI = (lineIndex >= width) ? (n) : (width + n);
                            //if (getPossibleLines(newI).Length == 0) {
                            //    setConsoleSpot(lineIndex, n);
                            //    switch (lastTile) {
                            //        default: v = "11"; break;
                            //        case Tile.Closed: v = "22"; break;
                            //        case Tile.Filled: v = "33"; break;
                            //    }
                            //    Console.Write(v);
                            //    if (lineIndex >= width) {
                            //        board[n, lineIndex - width] = Tile.Empty;
                            //    } else {
                            //        board[lineIndex, n] = Tile.Empty;
                            //    }
                            //    Thread.Sleep(2000);
                            //
                            //
                            //    return false;
                            //}
                            // move back after testing please
                            if (!newBoolean(newI, verbose, mLeft, mTop, mRight, mBottom)) {
                                return false;
                            }

                            setConsoleSpot(lineIndex, n);
                            switch (lastTile) {
                                default: v = "__"; break;
                                case Tile.Closed: v = "░░"; break;
                                case Tile.Filled: v = "██"; break;
                            }
                            Console.Write(v);
                            Thread.Sleep(10);
                        }
                    }
                }
                // do the recusion here?
            }

            // help
            return true;
        }

        public void setConsoleSpot(int index, int n = 0) {
            if (index >= width) {
                Console.CursorLeft = n * 2;
                Console.CursorTop = (index - width) + 1;
            } else {
                Console.CursorLeft = index * 2;
                Console.CursorTop = n + 1;
            }
        }

        public void setMouseSpot(int index, int n, Tile tile, int mLeft, int mTop, int mRight, int mBottom) {
            if (mBottom != -1 && tile == Tile.Filled) {
                int x, y;
                if (index >= width) {
                    x = n;
                    y = (index - width);
                } else {
                    x = index;
                    y = n;
                }
                // Board Bounds
                float bx = mLeft;
                float by = mTop;
                float bw = (mRight - mLeft) / (float)width;
                float bh = (mBottom - mTop) / (float)height;
                // Mouse Pos
                int mx = (int)(bx + (bw * (0.5f + ((float)x))));
                int my = (int)(by + (bh * (0.5f + ((float)y))));
                MouseOperations.SetCursorPosition(mx, my);
                Thread.Sleep(25);

                MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
                Thread.Sleep(5);
                MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
            }
        }

        // x x x _ x x _ _ _ _
        public Line booleanIntersect(Line l, Constraint c) {



            int size = l.states.Length;
            int[,] overlap = new int[size, 2];
            for (int i = 0; i < size; i++) {
                overlap[i, 0] = -1;
                overlap[i, 1] = -1;
            }

            int leftOffset = 0, rightOffset = size - 1;

            Console.WriteLine("Section LEngths = " +  l.sections.Length);

            for (int n = 0; n < c.c.Length; n++) {
                for (int i = 0; i < c.c[n]; i++) {
                    overlap[leftOffset, 0] = n;
                    leftOffset++;                   
                }
                leftOffset++;
                for (int i = 0; i < c.c[c.c.Length - (n + 1)]; i++) {
                    overlap[rightOffset, 1] = c.c.Length - (n + 1);
                    rightOffset--;
                }
                rightOffset--;
            }
            for (int i = 0; i < size; i++) {
                if (overlap[i, 0] >= 0 && overlap[i, 0] == overlap[i, 1]) {
                    l.states[i] = Tile.Filled;
                }
            }
            return l;
        }

        public Line singleCheck(Line l, Constraint c) {
            if (c.c.Length == 1) {
                int size = l.states.Length;
                if (c.c[0] == 0) {
                    for (int i = 0; i < size; i++) {
                        l.states[i] = Tile.Closed;                        
                    }
                } else {
                    int min = size, max = 0;
                    for (int i = 0; i < size; i++) {
                        if (l.states[i] == Tile.Filled) {
                            min = Math.Min(min, i);
                            max = Math.Max(max, i);
                        }
                    }

                    int length = (max - min) + 1;
                    Console.WriteLine(length);
                    if (length > 0) {
                        for (int i = min; i <= max; i++ ) {
                            l.states[i] = Tile.Filled;
                        }

                        int space = c.c[0] - length;
                        min -= space;
                        max += space;

                        for (int i = 0; i < min; i++) {
                            l.states[i] = Tile.Closed;
                        }
                        for (int i = max + 1; i < size; i++) {
                            l.states[i] = Tile.Closed;
                        }
                    }
                }
            }
            return l;
        }
    }
}
