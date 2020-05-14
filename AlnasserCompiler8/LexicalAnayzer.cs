using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AlnasserCompiler
{

    public class output
    {
        public string token { get; set; }
        public string lexume { get; set; }
        public int? value { get; set; }
        public double? valueR { get; set; }
        public string literal { get; set; }
        public int lineNumber { get; set; }
        public output(string token, string lexume, int? value = null, double? valueR = null, string literal = null, int lineNumber = 0)
        {
            this.value = value;
            this.valueR = valueR;
            this.lexume = lexume;
            this.token = token;
            this.literal = literal;
            this.lineNumber = lineNumber;
        }
    }

    public static class LexicalAnayzer
    {

        public static List<output> outputTokens = new List<output>();
        public enum ReservedTypes { classT, publicT, staticT, voidT, mainT, StringT, extendsT, returnT, intT, booleanT, ifT, elseT, whileT, SystemoutprintlnT, lengthT, trueT, falseT, thisT, newT, finalT }
        public enum TokenType { idT, numT, assignOpT, addOpT, mulOpT, lBracketT, rBracketT, lParT, rParT, semiT, commaT, lBraceT, rBraceT, relopT, periodT, literalT, unkownT, eofT, quotationT };

        public static void readSourceFile(string fileName)
        {

            using (StreamReader sr = new StreamReader(fileName))
            {
                bool streamEndFlag = sr.EndOfStream;
                global.characterReader = (char)sr.Read();
                while (!sr.EndOfStream || !streamEndFlag)
                {
                    streamEndFlag = sr.EndOfStream;
                    if (global.characterReader == ' ' || global.characterReader == '\n' || global.characterReader == '\t')
                    {
                        if (global.characterReader == '\n')
                            global.symbolTablelineCounter++;
                        global.characterReader = (char)sr.Read();
                    }
                    else if (global.characterReader != '\0')
                    {
                        ProcessToken(sr);
                    }
                }
                outputTokens.Add(new output(TokenType.eofT.ToString(), "", lineNumber: global.symbolTablelineCounter));
            }
        }

        public static void ProcessToken(StreamReader sr)
        {
            List<char> lexume = new List<char> { global.characterReader };

            if (char.IsLetter(lexume.First()))
            {
                ProcessWordToken(sr, lexume);
            }
            else if (char.IsDigit(lexume.First()))
            {
                ProcessNumToken(sr, lexume);
            }
            else if (lexume.First() == '/')
            {
                ProcessComment(sr);
            }
            else if (lexume.First() == '=' || lexume.First() == '<' || lexume.First() == '>' || lexume.First() == '!')
            {
                var priorChar = global.characterReader;
                var characterReaderLookAhead = (char)sr.Read();
                if (characterReaderLookAhead == '=')
                    ProcessDoubleToken(sr, priorChar);
                else
                    ProcessSingleToken(sr, characterReaderLookAhead);
            }
            else
                ProcessSingleToken(sr, ' ');
        }

        public static void ProcessWordToken(StreamReader sr, List<char> lexume)
        {
            var charCounter = 1;
            string resWord = null;
            var stringCombined = global.characterReader.ToString();
            global.characterReader = (char)sr.Read();
            charCounter++;
            var added = false;

            while (char.IsLetterOrDigit(global.characterReader) || global.characterReader == '_' || (stringCombined.ToLower().Contains("system") && (global.characterReader != '\r' && global.characterReader != '\n')))
            {
                lexume.Add(global.characterReader);
                var eofCheck = sr.Read();
                char lookAhead = (char)eofCheck;
                charCounter++;

                stringCombined = string.Join("", lexume.ToArray());
                if (!char.IsLetterOrDigit(lookAhead) && lookAhead != '_')
                    resWord = global.reservedWords.FirstOrDefault(t => t == stringCombined);
                if (resWord != null)
                {
                    // logic here
                    var enumIndex = global.reservedWords.IndexOf(stringCombined);
                    outputTokens.Add(new output(global.reservedList[enumIndex].ToString(), stringCombined, lineNumber: global.symbolTablelineCounter));
                    added = true;
                    if (eofCheck == -1)
                        global.characterReader = '\0';
                    else
                        global.characterReader = lookAhead;
                    break;
                }
                if (eofCheck == -1)
                    global.characterReader = '\0';
                else
                    global.characterReader = lookAhead;
            }
            if (!added)
                outputTokens.Add(new output(TokenType.idT.ToString(), stringCombined, lineNumber: global.symbolTablelineCounter));
        }

        public static void ProcessNumToken(StreamReader sr, List<char> lexume)
        {
            global.characterReader = (char)sr.Read();
            while (char.IsDigit(global.characterReader) || global.characterReader == '.')
            {
                lexume.Add(global.characterReader);
                if (global.characterReader == '.')
                {
                    global.characterReader = (char)sr.Read();
                    if (global.characterReader == '.')
                    {
                        Console.WriteLine("error, double '..' at line {0}!", global.symbolTablelineCounter);
                        return;
                    }
                }
                else
                {
                    global.characterReader = (char)sr.Read();
                }
            }
            var combinedString = string.Join("", lexume.ToArray());

            if (combinedString.Contains('.'))
            {
                outputTokens.Add(new output(TokenType.numT.ToString(), combinedString, null, double.Parse(combinedString), lineNumber: global.symbolTablelineCounter));
            }
            else
                outputTokens.Add(new output(TokenType.numT.ToString(), combinedString, int.Parse(combinedString), lineNumber: global.symbolTablelineCounter));
        }

        public static void ProcessComment(StreamReader sr, bool recursiveCall = false)
        {
            var lastCharRead = global.characterReader;
            if (!recursiveCall)
                global.characterReader = (char)sr.Read();

            if (global.characterReader == '*')
            {
                var endOFComment = false;
                global.characterReader = (char)sr.Read();
                while (!endOFComment)
                {
                    if (global.characterReader == '*')
                    {
                        global.characterReader = (char)sr.Read();
                        if (global.characterReader == '/')
                        {
                            endOFComment = true;
                            var eofCheck = sr.Read();
                            if (eofCheck == -1)
                                global.characterReader = (char)0;
                            else
                                global.characterReader = (char)eofCheck;
                        }
                    }
                    else if (global.characterReader == '/')
                    {
                        global.characterReader = (char)sr.Read();
                        if (global.characterReader == '*')
                        {
                            ProcessComment(sr, true);
                        }
                    }
                    else
                        global.characterReader = (char)sr.Read();
                }
            }
            else if (global.characterReader == '/')
            {
                while (global.characterReader != '\n')
                    global.characterReader = (char)sr.Read();
            }
            else
            {
                var nextChar = global.characterReader;
                global.characterReader = lastCharRead;
                ProcessSingleToken(sr, nextChar);
            }
        }

        public static void PrintLexumesAndTokens()
        {
            Console.WriteLine("{0,20} : {1,20} : {2,10} : {3,10} : {4,20} :", "Token", "Lexume", "value", "valueR", "literal");
            for (int i = 0; i < 94; i++)
                Console.Write("-");

            Console.WriteLine();
            foreach (var entry in outputTokens)
            {
                Console.WriteLine("{0,20} : {1,20} : {2,10} : {3,10} : {4,20} :", entry.token, entry.lexume, entry.value, entry.valueR, entry.literal);
            }
        }

        public static void ProcessSingleToken(StreamReader sr, char nextChar)
        {
            switch (global.characterReader)
            {
                case '/':
                    outputTokens.Add(new output(TokenType.mulOpT.ToString(), "/", lineNumber: global.symbolTablelineCounter));
                    break;
                case '*':
                    outputTokens.Add(new output(TokenType.mulOpT.ToString(), "*", lineNumber: global.symbolTablelineCounter));
                    break;
                case '{':
                    outputTokens.Add(new output(TokenType.lBraceT.ToString(), "{", lineNumber: global.symbolTablelineCounter));
                    break;
                case '}':
                    outputTokens.Add(new output(TokenType.rBraceT.ToString(), "}", lineNumber: global.symbolTablelineCounter));
                    break;
                case '(':
                    outputTokens.Add(new output(TokenType.lParT.ToString(), "(", lineNumber: global.symbolTablelineCounter));
                    break;
                case ')':
                    outputTokens.Add(new output(TokenType.rParT.ToString(), ")", lineNumber: global.symbolTablelineCounter));
                    break;
                case '[':
                    outputTokens.Add(new output(TokenType.lBracketT.ToString(), "[", lineNumber: global.symbolTablelineCounter));
                    break;
                case ']':
                    outputTokens.Add(new output(TokenType.rBracketT.ToString(), "]", lineNumber: global.symbolTablelineCounter));
                    break;
                case ',':
                    outputTokens.Add(new output(TokenType.commaT.ToString(), ",", lineNumber: global.symbolTablelineCounter));
                    break;
                case '-':
                    outputTokens.Add(new output(TokenType.addOpT.ToString(), "-", lineNumber: global.symbolTablelineCounter));
                    break;
                case '+':
                    outputTokens.Add(new output(TokenType.addOpT.ToString(), "+", lineNumber: global.symbolTablelineCounter));
                    break;
                case '=':
                    outputTokens.Add(new output(TokenType.assignOpT.ToString(), "=", lineNumber: global.symbolTablelineCounter));
                    break;
                case ';':
                    outputTokens.Add(new output(TokenType.semiT.ToString(), ";", lineNumber: global.symbolTablelineCounter));
                    break;
                case '.':
                    outputTokens.Add(new output(TokenType.periodT.ToString(), ".", lineNumber: global.symbolTablelineCounter));
                    break;
                case '<':
                    outputTokens.Add(new output(TokenType.relopT.ToString(), "<", lineNumber: global.symbolTablelineCounter));
                    break;
                case '>':
                    outputTokens.Add(new output(TokenType.relopT.ToString(), ">", lineNumber: global.symbolTablelineCounter));
                    break;
                case '\"':
                    ProcessLiteral(sr);
                    break;
                case '|':
                    global.characterReader = (char)sr.Read();
                    if (global.characterReader != '|')
                        outputTokens.Add(new output(TokenType.unkownT.ToString(), "|", lineNumber: global.symbolTablelineCounter));
                    else
                        outputTokens.Add(new output(TokenType.addOpT.ToString(), "||", lineNumber: global.symbolTablelineCounter));
                    break;
                case '&':
                    global.characterReader = (char)sr.Read();
                    if (global.characterReader != '&')
                        outputTokens.Add(new output(TokenType.unkownT.ToString(), "&", lineNumber: global.symbolTablelineCounter));
                    else
                        outputTokens.Add(new output(TokenType.mulOpT.ToString(), "&&", lineNumber: global.symbolTablelineCounter));
                    break;
                case '!':
                    outputTokens.Add(new output(TokenType.relopT.ToString(), "!", lineNumber: global.symbolTablelineCounter));
                    break;
                default:
                    if (global.characterReader != ' ' && !(global.characterReader.ToString().Contains("\\")))
                    {
                        outputTokens.Add(new output(TokenType.unkownT.ToString(), global.characterReader.ToString(), lineNumber: global.symbolTablelineCounter));
                    }
                    break;
            }
            if (global.characterReader == '=')
            {
                global.characterReader = nextChar;
            }
            else if (nextChar == ' ')
                global.characterReader = (char)sr.Read();
            else
                global.characterReader = nextChar;
        }

        public static void ProcessDoubleToken(StreamReader sr, char priorChar)
        {
            global.characterReader = (char)sr.Read();
            switch (priorChar)
            {
                case '=':
                    outputTokens.Add(new output(TokenType.relopT.ToString(), "==", lineNumber: global.symbolTablelineCounter));
                    break;
                case '!':
                    outputTokens.Add(new output(TokenType.relopT.ToString(), "!=", lineNumber: global.symbolTablelineCounter));
                    break;
                case '<':
                    outputTokens.Add(new output(TokenType.relopT.ToString(), "<=", lineNumber: global.symbolTablelineCounter));
                    break;
                case '>':
                    outputTokens.Add(new output(TokenType.relopT.ToString(), ">=", lineNumber: global.symbolTablelineCounter));
                    break;
            }
        }

        public static void ProcessLiteral(StreamReader sr)
        {
            global.characterReader = (char)sr.Read();
            string literal = "";

            while (global.characterReader != '\"')
            {
                literal += global.characterReader.ToString();
                global.characterReader = (char)sr.Read();
            }
            outputTokens.Add(new output(TokenType.literalT.ToString(), literal, literal: literal, lineNumber: global.symbolTablelineCounter));
        }

        public static List<output> GetTokenList()
        {
            return outputTokens;
        }
    }
}
