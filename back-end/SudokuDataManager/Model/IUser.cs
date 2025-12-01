using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuDataManager.Model
{
    public interface IUser :IPrint
    {
        int Id { get; set; }
        string Username { get; set; }
    
    }
}