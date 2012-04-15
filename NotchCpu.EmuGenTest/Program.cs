using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NotchCpu.Emulator;
using System.ComponentModel.Composition;

namespace NotchCpu.EmuGenTest
{
    [Export(typeof(IDCPUProgram))]
    class Program : IDCPUProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            IEmulator emu = Mef.GetEmulator();
            Application.Run(emu.GetMainForm());
        }

        ushort[] _BinCode = new ushort[]
        {
            0x8061,0x8071,0x7C11,0xF100,0x5801,0x001F,
            0x800C,0x7DC1,0x003B,0x7C0E,0x00FF,0x7DC1,
            0x0014,0x040A,0x0171,0x8000,0x8462,0x8472,
            0x7DC1,0x0004,0x0011,0x7C19,0x00FF,0xA017,
            0x7C0E,0x01FF,0x7C12,0x0080,0x8462,0x7DC1,
            0x0004,0x0170,0x0048,0x0065,0x006C,0x006C,
            0x006F,0x0020,0x0077,0x006F,0x0072,0x006C,
            0x0064,0x0170,0x002C,0x0020,0x0068,0x006F,
            0x0077,0x0020,0x0061,0x0072,0x0065,0x0020,
            0x0079,0x006F,0x0075,0x003F,0x0000,0x7DC1,
            0x0000,
        };


        IEmulator _Emulator;

        public Program()
        {
            _Emulator = Mef.GetEmulator();
        }

        public ushort[] GetDebugBinary()
        {
            return _BinCode;
        }

        public ushort[] GetReleaseBinary()
        {
            return null;
        }

        public void Run()
        {
            _Emulator.RunOnce(0);
            _Emulator.RunOnce(1);
        }
    }
}
