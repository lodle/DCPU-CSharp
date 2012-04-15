using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NotchCpu.CompilerTasks.Assembler;

namespace DCPUC.misc
{
    public class OpParamResult
    {
        public ushort Param;
        public bool nextWord;
        public ushort NextWordValue;
        public string labelName;

        public bool illegal = false;

        public OpParamResult()
        {
            Param = 0x0;
            nextWord = false;
            NextWordValue = 0x0;
            labelName = "";
            illegal = false;
        }

        static readonly Dictionary<string, dcpuRegisterCodes> _RegDictionary = new Dictionary<string, dcpuRegisterCodes>()
        {
            // Register dictionary, We'll only include the most common ones in here, others have to be constructred. 

            {"a", dcpuRegisterCodes.A},
            {"b", dcpuRegisterCodes.B},
            {"c", dcpuRegisterCodes.C},
            {"x", dcpuRegisterCodes.X},
            {"y", dcpuRegisterCodes.Y},
            {"z", dcpuRegisterCodes.Z},
            {"i", dcpuRegisterCodes.I},
            {"j", dcpuRegisterCodes.J},

            {"[a]", dcpuRegisterCodes.A_Mem},
            {"[b]", dcpuRegisterCodes.B_Mem},
            {"[c]", dcpuRegisterCodes.C_Mem},
            {"[x]", dcpuRegisterCodes.X_Mem},
            {"[y]", dcpuRegisterCodes.Y_Mem},
            {"[z]", dcpuRegisterCodes.Z_Mem},
            {"[i]", dcpuRegisterCodes.I_Mem},
            {"[j]", dcpuRegisterCodes.J_Mem},

            {"pop", dcpuRegisterCodes.POP},
            {"peek", dcpuRegisterCodes.PEEK},
            {"push", dcpuRegisterCodes.PUSH},

            {"sp", dcpuRegisterCodes.SP},
            {"pc", dcpuRegisterCodes.PC},
            {"o", dcpuRegisterCodes.O},

            {"[+a]", dcpuRegisterCodes.A_NextWord},
            {"[+b]", dcpuRegisterCodes.B_NextWord},
            {"[+c]", dcpuRegisterCodes.C_NextWord},
            {"[+x]", dcpuRegisterCodes.X_NextWord},
            {"[+y]", dcpuRegisterCodes.Y_NextWord},
            {"[+z]", dcpuRegisterCodes.Z_NextWord},
            {"[+i]", dcpuRegisterCodes.I_NextWord},
            {"[+j]", dcpuRegisterCodes.J_NextWord},
        };

        public static OpParamResult ParseParam(string parm)
        {
            OpParamResult opParamResult = new OpParamResult();

            // Find easy ones. 
            string Param = parm.Replace(" ", "").Trim(); // strip spaces

            if (_RegDictionary.ContainsKey(Param) != false)
            {
                // Ok things are really easy in this case. 
                opParamResult.Param = (ushort)_RegDictionary[Param];
            }
            else
            {
                if (Param[0] == '[' && Param[Param.Length - 1] == ']')
                {
                    if (Param.Contains("+") != false)
                        ParseParamMemRef(opParamResult, Param);
                    else
                        ParseParamMemRefNextWord(opParamResult, Param);
                }
                else
                {
                    ParseParamNextWord(opParamResult, Param);
                }
            }

            return opParamResult;
        }

        private static void ParseParamNextWord(OpParamResult opParamResult, string Param)
        {
            // if value is < 0x1F we can encode it into the param directly, 
            // else it has to be next value!

            UInt16 maxValue = Convert.ToUInt16("0x1F", 16);
            UInt16 literalValue = 0;
            try
            {
                if (Param[0] == '\'' && Param[Param.Length - 1] == '\'' && Param.Length == 3)
                {
                    literalValue = (ushort)Param[1];
                }
                else if (Param.Contains("0x") != false)
                    literalValue = Convert.ToUInt16(Param, 16);
                else
                    literalValue = Convert.ToUInt16(Param, 10);

                if (literalValue < maxValue)
                {
                    opParamResult.Param = Convert.ToUInt16("0x20", 16);
                    opParamResult.Param += literalValue;
                }
                else
                {
                    opParamResult.Param = (ushort)dcpuRegisterCodes.NextWord_Literal_Value;
                    opParamResult.nextWord = true;
                    opParamResult.NextWordValue = literalValue;
                }
            }
            catch
            {
                opParamResult.Param = (ushort)dcpuRegisterCodes.NextWord_Literal_Value;
                opParamResult.nextWord = true;
                opParamResult.labelName = Param;
            }
        }

        private static void ParseParamMemRefNextWord(OpParamResult opParamResult, string Param)
        {
            opParamResult.Param = (ushort)dcpuRegisterCodes.NextWord_Literal_Mem;
            opParamResult.nextWord = true;
            try
            {
                if (Param[1] == '\'' && Param[Param.Length - 2] == '\'' && Param.Length == 5) // nasty
                {
                    ushort val = (ushort)Param[1];
                    opParamResult.NextWordValue = (ushort)val;
                }
                else if (Param.Contains("0x") != false)
                {
                    ushort val = (ushort)Convert.ToUInt16(Param.Replace("[", "").Replace("]", "").Trim(), 16);
                    opParamResult.NextWordValue = val;
                }
                else
                {
                    ushort val = (ushort)Convert.ToUInt16(Param.Replace("[", "").Replace("]", "").Trim(), 10);
                    opParamResult.NextWordValue = val;
                }
            }
            catch
            {

                opParamResult.nextWord = true;
                opParamResult.labelName = Param.Replace("[", "").Replace("]", "").Trim();
            }
        }

        private static void ParseParamMemRef(OpParamResult opParamResult, string Param)
        {
            string[] psplit = Param.Replace("[", "").Replace("]", "").Replace(" ", "").Split('+');
            if (psplit.Length < 2)
            {
                throw new Exception(string.Format("malformated memory reference '{0}'", Param));
            }
            string addressValue = psplit[0];
            if (_RegDictionary.ContainsKey("[+" + psplit[1] + "]") != true)
            {
                throw new Exception(string.Format("Invalid register reference in '{0}'", Param));
            }
            opParamResult.Param = (ushort)_RegDictionary["[+" + psplit[1] + "]"];
            opParamResult.nextWord = true;
            try
            {
                if (psplit[0][0] == '\'' && psplit[0][psplit[0].Length - 1] == '\'' && psplit[0].Length == 3) // nasty
                {
                    ushort val = (ushort)psplit[0][1];
                    opParamResult.NextWordValue = (ushort)val;
                }
                else if (psplit[0].Contains("0x") != false)
                {
                    ushort val = Convert.ToUInt16(psplit[0].Trim(), 16);
                    opParamResult.NextWordValue = (ushort)val;
                }
                else
                {
                    ushort val = Convert.ToUInt16(psplit[0].Trim(), 10);
                    opParamResult.NextWordValue = (ushort)val;
                }
            }
            catch
            {
                opParamResult.nextWord = true;
                opParamResult.labelName = psplit[0].Trim();
            }
        }

    }
}
