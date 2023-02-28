using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuModel
{
    public class GameLevel
    {
        public string Level { get; set; }
        public string TimeLimit { get; set; }
        public int RevealBox { get; set; }

        public GameLevel(string level, string timeLimit, int revealBox)
        {
            Level = level;
            TimeLimit = timeLimit;
            RevealBox = revealBox;
        }

        public override bool Equals(object obj)
        {
            string level = obj.ToString();
            return Level.Equals(level);
        }

        public double Point(int seconds)
        {
            DateTime time = Convert.ToDateTime(TimeLimit);
            if (Level == "Easy") return 100 - Math.Round(seconds / 3.0);
            else if (Level == "Medium") return 100 - Math.Round(seconds / 4.8);
            else if (Level == "Hard") return 100 - Math.Round(seconds / 7.2);
            else return 100 - Math.Round(seconds / 9.0);
        }
    }
}