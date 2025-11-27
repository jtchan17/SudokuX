using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager
{
    public abstract class User : IUser
    {
        public int Id { get; set; }
        public string Username { get; set; }

        public abstract void PrintInfo();
    }
}