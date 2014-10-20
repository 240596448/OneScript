﻿using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public class MachineInstance
    {
        private List<Scope> _scopes;
        private Stack<IValue> _operationStack;
        private Stack<ExecutionFrame> _callStack;
        private ExecutionFrame _currentFrame;
        private int _lineNumber;
        private Action<int>[] _commands;
        private Stack<ExceptionJumpInfo> _exceptionsStack;
        private Stack<MachineState> _states;
        private LoadedModule _module;

        internal MachineInstance() 
        {
            InitCommands();
            Reset();
        }

        private struct ExceptionJumpInfo
        {
            public int handlerAddress;
            public ExecutionFrame handlerFrame;
        }

        private struct MachineState
        {
            public Scope topScope;
            public ExecutionFrame currentFrame;
            public LoadedModule module;
            public bool hasScope;
            public int lineNumber;
            public IValue[] operationStack;
            public ExecutionFrame[] callStack;
            public ExceptionJumpInfo[] exceptionsStack;
        }

        public void AttachContext(IAttachableContext context, bool detachable)
        {
            IVariable[] vars;
            MethodInfo[] methods;
            IRuntimeContextInstance instance;
            context.OnAttach(this, out vars, out methods, out instance);
            var scope = new Scope()
            {
                Variables = vars,
                Methods = methods,
                Instance = instance,
                Detachable = detachable
            };

            _scopes.Add(scope);

        }

        internal void StateConsistentOperation(Action action)
        {
            PushState();
            try
            {
                action();
            }
            finally
            {
                PopState();
            }
        }

        internal void ExecuteModuleBody()
        {
            if (_module.EntryMethodIndex >= 0)
            {
                PrepareMethodExecution(_module.EntryMethodIndex);
                ExecuteCode();
            }
        }

        internal IValue ExecuteMethod(int methodIndex, IValue[] arguments)
        {
            PrepareMethodExecutionDirect(methodIndex);
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] is IVariable)
                    _currentFrame.Locals[i] = (IVariable)arguments[i];
                else if(arguments[i] == null)
                    _currentFrame.Locals[i] = Variable.Create(GetDefaultArgValue(methodIndex, i));
                else
                    _currentFrame.Locals[i] = Variable.Create(arguments[i]);
            }
            ExecuteCode();

            if (_module.Methods[methodIndex].Signature.IsFunction)
            {
                return _operationStack.Pop();
            }
            else
                return null;
        }

        private IValue GetDefaultArgValue(int methodIndex, int paramIndex)
        {
            var meth = _module.Methods[methodIndex].Signature;
            var param = meth.Params[paramIndex];
            if (!param.HasDefaultValue)
                throw new ApplicationException("Invalid script arguments");

            return _module.Constants[param.DefaultValueIndex];
        }

        internal void SetModule(LoadedModule module)
        {
            _module = module;
        }

        internal void Cleanup()
        {
            Reset();
            GC.Collect();
        }

        private void PushState()
        {
            var stateToSave = new MachineState();
            stateToSave.hasScope = DetachTopScope(out stateToSave.topScope);
            stateToSave.currentFrame = _currentFrame;
            stateToSave.module = _module;
            stateToSave.lineNumber = _lineNumber;
            StackToArray(ref stateToSave.callStack, _callStack);
            StackToArray(ref stateToSave.exceptionsStack, _exceptionsStack);
            StackToArray(ref stateToSave.operationStack, _operationStack);

            _states.Push(stateToSave);

            _callStack.Clear();
            _exceptionsStack.Clear();
            _operationStack.Clear();
        }

        private void StackToArray<T>(ref T[] destination, Stack<T> source)
        {
            if (source != null)
            {
                destination = new T[source.Count];
                source.CopyTo(destination, 0);
            }
        }

        private void RestoreStack<T>(ref Stack<T> destination, T[] source)
        {
            if (source != null)
            {
                destination = new Stack<T>();
                for (int i = source.Length-1; i >=0 ; i--)
                {
                    destination.Push(source[i]);
                }
            }
            else
            {
                destination = null;
            }
        }

        private void PopState()
        {
            var savedState = _states.Pop();
            if (savedState.hasScope)
            {
                if (_scopes[_scopes.Count - 1].Detachable)
                {
                    _scopes[_scopes.Count - 1] = savedState.topScope;
                }
                else
                {
                    _scopes.Add(savedState.topScope);
                }
            }
            else if (_scopes[_scopes.Count - 1].Detachable)
            {
                Scope s;
                DetachTopScope(out s);
            }

            _module = savedState.module;
            _lineNumber = savedState.lineNumber;

            RestoreStack(ref _callStack, savedState.callStack);
            RestoreStack(ref _operationStack, savedState.operationStack);
            RestoreStack(ref _exceptionsStack, savedState.exceptionsStack);

            SetFrame(savedState.currentFrame);
        }

        private void PushFrame()
        {
            if(_currentFrame != null)
                _callStack.Push(_currentFrame);
        }

        private void PopFrame()
        {
            _currentFrame = _callStack.Pop();
        }

        private void SetFrame(ExecutionFrame frame)
        {
            _currentFrame = frame;
        }

        private bool DetachTopScope(out Scope topScope)
        {
            if (_scopes.Count > 0)
            {
                topScope = _scopes[_scopes.Count - 1];
                if (topScope.Detachable)
                {
                    _scopes.RemoveAt(_scopes.Count - 1);
                    return true;
                }
                else
                {
                    topScope = default(Scope);
                    return false;
                }
            }
            else
            {
                throw new InvalidOperationException("Nothing is attached");
            }
        }

        private Scope TopScope
        {
            get
            {
                if (_scopes.Count > 0)
                {
                    return _scopes[_scopes.Count - 1];
                }
                else
                {
                    throw new InvalidOperationException("Nothing is attached");
                }
            }
        }

        private void Reset()
        {
            _scopes = new List<Scope>();
            _operationStack = new Stack<IValue>();
            _callStack = new Stack<ExecutionFrame>();
            _exceptionsStack = new Stack<ExceptionJumpInfo>();
            _states = new Stack<MachineState>();
            _module = null;
            _currentFrame = null;
        }

        private void PrepareMethodExecution(int methodIndex)
        {
            var entryRef = _module.MethodRefs[methodIndex];
            PrepareMethodExecutionDirect(entryRef.CodeIndex);
        }

        private void PrepareMethodExecutionDirect(int methodIndex)
        {
            var methDescr = _module.Methods[methodIndex];
            var frame = new ExecutionFrame();
            frame.MethodName = methDescr.Signature.Name;
            frame.Locals = new IVariable[methDescr.VariableFrameSize];
            for (int i = 0; i < methDescr.VariableFrameSize; i++)
            {
                frame.Locals[i] = Variable.Create(ValueFactory.Create());
            }

            frame.InstructionPointer = methDescr.EntryPoint;
            SetFrame(frame);
        }

        private void ExecuteCode()
        {
            while (true)
            {
                try
                {
                    MainCommandLoop();
                    break;
                }
                catch (RuntimeException exc)
                {
                    if(exc.LineNumber == 0)
                        SetScriptExceptionSource(exc);

                    if (_exceptionsStack.Count == 0)
                    {
                        throw;
                    }

                    var handler = _exceptionsStack.Pop();

                    // Раскрутка стека вызовов
                    while (_currentFrame != handler.handlerFrame)
                    {
                        PopFrame();
                    }

                    SetFrame(handler.handlerFrame);
                    _currentFrame.InstructionPointer = handler.handlerAddress;
                    _currentFrame.LastException = exc;
                    

                }
            }
        }

        private void MainCommandLoop()
        {
            try
            {
                while (_currentFrame.InstructionPointer >= 0
                    && _currentFrame.InstructionPointer < _module.Code.Length)
                {
                    var command = _module.Code[_currentFrame.InstructionPointer];
                    _commands[(int)command.Code](command.Argument);
                }
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch(ScriptInterruptionException)
            {
                throw;
            }
            catch (Exception exc)
            {
                var excWrapper = new ExternalSystemException(exc);
                SetScriptExceptionSource(excWrapper);
                throw excWrapper;
            }
        }

        private void SetScriptExceptionSource(RuntimeException exc)
        {
            exc.LineNumber = _lineNumber;
            if (_module.Source != null)
            {
                exc.Code = _module.Source.GetCodeLine(_lineNumber);
            }
            else
            {
                exc.Code = "<исходный код недоступен>";
            }
        }

        #region Commands

        private void InitCommands()
        {
            _commands = new Action<int>[]
            {
                (i)=>{NextInstruction();},
                PushVar,
                PushConst,
                PushLoc,
                PushRef,
                LoadVar,
                LoadLoc,
                AssignRef,
                Add,
                Sub,
                Mul,
                Div,
                Mod,
                Neg,
                Equals,
                Less,
                Greater,
                LessOrEqual,
                GreaterOrEqual,
                NotEqual,
                Not,
                And,
                Or,
                CallFunc,
                CallProc,
                ArgNum,
                PushDefaultArg,
                ResolveProp,
                ResolveMethodProc,
                ResolveMethodFunc,
                Jmp,
                JmpFalse,
                PushIndexed,
                Return,
                JmpCounter,
                Inc,
                NewInstance,
                PushIterator,
                IteratorNext,
                StopIterator,
                BeginTry,
                EndTry,
                RaiseException,
                LineNum,
                MakeRawValue,
                MakeBool,
                PushTmp,
                PopTmp,

                //built-ins
                Question,
                Bool,
                Number,
                Str,
                Date,
                StrLen,
                TrimL,
                TrimR,
                TrimLR,
                Left,
                Right,
                Mid,
                StrPos,
                UCase,
                LCase,
                Chr,
                ChrCode,
                EmptyStr,
                StrReplace,
                Year,
                Month,
                Day,
                Hour,
                Minute,
                Second,
                BegOfYear,
                BegOfMonth,
                BegOfDay,
                BegOfHour,
                BegOfMinute,
                BegOfQuarter,
                EndOfYear,
                EndOfMonth,
                EndOfDay,
                EndOfHour,
                EndOfMinute,
                EndOfQuarter,
                WeekOfYear,
                DayOfYear,
                this.DayOfWeek,
                AddMonth,
                CurrentDate,
                Integer,
                Round,
                Pow,
                Sqrt,
                ExceptionInfo,
                ExceptionDescr
            };
        }

        #region Simple operations
        private void PushVar(int arg)
        {
            var vm = _module.VariableRefs[arg];
            var scope = _scopes[vm.ContextIndex];
            _operationStack.Push(scope.Variables[vm.CodeIndex]);
            NextInstruction();
        }

        private void PushConst(int arg)
        {
            _operationStack.Push(_module.Constants[arg]);
            NextInstruction();
        }

        private void PushLoc(int arg)
        {
            _operationStack.Push(_currentFrame.Locals[arg]);
            NextInstruction();
        }

        private void PushRef(int arg)
        {
            var vm = _module.VariableRefs[arg];
            var scope = _scopes[vm.ContextIndex];
            var reference = Variable.CreateContextPropertyReference(scope.Instance, vm.CodeIndex);
            _operationStack.Push(reference);
            NextInstruction();
        }

        private void LoadVar(int arg)
        {
            var vm = _module.VariableRefs[arg];
            var scope = _scopes[vm.ContextIndex];
            scope.Variables[vm.CodeIndex].Value = BreakVariableLink(_operationStack.Pop());
            NextInstruction();
        }

        private void LoadLoc(int arg)
        {
            _currentFrame.Locals[arg].Value = BreakVariableLink(_operationStack.Pop());
            NextInstruction();
        }

        private void AssignRef(int arg)
        {
            var value = BreakVariableLink(_operationStack.Pop());

            IVariable reference;
            try
            {
                reference = (IVariable)_operationStack.Pop();
            }
            catch (InvalidCastException)
            {
                throw new WrongStackConditionException();
            }
            reference.Value = value;
            NextInstruction();
        }

        private void Add(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();

            var type1 = op1.DataType;
            if (type1 == DataType.String)
            {
                var result = op1.AsString() + op2.AsString();
                _operationStack.Push(ValueFactory.Create(result));
            }
            else if (type1 == DataType.Date && op2.DataType == DataType.Number)
            {
                var date = op1.AsDate();
                var result = date.AddSeconds((double)op2.AsNumber());
                _operationStack.Push(ValueFactory.Create(result));
            }
            else
            {   // все к числовому типу.
                var result = op1.AsNumber() + op2.AsNumber();
                _operationStack.Push(ValueFactory.Create(result));
            }
            NextInstruction();

        }

        private void Sub(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            if (op1.DataType == DataType.Number)
            {
                _operationStack.Push(ValueFactory.Create(op1.AsNumber() - op2.AsNumber()));
            }
            else if (op1.DataType == DataType.Date && op2.DataType == DataType.Number)
            {
                var date = op1.AsDate();
                var result = date.AddSeconds(-(double)op2.AsNumber());
                _operationStack.Push(ValueFactory.Create(result));
            }
            else if (op1.DataType == DataType.Date && op2.DataType == DataType.Date)
            {
                var span = op1.AsDate() - op2.AsDate();
                _operationStack.Push(ValueFactory.Create((decimal)span.TotalSeconds));
            }
            else
            {   // все к числовому типу.
                var result = op1.AsNumber() - op2.AsNumber();
                _operationStack.Push(ValueFactory.Create(result));
            }
            NextInstruction();
        }

        private void Mul(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.AsNumber() * op2.AsNumber()));
            NextInstruction();
        }

        private void Div(int arg)
        {
            var op2 = _operationStack.Pop().AsNumber();
            var op1 = _operationStack.Pop().AsNumber();

            if (op2 == 0)
            {
                throw RuntimeException.DivideByZero();
            }

            _operationStack.Push(ValueFactory.Create(op1 / op2));
            NextInstruction();
        }

        private void Mod(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.AsNumber() % op2.AsNumber()));
            NextInstruction();
        }

        private void Neg(int arg)
        {
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.AsNumber() * -1));
            NextInstruction();
        }

        private void Equals(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.Equals(op2)));
            NextInstruction();
        }

        private void Less(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) < 0));
            NextInstruction();
        }

        private void Greater(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) > 0));
            NextInstruction();
        }

        private void LessOrEqual(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) <= 0));
            NextInstruction();
        }

        private void GreaterOrEqual(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(op1.CompareTo(op2) >= 0));
            NextInstruction();
        }

        private void NotEqual(int arg)
        {
            var op2 = _operationStack.Pop();
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(!op1.Equals(op2)));
            NextInstruction();
        }

        private void Not(int arg)
        {
            var op1 = _operationStack.Pop();
            _operationStack.Push(ValueFactory.Create(!op1.AsBoolean()));
            NextInstruction();
        }

        private void And(int arg)
        {
            var op = _operationStack.Peek().AsBoolean();
            if (op == false)
            {
                Jmp(arg);
            }
            else
            {
                _operationStack.Pop();
                NextInstruction();
            }
            
        }

        private void Or(int arg)
        {
            var op = _operationStack.Peek().AsBoolean();
            if (op == true)
            {
                Jmp(arg);
            }
            else
            {
                _operationStack.Pop();
                NextInstruction();
            }
        }

        private void CallFunc(int arg)
        {
            MethodCallImpl(arg, true);
            _currentFrame.DiscardReturnValue = false;
        }

        private void CallProc(int arg)
        {
            bool needsDiscarding = MethodCallImpl(arg, false);
            if (needsDiscarding)
                _currentFrame.DiscardReturnValue = true;
            else
                _currentFrame.DiscardReturnValue = false;
        }

        private bool MethodCallImpl(int arg, bool asFunc)
        {
            var methodRef = _module.MethodRefs[arg];
            var scope = _scopes[methodRef.ContextIndex];
            var methInfo = scope.Methods[methodRef.CodeIndex];

            int argCount = (int)_operationStack.Pop().AsNumber();
            IValue[] argValues = new IValue[argCount];

            // fact args
            for (int i = argCount - 1; i >= 0; i--)
            {
                var argValue = _operationStack.Pop();
                if (argValue.DataType == DataType.NotAValidValue)
                {
                    if (i < methInfo.Params.Length)
                    {
                        var constId = methInfo.Params[i].DefaultValueIndex;
                        if (constId == ParameterDefinition.UNDEFINED_VALUE_INDEX)
                            argValue = null;
                        else
                            argValue = _module.Constants[constId];
                    }
                    else
                    {
                        argValue = null;
                    }
                }

                argValues[i] = argValue;

            }

            bool needsDiscarding;

            if (scope.Instance == this.TopScope.Instance)
            {
                var sdo = scope.Instance as ScriptDrivenObject;
                System.Diagnostics.Debug.Assert(sdo != null);

                if (sdo.MethodDefinedInScript(methodRef.CodeIndex))
                {
                    var methDescr = _module.Methods[methodRef.CodeIndex];
                    var frame = new ExecutionFrame();
                    frame.MethodName = methInfo.Name;
                    frame.Locals = new IVariable[methDescr.VariableFrameSize];
                    for (int i = 0; i < methDescr.VariableFrameSize; i++)
                    {
                        if (i < argValues.Length)
                        {
                            var paramDef = methInfo.Params[i];
                            if (argValues[i] is IVariable)
                            {
                                if (paramDef.IsByValue)
                                {
                                    var value = ((IVariable)argValues[i]).Value;
                                    frame.Locals[i] = Variable.Create(value);
                                }
                                else
                                {
                                    frame.Locals[i] = (IVariable)argValues[i];
                                }
                            }
                            else
                            {
                                frame.Locals[i] = Variable.Create(argValues[i]);
                            }

                        }
                        else if (i < methInfo.Params.Length && methInfo.Params[i].HasDefaultValue)
                        {
                            if (methInfo.Params[i].DefaultValueIndex == ParameterDefinition.UNDEFINED_VALUE_INDEX)
                            {
                                frame.Locals[i] = Variable.Create(ValueFactory.Create());
                            }
                            else
                            {
                                frame.Locals[i] = Variable.Create(_module.Constants[methInfo.Params[i].DefaultValueIndex]);
                            }
                        }
                        else
                            frame.Locals[i] = Variable.Create(ValueFactory.Create());

                    }

                    frame.InstructionPointer = methDescr.EntryPoint;
                    PushFrame();
                    SetFrame(frame);

                    needsDiscarding = methInfo.IsFunction;
                }
                else
                {
                    needsDiscarding = false;
                    CallContext(scope.Instance, methodRef.CodeIndex, ref methInfo, argValues, asFunc);
                }

            }
            else
            {
                // при вызове библиотечного метода (из другого scope)
                // статус вызова текущего frame не должен изменяться.
                //
                needsDiscarding = _currentFrame.DiscardReturnValue;
                CallContext(scope.Instance, methodRef.CodeIndex, ref methInfo, argValues, asFunc);
            }

            return needsDiscarding;
        }

        private void CallContext(IRuntimeContextInstance instance, int index, ref MethodInfo methInfo, IValue[] argValues, bool asFunc)
        {
            IValue[] realArgs;
            if (!instance.DynamicMethodSignatures)
            {
                realArgs = new IValue[methInfo.ArgCount];
                for (int i = 0; i < realArgs.Length; i++)
                {
                    if (i < argValues.Length)
                    {
                        realArgs[i] = argValues[i];
                    }
                    else
                    {
                        realArgs[i] = null;
                    }
                }
            }
            else
            {
                realArgs = argValues;
            }

            if (asFunc)
            {
                IValue retVal;
                instance.CallAsFunction(index, realArgs, out retVal);
                _operationStack.Push(retVal);
            }
            else
            {
                instance.CallAsProcedure(index, realArgs);
            }
            NextInstruction();
        }

        private void ArgNum(int arg)
        {
            _operationStack.Push(ValueFactory.Create(arg));
            NextInstruction();
        }

        private void PushDefaultArg(int arg)
        {
            _operationStack.Push(ValueFactory.CreateInvalidValueMarker());
            NextInstruction();
        }

        private void ResolveProp(int arg)
        {
            var objIValue = _operationStack.Pop();
            if (objIValue.DataType != DataType.Object)
            {
                throw RuntimeException.ValueIsNotObjectException();
            }

            var context = objIValue.AsObject();
            var propName = _module.Constants[arg].AsString();
            var propNum = context.FindProperty(propName);

            var propReference = Variable.CreateContextPropertyReference(context, propNum);
            _operationStack.Push(propReference);
            NextInstruction();

        }

        private void ResolveMethodProc(int arg)
        {
            IRuntimeContextInstance context;
            int methodId;
            IValue[] argValues;
            PrepareContextCallArguments(arg, out context, out methodId, out argValues);

            context.CallAsProcedure(methodId, argValues);
            NextInstruction();

        }

        private void ResolveMethodFunc(int arg)
        {
            IRuntimeContextInstance context;
            int methodId;
            IValue[] argValues;
            PrepareContextCallArguments(arg, out context, out methodId, out argValues);

            if (!context.DynamicMethodSignatures && !context.GetMethodInfo(methodId).IsFunction)
            {
                throw RuntimeException.UseProcAsAFunction();
            }

            IValue retVal;
            context.CallAsFunction(methodId, argValues, out retVal);
            _operationStack.Push(retVal);
            NextInstruction();
        }

        private void PrepareContextCallArguments(int arg, out IRuntimeContextInstance context, out int methodId, out IValue[] argValues)
        {
            var argCount = (int)_operationStack.Pop().AsNumber();
            IValue[] factArgs = new IValue[argCount];
            for (int i = argCount - 1; i >= 0; i--)
            {
                factArgs[i] = _operationStack.Pop();
            }

            var objIValue = _operationStack.Pop();
            if (objIValue.DataType != DataType.Object)
            {
                throw RuntimeException.ValueIsNotObjectException();
            }

            context = objIValue.AsObject();
            var methodName = _module.Constants[arg].AsString();
            methodId = context.FindMethod(methodName);
            var methodInfo = context.GetMethodInfo(methodId);

            if(context.DynamicMethodSignatures)
                argValues = new IValue[argCount];
            else
                argValues = new IValue[methodInfo.Params.Length];

            bool[] signatureCheck = new bool[argCount];

            // fact args
            for (int i = 0; i < factArgs.Length; i++)
            {
                var argValue = factArgs[i];
                if (argValue.DataType == DataType.NotAValidValue)
                {
                    argValue = null;
                    signatureCheck[i] = false;
                }
                else
                {
                    signatureCheck[i] = true;
                    if (context.DynamicMethodSignatures)
                    {
                        argValues[i] = BreakVariableLink(argValue);
                    }
                    else
                    {
                        if (methodInfo.Params[i].IsByValue)
                            argValues[i] = BreakVariableLink(argValue);
                        else
                            argValues[i] = argValue;
                    }
                }

            }
            factArgs = null;
            if (!context.DynamicMethodSignatures)
            {
                CheckFactArguments(methodInfo, signatureCheck);

                //manage default vals
                for (int i = argCount; i < argValues.Length; i++)
                {
                    if (methodInfo.Params[i].HasDefaultValue)
                    {
                        argValues[i] = null;
                    }
                }
            }
        }

        private void CheckFactArguments(MethodInfo methInfo, bool[] argsPassed)
        {
            if (argsPassed.Length > methInfo.Params.Length)
            {
                throw RuntimeException.TooManyArgumentsPassed();
            }

            for (int i = 0; i < methInfo.Params.Length; i++)
            {
                var paramDef = methInfo.Params[i];
                if (i < argsPassed.Length)
                {
                    if (argsPassed[i] == false && !paramDef.HasDefaultValue)
                    {
                        throw RuntimeException.ArgHasNoDefaultValue(i + 1);
                    }
                }
                else if (!paramDef.HasDefaultValue)
                {
                    throw RuntimeException.TooLittleArgumentsPassed();
                }
            }
        }

        private void Jmp(int arg)
        {
            _currentFrame.InstructionPointer = arg;
        }

        private void JmpFalse(int arg)
        {
            var op1 = _operationStack.Pop();

            if (!op1.AsBoolean())
            {
                _currentFrame.InstructionPointer = arg;
            }
            else
            {
                NextInstruction();
            }
        }

        private void PushIndexed(int arg)
        {
            var index = BreakVariableLink(_operationStack.Pop());
            var context = _operationStack.Pop().AsObject();
            if (context == null || !context.IsIndexed)
            {
                throw RuntimeException.IndexedAccessIsNotSupportedException();
            }

            _operationStack.Push(Variable.CreateIndexedPropertyReference(context, index));
            NextInstruction();

        }

        private void Return(int arg)
        {
            if (_currentFrame.DiscardReturnValue)
                _operationStack.Pop();

            while(_exceptionsStack.Count > 0 && _exceptionsStack.Peek().handlerFrame == _currentFrame)
            {
                _exceptionsStack.Pop();
            }
            
            if (_callStack.Count != 0)
            {
                PopFrame();
                NextInstruction();
            }
            else
                _currentFrame.InstructionPointer = -1;
            
        }

        private void JmpCounter(int arg)
        {
            var counter = _operationStack.Pop();
            var limit = _currentFrame.LocalFrameStack.Peek();

            if(counter.DataType != DataType.Number || limit.DataType != DataType.Number)
                throw new WrongStackConditionException(); 

            if (counter.CompareTo(limit) <= 0)
            {
                NextInstruction();
            }
            else
            {
                Jmp(arg);
            }
        }

        private void Inc(int arg)
        {
            var operand = _operationStack.Pop().AsNumber();
            operand = operand + 1;
            _operationStack.Push(ValueFactory.Create(operand));
            NextInstruction();
        }

        private void NewInstance(int argCount)
        {
            var typeName = _operationStack.Pop().AsString();
            IValue[] argValues = new IValue[argCount];
            // fact args
            for (int i = argCount - 1; i >= 0; i--)
            {
                var argValue = _operationStack.Pop();
                argValues[i] = argValue;
            }

            var clrType = TypeManager.GetFactoryFor(typeName);

            var ctors = clrType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .Where(x => x.GetCustomAttributes(false).Any(y => y is ScriptConstructorAttribute))
                .Select(x => new 
                    {   CtorInfo = x,
                        Parametrized = ((ScriptConstructorAttribute)x.GetCustomAttributes(typeof(ScriptConstructorAttribute), false)[0]).ParametrizeWithClassName 
                    });

            foreach (var ctor in ctors)
            {
                var parameters = ctor.CtorInfo.GetParameters();
                List<object> argsToPass = new List<object>();
                if (ctor.Parametrized)
                {
                    if (parameters.Length < 1)
                    {
                        continue;
                    }
                    if (parameters[0].ParameterType != typeof(string))
                    {
                        throw new InvalidOperationException("Type parametrized constructor must have first argument of type String");
                    }
                    argsToPass.Add(typeName);
                    parameters = parameters.Skip(1).ToArray();
                }

                bool success = (parameters.Length == 0 && argCount == 0)
                    ||(parameters.Length > 0 && parameters[0].ParameterType.IsArray);

                if (parameters.Length > 0 && parameters.Length < argCount 
                    && !parameters[parameters.Length-1].ParameterType.IsArray)
                {
                    success = false;
                    continue;
                }
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType.IsArray)
                    {
                        // captures all remained args
                        IValue[] varArgs = new IValue[argCount - i];
                        for (int j = i, k = 0; k < varArgs.Length; j++, k++)
                        {
                            varArgs[k] = argValues[j];
                        }
                        argsToPass.Add(varArgs);
                        success = true;
                        break;
                    }
                    else
                    {
                        if (i < argValues.Length)
                        {
                            argsToPass.Add(argValues[i]);
                            success = true;
                        }
                        else
                        {
                            success = false;
                            break; // no match
                        }
                    }
                }

                if (success)
                {
                    object instance = null;
                    try
                    {
                        instance = ctor.CtorInfo.Invoke(null, argsToPass.ToArray());
                    }
                    catch (System.Reflection.TargetInvocationException e)
                    {
                        if (e.InnerException != null && e.InnerException is RuntimeException)
                            throw e.InnerException;
                        else
                            throw;
                    }

                    _operationStack.Push((IValue)instance);
                    NextInstruction();
                    return;
                }

            }

            throw new RuntimeException("Конструктор не найден ("+typeName+")");


        }

        private void PushIterator(int arg)
        {
            var collection = _operationStack.Pop();
            if (collection.DataType == DataType.Object)
            {
                var context = collection.AsObject() as ICollectionContext;
                if (context == null)
                {
                    throw RuntimeException.IteratorIsNotDefined();
                }

                var iterator = context.GetManagedIterator();
                _currentFrame.LocalFrameStack.Push(iterator);
                NextInstruction();

            }
            else
            {
                throw RuntimeException.ValueIsNotObjectException();
            }
        }

        private void IteratorNext(int arg)
        {
            var iterator = _currentFrame.LocalFrameStack.Peek() as CollectionEnumerator;
            if (iterator == null)
            {
                throw new WrongStackConditionException();
            }

            var hasNext = iterator.MoveNext();
            if (hasNext)
            {
                _operationStack.Push(iterator.Current);
            }
            _operationStack.Push(ValueFactory.Create(hasNext));
            NextInstruction();
        }

        private void StopIterator(int arg)
        {
            var iterator = _currentFrame.LocalFrameStack.Pop() as CollectionEnumerator;
            if (iterator == null)
            {
                throw new WrongStackConditionException();
            }

            iterator.Dispose();
            NextInstruction();
        }

        private void BeginTry(int exceptBlockAddress)
        {
            var info = new ExceptionJumpInfo();
            info.handlerAddress = exceptBlockAddress;
            info.handlerFrame = _currentFrame;

            _exceptionsStack.Push(info);
            NextInstruction();
        }

        private void EndTry(int arg)
        {
            if (_exceptionsStack.Count > 0)
                _exceptionsStack.Pop();
            _currentFrame.LastException = null;
            NextInstruction();
        }

        private void RaiseException(int arg)
        {
            if (arg < 0)
            {
                if (_currentFrame.LastException == null)
                    throw new ApplicationException("Некорректное состояние стека исключений");

                throw _currentFrame.LastException;
            }
            else
            {
                throw new RuntimeException(_operationStack.Pop().AsString());
            }
        }

        private void LineNum(int arg)
        {
            _lineNumber = arg;
            NextInstruction();
        }

        private void MakeRawValue(int arg)
        {
            var value = BreakVariableLink(_operationStack.Pop());
            _operationStack.Push(value);
            NextInstruction();
        }

        private void MakeBool(int arg)
        {
            var value = _operationStack.Pop().AsBoolean();            
            _operationStack.Push(ValueFactory.Create(value));
            NextInstruction();
        }

        private void PushTmp(int arg)
        {
            var value = _operationStack.Pop();
            _currentFrame.LocalFrameStack.Push(value);
            NextInstruction();
        }

        private void PopTmp(int arg)
        {
            var tmpVal = _currentFrame.LocalFrameStack.Pop();

            if (arg == 0)
                _operationStack.Push(tmpVal);

            NextInstruction();
        }

        #endregion

        #region Built-in functions

        private void Question(int arg)
        {
            var falseVal = BreakVariableLink(_operationStack.Pop());
            var trueVal = BreakVariableLink(_operationStack.Pop());
            var condition = _operationStack.Pop().AsBoolean();

            if (condition)
            {
                _operationStack.Push(trueVal);
            }
            else
            {
                _operationStack.Push(falseVal);
            }
            NextInstruction();
        }

        private void Bool(int arg)
        {
            bool value = _operationStack.Pop().AsBoolean();
            _operationStack.Push(ValueFactory.Create(value));
            NextInstruction();
        }

        private void Number(int arg)
        {
            decimal value = _operationStack.Pop().AsNumber();
            _operationStack.Push(ValueFactory.Create(value));
            NextInstruction();
        }

        private void Str(int arg)
        {
            string value = _operationStack.Pop().AsString();
            _operationStack.Push(ValueFactory.Create(value));
            NextInstruction();
        }

        private void Date(int arg)
        {
            if (arg == 1)
            {
                var strDate = _operationStack.Pop().AsString();
                _operationStack.Push(ValueFactory.Parse(strDate, DataType.Date));
            }
            else if (arg >= 3 && arg <= 6)
            {
                int[] factArgs = new int[6];

                for (int i = arg - 1; i >= 0; i--)
                {
                    factArgs[i] = (int)_operationStack.Pop().AsNumber();
                }

                var date = new DateTime(
                                factArgs[0],
                                factArgs[1],
                                factArgs[2],
                                factArgs[3],
                                factArgs[4],
                                factArgs[5]);
                
                _operationStack.Push(ValueFactory.Create(date));
                       
            }
            else
            {
                throw new RuntimeException("Неверное количество параметров");
            }

            NextInstruction();
        }

        private void StrLen(int arg)
        {
            var str = _operationStack.Pop().AsString();
            _operationStack.Push(ValueFactory.Create(str.Length));
            NextInstruction();
        }

        private void TrimL(int arg)
        {
            var str = _operationStack.Pop().AsString();

            for (int i = 0; i < str.Length; i++)
            {
                if(!Char.IsWhiteSpace(str[i]))
                {
                    var trimmed = str.Substring(i);
                    _operationStack.Push(ValueFactory.Create(trimmed));
                    NextInstruction();
                    return;
                }
            }

            _operationStack.Push(ValueFactory.Create(""));
            NextInstruction();

        }

        private void TrimR(int arg)
        {
            var str = _operationStack.Pop().AsString();

            int lastIdx = str.Length-1;
            for (int i = lastIdx; i >= 0; i--)
            {
                if (!Char.IsWhiteSpace(str[i]))
                {
                    var trimmed = str.Substring(0, i+1);
                    _operationStack.Push(ValueFactory.Create(trimmed));
                    NextInstruction();
                    return;
                }
            }

            _operationStack.Push(ValueFactory.Create(""));
            NextInstruction();

        }

        private void TrimLR(int arg)
        {
            var str = _operationStack.Pop().AsString().Trim();
            _operationStack.Push(ValueFactory.Create(str));
            NextInstruction();
        }

        private void Left(int arg)
        {
            var len = (int)_operationStack.Pop().AsNumber();
            var str = _operationStack.Pop().AsString();

            if (len > str.Length)
                len = str.Length;
            else if (len < 0)
                throw RuntimeException.InvalidArgumentValue();

            _operationStack.Push(ValueFactory.Create(str.Substring(0, len)));
            NextInstruction();
        }

        private void Right(int arg)
        {
            var len = (int)_operationStack.Pop().AsNumber();
            var str = _operationStack.Pop().AsString();

            if (len > str.Length)
                len = str.Length;
            else if(len < 0)
                throw RuntimeException.InvalidArgumentValue();

            int startIdx = str.Length - len;
            _operationStack.Push(ValueFactory.Create(str.Substring(startIdx, len)));

            NextInstruction();
        }

        private void Mid(int arg)
        {
            string str;
            int start;
            int len;
            if (arg == 2)
            {
                start = (int)_operationStack.Pop().AsNumber();
                str = _operationStack.Pop().AsString();
                len = str.Length-start+1;
            }
            else
            {
                len = (int)_operationStack.Pop().AsNumber();
                start = (int)_operationStack.Pop().AsNumber();
                str = _operationStack.Pop().AsString();
            }

            if (start < 1)
                start = 1;

            if (len > str.Length || len < 0)
                len = str.Length-start+1;

            string result;

            if (start > str.Length || len == 0)
            {
                result = "";
            }
            else
            {
                result = str.Substring(start - 1, len);
            }

            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }
        
        private void StrPos(int arg)
        {
            var needle = _operationStack.Pop().AsString();
            var haystack = _operationStack.Pop().AsString();

            var result = haystack.IndexOf(needle) + 1;
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void UCase(int arg)
        {
            var result = _operationStack.Pop().AsString().ToUpper();
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void LCase(int arg)
        {
            var result = _operationStack.Pop().AsString().ToLower();
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void Chr(int arg)
        {
            var code = (int)_operationStack.Pop().AsNumber();

            var result = new string(new char[1] { (char)code });
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void ChrCode(int arg)
        {
            var strChar = _operationStack.Pop().AsString();
            int result;
            if (strChar.Length == 0)
                result = 0;
            else
                result = (int)strChar[0];

            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EmptyStr(int arg)
        {
            var str = _operationStack.Pop().AsString();

            _operationStack.Push(ValueFactory.Create(String.IsNullOrWhiteSpace(str)));
            NextInstruction();
        }

        private void StrReplace(int arg)
        {
            var newVal = _operationStack.Pop().AsString();
            var searchVal = _operationStack.Pop().AsString();
            var sourceString = _operationStack.Pop().AsString();

            var result = sourceString.Replace(searchVal, newVal);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void Year(int arg)
        {
            var date = _operationStack.Pop().AsDate().Year;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Month(int arg)
        {
            var date = _operationStack.Pop().AsDate().Month;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Day(int arg)
        {
            var date = _operationStack.Pop().AsDate().Day;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Hour(int arg)
        {
            var date = _operationStack.Pop().AsDate().Hour;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Minute(int arg)
        {
            var date = _operationStack.Pop().AsDate().Minute;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void Second(int arg)
        {
            var date = _operationStack.Pop().AsDate().Second;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void BegOfYear(int arg)
        {
            var year = _operationStack.Pop().AsDate().Year;
            _operationStack.Push(ValueFactory.Create(new DateTime(year,1,1)));
            NextInstruction();
        }

        private void BegOfMonth(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, 1);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void BegOfDay(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void BegOfHour(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void BegOfMinute(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void BegOfQuarter(int arg)
        {
            //1,4,7,10
            var date = _operationStack.Pop().AsDate();
            var month = date.Month;
            int quarterMonth;
            if (date.Month >= 1 && date.Month <= 3)
            {
                quarterMonth = 1;
            }
            else if (date.Month >= 4 && date.Month <= 6)
            {
                quarterMonth = 4;
            }
            else if (date.Month >= 7 && date.Month <= 9)
            {
                quarterMonth = 7;
            }
            else
            {
                quarterMonth = 10;
            }
            var result = new DateTime(date.Year, quarterMonth, 1);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EndOfYear(int arg)
        {
            var year = _operationStack.Pop().AsDate().Year;
            _operationStack.Push(ValueFactory.Create(new DateTime(year, 12, DateTime.DaysInMonth(year,12), 23, 59, 59)));
            NextInstruction();
        }

        private void EndOfMonth(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EndOfDay(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EndOfHour(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day, date.Hour, 59, 59);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EndOfMinute(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 59);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void EndOfQuarter(int arg)
        {
            //1,4,7,10
            var date = _operationStack.Pop().AsDate();
            var month = date.Month;
            int quarterMonth;
            if (date.Month >= 1 && date.Month <= 3)
            {
                quarterMonth = 3;
            }
            else if (date.Month >= 4 && date.Month <= 6)
            {
                quarterMonth = 6;
            }
            else if (date.Month >= 7 && date.Month <= 9)
            {
                quarterMonth = 9;
            }
            else
            {
                quarterMonth = 12;
            }
            var result = new DateTime(date.Year, quarterMonth, DateTime.DaysInMonth(date.Year, quarterMonth), 23, 59, 59);
            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void WeekOfYear(int arg)
        {
            var date = _operationStack.Pop().AsDate();
            var cal = new System.Globalization.GregorianCalendar();

            _operationStack.Push(ValueFactory.Create(cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)));
            NextInstruction();
        }

        private void DayOfYear(int arg)
        {
            var date = _operationStack.Pop().AsDate().DayOfYear;
            _operationStack.Push(ValueFactory.Create(date));
            NextInstruction();
        }

        private void DayOfWeek(int arg)
        {
            var day = (int)_operationStack.Pop().AsDate().DayOfWeek;

            if (day == 0)
            {
                day = 7;
            }

            _operationStack.Push(ValueFactory.Create(day));
            NextInstruction();
        }

        private void AddMonth(int arg)
        {
            var numToAdd = (int)_operationStack.Pop().AsNumber();
            var date = _operationStack.Pop().AsDate();
            _operationStack.Push(ValueFactory.Create(date.AddMonths(numToAdd)));
            NextInstruction();
        }

        private void CurrentDate(int arg)
        {
            _operationStack.Push(ValueFactory.Create(DateTime.Now));
            NextInstruction();
        }

        private void Integer(int arg)
        {
            var num = (int)_operationStack.Pop().AsNumber();
            _operationStack.Push(ValueFactory.Create(num));
            NextInstruction();
        }

        private void Round(int arg)
        {
            decimal num;
            int digits;
            int mode;
            if (arg == 1)
            {
                num = _operationStack.Pop().AsNumber();
                digits = 0;
                mode = 0;
            }
            else if (arg == 2)
            {
                digits = (int)_operationStack.Pop().AsNumber();
                num = _operationStack.Pop().AsNumber();
                mode = 0;
            }
            else
            {
                mode = (int)_operationStack.Pop().AsNumber();
                mode = mode == 0 ? 0 : 1;
                digits = (int)_operationStack.Pop().AsNumber();
                num = _operationStack.Pop().AsNumber();
            }

            decimal scale = (decimal)Math.Pow(10.0, digits);
            decimal scaled = Math.Abs(num) * scale;

            var director = (int)((scaled - (long)scaled) * 10 % 10);

            decimal round;
            if (director == 5)
                round = Math.Floor(scaled + mode * 0.5m * Math.Sign(digits));
            else if (director > 5)
                round = Math.Ceiling(scaled);
            else
                round = Math.Floor(scaled);
            
            decimal result;
            
            if(digits >= 0)
                result = (Math.Sign(num) * round / scale);
            else
                result = (Math.Sign(num) * round * scale);

            _operationStack.Push(ValueFactory.Create(result));
            NextInstruction();
        }

        private void Pow(int arg)
        {
            var powPower = (double)_operationStack.Pop().AsNumber();
            var powBase = (double)_operationStack.Pop().AsNumber();
            double power = Math.Pow(powBase, powPower);
            _operationStack.Push(ValueFactory.Create((decimal)power));
            NextInstruction();
        }

        private void Sqrt(int arg)
        {
            var num = (double)_operationStack.Pop().AsNumber();
            var root = Math.Sqrt(num);
            _operationStack.Push(ValueFactory.Create((decimal)root));
            NextInstruction();
        }

        private void ExceptionInfo(int arg)
        {
            if (_currentFrame.LastException != null)
            {
                var excInfo = new ExceptionInfoContext(_currentFrame.LastException);
                _operationStack.Push(ValueFactory.Create(excInfo));
            }
            else
            {
                _operationStack.Push(ValueFactory.Create());
            }
            NextInstruction();
        }
        
        private void ExceptionDescr(int arg)
        {
            if (_currentFrame.LastException != null)
            {
                var excInfo = new ExceptionInfoContext(_currentFrame.LastException);
                _operationStack.Push(ValueFactory.Create(excInfo.Message));
            }
            else
            {
                _operationStack.Push(ValueFactory.Create(""));
            }
            NextInstruction();
        }

        #endregion

        #endregion

        private void NextInstruction()
        {
            _currentFrame.InstructionPointer++;
        }

        private IValue BreakVariableLink(IValue value)
        {
            return value.GetRawValue();
        }

    }
}
