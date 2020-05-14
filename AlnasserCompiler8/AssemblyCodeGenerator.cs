using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AlnasserCompiler
{
    class AssemblyCodeGenerator
    {
        public static List<string> writeList = new List<string>();
        public static SymbolTable symTab;
        public static void ReadTAC()
        {
            symTab = global.codeGenSymbolTable;
            using (StreamReader sr = new StreamReader(Program.inputFileSourceCodeName + ".tac"))
            {
                string lineRead;
                while (!sr.EndOfStream)
                {
                    lineRead = sr.ReadLine();
                    var splitLineRead = lineRead.Split(' ');
                    FormatLine(splitLineRead);
                }
                PrintASM();
            }
        }

        private static void FormatLine(string[] splitLineRead)
        {
            switch (splitLineRead.Count())
            {
                case 1:
                    {
                        writeList.Add("call writeln");
                        break;
                    }
                case 2:
                    ProcessTwoStatement(splitLineRead);
                    break;
                case 3:
                    ProcessThreeStatement(splitLineRead);
                    break;
                case 5:
                    ProcessFiveStatement(splitLineRead);
                    break;
                default:
                    Console.WriteLine("error on formatting line");
                    break;
            }
        }


        private static void ProcessTwoStatement(string[] splitLineRead)
        {
            switch (splitLineRead[0].ToLower())
            {
                case "proc":
                    {
                        writeList.Add(splitLineRead[1] + " " + splitLineRead[0]);
                        if (splitLineRead[1].ToLower() == "main")
                            return;
                        writeList.Add("push bp");
                        writeList.Add("mov bp, sp");
                        var currentMethod = (MethodEntry)symTab.search(splitLineRead[1]).First(t => t.GetType() == typeof(MethodEntry));
                        writeList.Add("sub sp, " + (currentMethod.sizeOfLocalVars));
                        break;
                    }
                case "endp":
                    {
                        if (splitLineRead[1].ToLower() == "main")
                        {
                            writeList.Add("ret");
                            writeList.Add(splitLineRead[1] + " " + splitLineRead[0]);
                            return;
                        }
                        var currentMethod = (MethodEntry)symTab.search(splitLineRead[1]).First(t => t.GetType() == typeof(MethodEntry));
                        writeList.Add("add sp, " + (currentMethod.sizeOfLocalVars));
                        writeList.Add("pop bp");
                        writeList.Add("ret " + currentMethod.sizeOfParameters);
                        writeList.Add(splitLineRead[1] + " " + splitLineRead[0]);
                        break;
                    }
                case "push":
                    {
                        writeList.Add("push [" + splitLineRead[1].Substring(1) + "]");
                        break;
                    }
                case "wri":
                    {
                        writeList.Add("mov ax, [" + splitLineRead[1].Substring(1) + "]");
                        writeList.Add("call writeint");
                        break;
                    }
                case "wrs":
                    {

                        writeList.Add("mov dx, OFFSET " + splitLineRead[1]);
                        writeList.Add("call writestr");
                        break;
                    }
                case "rdi":
                    {
                        writeList.Add("call readint");
                        writeList.Add("mov [" + splitLineRead[1].Substring(1
                            ) + "], bx");
                        break;
                    }
                case "call":
                    {
                        writeList.Add("call " + splitLineRead[1]);
                        break;
                    }
            }
        }
        private static void ProcessThreeStatement(string[] splitLineRead)
        {
            if (splitLineRead[2].ToLower().Contains("ax"))
            {
                writeList.Add("mov [" + splitLineRead[0].Substring(1) + "], ax");
                return;
            }
            if (splitLineRead[2].Contains('_'))
                writeList.Add("mov ax, [" + splitLineRead[2].Substring(1) + "]");
            else
                writeList.Add("mov ax, " + splitLineRead[2]);
            if (splitLineRead[0].ToLower().Contains("ax"))
                return;
            writeList.Add("mov [" + splitLineRead[0].Substring(1) + "], ax");
        }
        private static void ProcessFiveStatement(string[] splitLineRead)
        {
            switch (splitLineRead[3])
            {
                case "+":
                    {
                        writeList.Add("mov ax, 0");
                        writeList.Add("mov  al, [" + splitLineRead[2].Substring(1) + "]");
                        writeList.Add("add al, [" + splitLineRead[4].Substring(1) + "]");
                        writeList.Add("mov [" + splitLineRead[0].Substring(1) + "], ax");
                        break;
                    }
                case "-":
                    {
                        writeList.Add("mov ax, 0");
                        writeList.Add("mov  al, [" + splitLineRead[2].Substring(1) + "]");
                        writeList.Add("sub al, [" + splitLineRead[4].Substring(1) + "]");
                        writeList.Add("mov [" + splitLineRead[0].Substring(1) + "], ax");
                        break;
                    }

                case "*":
                    {
                        writeList.Add("mov ax, [" + splitLineRead[2].Substring(1) + "]");
                        writeList.Add("mov bx, [" + splitLineRead[4].Substring(1) + "]");
                        writeList.Add("imul bx");
                        writeList.Add("mov [" + splitLineRead[0].Substring(1) + "], ax");
                        break;
                    }
                case "/":
                    {
                        writeList.Add("mov ax, [" + splitLineRead[2].Substring(1) + "]");
                        writeList.Add("mov bx, [" + splitLineRead[4].Substring(1) + "]");
                        writeList.Add("idiv bx");
                        writeList.Add("mov [" + splitLineRead[0].Substring(1) + "], ax");
                        break;
                    }
                default:
                    Console.WriteLine("hit default: errr on process five statement");
                    break;
            }
        }

        private static void PrintASM()
        {
            using (StreamWriter sw = new StreamWriter(Program.inputFileSourceCodeName + ".asm"))
            {
                sw.WriteLine(".model small");
                sw.WriteLine(".stack 100h");
                sw.WriteLine(".data");

                var literalList = global.tokenList.Where(t => t.literal != null);
                int i = 0;
                foreach (var literal in literalList)
                {
                    sw.WriteLine("_S" + i + " db " + "\"" + literal.literal + "\"" + ",\"$\"");
                    i++;
                }
                sw.WriteLine(".code");
                sw.WriteLine("include io.asm");
                sw.WriteLine("start PROC");
                sw.WriteLine("mov ax, @data");
                sw.WriteLine("mov ds, ax");
                sw.WriteLine("call main");
                sw.WriteLine("mov ah, 04ch");
                sw.WriteLine("int 21h");
                sw.WriteLine("start ENDP");

                foreach (var entry in writeList)
                {
                    sw.WriteLine(entry);
                }
                sw.WriteLine("END start");
            }
        }
    }
}
