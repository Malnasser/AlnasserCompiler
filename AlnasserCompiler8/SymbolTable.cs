using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlnasserCompiler
{
    public enum VarType { intT, boolenT, voidT }
    public enum EntryType { VarEntry, MethodEntry, ClassEntry, Constant }
    public enum PassingMode { value, reference }

    public class BaseEntry
    {
        public string Token;
        public int depth;
        public string lexume;
        public string bpString;
    }

    public class VarEntry : BaseEntry
    {
        public VarType variableType;
        public int offset;
        public int size;

        public VarEntry(string lexume, string token, int depth, int offset, int size)
        {
            this.lexume = lexume;
            this.depth = depth;
            this.Token = token;
            this.offset = offset;
            this.size = size;
        }
    }

    public class ConstEntry : BaseEntry
    {
        public int offset;
        public double value;

        public ConstEntry(string lexume, string token, int depth, int offset, int value)
        {
            this.lexume = lexume;
            this.depth = depth;
            this.Token = token;
            this.offset = offset;
            this.value = value;
        }
    }

    public class MethodEntry : BaseEntry
    {
        public int sizeOfLocalVars;
        public int numOfParameters;
        public int sizeOfParameters;

        public MethodEntry(string lexume, string token, int depth, int numOfParameters, int sizeOfLocalVars, int sizeOfParameters)
        {
            this.lexume = lexume;
            this.depth = depth;
            this.Token = token;
            this.sizeOfLocalVars = sizeOfLocalVars;
            this.numOfParameters = numOfParameters;
            this.sizeOfParameters = sizeOfParameters;
        }
    }

    public class ClassEntry : BaseEntry
    {
        public int sizeOfLocalVars;
        public LinkedList<string> methodNames;
        public LinkedList<string> variableNames;

        public ClassEntry(string lexume, string token, int depth)
        {
            this.depth = depth;
            this.lexume = lexume;
            this.Token = token;
            this.sizeOfLocalVars = 0;
            this.methodNames = new LinkedList<string>();
            this.variableNames = new LinkedList<string>();
        }
    }

    public class SymbolTable
    {
        private LinkedList<Object>[] symbolTable;
        public const int PRIME_SIZE = 211;

        public SymbolTable()
        {
            symbolTable = new LinkedList<Object>[PRIME_SIZE];
        }


        public void Insert(string lex, string token, int depth, EntryType entryType, int numOfParam = 0, int sizeOfLocalVars = 0, int offset = 0, int size = 0, int value = 0, int sizeOfParams = 0)
        {
            var locToEnterAt = Hash(lex);
            if (symbolTable[locToEnterAt] != null)
            {
                BaseEntry tempEntry = new BaseEntry();
                if (entryType == EntryType.VarEntry)
                    tempEntry = new VarEntry(lex, token, depth, offset, size);
                else if (entryType == EntryType.ClassEntry)
                    tempEntry = new ClassEntry(lex, token, depth);
                else if (entryType == EntryType.Constant)
                    tempEntry = new ConstEntry(lex, token, depth, offset, value);
                else if (entryType == EntryType.MethodEntry)
                    tempEntry = new MethodEntry(lex, token, depth, numOfParam, sizeOfLocalVars, sizeOfParams);
                switch (entryType)
                {
                    case EntryType.VarEntry:
                        tempEntry = new VarEntry(lex, token, depth, offset, size);
                        break;
                    case EntryType.ClassEntry:
                        tempEntry = new ClassEntry(lex, token, depth);
                        break;
                    case EntryType.Constant:
                        tempEntry = new ConstEntry(lex, token, depth, offset, value);
                        break;
                    case EntryType.MethodEntry:
                        tempEntry = new MethodEntry(lex, token, depth, numOfParam, sizeOfLocalVars, sizeOfParams);
                        break;
                }
                symbolTable[locToEnterAt].AddLast(tempEntry);
            }
            else
            {
                BaseEntry tempEntry = new BaseEntry();

                switch (entryType)
                {
                    case EntryType.VarEntry:
                        tempEntry = new VarEntry(lex, token, depth, offset, size);
                        break;
                    case EntryType.ClassEntry:
                        tempEntry = new ClassEntry(lex, token, depth);
                        break;
                    case EntryType.Constant:
                        tempEntry = new ConstEntry(lex, token, depth, offset, value);
                        break;
                    case EntryType.MethodEntry:
                        tempEntry = new MethodEntry(lex, token, depth, numOfParam, sizeOfLocalVars, sizeOfParams);
                        break;
                }

                symbolTable[locToEnterAt] = new LinkedList<Object>();
                symbolTable[locToEnterAt].AddLast(tempEntry);
            }
        }

        public LinkedList<Object> search(string lexume)
        {
            for (int i = 0; i < symbolTable.Count(); i++)
            {
                if (symbolTable[i] != null && symbolTable[i].FirstOrDefault(t => ((BaseEntry)t).lexume == lexume) != null)
                    return symbolTable[i];
            }
            return null;
        }

        public void DeleteDepth(int depth)
        {
            for (int i = 0; i < symbolTable.Count(); i++)
            {
                if (symbolTable[i] == null)
                    continue;
                var matchedEntries = (symbolTable[i]).Where(t => ((BaseEntry)t).depth == depth).ToList();
                int x = 0;
                while (x < matchedEntries.Count())
                {
                    symbolTable[i].Remove(matchedEntries[x]);
                    x++;
                }
            }
        }


        private long Hash(string lexumeTohash)
        {
            long h = 0, g;

            for (int counter = 0; counter < lexumeTohash.Length; counter++)
            {
                h = (h << 4) + (int)lexumeTohash[counter];
                if (Convert.ToBoolean(g = h & 0xf0000000))
                {
                    h = h ^ (g >> 24);
                    h = h ^ g;
                }
            }
            return h % PRIME_SIZE;
        }

    }
}


