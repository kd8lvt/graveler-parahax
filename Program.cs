using System.Runtime.InteropServices;

namespace GravelerParaHax
{
    public static class Program
    {
        private readonly static int iterations = 1_000_000_000; //One billion
        private static MovingAverage avg = new MovingAverage(10_000); //Keep the last 10k iterations' durations for the purposes of calculating an ETC.
        private static DateTime start = DateTime.UtcNow; //Save when the program started

        private static string version = "1.0.0";
        private static DateTime prevUpdateTime = start;
        private static TimeSpan prevEstimate = TimeSpan.Zero;
        private static int safeTurns = 54; //Graveler has 54 status move PP to burn
        private static bool austinMode = false;
        private static string[] prevBuffer = { "", "", "", "", "" };
        private static int reqParaTurns = 177;
        public static void Main(string[] args)
        {
            if (args.Any(arg => arg.ToLower() == "--austinmode" || arg.ToLower() == "-a")) austinMode = true;
            Console.Clear();
            Console.WriteLine("GravelerParaHax v" + version + " by Kasi T.");
            Console.WriteLine("Start time: " + start.ToString() + " UTC");
            Run();
            DateTime end = DateTime.UtcNow;
            Console.SetCursorPosition(0, 7);
            Console.WriteLine("End time: " + end.ToString() + " UTC");
            Console.WriteLine("Total time taken: " + LongTimeString(end - start));
        }

        private static void Run()
        {
            int bestStreak = -1;
            int bestTrial = -1;
            Random random = new Random((int)DateTime.UtcNow.Ticks);
            int[] rolls = new int[4];
            for (int trial = 0; trial < iterations; trial++)
            {
                rolls = new int[] { (austinMode ? 0 : safeTurns), 0, 0, 0 };
                for (int streak = (austinMode?0:safeTurns); streak <= reqParaTurns; streak++)
                {
                    int roll = random.Next(0, 3);
                    rolls[roll]++;
                    /*
                     * Austin's code was egregiously slow mainly because it rolled every turn, even if an unsafe non-paralyzed turn had already been rolled.
                     * You can use the --austinMode cli argument to switch this optimization/accuracy off.
                     * Just know, it makes the program take up to a half hour on my machine! (Which is still an improvement of ~41,000% over Austin's implementation >.>)
                     * 
                     */
                    if (!austinMode && roll != 0) break;
                }
                bestTrial = (rolls[0] > bestStreak ? trial : bestTrial);
                bestStreak = (rolls[0] > bestStreak ? rolls[0] : bestStreak);
                if (trial % 10_000 == 0) UpdateScreen(trial, bestTrial, bestStreak);
                if (bestStreak >= reqParaTurns) return;

                DateTime now = DateTime.UtcNow;
                avg.Update(now - prevUpdateTime);
                prevUpdateTime = now;
            }
            UpdateScreen(iterations, bestTrial, bestStreak);
        }

        private static void UpdateScreen(int trial, int bestTrial, int bestStreak)
        {
            DateTime now = DateTime.UtcNow;
            string percentStr = ((((NFloat)trial) / iterations) * 100).ToString();
            if (percentStr.IndexOf(".") > -1 && percentStr.Length - percentStr.IndexOf(".") > 3)
            {
                percentStr = percentStr.Substring(0, percentStr.IndexOf(".") + 3);
            }
            string progress = trial + "/" + iterations + " (" + percentStr + "%)";
            TimeSpan estComplete = EstTimeToComplete(trial);
            DateTime estFinish = now + estComplete;

            string[] buffer = new string[5];
            buffer[0] = progress;
            buffer[1] = "Should be done on: " + (estFinish.ToShortDateString()) + " at " + estFinish.ToShortTimeString() + " UTC";
            buffer[2] = "Estimated run duration remaining: " + LongTimeString(estComplete);
            buffer[3] = "Best Trial: " + bestTrial + "       ";
            buffer[4] = "Best Streak: " + bestStreak + "/" + reqParaTurns;
            for (int i = 0; i < 5; i++)
            {
                if (buffer[i].Equals(prevBuffer[i])) continue;
                Console.SetCursorPosition(0, i + 2); //Skip over the initial two lines
                Console.WriteLine(buffer[i] + "                    "); //Big empty space prevents smaller strings from failing to clear their predecessor fully
                Console.SetCursorPosition(0, 7);
            }
            prevBuffer = buffer;
        }
        private static TimeSpan EstTimeToComplete(int trial)
        {
            if (trial % 100_000 != 0) return prevEstimate;
            TimeSpan ret = avg.Get() * (iterations - trial);
            prevEstimate = ret;
            return ret;
        }

        private static string LongTimeString(TimeSpan timespan)
        {
            string str = "";
            double days = timespan.Days;
            double hours = timespan.Hours;
            double minutes = timespan.Minutes;
            double seconds = timespan.Seconds;
            if (days > 0) str += days.ToString() + " day" + (days != 1 ? "s" : "") + (hours + minutes + seconds != 0 ? ", " : " ");
            if (hours > 0) str += hours.ToString() + " hour" + (hours != 1 ? "s" : "") + (minutes + seconds != 0 ? ", " : " ");
            if (minutes > 0) str += minutes.ToString() + " minute" + (minutes != 1 ? "s" : "") + (seconds != 0 ? ", " : " ");
            if (seconds > 0) str += seconds.ToString() + " second" + (seconds != 1 ? "s" : "");
            return str;
        }
    }

    //In its own class for organization. Program is getting messy
    public class MovingAverage
    {
        private readonly int poolSize;
        private readonly TimeSpan[] values;

        private int index = 0;
        private TimeSpan sum = TimeSpan.Zero;

        public MovingAverage(int poolSize)
        {
            this.poolSize = poolSize;
            values = new TimeSpan[poolSize];
        }

        public TimeSpan Update(TimeSpan nextInput)
        {
            // calculate the new sum
            sum = sum - values[index] + nextInput;

            // overwrite the old value with the new one
            values[index] = nextInput;

            // increment the index (wrapping back to 0)
            index = (index + 1) % poolSize;

            // calculate the average
            return sum / poolSize;
        }

        public TimeSpan Get()
        {
            return sum / poolSize;
        }
    }

}