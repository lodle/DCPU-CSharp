using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NotchCpu.CompilerTasks.Assembler;
using DCPUC.misc;
using TriAxis.RunSharp;
using NotchCpu.Emulator;

namespace DCPUC
{
    public class Instruction
    {
        public string ins;
        public string a;
        public string b;
        public string comment;

        public Annotation anotation = new Annotation();
 
        public Instruction()
        {
        }

        public Instruction(string ins, string a, string b, string comment = null)
        {
            this.ins = ins;
            this.a = a;
            this.b = b;
            this.comment = comment;
        }

        public override string ToString()
        {
            var i = GetInsAsString();

            if (String.IsNullOrEmpty(comment))
                return i;

            var d = i.Length / 4;
            var m = i.Length % 4;

            if (m != 0)
            {
                d++;
                i += "\t";
            }

            while (d < 8)
            {
                d++;
                i += "\t";
            }

            i += "; " + comment;
            return i;
        }

        private string GetInsAsString()
        {
            if (String.IsNullOrEmpty(a))
                return ins;
            else if (String.IsNullOrEmpty(b))
                return ins + " " + a;
            else
                return ins + " " + a + (a != "DAT" ? ", " : " ") + b;
        }

        public bool Ignore
        {
            get
            {
                return ins == null || ins == "NOP";
            }
        }
    }

    public class AsmInfo
    {
        public Instruction Ins;
        public Annotation Ano;
        public List<ushort> BytesGeneratedPC = new List<ushort>();

        public AsmInfo(Instruction i, Annotation a)
        {
            Ins = i;
            Ano = a;
        }
    }

    public class ByteInfo
    {
        public ushort Ins;
        public Annotation Ano;

        public ByteInfo(ushort i, Annotation a)
        {
            Ins = i;
            Ano = a;
        }
    }



    public class Assembly
    {
        public List<AsmInfo> AsmCode = new List<AsmInfo>();
        public List<ByteInfo> ByteCode = new List<ByteInfo>();

        private Dictionary<string, ByteInfo> _LabelAddressDitionary = new Dictionary<string, ByteInfo>();
        private Dictionary<ByteInfo, string> _LabelReferences = new Dictionary<ByteInfo, string>();

        public void Add(AsmInfo instruction)
        {
            AsmCode.Add(instruction);

            int pos = ByteCode.Count();
            AssembleLine(instruction);

            for (int x=pos; x<ByteCode.Count(); ++x)
                instruction.BytesGeneratedPC.Add((ushort)x);
        }

        private void AddByteCode(ushort p, Annotation ano)
        {
            ByteCode.Add(new ByteInfo(p, ano));
        }

        private void AddReference(string name, Annotation ano)
        {
            name = name.ToUpper();
            _LabelReferences.Add(new ByteInfo((ushort)ByteCode.Count, ano), name);
        }

        public void Finalise()
        {
            // lets loop through all the locations where we have label references
            foreach (var key in _LabelReferences.Keys)
            {
                string labelName = _LabelReferences[key];

                if (_LabelAddressDitionary.ContainsKey(labelName) != true)
                    throw new Exception(string.Format("Unknown label reference '{0}'", labelName));

                ByteCode[key.Ins] = _LabelAddressDitionary[labelName];
            }
        }

        public byte[] GetByteCode()
        {
            List<byte> ret = new List<byte>();

            foreach (var word in ByteCode)
            {
                ret.Add((byte)(word.Ins >> 8));
                ret.Add((byte)(word.Ins & 0xFF));
            }

            return ret.ToArray();
        }

        public string[] GetAsmCode()
        {
            List<String> ret = new List<string>();

            ushort x = 0;
            foreach (var str in AsmCode)
            {
                if (str.Ins.Ignore)
                    continue;

                ret.Add(str.Ins.ToString());
                x++;
            }

            return ret.ToArray();
        }

        /// <summary>
        /// returns pc of first instuction generated or -1 if none
        /// </summary>
        private ushort AssembleLine(AsmInfo instruction)
        {
            var ano = instruction.Ano;
            var ins = instruction.Ins.ins;
            var a = instruction.Ins.a;
            var b = instruction.Ins.b;

            if (String.IsNullOrEmpty(ins))
                return 0xFFFF;

            if (ins[0] == ':')
            {
                AssembleLabel(instruction, ins);
                return 0xFFFF;
            }

            ins = ins.ToLower();
            a = a.ToLower();
            b = b.ToLower();

            if (ins == "nop")
                return 0xFFFF;
            else if (ins == "dat")
                return AssembleData(a, ano);

            if (m_opDictionary.ContainsKey(ins) != true)
                throw new Exception(string.Format("Illegal cpu opcode --> {0}", ins));

            ushort opCode = (ushort)((int)m_opDictionary[ins] & 0xF);
            ushort ret = (ushort)ByteCode.Count();

            if (opCode > 0x00)
                AssembleBasicIns(opCode, a, b, ano);
            else
                AssembleAdvIns(ins, a, ano);

            return ret;
        }

        private void AssembleAdvIns(string ins, string a, Annotation ano)
        {
            ushort opCode = (ushort)m_opDictionary[ins];

            OpParamResult p1 = OpParamResult.ParseParam(a);
            opCode |= (ushort)(((ushort)p1.Param << 10) & 0xFC00);

            AddByteCode((ushort)opCode, ano);

            if (p1.nextWord != false)
            {
                if (p1.labelName.Length > 0)
                    AddReference(p1.labelName, ano);

                AddByteCode(p1.NextWordValue, ano);
            }
        }

        private void AssembleBasicIns(ushort opCode, string a, string b, Annotation ano)
        {
            OpParamResult p1 = OpParamResult.ParseParam(a);
            OpParamResult p2 = OpParamResult.ParseParam(b);

            opCode |= (ushort)(((uint)p1.Param << 4) & 0x3F0);
            opCode |= (ushort)(((uint)p2.Param << 10) & 0xFC00);

            AddByteCode((ushort)opCode, ano);

            if (p1.nextWord != false)
            {
                if (p1.labelName.Length > 0)
                    AddReference(p1.labelName, ano);

                AddByteCode(p1.NextWordValue, ano);
            }

            if (p2.nextWord != false)
            {
                if (p2.labelName.Length > 0)
                    AddReference(p2.labelName, ano);

                AddByteCode(p2.NextWordValue, ano);

            }
        }

        private void AssembleLabel(AsmInfo instruction, string ins)
        {
            string labelName = ins.Substring(1, ins.Length - 1).ToUpper();

            if (_LabelAddressDitionary.ContainsKey(labelName) != false)
                throw new Exception(string.Format("Error! Label '{0}' already exists!", labelName));

            _LabelAddressDitionary.Add(labelName.Trim(), new ByteInfo((ushort)ByteCode.Count, instruction.Ano));
        }

        private ushort AssembleData(string data, Annotation ano)
        {
            List<String> dataFields = GetDataSegments(data);

            if (dataFields.Count() == 0)
                return 0xFFFF;

            ushort ret = (ushort)ByteCode.Count();

            foreach (string datSegment in dataFields)
                AssembleDataSegment(datSegment, ano);

            return ret;
        }

        private void AssembleDataSegment(string datSegment, Annotation ano)
        {
            string valStr = datSegment.Trim();
            if (valStr.IndexOf('"') > -1)
            {
                string asciiLine = datSegment.Replace("\"", "").Trim();

                for (int i = 0; i < asciiLine.Length; i++)
                    AddByteCode((ushort)asciiLine[i], ano);
            }
            else
            {
                ushort val = 0;
                if (valStr.Contains("0x") != false)
                    val = Convert.ToUInt16(valStr, 16);
                else
                    val = Convert.ToUInt16(valStr, 10);

                AddByteCode((ushort)val, ano);
            }
        }


        private static List<String> GetDataSegments(string data)
        {
            List<String> dataFields = new List<String>();

            foreach (var field in data.Trim().Split(','))
            {
                if (dataFields.Count == 0)
                {
                    dataFields.Add(field);
                }
                else
                {
                    int count = 0;
                    int last = -1;
                    String lastStr = dataFields[dataFields.Count - 1];

                    while ((last = lastStr.IndexOf('\"', last + 1)) != -1)
                    {
                        count++;
                    }

                    if (count == 1)
                        dataFields[dataFields.Count - 1] += "," + field;
                    else
                        dataFields.Add(field);
                }
            }

            return dataFields;
        }


        static readonly Dictionary<string, dcpuOpCode> m_opDictionary = new Dictionary<string, dcpuOpCode>()
        {
            // non basic instructions
            {"jsr", dcpuOpCode.JSR_OP},

            // basic instructions
            {"set", dcpuOpCode.SET_OP},
            {"add", dcpuOpCode.ADD_OP},
            {"sub", dcpuOpCode.SUB_OP},
            {"mul", dcpuOpCode.MUL_OP},
            {"div", dcpuOpCode.DIV_OP},
            {"mod", dcpuOpCode.MOD_OP},
            {"shl", dcpuOpCode.SHL_OP},
            {"shr", dcpuOpCode.SHR_OP},
            {"and", dcpuOpCode.AND_OP},
            {"bor", dcpuOpCode.BOR_OP},
            {"xor", dcpuOpCode.XOR_OP},
            {"ife", dcpuOpCode.IFE_OP},
            {"ifn", dcpuOpCode.IFN_OP},
            {"ifg", dcpuOpCode.IFG_OP},
            {"ifb", dcpuOpCode.IFB_OP},
        };




        //public void Optimise()
        //{
        //    if (instructions.Count() == 0)
        //        return;

        //    var last = instructions.Count() - 1;

        //    List<List<Instruction>> parts = new List<List<Instruction>>();

        //    parts.Add(new List<Instruction>());

        //    foreach (var i in instructions)
        //    {
        //        if (i.anotation.type == AnotationType.Barrier)
        //        {
        //            parts.Add(new List<Instruction>());
        //        }
        //        else
        //        {
        //            i.anotation = new Annotation();
        //            parts.Last().Add(i);
        //        }
        //    }

        //    for (int x=0; x<parts.Count(); ++x)
        //    {
        //        var list = parts[x];
        //        Optimise(ref list);
        //        parts[x] = list;
        //    }

        //    instructions.Clear();

        //    foreach (var list in parts)
        //        instructions.AddRange(list);
        //}

        //private static void Optimise(ref List<Instruction> instructions)
        //{
        //    var pos = 0;

        //    while (pos+1 < instructions.Count())
        //    {
        //        var insA = instructions[pos];
        //        var insB = instructions[pos+1];

        //        //SET A, POP
        //        //SET PUSH, A
        //        if (insA.ins == "SET" && insB.ins == "SET" && insA.b == "POP" && insB.a == "PUSH" && insB.b == insA.a)
        //        {
        //            //remove both
        //            instructions.Remove(insA);
        //            instructions.Remove(insB);
        //        }
        //        //SET A, !POP
        //        //SET !PUSH, A
        //        else if (insA.ins == "SET" && insB.ins == "SET" && insA.a == insB.b && insA.b != "POP" && insB.a != "PUSH")
        //        {
        //            insA.a = insB.a;
        //            instructions.Remove(insB);
        //        }
        //        //SET PUSH, A
        //        //SET A, POP
        //        else if (insA.ins == "SET" && insB.ins == "SET" && insA.b == insB.a && insA.a == "PUSH" && insB.b == "pop")
        //        {
        //            instructions.Remove(insA);
        //            instructions.Remove(insB);
        //        }
        //        //SET PUSH, A
        //        //SET A, PEEK
        //        else if (insA.ins == "SET" && insB.ins == "SET" && insA.b == insB.a && insA.a == "PUSH" && insB.b == "PEEK")
        //        {
        //            instructions.Remove(insB);
        //        }
        //        //SET A, ?             -> IFN|IFE|IFG ?, A
        //        //IFN|IFE|IFG ?, A
        //        else if (insA.ins == "SET" && (insB.ins == "IFN" || insB.ins == "IFE" || insB.ins == "IFG")
        //            && insA.a == insB.b)
        //        {
        //            insA.ins = insB.ins;
        //            insA.a = insB.a;
        //            instructions.Remove(insB);
        //        }
        //        else
        //        {
        //            pos++;
        //        }
        //    }
        //}

        internal void Clear()
        {
            AsmCode.Clear();
            ByteCode.Clear();
        }
    }
}
