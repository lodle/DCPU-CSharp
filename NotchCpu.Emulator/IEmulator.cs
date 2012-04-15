using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace NotchCpu.Emulator
{
    public delegate void LogHandler(String message);
    public delegate void ErrorHandler(Exception e);
    public delegate void CompleteHandler();
    public delegate void MemUpdateHandler(ushort loc, ushort value);

    public enum RunMode
    {
        Step,
        Run,
    }

    internal interface IProgEmulator
    {
        event LogHandler LogEvent;
        event ErrorHandler ErrorEvent;
        event CompleteHandler CompleteEvent;
        event MemUpdateHandler MemUpdateEvent;
        event MemUpdateHandler RegUpdateEvent;

        double SpeedMultiplier { get; set; }

        int AvgHertz { get; }
        long AvgTicks { get; }
        int InstructionCount { get; }
        Registers Registers { get; }

        RunMode RunMode { get; set; }

        void StartEmulation();
        void StopEmulation();

        void Step();
        void Reset();
    }

    public interface IEmulator
    {
        /// <summary>
        /// Runs all commands
        /// </summary>
        [DebuggerStepThrough]
        void RunAll();

        /// <summary>
        /// Runs once command
        /// </summary>
        /// <param name="pc">The pc of the expected command</param>
        [DebuggerStepThrough]
        void RunOnce(ushort pc);

        [DebuggerStepThrough]
        Form GetMainForm();
    }
}
