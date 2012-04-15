using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotchCpu.Emulator
{
    public interface IDCPUProgram
    {
        ushort[] GetDebugBinary();

        ushort[] GetReleaseBinary();

        void Run();
    }
}
