using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotchCpu.Emulator
{
    class MemLoc
    {
        Registers _Reg;
        RegisterCodes _Code;
        ushort _Offset;

        internal MemLoc(Registers reg, ushort offset)
        {
            _Reg = reg;
            _Offset = offset;
        }

        internal MemLoc(Registers reg, RegisterCodes code, ushort offset) : this(reg, offset)
        {
            _Code = code;
        }

        internal MemLoc(Registers reg, int code, ushort offset)
            : this(reg, offset)
        {
            _Code = (RegisterCodes)code;
        }

        public ushort Value
        {
            get
            {
                switch (_Code)
                {
                    case RegisterCodes.A:
                    case RegisterCodes.B:
                    case RegisterCodes.C:
                    case RegisterCodes.X:
                    case RegisterCodes.Y:
                    case RegisterCodes.Z:
                    case RegisterCodes.I:
                    case RegisterCodes.J:
                        return _Reg.Reg[_Offset];

                    case RegisterCodes.SP:
                        return _Reg.SP;

                    case RegisterCodes.PC:
                        return _Reg.PC;

                    case RegisterCodes.O:
                        return _Reg.O;

                    case RegisterCodes.Literal:
                        return _Offset;

                    default:
                        return _Reg.Ram[_Offset];
                }
            }
            set
            {
                switch (_Code)
                {
                    case RegisterCodes.A:
                    case RegisterCodes.B:
                    case RegisterCodes.C:
                    case RegisterCodes.X:
                    case RegisterCodes.Y:
                    case RegisterCodes.Z:
                    case RegisterCodes.I:
                    case RegisterCodes.J:
                        _Reg.Reg[_Offset] = value;
                        _Reg.RegUpdate((ushort)_Code, value);
                        break;

                    case RegisterCodes.Literal:
                        break;

                    case RegisterCodes.SP:
                        _Reg.SP = value;
                        _Reg.RegUpdate((ushort)(_Code - 20), value);
                        break;

                    case RegisterCodes.PC:
                        value = (ushort)(value - 1); //take 1 as we increment after the cur instruction
                        _Reg.PC = value;
                        _Reg.RegUpdate((ushort)(_Code - 20), value);
                        break;

                    case RegisterCodes.O:
                        _Reg.O = value;
                        _Reg.RegUpdate((ushort)(_Code-20), value);
                        break;

                    default:
                        _Reg.Ram[_Offset] = value;
                        _Reg.MemUpdate(_Offset, value);
                        break;
                }
            }
        }
    }
}
