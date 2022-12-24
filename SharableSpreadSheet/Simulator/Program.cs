using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shareableSpreadSheet
{
    class Program
    {
        private string filePath = Environment.CurrentDirectory + "\\pooh.txt";
        private static SharableSpreadSheet share_ss = new shareableSpreadSheet(1000, 1000);
        static void Main(string[] args)
        {
        }
    }
}
