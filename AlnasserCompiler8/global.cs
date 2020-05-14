using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AlnasserCompiler.LexicalAnayzer;

namespace AlnasserCompiler
{


    class global
    {
        #region SymbolTable class variables
        public static List<string> reservedWords = new List<string> {"class", "public", "static", "void", "main", "String", "extends", "return", "int",
            "boolean", "if", "else", "while", "System.out.println", "length", "true", "false", "this", "new", "final"};
        public static List<ReservedTypes> reservedList = new List<ReservedTypes>(Enum.GetValues(typeof(ReservedTypes)).Cast<ReservedTypes>().ToList());

        public static char characterReader;
        public static int symbolTablelineCounter = 1;
        #endregion

        #region Parser class variables
        public static List<output> tokenList = new List<output>();
        public static string currentTok;
        public static string currentLex;
        public static string currentClass;
        public static string currentMethod;
        public static int tokenCounter = 0;
        public static int ParselineCounter = 0;
        public static int depthCounter = 0;
        public static int offsetTracker = 0;
        public static SymbolTable globalSymbolbTable;
        public static Stack<string> variableStack = new Stack<string>();
        public static Stack<VarEntry> paramStack = new Stack<VarEntry>();
        public static int tempNum = 1;
        public static SymbolTable codeGenSymbolTable = new SymbolTable();
        public static bool wasWriteLine = false;

        public static VarType varEntryType;
        public static List<KeyValuePair<string, VarType>> variableTracker = new List<KeyValuePair<string, VarType>>();
        public static int varTrackerAmountLastAdded = 0;
        #endregion
    }
}
