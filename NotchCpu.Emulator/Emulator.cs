using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel.Composition;

namespace NotchCpu.Emulator
{
    [Export(typeof(IEmulator))]
    [Export(typeof(IProgEmulator))]
    class Emu : IEmulator, IProgEmulator
    {
        const long _TicksPerInstruction = TimeSpan.TicksPerSecond / (100 * 1000);

        double _SpeedMultiplier;
        long _AvgTicks;
        Thread _Thread;

        Stopwatch _Stopwatch = new Stopwatch();
        MainUi _MainUi;
        Random _Rand = new Random();
        RunMode _RunMode;

        ManualResetEvent _WaitEvent = new ManualResetEvent(false);

        
        public Registers Registers { get; protected set; }

        public event LogHandler LogEvent;
        public event ErrorHandler ErrorEvent;
        public event CompleteHandler CompleteEvent;
        public event MemUpdateHandler MemUpdateEvent;
        public event MemUpdateHandler RegUpdateEvent;

        public RunMode RunMode 
        {
            get
            {
                return _RunMode;
            }
            set
            {
                var old = RunMode;
                _RunMode = value;

                if (old == RunMode.Step)
                    _WaitEvent.Set();
            }
        }

        public int AvgHertz
        {
            get;
            protected set;
        }

        public double SpeedMultiplier 
        {
            get
            {
                return _SpeedMultiplier;
            }
            set
            {
                if (SpeedMultiplier != value)
                    _AvgTicks = 0;
                _SpeedMultiplier = value;
            }
        }

        public long AvgTicks
        {
            get
            {
                return _AvgTicks;
            }
            protected set
            {
                AvgHertz = (int)(TimeSpan.TicksPerSecond / (value * SpeedMultiplier));
                _AvgTicks = value;
            }
        }

        public int InstructionCount
        {
            get;
            protected set;
        }

        IDCPUProgram _Program;
        public IDCPUProgram Program
        {
            get
            {
                if (_Program == null)
                    _Program = Mef.GetExportedValue<IDCPUProgram>();

                return _Program;
            }
        }

        ushort[] _Binary
        {
            get
            {
                return Program.GetDebugBinary();
            }
        }

        public Emu()
        {
            RunMode = RunMode.Step;
            SpeedMultiplier = 1.0;
        }

        public Form GetMainForm()
        {
            if (_MainUi == null)
                _MainUi = new MainUi();

            return _MainUi;
        }

        public void StartEmulation()
        {
            if (_Thread != null)
                throw new Exception("All ready running");

            Reset();

            _Thread = new Thread(new ThreadStart(RunInternal));
            _Thread.Start();
        }

        public void StopEmulation()
        {
            if (_Thread == null)
                return;

            _Thread.Abort();
            _Thread = null;
        }

        public void Step()
        {
            _WaitEvent.Set();
        }

        public void Reset()
        {
            Registers = new Registers();
            Registers.MemUpdateEvent += new MemUpdateHandler(OnMemUpdate);
            Registers.RegUpdateEvent += new MemUpdateHandler(OnRegUpdate);

            Array.Copy(_Binary, Registers.Ram, _Binary.Length);
            _Stopwatch.Restart();
        }

        [DebuggerStepThroughAttribute]
        public void RunAll()
        {
            ushort val;
            while (true)
            {
                Step(out val);
            }
        }

        public void RunOnce(ushort pc)
        {
            if (Registers.PC != pc)
                throw new Exception("Run Once pc doesnt match registers pc");

            ushort val;
            Step(out val);
        }

        private void OnMemUpdate(ushort loc, ushort value)
        {
            if (MemUpdateEvent != null)
                MemUpdateEvent(loc, value);
        }

        private void OnRegUpdate(ushort loc, ushort value)
        {
            if (RegUpdateEvent != null)
                RegUpdateEvent(loc, value);
        }

        private void RunInternal()
        {
            try
            {
                Program.Run();
            }
            catch (Exception e)
            {
                if (ErrorEvent != null && e.GetType() != typeof(ThreadAbortException))
                    ErrorEvent(e);
            }
            finally
            {
                if (CompleteEvent != null)
                    CompleteEvent();
            }
        }

        private void Step(out ushort value, bool ignore = false)
        {
            var r = Registers.Ram[Registers.PC];

            ushort o = (ushort)(r & 0xF);
            ushort a = (ushort)((r >> 4) & 0x3F);
            ushort b = (ushort)((r >> 10) & 0x3F);

            value = 0;

            RunOpCode(o, a, b, out value, ignore);
            Registers.PC++;
        }

        private void RunOpCode(ushort opCode, ushort a, ushort b, out ushort aOut, bool ignore)
        {
            System.GC.Collect();

            if (RunMode == RunMode.Step)
            {
                _WaitEvent.WaitOne();
                _WaitEvent.Reset();
            }

            InstructionCount++;

            aOut = 0;
            var op = GetOpCode(opCode);
            int cost = 0;
            long start = 0;

            if (!ignore)
                start = _Stopwatch.Elapsed.Ticks;

            if (op == OpCode.NB_OP)
            {
                var ra = Registers.Get(b);
                PerformAdvancedOperation(GetOpCode(a), ra);
                cost = GetOpCodeAdvancedCost(op);
            }
            else
            {
                cost = GetOpCodeCost(op);

                bool skip = false;

                var ra = Registers.Get(a);
                var rb = Registers.Get(b);

                if (!ignore)
                {
                    var res = PerformOperation(op, ra, rb, out skip);

                    if (skip)
                    {
                        cost++;
                        ushort temp;
                        Registers.PC++;
                        Step(out temp, true);
                        Registers.PC--;
                    }
                    else if (res != -1)
                    {
                        ra.Value = (ushort)res;
                        aOut = (ushort)res;
                        Registers.O = (ushort)(res >> 16);
                    }
                }
            }

            if (ignore)
                return;

            var elapse = _Stopwatch.Elapsed.Ticks - start;
            var total = (long)(_TicksPerInstruction * SpeedMultiplier * cost);

            if (elapse > total)
            {
                if (LogEvent != null)
                    LogEvent(String.Format("Instruction {0} took {1} longer than {2} to run", op, elapse-total, total));
            }
            else
            {
                while (_Stopwatch.IsRunning)
                {
                    elapse = _Stopwatch.Elapsed.Ticks - start;

                    if (elapse > total)
                        break;

                    //chew some cycles
                    Thread.SpinWait((int)(total - elapse));
                }
            }

            _Stopwatch.Restart();

            var ticks = (long)(elapse / SpeedMultiplier / cost);

            if (AvgTicks != 0)
                AvgTicks = (AvgTicks + ticks) / 2;
            else
                AvgTicks += ticks;
        }


        private void PerformAdvancedOperation(OpCode opCode, MemLoc a)
        {
            switch (opCode)
            {
               case OpCode.JSR_OP:
                    Registers.Ram[--Registers.SP] = (ushort)(Registers.PC+1);
                    Registers.PC = (ushort)(a.Value-1);
                    break;
            }
        }

        private int PerformOperation(OpCode opCode, MemLoc a, MemLoc b, out bool skip)
        {
            skip = false;

            switch (opCode)
            {
                case OpCode.ADD_OP:
                    return (ushort)(a.Value + b.Value);

                case OpCode.AND_OP:
                    return (ushort)(a.Value & b.Value);

                case OpCode.BOR_OP:
                    return (ushort)(a.Value | b.Value);

                case OpCode.DIV_OP:
                    return (b.Value != 0) ? (ushort)(a.Value / b.Value) : (ushort)0;

                case OpCode.MOD_OP:
                    return (b.Value != 0) ? (ushort)(a.Value % b.Value) : (ushort)0;

                case OpCode.MUL_OP:
                    return (ushort)(a.Value * b.Value);

                case OpCode.SET_OP:
                    return a.Value = b.Value;

                case OpCode.SHL_OP:
                    return (ushort)(a.Value << b.Value);

                case OpCode.SHR_OP:
                    return (ushort)(a.Value >> b.Value);

                case OpCode.SUB_OP:
                    return (ushort)(a.Value - b.Value);

                case OpCode.XOR_OP:
                    return (ushort)(a.Value ^ b.Value);

                case OpCode.IFB_OP:
                    skip = (a.Value & b.Value) == 0;
                    return -1;

                case OpCode.IFE_OP:
                    skip = (a.Value != b.Value);
                    return -1;

                case OpCode.IFG_OP:
                    skip = (a.Value <= b.Value);
                    return -1;

                case OpCode.IFN_OP:
                    skip = (a.Value == b.Value);
                    return -1;

                case OpCode.NB_OP:
                    return -1;
            }

            throw new Exception("Invalid op code for operation");
        }

        private OpCode GetOpCode(ushort opCode)
        {
            return (OpCode)opCode;
        }

        private int GetOpCodeAdvancedCost(OpCode opCode)
        {
            switch (opCode)
            {
                case OpCode.JSR_OP:
                    return 2;
            }

            return 0;
        }

        private int GetOpCodeCost(OpCode opCode)
        {
            switch (opCode)
            {
                case OpCode.AND_OP:
                case OpCode.BOR_OP:
                case OpCode.SET_OP:
                case OpCode.XOR_OP:
                    return 1;

                case OpCode.ADD_OP:
                case OpCode.SUB_OP:
                case OpCode.MUL_OP:
                case OpCode.SHL_OP:
                case OpCode.SHR_OP:
                    return 2;

                case OpCode.DIV_OP:
                case OpCode.MOD_OP:
                    return 3;

                case OpCode.IFB_OP:
                case OpCode.IFE_OP:
                case OpCode.IFG_OP:
                case OpCode.IFN_OP:
                    return 2;                
            }

            return 0;
        }
    }
}
