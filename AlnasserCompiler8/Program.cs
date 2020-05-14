/*
 * Name: Mahmood Alnasser
 * Assignment 8
 * Due Date: 5/3/2020
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AlnasserCompiler
{
    class Program
    {
        internal static string inputFileSourceCodeName;

        static void Main(string[] args)
        {
            try
            {
                inputFileSourceCodeName = args[0].Remove(args[0].IndexOf('.'));
                if (File.Exists(inputFileSourceCodeName + ".tac"))
                    File.Delete(inputFileSourceCodeName + ".tac");
                if (File.Exists(inputFileSourceCodeName + ".asm"))
                    File.Delete(inputFileSourceCodeName + ".asm");

                LexicalAnayzer.readSourceFile(args[0]);
                SymbolTable symTab = new SymbolTable();
                Parser.Parse(LexicalAnayzer.GetTokenList(), symTab);
                AssemblyCodeGenerator.ReadTAC();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("End of program");
            Console.ReadKey();
        }
    }
}
