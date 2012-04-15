using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NotchCpu.Emulator;
using System.IO;
using System.Diagnostics;
using System.ComponentModel.Composition;

namespace NotchCpu.EmuTest
{
    [Export(typeof(IDCPUProgram))]
    class Program : IDCPUProgram
    {
        static String _File;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String [] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            var browser = new OpenFileDialog();
            browser.Filter = "bin files (*.bin, *.obj)|*.txt;*.obj|All files (*.*)|*.*" ;
            browser.InitialDirectory = Directory.GetCurrentDirectory();

            if (browser.ShowDialog() != DialogResult.OK)
                return;

            _File = browser.FileName;
#else
            _File = "helloworld.obj";
#endif
            IEmulator emu = Mef.GetEmulator();
            Application.Run(emu.GetMainForm());
        }

        ushort[] _Binary;
        IEmulator _Emulator;

        public Program()
        {
            _Emulator = Mef.GetEmulator();

            var bytes = File.ReadAllBytes(_File);

            _Binary = new ushort[bytes.Length / 2];

            for (int x = 0; x < _Binary.Length; x++)
                _Binary[x] = (ushort)((bytes[x * 2] << 8) + bytes[x * 2 + 1]);
        }

        public ushort[] GetDebugBinary()
        {
            return _Binary;
        }

        public ushort[] GetReleaseBinary()
        {
            return null;
        }

        public void Run()
        {
            _Emulator.RunAll();
        }
    }
}
