using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser {
    class Program {
        static void Main(string[] args) {

            Parser p = new MathParser();

            while (true) {
                string s = Console.ReadLine();

                ParseResult r = p.Parse(s);
                if (!r.WasSuccesfull)
                    Console.WriteLine(r.Exception.ToString());
                else
                    Console.WriteLine(r.Value);
            }
        }
    }
}
