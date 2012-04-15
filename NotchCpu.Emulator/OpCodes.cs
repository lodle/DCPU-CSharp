using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotchCpu.Emulator
{
    enum OpCode : ushort
    {
        NB_OP = 0x0,      // Signals non basic instruction

        SET_OP = 0x1,      // Set instruciton          -> A = B 
        ADD_OP = 0x2,      // Add instruction          -> A = A + B
        SUB_OP = 0x3,      // Subtract instruciton     -> A = A - B
        MUL_OP = 0x4,      // Muliply  instruction     -> A = A * B
        DIV_OP = 0x5,      // Divide instruction       -> A = A / B
        MOD_OP = 0x6,      // Modulate instruction     -> A = A % B
        SHL_OP = 0x7,      // Shift Left instruction   -> A = A << B
        SHR_OP = 0x8,      // Shift right instruction  -> A = A >> B
        AND_OP = 0x9,      // Boolean AND instruction  -> A = A & B
        BOR_OP = 0xA,      // Boolean OR instruction   -> A = A | B
        XOR_OP = 0xB,      // Boolean XOR instruction  -> A = A ^ B
        IFE_OP = 0xC,      // Branch! if(A == B) run next instruction
        IFN_OP = 0xD,      // Branch! if(A != B) run next instruction
        IFG_OP = 0xE,      // Branch! if(A > B) run next instruction
        IFB_OP = 0xF,      // Branch! if((A & B) != 0) run next instruction

        // Non basic instructions
        // Encoded as follows
        // AAAAAAoooooo0000 
        // Basically unlike basic instructions, we lose a register spot and use that for the op code.
        // the old op code is zeroed out (which signals a non basic instruction). This means 
        // any non basic instruction, even if its something like derp X, Y will use 2 words (unlike a basic instruction in that case)
        JSR_OP = 0x01
    }
}
