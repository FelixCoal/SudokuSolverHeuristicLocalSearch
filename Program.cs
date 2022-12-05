using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

namespace ConsoleApp6

{
    class Program
    {
        static void Main(string[] args)
        {

            List<long> Times = new List<long>();
            string input = Console.ReadLine();

            // n is the amount of times the sudoku solver tries to solve the input
            int n = 25;

            for (int i = 0; i < n; i++)
            {
                Console.WriteLine("Solivng: " + (i + 1) + "/" + n );
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Sudoku test = new Sudoku(input);
                SudokuSolver to_solve = new SudokuSolver(test);
                to_solve.Solve();

                stopwatch.Stop();

                Console.WriteLine("Solved: " + (i + 1) + " in " + stopwatch.Elapsed + " seconds.");

                Times.Add(stopwatch.ElapsedMilliseconds);
            }

            long total = Times.Sum();
            long average = total / Times.Count();
            Console.WriteLine(average);

            
        }

        public class Sudoku
        {
            public int[,,] sudoku = new int[9, 9, 2];

            // eval_hor keeps track of the evaluation number of all 9 rows
            public int[] eval_hor = new int[9];

            //eval_ver keeps track of the evaluation number of all 9 columns
            public int[] eval_ver = new int[9];

            public int eval_tot;

            public Sudoku(string s)
            {
                this.SudokuFromString(s);
                FillSudoku();
                EvaluateAll();
                //Console.WriteLine(eval_tot);
            }

            void SudokuFromString(string s) //WORKS
                //Converts an input string with 81 numbers (1 to 9) into a 9x9 2d array.
                // if nr is 0, it is unfixated so sudoku[x,y,1] is set to 0, if it is fixated it is set to 1
            {
                string[] input = s.Split();
                int x = 0;
                int y = 0;


                foreach (string c in input)
                {
                    int getal = Convert.ToInt32(c);
                    this.sudoku[x, y, 0] = getal;
                    if (getal == 0) this.sudoku[x, y, 1] = 0;
                    else this.sudoku[x, y, 1] = 1;

                    x++;

                    if (x >= 9)
                    {
                        x = 0;
                        y++;
                    }

                    if (y > 9) break;
                }

            }

            void FillSudoku()// WORKS
                // Fills all empty (nr = 0) boxes in the sudoku such that in each block there is exactly 1 of every number between 1 and 9
            {
                int x = 0;
                int y = 0;

                for (int y1 = y; y1 <= 8; y1 = y1 + 3)
                {
                    for (int x1 = x; x1 <= 8; x1 = x1 + 3)
                    {
                        FillBlock(x1, y1);
                    }
                }

            }

            void FillBlock(int x, int y)// WORKS
            {
                //Fills the box with x and y as top left coord

                List<int> nrs = CheckNrs(x, y);

                for (int y1 = y; y1 <= y + 2; y1++)
                {
                    for (int x1 = x; x1 <= x + 2; x1++)
                    {
                        //LATER WELLICHT UITBREIDEN NAAR RANDOM, NU GEWOON LIJSTJE AFLOPEN
                        if (this.sudoku[x1, y1, 1] == 0)
                        {
                            //this.sudoku[x1, y1, 0] = nrs[0];
                            //nrs.RemoveAt(0);

                            Random r = new Random();
                            int index = r.Next(nrs.Count());
                            this.sudoku[x1, y1, 0] = nrs[index];
                            nrs.RemoveAt(index);
                        }
                    }
                }

            }

            List<int> CheckNrs(int x, int y) // WORKS
            //Returns a list with the numbers NOT present in the block we are working on
            {
                List<int> nrs = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                for(int y1 = y; y1 <= y + 2; y1++ )
                {
                    for(int x1 = x; x1 <= x + 2; x1++ )
                    {
                        if (nrs.Contains(this.sudoku[x1, y1, 0])) nrs.Remove(this.sudoku[x1, y1, 0]);
                    }
                }
                return nrs;
            }

            public void Swap(Point p1, Point p2) //UNTESTED
            //Swaps 2 items inside the same block, both items should not be fixed (sudoku[x,y,1] = 0)
            {

                int x1, y1, x2, y2;

                x1 = p1.X;
                y1 = p1.Y;
                x2 = p2.X;
                y2 = p2.Y;

                if (Math.Abs(x1 - x2) > 3 | Math.Abs(y1-y2) > 3) Console.WriteLine("ERROR: Can't swap items in between blocks"); //Deze error message werkt nog niet helemaal maar hoeft eigenlijk ooko niet helemaal perfect te werken
                if (sudoku[x1, y1, 1] == 1 | sudoku[x2, y2, 1] == 1) Console.WriteLine("ERROR: Can't swap fixed items");

                int tmp = sudoku[x1, y1, 0];
                sudoku[x1, y1, 0] = sudoku[x2, y2, 0];
                sudoku[x2, y2, 0] = tmp;

                EvalAfterSwap(p1, p2);
            }

            public void Evaluate(string direction, int n) //GEEFT GEEN ERRORS
            //Kan waarschijnlijk sneller
            {
                //Amount is an array with length 9, where each index corresponds with one of the numbers 1 to 9
                int[] arr = new int[] {0,0,0,0,0,0,0,0,0};

                //Checks what direction is being checked in then checks that row/column. Then loops through the row/column and adds up the corresponding number in the amount array
                if (direction == "hor")
                {
                    for(int i = 0; i < 9; i++)
                    {
                        arr[sudoku[i, n, 0] - 1]++;
                    }

                    eval_hor[n] = CountZeros(arr);
                }
                else if (direction == "ver")
                {
                    for (int i = 0; i < 9; i++)
                    {
                        arr[sudoku[n, i, 0] - 1]++;
                    }

                    eval_ver[n] = CountZeros(arr);
                }

                UpdateEval();
            }

            public int EvaluateAll() //GEEFT GEEN ERRORS
                               //Evaluates all rows and columns
            {
                for(int i = 0; i < 9; i++)
                {
                    Evaluate("hor", i);
                    Evaluate("ver", i);
                }

                UpdateEval();
                return eval_tot;

            }

            public void UpdateEval() //GEEFT GEEN ERRORS
            {
                eval_tot = eval_hor.Sum() + eval_ver.Sum();
            }

            public void EvalAfterSwap(Point p1, Point p2)
            {
                Evaluate("ver", p1.X);
                Evaluate("hor", p1.Y);
                Evaluate("ver", p2.X);
                Evaluate("hor", p2.Y);
                UpdateEval();
            }

            int CountZeros(int[] arr) //GEEFT GEEN ERRORS
            {
                int n = 0;
                foreach(int i in arr)
                {
                    if (i == 0) n++;
                }
                return n;
            }

            public void Visualize() // WORKS
                //Visualizes sudoku
            {
                Console.WriteLine("-----------------------------------");
                for (int y = 0; y < 9; y++)
                {
                    for(int x=0; x < 9; x++)
                    {
                        Console.Write(sudoku[x, y, 0] + " | ");
                    }
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------");
                }
            }

            public List<Point> Unfixated(Point p)
            {
                //Console.WriteLine("Unfixated");
                List<Point> coords = new List<Point>();
                int x = p.X;
                int y = p.Y;
                for (int y1 = y; y1 <= y + 2; y1++)
                {
                    for (int x1 = x; x1 <= x + 2; x1++)
                    {
                        //Console.WriteLine("Iets");
                        if (sudoku[x1, y1, 1] == 0)
                        {
                            Point coord = new Point(x1, y1);
                            coords.Add(coord);
                        }
                    }
                }

                return coords;
            }

        }

        public class SudokuSolver
        {
            int best_score = 180;
            Sudoku to_solve;

            public SudokuSolver(Sudoku input)
            {
                to_solve = input;
            }

            public void Solve()
            {
                int repeat_counter = 0;
                int last_eval = 180;

                while (to_solve.eval_tot > 0)
                {
                    Normal();

                    //Counts how many times in a row the evaluation score stays the same, if it comes above a certain number, a random walk is done for a certain amount of times.
                    if (to_solve.eval_tot == last_eval) repeat_counter++;
                    else repeat_counter = 0;

                    if (repeat_counter == 10)
                    {
                        RandomWalk(3);
                        repeat_counter = 0;
                        //Console.WriteLine(to_solve.eval_tot);
                    }

                    last_eval = to_solve.eval_tot;

                    //Console.WriteLine(to_solve.eval_tot);
                    //Thread.Sleep(10);
                    
                    
                }

                //to_solve.Visualize();
            }

            public List<Tuple<Point,Point,int>> GenSwappable(Point Block)
            {
                List<Point> unfixated = to_solve.Unfixated(Block);
                List<Tuple<Point, Point, int>> Swappable = new List<Tuple<Point, Point, int>>();

                for (int a = 0; a < unfixated.Count(); a++)
                {
                    for (int b = a + 1; b < unfixated.Count(); b++)
                    {
                        //Console.WriteLine(a + ":" + b);
                        Tuple<Point, Point, int> to_add = new Tuple<Point, Point, int>(unfixated[a], unfixated[b], 0);
                        Swappable.Add(to_add);
                    }
                }

                return Swappable;
            }

            public void Normal()
            {
                // Kies een van de 9 3x3 blokken
                Random r = new Random();
                int x = (r.Next(1, 4) - 1) * 3;
                int y = (r.Next(1, 4) - 1) * 3;
                Point Block = new Point(x, y);


                //Maak lijst met mogelijke actions
                //Console.WriteLine("Maak een lijst met mogelijke acties");
                List<Tuple<Point, Point, int>> Swappable = GenSwappable(Block);

                Tuple<Point, Point, int> top = new Tuple<Point, Point, int>(Point.Empty, Point.Empty, best_score);

                //Voer swap uit
                //Console.WriteLine("Loop over alle acties");
                foreach (Tuple<Point, Point, int> action in Swappable)
                {
                    //Console.WriteLine("Voer swap uit");
                    to_solve.Swap(action.Item1, action.Item2);

                    //Console.WriteLine("Start met evaluaten");
                    to_solve.EvalAfterSwap(action.Item1, action.Item2);

                    //Console.WriteLine("Check of huidige swap beter dan beste");
                    if (to_solve.eval_tot < top.Item3)
                    {
                        top = new Tuple<Point, Point, int>(action.Item1, action.Item2, to_solve.eval_tot);
                    }

                    //swap terug
                    to_solve.Swap(action.Item1, action.Item2);


                }


                //Als de top veranderd is, en er dus een betere toestand gevonden is, word deze nieuwe betere toestand toegepast
                if (top.Item1 != Point.Empty)
                {
                    to_solve.Swap(top.Item1, top.Item2);
                }

                //Console.WriteLine("Klaar met oplossen!");
                to_solve.UpdateEval();
                best_score = to_solve.eval_tot;
            }

            public void RandomWalk(int s)
            {
                for(int i = 0; i < s; i++)
                {
                    Random r = new Random();
                    int x = (r.Next(1, 4) - 1) * 3;
                    int y = (r.Next(1, 4) - 1) * 3;
                    Point Block = new Point(x, y);


                    //Maak lijst met mogelijke actions
                    //Console.WriteLine("Maak een lijst met mogelijke acties");
                    List<Tuple<Point, Point, int>> Swappable = GenSwappable(Block);

                    Tuple<Point, Point, int> top = new Tuple<Point, Point, int>(Point.Empty, Point.Empty, best_score);

                    int index = r.Next(Swappable.Count());
                    Tuple<Point, Point, int> to_swap = Swappable[index];
                    to_solve.Swap(to_swap.Item1, to_swap.Item2);
                    best_score = to_solve.eval_tot;
                }
            }


        }
    }
}
