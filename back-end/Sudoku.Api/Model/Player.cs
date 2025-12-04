using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager.Model
{
    public class Player : User
    {        
        public int Score { get; set; }
        public int TotalGames { get; set; }
        public TimeSpan BestTime { get; set; } = TimeSpan.MaxValue;

        public override void PrintInfo()
        {
            Console.WriteLine(
                $"{Id}. {Username} - Score: {Score}, Games: {TotalGames}, " +
                $"Best Time: {(BestTime == TimeSpan.MaxValue ? "N/A" : BestTime.ToString())}"
            );
        }
    }
}