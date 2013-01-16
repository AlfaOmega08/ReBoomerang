using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPCDisassembler
{
    class Instruction
    {
        private class FieldDesc
        {
            public FieldDesc(int bitBegin, int bitEnd)
            {
                Begin = 31 - bitBegin;
                End = 31 - bitEnd;
                Signed = false;
                Adjust = 0;
            }

            public FieldDesc(int bitBegin, int bitEnd, bool signed, int adj)
            {
                Begin = 31 - bitBegin;
                End = 31 - bitEnd;
                Signed = signed;
                Adjust = adj;
            }

            public int Begin, End;
            public bool Signed;
            public int Adjust;
        }

        public Instruction(uint instruction)
        {
            instr = instruction;
        }

        public ulong Field(String f)
        {
            FieldDesc desc = fields[f];
            ulong mask = 0;
            for (int i = desc.Begin; i >= desc.End; i--)
                mask |= (ulong) (1 << i);

            ulong val = (instr & mask) >> (int)desc.End;
            if (desc.Signed)
            {
                long sign = (long)val;
                sign <<= (int)(64 - (desc.Begin - desc.End + 1));
                sign >>= (int)(64 - (desc.Begin - desc.End + 1));
                val = (ulong)sign;
            }

            if (desc.Adjust != 0)
                val <<= desc.Adjust;

            return val;
        }

        public String Fields(params String[] f)
        {
            String ret = "";
            for (int i = 0; i < f.Length; i++)
            {
                ulong field = Field(f[i]);
                if (fields[f[i]].Signed)
                    ret += (long)field;
                else
                    ret += field;

                if (i != f.Length - 1)
                    ret += ", ";
            }

            return ret;
        }

        Dictionary<String, FieldDesc> fields = new Dictionary<String, FieldDesc>()
        {
            { "MainOP", new FieldDesc(0, 5) },

            { "A", new FieldDesc(11, 15) },
            { "B", new FieldDesc(16, 20) },
            { "C", new FieldDesc(21, 25) },
            { "D", new FieldDesc(6, 10) },
            { "S", new FieldDesc(6, 10) },
            { "d", new FieldDesc(16, 31, true, 0) },
            { "ds", new FieldDesc(16, 29, true, 2) },
            { "SIMM", new FieldDesc(16, 31, true, 0) },
            { "UIMM", new FieldDesc(16, 31) },
            { "crfD", new FieldDesc(6, 8) },
            { "crbD", new FieldDesc(6, 10) },
            { "crbA", new FieldDesc(11, 15) },
            { "crbB", new FieldDesc(16, 20) },
            { "OE", new FieldDesc(21, 21) },
            { "L", new FieldDesc(10, 10) },
            { "Rc", new FieldDesc(31, 31) },
            { "TO", new FieldDesc(6, 10) },
            { "BO", new FieldDesc(6, 10) },
            { "BI", new FieldDesc(11, 15) },
            { "BD", new FieldDesc(16, 29, true, 2) },
            { "AA", new FieldDesc(30, 30) },
            { "LK", new FieldDesc(31, 31) },
            { "LI", new FieldDesc(6, 29, true, 0) },
        };

        uint instr;
    }

    public class PPCDisassembler : ReBoomerang.IDisassembler
    {
        public String Decode(uint instruction)
        {
            Instruction i = new Instruction(instruction);
            switch (i.Field("MainOP"))
            {
                case 2:
                    return "tdi " + i.Fields("TO", "A", "SIMM");
                case 3:
                    return "twi " + i.Fields("TO", "A", "SIMM");
                case 7:
                    return "mulli " + i.Fields("D", "A", "SIMM");
                case 8:
                    return "subfic " + i.Fields("D", "A", "SIMM");
                case 10:
                    return "cmpli " + i.Fields("crfD", "L", "A", "UIMM");
                case 11:
                    return "cmpi " + i.Fields("crfD", "L", "A", "SIMM");
                case 12:
                    return "addic " + i.Fields("D", "A", "SIMM");
                case 13:
                    return "addic. " + i.Fields("D", "A", "SIMM");
                case 14:
                    return "addi " + i.Fields("D", "A", "SIMM");
                case 15:
                    return "addis " + i.Fields("D", "A", "SIMM");
                case 16:
                    return "bc " + i.Fields("BO", "BI", "BD", "AA", "LK");
                case 17:
                    return "sc";
                case 18:
                    return "b" + i.Fields("LI", "AA", "LK");
                case 19:
                    switch (i.Field("SecOP"))
                    {
                    }
                    break;
                case 20:
                    if (i.Field("Rc") == 0)
                        return "rlwimi " + i.Fields("S", "A", "SH", "MB", "ME");
                    else
                        return "rlwimi. " + i.Fields("S", "A", "SH", "MB", "ME");
                case 21:
                    if (i.Field("Rc") == 0)
                        return "rlwinm " + i.Fields("S", "A", "SH", "MB", "ME");
                    else
                        return "rlwinm. " + i.Fields("S", "A", "SH", "MB", "ME");
                case 23:
                    if (i.Field("Rc") == 0)
                        return "rlwnm " + i.Fields("S", "A", "B", "MB", "ME");
                    else
                        return "rlwnm. " + i.Fields("S", "A", "B", "MB", "ME");
                case 24:
                    return "ori " + i.Fields("S", "A", "UIMM");
                case 25:
                    return "oris " + i.Fields("S", "A", "UIMM");
                case 26:
                    return "xori " + i.Fields("S", "A", "UIMM");
                case 27:
                    return "xoris " + i.Fields("S", "A", "UIMM");
                case 28:
                    return "andi. " + i.Fields("S", "A", "UIMM");
                case 29:
                    return "andis. " + i.Fields("S", "A", "UIMM");
                case 30:
                case 31:
                    switch (i.Field("SecOP"))
                    {
                    }
                    break;
                default:
                    throw new Exception("Unknown instruction.");
            }
        }
    }
}
