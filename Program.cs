// See https://aka.ms/new-console-template for more information
using System;
using System.Runtime.InteropServices;

namespace GravelerParaHax
{
    public static class Program
    {
        private static string version = "1.0.0";
        private readonly static int iterations = 1_000_000_000; //One billion;
        private static SimpleMovingAverage avg = new SimpleMovingAverage(10000);
        private static DateTime start = DateTime.UtcNow;
        private static DateTime prevUpdateTime = start;
        private static TimeSpan prevEstimate = TimeSpan.Zero;
        private static bool austinMode = false;
        private static string[] prevBuffer = {"","","","",""};
        public static void Main(string[] args)
        {
            if (args.Any(arg => arg.ToLower() == "--austinmode")) austinMode = true;
            Console.Clear();
            Console.WriteLine("GravelerParaHax v" + version + " by Kasi T.");
            Console.WriteLine("Start time: " + start.ToString() + " UTC");
            Run();
            DateTime end = DateTime.UtcNow;
            Console.SetCursorPosition(0, 7);
            Console.WriteLine("End time: "+end.ToString()+" UTC");
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
                rolls = new int[4];
                for (int streak = 0;streak < 177;streak++)
                {
                    int roll = random.Next(1,4);
                    rolls[roll-1]++;
                    /*
                     * Austin's code was slow mainly because it rolled every turn, even if a non-1 had already been rolled.
                     * There is no reason to do this, and in fact it skews the numbers towards the best-case scenario where every paralysis happens in a row!
                     * That's why my implementation returns much lower numbers, in the 15-25 range instead of 90+.
                     * You can use the --austinMode cli argument to switch this optimization off.
                     * Just know, it makes the program take up to a half hour on my machine!
                     * 
                     */
                    if (!austinMode && roll != 1) break;
                }
                bestTrial = (rolls[0] > bestStreak ? trial : bestTrial);
                bestStreak = (rolls[0] > bestStreak ? rolls[0] : bestStreak);
                if (trial % 10_000 == 0) UpdateOutput(trial, bestTrial, bestStreak);
                if (bestStreak == 176) return;

                DateTime now = DateTime.UtcNow;
                avg.Update(now - prevUpdateTime);
                prevUpdateTime = now;
            }
            UpdateOutput(iterations, bestTrial, bestStreak);
        }

        private static void UpdateOutput(int trial,int bestTrial,int bestStreak)
        {
            DateTime now = DateTime.UtcNow;
            string percentStr = ((((NFloat)trial) / iterations)*100).ToString();
            if (percentStr.IndexOf(".") > -1 && percentStr.Length - percentStr.IndexOf(".") > 3)
            {
                percentStr = percentStr.Substring(0, percentStr.IndexOf(".") + 3);
            }
            string progress = trial + "/"+iterations+" (" + percentStr + "%)";
            TimeSpan estComplete = EstTimeToComplete(trial);
            DateTime estFinish = now+estComplete;

            string[] buffer = new string[5];
            buffer[0]=progress;
            buffer[1]="Should be done on: "+(estFinish.ToShortDateString())+" at "+estFinish.ToShortTimeString();
            buffer[2]="Estimated run duration remaining: " + LongTimeString(estComplete);
            buffer[3]="Best Trial: " + bestTrial + "       ";
            buffer[4]="Best Streak: " + bestStreak + "/176";
            for (int i=0;i<5;i++)
            {
                if (buffer[i].Equals(prevBuffer[i])) continue;
                Console.SetCursorPosition(0, i+2); //Skip over the initial two lines
                Console.WriteLine(buffer[i] + "                    ");
                Console.SetCursorPosition(0, 7);
            }
            prevBuffer = buffer;
        }
        private static TimeSpan EstTimeToComplete(int trial)
        {
            if (trial % 100_000 != 0) return prevEstimate;
            TimeSpan ret = avg.Get() * (1000000000 - trial);
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
            if (days > 0) str += days.ToString() + " day" + (days!= 1?"s":"")+(hours+minutes+seconds!=0?", ":" ");
            if (hours > 0) str += hours.ToString() + " hour" + (hours!= 1 ? "s" : "")+(minutes+seconds!=0?", ":" ");
            if (minutes > 0) str += minutes.ToString() + " minute" + (minutes!= 1 ? "s" : "")+(seconds!=0?", ":" ");
            if (seconds > 0) str += seconds.ToString() + " second" + (seconds!= 1 ? "s" : "");
            return str;
        }
    }

    //Courtesy https://andrewlock.net/creating-a-simple-moving-average-calculator-in-csharp-1-a-simple-moving-average-calculator/
    public class SimpleMovingAverage
    {
        private readonly int _k;
        private readonly TimeSpan[] _values;

        private int _index = 0;
        private TimeSpan _sum = TimeSpan.Zero;

        public SimpleMovingAverage(int k)
        {
            if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k), "Must be greater than 0");

            _k = k;
            _values = new TimeSpan[k];
        }

        public TimeSpan Update(TimeSpan nextInput)
        {
            // calculate the new sum
            _sum = _sum - _values[_index] + nextInput;

            // overwrite the old value with the new one
            _values[_index] = nextInput;

            // increment the index (wrapping back to 0)
            _index = (_index + 1) % _k;

            // calculate the average
            return _sum / _k;
        }

        public TimeSpan Get()
        {
            return _sum / _k;
        }
    }

}