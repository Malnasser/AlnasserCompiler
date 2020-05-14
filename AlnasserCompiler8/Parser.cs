using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AlnasserCompiler
{

    class Parser
    {
        public static List<KeyValuePair<string, VarType>> paramTracker = new List<KeyValuePair<string, VarType>>();

        public static void Parse(List<output> tokenListIn, SymbolTable symTab)
        {
            global.globalSymbolbTable = symTab;
            global.tokenList = tokenListIn;
            GetNextToken();
            Prog();
            if (global.currentTok != LexicalAnayzer.TokenType.eofT.ToString())
                throw new Exception("eof token: " + global.currentTok);
            symTab = global.globalSymbolbTable;
        }

        private static void Match(string expectedToken)
        {
            if (expectedToken == global.currentTok)
                GetNextToken();
        }

        private static void GetNextToken()
        {
            global.currentTok = global.tokenList[global.tokenCounter].token;
            global.ParselineCounter = global.tokenList[global.tokenCounter].lineNumber;
            global.currentLex = global.tokenList[global.tokenCounter].lexume;
            global.tokenCounter++;
        }

        private static void MainClass()
        {
            Match(LexicalAnayzer.ReservedTypes.finalT.ToString());
            Match(LexicalAnayzer.ReservedTypes.classT.ToString());
            Match(LexicalAnayzer.TokenType.idT.ToString());
            Match(LexicalAnayzer.TokenType.lBraceT.ToString());

            global.globalSymbolbTable.Insert(global.tokenList[global.tokenCounter - 3].lexume, global.tokenList[global.tokenCounter - 3].token, global.depthCounter, EntryType.ClassEntry);
            global.currentClass = global.tokenList[global.tokenCounter - 3].lexume;
            global.depthCounter++;

            Match(LexicalAnayzer.ReservedTypes.publicT.ToString());
            Match(LexicalAnayzer.ReservedTypes.staticT.ToString());
            Match(LexicalAnayzer.ReservedTypes.voidT.ToString());
            var mainClassName = global.currentLex;
            Emit("Proc " + mainClassName);
            Match(LexicalAnayzer.ReservedTypes.mainT.ToString());
            Match(LexicalAnayzer.TokenType.lParT.ToString());
            Match(LexicalAnayzer.ReservedTypes.StringT.ToString());
            Match(LexicalAnayzer.TokenType.lBracketT.ToString());
            Match(LexicalAnayzer.TokenType.rBracketT.ToString());
            Match(LexicalAnayzer.TokenType.idT.ToString());
            Match(LexicalAnayzer.TokenType.rParT.ToString());
            Match(LexicalAnayzer.TokenType.lBraceT.ToString());

            global.globalSymbolbTable.Insert(global.tokenList[global.tokenCounter - 9].lexume, global.tokenList[global.tokenCounter - 9].token, global.depthCounter, EntryType.MethodEntry);

            var parentClass = (ClassEntry)global.globalSymbolbTable.search(global.currentClass).First();
            parentClass.methodNames.AddLast(global.tokenList[global.tokenCounter - 9].lexume);

            SequenceOfStatements();

            Match(LexicalAnayzer.TokenType.rBraceT.ToString());
            Emit("Endp " + mainClassName);

            global.globalSymbolbTable.DeleteDepth(global.depthCounter);
            global.depthCounter--;

            Match(LexicalAnayzer.TokenType.rBraceT.ToString());
            global.globalSymbolbTable.DeleteDepth(global.depthCounter);
            global.depthCounter--;

        }

        private static void Prog()
        {
            MoreClasses();
            MainClass();
        }

        private static void MoreClasses()
        {
            if (global.currentTok == LexicalAnayzer.ReservedTypes.classT.ToString())
            {
                ClassDecl();
                MoreClasses();
            }
        }

        private static void ClassDecl()
        {
            global.offsetTracker = 0;
            global.variableTracker.RemoveAll(t => t.Key == t.Key);
            global.varTrackerAmountLastAdded = 0;

            Match(LexicalAnayzer.ReservedTypes.classT.ToString());
            Match(LexicalAnayzer.TokenType.idT.ToString());
            global.currentClass = global.tokenList[global.tokenCounter - 2].lexume;
            ClassDeclPrime();
        }

        private static void ClassDeclPrime()
        {
            ClassEntry classEntered = new ClassEntry("", "", 0);
            if (global.currentTok == LexicalAnayzer.TokenType.lBraceT.ToString())
            {
                Match(LexicalAnayzer.TokenType.lBraceT.ToString());

                global.depthCounter++;

                VarDecl();
                global.globalSymbolbTable.Insert(global.currentClass, LexicalAnayzer.ReservedTypes.classT.ToString(), global.depthCounter - 1, EntryType.ClassEntry);

                var itemsAtDepth = global.globalSymbolbTable.search(global.currentClass);

                foreach (var item in itemsAtDepth)
                {
                    if (((BaseEntry)item).depth == global.depthCounter - 1)
                    {
                        classEntered = (ClassEntry)item;
                        break;
                    }
                }

                int localVarSize = 0;
                LinkedList<string> varNames = new LinkedList<string>();

                if (global.variableTracker != null)
                {
                    foreach (var entry in global.variableTracker)
                    {
                        if (entry.Value == VarType.intT)
                            localVarSize += 2;
                        else
                            localVarSize += 1;
                        varNames.AddLast(entry.Key);
                    }

                    classEntered.sizeOfLocalVars = localVarSize;
                    classEntered.variableNames = varNames;
                }
                MethodDecl();
                Match(LexicalAnayzer.TokenType.rBraceT.ToString());
                global.globalSymbolbTable.DeleteDepth(global.depthCounter);
                global.depthCounter--;
            }
            else if (global.currentTok == LexicalAnayzer.ReservedTypes.extendsT.ToString())
            {
                Match(LexicalAnayzer.ReservedTypes.extendsT.ToString());
                Match(LexicalAnayzer.TokenType.idT.ToString());
                Match(LexicalAnayzer.TokenType.lBraceT.ToString());
                global.depthCounter++;

                VarDecl();
                global.globalSymbolbTable.Insert(global.currentClass, LexicalAnayzer.ReservedTypes.classT.ToString(), global.depthCounter - 1, EntryType.ClassEntry);

                var itemsAtDepth = global.globalSymbolbTable.search(global.currentClass);

                foreach (var item in itemsAtDepth)
                {
                    if (((BaseEntry)item).depth == global.depthCounter - 1)
                    {
                        classEntered = (ClassEntry)item;
                        break;
                    }
                }

                int localVarSize = 0;
                LinkedList<string> varNames = new LinkedList<string>();

                if (global.variableTracker != null)
                {
                    foreach (var entry in global.variableTracker)
                    {
                        if (entry.Value == VarType.intT)
                            localVarSize += 2;
                        else
                            localVarSize += 1;
                        varNames.AddLast(entry.Key);
                    }

                    classEntered.sizeOfLocalVars = localVarSize;
                    classEntered.variableNames = varNames;
                }
                MethodDecl();
                Match(LexicalAnayzer.TokenType.rBraceT.ToString());
                global.globalSymbolbTable.DeleteDepth(global.depthCounter);
                global.depthCounter--;
            }
        }

        private static void VarDecl(int localVarSize = 0)
        {
            if (global.currentTok == LexicalAnayzer.ReservedTypes.intT.ToString() || global.currentTok == LexicalAnayzer.ReservedTypes.booleanT.ToString() || global.currentTok == LexicalAnayzer.ReservedTypes.voidT.ToString())
            {
                Type();
                IdentifierList();
                int lcv = 0;
                foreach (var entry in global.variableTracker)
                {
                    if (lcv < global.varTrackerAmountLastAdded)
                    {
                        lcv++;
                        continue;
                    }
                    if (entry.Value == VarType.intT)
                    {
                        global.globalSymbolbTable.Insert(entry.Key, LexicalAnayzer.ReservedTypes.intT.ToString(), global.depthCounter, EntryType.VarEntry, offset: -global.offsetTracker, size: 2);
                        global.offsetTracker += 2;
                    }
                    else if (entry.Value == VarType.boolenT)
                    {
                        global.globalSymbolbTable.Insert(entry.Key, LexicalAnayzer.ReservedTypes.booleanT.ToString(), global.depthCounter, EntryType.VarEntry, offset: -global.offsetTracker, size: 1);
                        global.offsetTracker++;
                    }
                    else
                    {
                        global.globalSymbolbTable.Insert(entry.Key, LexicalAnayzer.ReservedTypes.voidT.ToString(), global.depthCounter, EntryType.VarEntry);
                    }
                    lcv++;
                    global.varTrackerAmountLastAdded++;
                }

                Match(LexicalAnayzer.TokenType.semiT.ToString());

                VarDecl();
            }
            else if (global.currentTok == LexicalAnayzer.ReservedTypes.finalT.ToString())
            {
                var negativeVal = false;
                var insertOffset = 0;

                Match(LexicalAnayzer.ReservedTypes.finalT.ToString());
                Type();
                var constLex = global.currentLex;
                Match(LexicalAnayzer.TokenType.idT.ToString());
                Match(LexicalAnayzer.TokenType.assignOpT.ToString());
                if (global.currentLex == "-")
                {
                    negativeVal = true;
                    Match(LexicalAnayzer.TokenType.addOpT.ToString());
                }
                var constVal = global.currentLex;
                var priorOffsset = global.offsetTracker;
                Match(LexicalAnayzer.TokenType.numT.ToString());
                Match(LexicalAnayzer.TokenType.semiT.ToString());

                insertOffset = negativeVal == true ? 6 : 5;

                global.variableTracker.Add(new KeyValuePair<string, VarType>(global.tokenList[global.tokenCounter - insertOffset].lexume, global.varEntryType));
                if (global.varEntryType == VarType.intT)
                {
                    global.globalSymbolbTable.Insert(global.tokenList[global.tokenCounter - insertOffset].lexume, LexicalAnayzer.ReservedTypes.intT.ToString(), global.depthCounter, EntryType.VarEntry,
                        value: global.tokenList[global.tokenCounter - 3].lexume.Contains(".") ? (int)global.tokenList[global.tokenCounter - 3].valueR : (int)global.tokenList[global.tokenCounter - 3].value, offset: -global.offsetTracker);
                    global.varTrackerAmountLastAdded++;
                    global.offsetTracker += 2;
                }
                else if (global.varEntryType == VarType.boolenT)
                {
                    global.globalSymbolbTable.Insert(global.tokenList[global.tokenCounter - insertOffset].lexume, LexicalAnayzer.ReservedTypes.booleanT.ToString(), global.depthCounter, EntryType.VarEntry,
                        value: (int)global.tokenList[global.tokenCounter - 3].value, offset: -global.offsetTracker);
                    global.varTrackerAmountLastAdded++;
                    global.offsetTracker++;
                }

                var tempLexume = "_bp-" + (priorOffsset + 2 * global.tempNum);
                var currEnt = global.globalSymbolbTable.search(global.currentLex).First(t => ((BaseEntry)t).depth == global.depthCounter);
                ((BaseEntry)currEnt).bpString = tempLexume;
                global.tempNum++;
                Emit(tempLexume + " = " + constVal);

                Emit(GetBpEmit(constLex) + " = " + tempLexume);
                VarDecl();
            }
        }

        private static void IdentifierList()
        {
            Match(LexicalAnayzer.TokenType.idT.ToString());
            global.variableTracker.Add(new KeyValuePair<string, VarType>(global.tokenList[global.tokenCounter - 2].lexume, global.varEntryType));
            IdentifierListPrime();
        }

        private static void IdentifierListPrime()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.commaT.ToString())
            {
                Match(LexicalAnayzer.TokenType.commaT.ToString());
                Match(LexicalAnayzer.TokenType.idT.ToString());
                global.variableTracker.Add(new KeyValuePair<string, VarType>(global.tokenList[global.tokenCounter - 2].lexume, global.varEntryType));
                IdentifierListPrime();
            }
        }

        private static void Type()
        {
            if (global.currentTok == LexicalAnayzer.ReservedTypes.intT.ToString())
            {
                global.varEntryType = VarType.intT;
                Match(LexicalAnayzer.ReservedTypes.intT.ToString());
            }
            else if (global.currentTok == LexicalAnayzer.ReservedTypes.booleanT.ToString())
            {
                global.varEntryType = VarType.boolenT;
                Match(LexicalAnayzer.ReservedTypes.booleanT.ToString());
            }
            else if (global.currentTok == LexicalAnayzer.ReservedTypes.voidT.ToString())
            {
                global.varEntryType = VarType.voidT;
                Match(LexicalAnayzer.ReservedTypes.voidT.ToString());
            }

        }

        private static void MethodDecl()
        {
            global.tempNum = 1;
            int localVarSize = 0;
            VarType returnType = VarType.voidT;
            global.offsetTracker = 2;
            global.variableTracker.RemoveAll(t => t.Key == t.Key);
            global.varTrackerAmountLastAdded = 0;
            paramTracker.RemoveAll(t => t.Key == t.Key);

            if (global.currentTok == LexicalAnayzer.ReservedTypes.publicT.ToString())
            {
                Match(LexicalAnayzer.ReservedTypes.publicT.ToString());
                Type();
                returnType = global.varEntryType;
                Emit("Proc " + global.currentLex);
                Match(LexicalAnayzer.TokenType.idT.ToString());
                global.currentMethod = global.tokenList[global.tokenCounter - 2].lexume;
                Match(LexicalAnayzer.TokenType.lParT.ToString());

                global.depthCounter++;

                FormalList();
                foreach (var param in paramTracker)
                {
                    if (param.Value == VarType.intT)
                    {
                        global.globalSymbolbTable.Insert(param.Key, LexicalAnayzer.ReservedTypes.intT.ToString(), global.depthCounter, EntryType.VarEntry, offset: localVarSize + 4);
                        localVarSize += 2;
                    }
                    else if (param.Value == VarType.boolenT)
                    {
                        global.globalSymbolbTable.Insert(param.Key, LexicalAnayzer.ReservedTypes.booleanT.ToString(), global.depthCounter, EntryType.VarEntry, offset: localVarSize + 4);
                        localVarSize++;
                    }
                    else
                    {
                        global.globalSymbolbTable.Insert(param.Key, LexicalAnayzer.ReservedTypes.voidT.ToString(), global.depthCounter, EntryType.VarEntry);
                    }
                }

                Match(LexicalAnayzer.TokenType.rParT.ToString());
                Match(LexicalAnayzer.TokenType.lBraceT.ToString());
                VarDecl(localVarSize);
                foreach (var item in global.variableTracker)
                    localVarSize += item.Value == VarType.intT ? 2 : 1;

                int paramSize = 0;
                foreach (var item in paramTracker)
                    paramSize += item.Value == VarType.intT ? 2 : 1;

                global.globalSymbolbTable.Insert(global.currentMethod, returnType.ToString(), global.depthCounter - 1, EntryType.MethodEntry, paramTracker.Count(), localVarSize, sizeOfParams: paramSize);

                SequenceOfStatements();
                global.codeGenSymbolTable.Insert(global.currentMethod, returnType.ToString(), global.depthCounter - 1, EntryType.MethodEntry, paramTracker.Count(), localVarSize +
                    (global.tempNum > 1 ? 2 * (global.tempNum - 1) : 0), sizeOfParams: paramSize);


                Match(LexicalAnayzer.ReservedTypes.returnT.ToString());
                BaseEntry Eplace = new BaseEntry();
                Expr(ref Eplace);
                if (Eplace.lexume != null)
                    Emit("_AX = " + GetBpEmit(Eplace.lexume));
                Match(LexicalAnayzer.TokenType.semiT.ToString());
                Match(LexicalAnayzer.TokenType.rBraceT.ToString());
                Emit("EndP " + global.currentMethod);

                global.globalSymbolbTable.DeleteDepth(global.depthCounter);
                global.depthCounter--;
                MethodDecl();
            }
        }

        private static void FormalList()
        {
            if (global.currentTok == LexicalAnayzer.ReservedTypes.intT.ToString() || global.currentTok == LexicalAnayzer.ReservedTypes.booleanT.ToString() || global.currentTok == LexicalAnayzer.ReservedTypes.voidT.ToString())
            {
                Type();
                Match(LexicalAnayzer.TokenType.idT.ToString());
                paramTracker.Add(new KeyValuePair<string, VarType>(global.tokenList[global.tokenCounter - 2].lexume, global.varEntryType));
                FormalRest();
            }
        }

        private static void FormalRest()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.commaT.ToString())
            {
                Match(LexicalAnayzer.TokenType.commaT.ToString());
                Type();
                Match(LexicalAnayzer.TokenType.idT.ToString());
                paramTracker.Add(new KeyValuePair<string, VarType>(global.tokenList[global.tokenCounter - 2].lexume, global.varEntryType));
                FormalRest();
            }
        }

        private static void SequenceOfStatements()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.idT.ToString())
            {
                Statement();
                Match(LexicalAnayzer.TokenType.semiT.ToString());
                StatTail();
            }
        }

        private static void StatTail()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.idT.ToString())
            {
                Statement();
                Match(LexicalAnayzer.TokenType.semiT.ToString());
                StatTail();
            }
        }

        private static void Statement()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.idT.ToString())
            {
                if (global.currentLex != "read" && global.currentLex != "write" && global.currentLex != "writeln")
                    AssignStat();
                else
                    IOStat();
            }
        }

        private static void AssignStat()
        {
            var idtsearch = global.globalSymbolbTable.search(global.currentLex);
            BaseEntry Eplace = new BaseEntry();

            var idPtr = global.currentLex;
            var isVarEntry = idtsearch.Any(t => t.GetType() == typeof(VarEntry) && ((BaseEntry)t).depth == global.depthCounter);

            if (isVarEntry)
            {
                var assignVar = global.currentLex;
                Match(LexicalAnayzer.TokenType.idT.ToString());
                Match(LexicalAnayzer.TokenType.assignOpT.ToString());

                if (global.currentTok == LexicalAnayzer.TokenType.idT.ToString() && global.globalSymbolbTable.search(global.currentLex).Any(t => t.GetType() == typeof(ClassEntry)))
                {
                    MethodCall();
                    Emit(GetBpEmit(assignVar) + " = _AX");
                }

                else
                {
                    Expr(ref Eplace);
                    var emitString = GetBpEmit(idPtr) + " = " + GetBpEmit(Eplace.lexume);
                    ////GetBpEmit(idPtr);
                    Emit(emitString);
                }
            }
            else
                MethodCall();

        }

        private static void MethodCall()
        {
            ClassName();
            Match(LexicalAnayzer.TokenType.periodT.ToString());
            var methodName = global.currentLex;
            Match(LexicalAnayzer.TokenType.idT.ToString());
            Match(LexicalAnayzer.TokenType.lParT.ToString());
            Params();
            Match(LexicalAnayzer.TokenType.rParT.ToString());
            Emit("Call " + methodName);
        }

        private static void ClassName()
        {
            Match(LexicalAnayzer.TokenType.idT.ToString());
        }

        private static void Params()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.idT.ToString())
            {
                global.paramStack.Push((VarEntry)global.globalSymbolbTable.search(global.currentLex).First(t => ((BaseEntry)t).depth == global.depthCounter));
                Match(LexicalAnayzer.TokenType.idT.ToString());
                ParamsTail();
            }
            else if (global.currentTok == LexicalAnayzer.TokenType.numT.ToString())
            {
                global.paramStack.Push((VarEntry)global.globalSymbolbTable.search(global.currentLex).First(t => ((BaseEntry)t).depth == global.depthCounter));
                Match(LexicalAnayzer.TokenType.numT.ToString());
                ParamsTail();
            }
        }

        private static void ParamsTail()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.commaT.ToString())
            {
                Match(LexicalAnayzer.TokenType.commaT.ToString());
                if (LexicalAnayzer.TokenType.idT.ToString() == global.currentTok)
                {
                    global.paramStack.Push((VarEntry)global.globalSymbolbTable.search(global.currentLex).First(t => ((BaseEntry)t).depth == global.depthCounter));
                    Match(LexicalAnayzer.TokenType.idT.ToString());
                }
                else
                {
                    global.paramStack.Push((VarEntry)global.globalSymbolbTable.search(global.currentLex).First(t => ((BaseEntry)t).depth == global.depthCounter));
                    Match(LexicalAnayzer.TokenType.numT.ToString());
                }
                ParamsTail();
            }
            Emit("Push _bp" + global.paramStack.Pop().offset);
        }

        private static void IOStat()
        {
            if (global.currentLex == "read")
            {
                InStat();
            }
            else if (global.currentLex == "writeln" || global.currentLex == "write")
            {
                OutStat();
            }
        }

        #region assignment8work
        private static void InStat()
        {
            Match(LexicalAnayzer.TokenType.idT.ToString());
            Match(LexicalAnayzer.TokenType.lParT.ToString());
            IdList();
            Match(LexicalAnayzer.TokenType.rParT.ToString());
        }

        private static void OutStat()
        {
            if (global.currentLex == "write")
            {
                global.wasWriteLine = false;
                Match(LexicalAnayzer.TokenType.idT.ToString());
                Match(LexicalAnayzer.TokenType.lParT.ToString());
                WriteList();
                Match(LexicalAnayzer.TokenType.rParT.ToString());
            }
            else 
            {
                global.wasWriteLine = true;
                Match(LexicalAnayzer.TokenType.idT.ToString());
                Match(LexicalAnayzer.TokenType.lParT.ToString());
                WriteList();
                Match(LexicalAnayzer.TokenType.rParT.ToString());
            }
        }

        private static void IdList()
        {
            var currLex = (VarEntry)global.globalSymbolbTable.search(global.currentLex).First(t => ((BaseEntry)t).depth == global.depthCounter);
            Emit("rdi " + (currLex.offset > 0 ? "_bp+" + currLex.offset : "_bp" + currLex.offset));
            Match(LexicalAnayzer.TokenType.idT.ToString());
            IdListTail();
        }

        private static void IdListTail()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.commaT.ToString())
            {
                Match(LexicalAnayzer.TokenType.commaT.ToString());
                var currLex = (VarEntry)global.globalSymbolbTable.search(global.currentLex).First(t => ((BaseEntry)t).depth == global.depthCounter);
                Emit("rdi " + (currLex.offset > 0 ? "_bp+" + currLex.offset : "_bp" + currLex.offset));
                Match(LexicalAnayzer.TokenType.idT.ToString());
                IdListTail();
            }
        }

        private static void WriteList()
        {
            WriteToken();
            WriteListTail();
        }

        private static void WriteListTail()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.commaT.ToString())
            {
                Match(LexicalAnayzer.TokenType.commaT.ToString());
                WriteToken();
                WriteListTail();
            }
        }

        private static void WriteToken()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.idT.ToString())
            {
                var currLex = (VarEntry)global.globalSymbolbTable.search(global.currentLex).First(t => ((BaseEntry)t).depth == global.depthCounter);
                Emit("wri " + (currLex.offset > 0 ? "_bp+" + currLex.offset : "_bp" + currLex.offset));
                Match(LexicalAnayzer.TokenType.idT.ToString());
            }
            else if (global.currentTok == LexicalAnayzer.TokenType.numT.ToString())
            {
                Emit("wri " + global.currentLex);
                Match(LexicalAnayzer.TokenType.numT.ToString());
            }
            else if (global.currentTok == LexicalAnayzer.TokenType.literalT.ToString()) 
            {
                var litList = global.tokenList.Where(t => t.literal != null).ToList();
                var posOfLit = litList.IndexOf(litList.First(t => t.literal == global.currentLex));

                Emit("wrs " + "_S" + posOfLit);
                Match(LexicalAnayzer.TokenType.literalT.ToString());
            }
            if (global.wasWriteLine)
                Emit("wrln");
        }

        #endregion

        private static void Expr(ref BaseEntry Eplace)
        {

            if (global.currentTok == LexicalAnayzer.TokenType.idT.ToString() ||
                global.currentTok == LexicalAnayzer.TokenType.numT.ToString() ||
                global.currentTok == LexicalAnayzer.TokenType.lParT.ToString() ||
                global.currentLex.First() == '!' ||
                global.currentLex.First() == '-' ||
                global.currentLex == "true" || global.currentLex == "false")
            {
                Relation(ref Eplace);
            }

        }

        private static void Relation(ref BaseEntry Eplace)
        {
            SimpleExpr(ref Eplace);
        }

        private static void SimpleExpr(ref BaseEntry Eplace)
        {
            var Tplace = new BaseEntry();
            Term(ref Tplace);
            MoreTerm(ref Tplace);
            Eplace = Tplace;
        }

        private static void MoreTerm(ref BaseEntry Eplace)
        {
            var emitString = "";
            if (global.currentTok == LexicalAnayzer.TokenType.addOpT.ToString())
            {
                BaseEntry tempEntry = new BaseEntry();
                BaseEntry mfPlace = new BaseEntry();
                var currMethodEntry = (MethodEntry)global.globalSymbolbTable.search(global.currentMethod).First(t => t.GetType() == typeof(MethodEntry) && ((MethodEntry)t).depth == global.depthCounter - 1);
                tempEntry.lexume = "_bp-" + (currMethodEntry.sizeOfLocalVars + 2 * global.tempNum);
                global.tempNum++;
                emitString = tempEntry.lexume + " = " + GetBpEmit(Eplace.lexume) + " " + GetBpEmit(global.currentLex);
                Addop();
                Term(ref mfPlace);
                emitString += " " + GetBpEmit(mfPlace.lexume);
                Eplace = tempEntry;
                Emit(emitString);
                MoreTerm(ref Eplace);
            }
        }

        private static void Term(ref BaseEntry Tplace)
        {
            BaseEntry Fplace = new BaseEntry();
            Factor(ref Fplace);
            MoreFactor(ref Fplace);
            Tplace = Fplace;
        }

        private static void MoreFactor(ref BaseEntry Eplace)
        {
            string emitString;
            if (global.currentTok == LexicalAnayzer.TokenType.mulOpT.ToString())
            {
                var Mfplace = new BaseEntry();
                var tempEntry = new BaseEntry();
                var currMethodEntry = (MethodEntry)global.globalSymbolbTable.search(global.currentMethod).First(t => t.GetType() == typeof(MethodEntry) && ((MethodEntry)t).depth == global.depthCounter - 1);
                tempEntry.lexume = "_bp-" + (currMethodEntry.sizeOfLocalVars + 2 * global.tempNum);
                global.tempNum++;

                emitString = tempEntry.lexume + " = " + GetBpEmit(Eplace.lexume) + " " + GetBpEmit(global.currentLex);
                Mulop();
                Factor(ref Mfplace);
                emitString += " " + GetBpEmit(Mfplace.lexume);
                Eplace = tempEntry;
                Emit(emitString);
                MoreFactor(ref Eplace);
            }
        }

        private static void Factor(ref BaseEntry Tplace)
        {
            if (global.currentTok == LexicalAnayzer.TokenType.idT.ToString())
            {
                var idtsearch = global.globalSymbolbTable.search(global.currentLex);
                Tplace.lexume = global.currentLex;
                Match(LexicalAnayzer.TokenType.idT.ToString());
            }
            else if (global.currentTok == LexicalAnayzer.TokenType.numT.ToString())
            {
                var emitString = "";
                var currMethodEntry = (MethodEntry)global.globalSymbolbTable.search(global.currentMethod).First(t => t.GetType() == typeof(MethodEntry) && ((MethodEntry)t).depth == global.depthCounter - 1);
                Tplace.lexume = "_bp-" + (currMethodEntry.sizeOfLocalVars + 2 * global.tempNum);
                global.tempNum++;
                emitString += Tplace.lexume + " = " + GetBpEmit(global.currentLex);
                Emit(emitString);
                Match(LexicalAnayzer.TokenType.numT.ToString());
            }
            else if (global.currentTok == LexicalAnayzer.TokenType.lParT.ToString())
            {
                Match(LexicalAnayzer.TokenType.lParT.ToString());
                Expr(ref Tplace);
                Match(LexicalAnayzer.TokenType.rParT.ToString());
            }
            else if (global.currentLex.First() == '!')
            {
                Match(LexicalAnayzer.TokenType.relopT.ToString());
                Factor(ref Tplace);
            }
            else if (global.currentLex.First() == '-')
            {
                SignOp();
                var emitString = "";
                var currMethodEntry = (MethodEntry)global.globalSymbolbTable.search(global.currentMethod).First(t => t.GetType() == typeof(MethodEntry) && ((MethodEntry)t).depth == global.depthCounter - 1);
                Tplace.lexume = "_bp-" + (currMethodEntry.sizeOfLocalVars + 2 * global.tempNum);
                var tempLexSave = Tplace.lexume;
                global.tempNum++;
                emitString = Tplace.lexume + " = " + "-" + GetBpEmit(global.currentLex);
                Emit(emitString);
                Factor(ref Tplace);
                Tplace.lexume = tempLexSave;
            }
            else if (global.currentLex == "false")
                Match(LexicalAnayzer.ReservedTypes.falseT.ToString());
            else if (global.currentLex == "true")
                Match(LexicalAnayzer.ReservedTypes.trueT.ToString());
        }

        private static void Addop()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.addOpT.ToString())
            {
                Match(LexicalAnayzer.TokenType.addOpT.ToString());
            }
        }

        private static void Mulop()
        {
            if (global.currentTok == LexicalAnayzer.TokenType.mulOpT.ToString())
            {
                Match(LexicalAnayzer.TokenType.mulOpT.ToString());
            }
        }

        private static void SignOp()
        {
            Match(LexicalAnayzer.TokenType.addOpT.ToString());
        }

        private static void Emit(string line)
        {
            string fileToSaveTo = Program.inputFileSourceCodeName + ".tac";
            using (StreamWriter sr = new StreamWriter(fileToSaveTo, true))
            {
                sr.WriteLine(line);
            }
        }

        private static string GetBpEmit(string lexume)
        {

            var returnString = "";
            var searchResults = global.globalSymbolbTable.search(lexume);
            if (searchResults == null)
            {
                return lexume;
            }
            var resultAtDepth = (VarEntry)searchResults.First(t => ((VarEntry)t).depth == global.depthCounter);
            if (resultAtDepth.offset < 0)
            {
                returnString = "_bp" + resultAtDepth.offset;
            }
            else
            {
                returnString = "_bp+" + resultAtDepth.offset;
            }
            return returnString;
        }

    }
}
