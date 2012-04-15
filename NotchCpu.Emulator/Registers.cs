using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotchCpu.Emulator
{
    


    enum RegisterCodes : ushort
    {
        // Basic register code, used to read value from register
        // ie SET A, X
        A = 0x00,
        B = 0x01,
        C = 0x02,
        X = 0x03,
        Y = 0x04,
        Z = 0x05,
        I = 0x06,
        J = 0x07,

        // References memory location of register value
        // ie SET A, [X] 
        A_Mem = 0x08,
        B_Mem = 0x09,
        C_Mem = 0x0A,
        X_Mem = 0x0B,
        Y_Mem = 0x0C,
        Z_Mem = 0x0D,
        I_Mem = 0x0E,
        J_Mem = 0x0F,

        // References memory location with modifier
        // ie SET A [X+2]
        A_NextWord = 0x10,
        B_NextWord = 0x11,
        C_NextWord = 0x12,
        X_NextWord = 0x13,
        Y_NextWord = 0x14,
        Z_NextWord = 0x15,
        I_NextWord = 0x16,
        J_NextWord = 0x17,

        POP = 0x18,     // [SP++]
        PEEK = 0x19,    // [SP]
        PUSH = 0x1A,    // [--SP]

        SP = 0x1B,      // Stack pointer
        PC = 0x1C,      // Program Counter
        O = 0x1D,       // Overflow Register

        NextWord_Literal_Mem = 0x1E,    // IE for "SET A, [0x1000]" B register will be 0x1E and we assume the next word (PC++)'s value is to reference a memory location
        NextWord_Literal_Value = 0x1F,   // Same as above but its a literal value, not literal value to a memory location

        Literal = 0x20,

        // if Literal value is < 0x1F we can encode it in 0x20-0x3F and skip the next word requirement. 
        // this is really handy for simple register initialization and incrementation, as we can encode it in as 
        // little as 1 word!
    }

    class Registers
    {
        public ushort SP;
        public ushort PC;
        public ushort O;

        public ushort[] Reg = new ushort[8];
        public ushort[] Ram = new ushort[65536];

        public event MemUpdateHandler MemUpdateEvent;
        public event MemUpdateHandler RegUpdateEvent;

        public void MemUpdate(ushort loc, ushort val)
        {
            if (MemUpdateEvent != null)
                MemUpdateEvent(loc, val);
        }

        public void RegUpdate(ushort loc, ushort val)
        {
            if (RegUpdateEvent != null)
                RegUpdateEvent(loc, val);
        }

        public MemLoc Get(ushort code)
        {
            int r = (ushort)(((int)code) & 0x7);

            switch ((RegisterCodes)code)
            {
                case RegisterCodes.A:
                case RegisterCodes.B:
                case RegisterCodes.C:
                case RegisterCodes.X:
                case RegisterCodes.Y:
                case RegisterCodes.Z:
                case RegisterCodes.I:
                case RegisterCodes.J:
                    return new MemLoc(this, code, (ushort)code);

                case RegisterCodes.A_Mem:
                case RegisterCodes.B_Mem:
                case RegisterCodes.C_Mem:
                case RegisterCodes.X_Mem:
                case RegisterCodes.Y_Mem:
                case RegisterCodes.Z_Mem:
                case RegisterCodes.I_Mem:
                case RegisterCodes.J_Mem:
                    return new MemLoc(this, code, (ushort)Reg[r]);

                case RegisterCodes.A_NextWord:
                case RegisterCodes.B_NextWord:
                case RegisterCodes.C_NextWord:
                case RegisterCodes.X_NextWord:
                case RegisterCodes.Y_NextWord:
                case RegisterCodes.Z_NextWord:
                case RegisterCodes.I_NextWord:
                case RegisterCodes.J_NextWord:
                    return new MemLoc(this, code, (ushort)(Reg[r] + Ram[++PC]));

                case RegisterCodes.POP:
                    return new MemLoc(this, code, (ushort)SP++);
                case RegisterCodes.PEEK:
                    return new MemLoc(this, code, (ushort)SP);
                case RegisterCodes.PUSH:
                    return new MemLoc(this, code, (ushort)--SP);
                case RegisterCodes.SP:
                    return new MemLoc(this, code, (ushort)SP);
                case RegisterCodes.PC:
                    return new MemLoc(this, code, (ushort)PC);
                case RegisterCodes.O:
                    return new MemLoc(this, code, (ushort)O);
                case RegisterCodes.NextWord_Literal_Mem:
                    return new MemLoc(this, code, (ushort)Ram[++PC]);
                case RegisterCodes.NextWord_Literal_Value:
                    return new MemLoc(this, code, ++PC);

                default:
                    return new MemLoc(this, RegisterCodes.Literal, (ushort)(code - 0x20));
            }

            throw new Exception("Invalid Op Code");
        }
    }
}
