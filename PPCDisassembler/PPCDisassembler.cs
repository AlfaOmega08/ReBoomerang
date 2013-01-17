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
            { "IMM", new FieldDesc(16, 19) },
            { "FM", new FieldDesc(7, 14) },

            { "Op63", new FieldDesc(21, 30) },
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
                    switch (i.Field("Op19"))
                    {
                        case 0:
                            return "mcrf " + i.Fields("crfD", "crfS");
                        case 16:
                            return "bclr " + i.Fields("BO", "BI", "BH", "LK");
                        case 18:
                            return "rfid";
                        case 33:
                            return "crnor " + i.Fields("crbD", "crbA", "crbB");
                        case 129:
                            return "crandc " + i.Fields("crbD", "crbA", "crbB");
                        case 150:
                            return "isync";
                        case 193:
                            return "crxor " + i.Fields("crbD", "crbA", "crbB");
                        case 225:
                            return "crnand " + i.Fields("crbD", "crbA", "crbB");
                        case 257:
                            return "crand " + i.Fields("crbD", "crbA", "crbB");
                        case 289:
                            return "creqv " + i.Fields("crbD", "crbA", "crbB");
                        case 417:
                            return "crorc " + i.Fields("crbD", "crbA", "crbB");
                        case 449:
                            return "cror " + i.Fields("crbD", "crbA", "crbB");
                        case 528:
                            return "bcctr " + i.Fields("BO", "BI", "BH", "LK");
                        default:
                            throw new Exception("Invalid OpCode");
                    }
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
                    throw new NotImplementedException("Missing opcode decoding.");
                case 31:
                    switch (i.Field("Op31"))
                    {
                        case 0:
                        case 4:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 19:
                        case 20:
                        case 21:
                        case 23:
                        case 24:
                        case 26:
                        case 27:
                        case 28:
                        case 32:
                        case 40:
                        case 53:
                        case 54:
                        case 55:
                        case 58:
                        case 60:
                        case 68:
                        case 73:
                        case 75:
                        case 83:
                        case 84:
                        case 86:
                        case 87:
                        case 104:
                        case 119:
                        case 124:
                        case 136:
                        case 138:
                        case 144:
                        case 146:
                        case 149:
                        case 150:
                        case 151:
                        case 178:
                        case 181:
                        case 183:
                        case 200:
                        case 202:
                        case 210:
                        case 214:
                        case 215:
                        case 232:
                        case 233:
                        case 234:
                        case 235:
                        case 242:
                        case 246:
                        case 247:
                        case 266:
                        case 274:
                        case 278:
                        case 279:
                        case 284:
                        case 306:
                        case 310:
                        case 311:
                        case 316:
                        case 339:
                        case 341:
                        case 343:
                        case 370:
                        case 371:
                        case 373:
                        case 375:
                        case 402:
                        case 407:
                        case 412:
                        case 434:
                        case 438:
                        case 439:
                        case 444:
                        case 457:
                        case 459:
                        case 467:
                        case 476:
                        case 489:
                        case 491:
                        case 498:
                        case 512:
                        case 533:
                        case 534:
                        case 535:
                        case 536:
                        case 539:
                        case 566:
                        case 567:
                        case 595:
                        case 597:
                        case 598:
                        case 599:
                        case 631:
                        case 659:
                        case 661:
                        case 662:
                        case 663:
                        case 695:
                        case 725:
                        case 727:
                        case 759:
                        case 790:
                        case 792:
                        case 794:
                        case 824:
                        case 826:
                        case 827: //826-827 sradix
                        case 851:
                        case 854:
                        case 915:
                        case 918:
                        case 922:
                        case 954:
                        case 982:
                        case 983:
                        case 986:
                        case 1014:
                        default:
                            throw new Exception("Invalid instruction.");
                    }
                case 32:
                    return "lwz " + i.Fields("D", "A", "d");
                case 33:
                    return "lwzu " + i.Fields("D", "A", "d");
                case 34:
                    return "lbz " + i.Fields("D", "A", "d");
                case 35:
                    return "lbzu " + i.Fields("D", "A", "d");
                case 36:
                    return "stw " + i.Fields("S", "A", "d");
                case 37:
                    return "stwu " + i.Fields("S", "A", "d");
                case 38:
                    return "stb " + i.Fields("S", "A", "d");
                case 39:
                    return "stbu " + i.Fields("S", "A", "d");
                case 40:
                    return "lhz " + i.Fields("D", "A", "d");
                case 41:
                    return "lhzu " + i.Fields("D", "A", "d");
                case 42:
                    return "lha " + i.Fields("D", "A", "d");
                case 43:
                    return "lhau " + i.Fields("D", "A", "d");
                case 44:
                    return "sth " + i.Fields("S", "A", "d");
                case 45:
                    return "sthu " + i.Fields("S", "A", "d");
                case 46:
                    return "lmw " + i.Fields("D", "A", "d");
                case 47:
                    return "stmw " + i.Fields("S", "A", "d");
                case 48:
                    return "lfs " + i.Fields("D", "A", "d");
                case 49:
                    return "lfsu " + i.Fields("D", "A", "d");
                case 50:
                    return "lfd " + i.Fields("D", "A", "d");
                case 51:
                    return "lfdu " + i.Fields("D", "A", "d");
                case 52:
                    return "stfs " + i.Fields("S", "A", "d");
                case 53:
                    return "stfsu " + i.Fields("S", "A", "d");
                case 54:
                    return "stfd " + i.Fields("S", "A", "d");
                case 55:
                    return "stfdu " + i.Fields("S", "A", "d");
                case 58:
                    switch (i.Field("Op58"))
                    {
                        case 0:
                            return "ld " + i.Fields("D", "A", "ds");
                        case 1:
                            return "ldu " + i.Fields("D", "A", "ds");
                        case 2:
                            return "lwa " + i.Fields("D", "A", "ds");
                        default:
                            throw new Exception("Invalid instruction.");
                    }
                case 59:
                    switch (i.Field("Op59"))
                    {
                        case 18:
                            if (i.Field("Rc") == 0)
                                return "fdivs " + i.Fields("D", "A", "B");
                            else
                                return "fdivs. " + i.Fields("D", "A", "B");
                        case 20:
                            if (i.Field("Rc") == 0)
                                return "fsubs " + i.Fields("D", "A", "B");
                            else
                                return "fsubs. " + i.Fields("D", "A", "B");
                        case 21:
                            if (i.Field("Rc") == 0)
                                return "fadds " + i.Fields("D", "A", "B");
                            else
                                return "fadds. " + i.Fields("D", "A", "B");
                        case 22:
                            if (i.Field("Rc") == 0)
                                return "fsqrts " + i.Fields("D", "B");
                            else
                                return "fsqrts. " + i.Fields("D", "B");
                        case 24:
                            if (i.Field("Rc") == 0)
                                return "fres " + i.Fields("D", "B");
                            else
                                return "fres. " + i.Fields("D", "B");
                        case 25:
                            if (i.Field("Rc") == 0)
                                return "fmuls " + i.Fields("D", "A", "C");
                            else
                                return "fmuls. " + i.Fields("D", "A", "C");
                        case 28:
                            if (i.Field("Rc") == 0)
                                return "fmsubs " + i.Fields("D", "A", "B", "C");
                            else
                                return "fmsubs. " + i.Fields("D", "A", "B", "C");
                        case 29:
                            if (i.Field("Rc") == 0)
                                return "fmadds " + i.Fields("D", "A", "B", "C");
                            else
                                return "fmadds. " + i.Fields("D", "A", "B", "C");
                        case 30:
                            if (i.Field("Rc") == 0)
                                return "fnmsubs " + i.Fields("D", "A", "B", "C");
                            else
                                return "fnmsubs. " + i.Fields("D", "A", "B", "C");
                        case 31:
                            if (i.Field("Rc") == 0)
                                return "fnmadds " + i.Fields("D", "A", "B", "C");
                            else
                                return "fnmadds. " + i.Fields("D", "A", "B", "C");
                        default:
                            throw new Exception("Invalid OpCode");
                    }
                    break;
                case 62:
                    switch (i.Field("Op62"))
                    {
                        case 0:
                            return "std " + i.Fields("S", "A", "ds");
                        case 1:
                            return "stdu " + i.Fields("A", "A", "ds");
                        default:
                            throw new Exception("Invalid OpCode");
                    }
                case 63:
                    switch (i.Field("Op63"))
                    {
                        case 0:
                            return "fcmpu " + i.Fields("crfD", "A", "B");
                        case 12:
                            if (i.Field("Rc") == 0)
                                return "frsp " + i.Fields("D", "B");
                            else
                                return "frsp. " + i.Fields("D", "B");
                        case 14:
                            if (i.Field("Rc") == 0)
                                return "fctiw " + i.Fields("D", "B");
                            else
                                return "fctiw. " + i.Fields("D", "B");
                        case 15:
                            if (i.Field("Rc") == 0)
                                return "fctiwz " + i.Fields("D", "B");
                            else
                                return "fctiwz. " + i.Fields("D", "B");
                        case 32:
                            return "fcmpo " + i.Fields("crfD", "A", "B");
                        case 38:
                            if (i.Field("Rc") == 0)
                                return "mtfsb1 " + i.Fields("crbD");
                            else
                                return "mtfsb1. " + i.Fields("crbD");
                        case 40:
                            if (i.Field("Rc") == 0)
                                return "fneg " + i.Fields("D", "B");
                            else
                                return "fneg. " + i.Fields("D", "B");
                        case 64:
                            if (i.Field("Rc") == 0)
                                return "mcrfs " + i.Fields("crfD", "crfS");
                            else
                                return "mcrfs. " + i.Fields("crfD", "crfS");
                        case 70:
                            if (i.Field("Rc") == 0)
                                return "mtfsb0 " + i.Fields("crbD");
                            else
                                return "mtfsb0. " + i.Fields("crbD");
                        case 72:
                            if (i.Field("Rc") == 0)
                                return "fmr " + i.Fields("D", "B");
                            else
                                return "fmr. " + i.Fields("D", "B");
                        case 136:
                            if (i.Field("Rc") == 0)
                                return "mtfsfi " + i.Fields("crfD", "IMM");
                            else
                                return "mtfsfi. " + i.Fields("crfD", "IMM");
                        case 138:
                            if (i.Field("Rc") == 0)
                              return "fnabs " + i.Fields("D", "B");
                            else
                              return "fnabs. " + i.Fields("D", "B");
                        case 264:
                            if (i.Field("Rc") == 0)
                                return "fabs " + i.Fields("D", "B");
                            else
                                return "fabs. " + i.Fields("D", "B");
                        case 583:
                            if (i.Field("Rc") == 0)
                                return "mffs " + i.Fields("D");
                            else
                                return "mffs. " + i.Fields("D");
                        case 711:
                            if (i.Field("Rc") == 0)
                                return "mtfsf " + i.Fields("FM", "B");
                            else
                                return "mtfsf. " + i.Fields("FM", "B");
                        case 814:
                            if (i.Field("Rc") == 0)
                                return "fctid " + i.Fields("D", "B");
                            else
                                return "fctid. " + i.Fields("D", "B");
                        case 815:
                            if (i.Field("Rc") == 0)
                                return "fctidz " + i.Fields("D", "B");
                            else
                                return "fctidz. " + i.Fields("D", "B");
                        case 846:
                            if (i.Field("Rc") == 0)
                                return "fcfidx " + i.Fields("D", "B");
                            else
                                return "fcfidx. " + i.Fields("D", "B");
                        default:
                            if ((i.Field("Op63") & 0x10) == 0x10)
                            {
                                switch (i.Field("Op63") & 0xF)
                                {
                                    case 2:
                                        if (i.Field("Rc") == 0)
                                            return "fdiv " + i.Fields("D", "A", "B");
                                        else
                                            return "fdiv. " + i.Fields("D", "A", "B");
                                    case 4:
                                        if (i.Field("Rc") == 0)
                                            return "fsub " + i.Fields("D", "A", "B");
                                        else
                                            return "fsub. " + i.Fields("D", "A", "B");
                                    case 5:
                                        if (i.Field("Rc") == 0)
                                            return "fadd " + i.Fields("D", "A", "B");
                                        else
                                            return "fadd. " + i.Fields("D", "A", "B");
                                    case 6:
                                        if (i.Field("Rc") == 0)
                                            return "fsqrt " + i.Fields("D", "B");
                                        else
                                            return "fsqrt. " + i.Fields("D", "B");
                                    case 7:
                                        if (i.Field("Rc") == 0)
                                            return "fsel " + i.Fields("D", "A", "B", "C");
                                        else
                                            return "fsel. " + i.Fields("D", "A", "B", "C");
                                    case 9:
                                        if (i.Field("Rc") == 0)
                                            return "fmul " + i.Fields("D", "A", "C");
                                        else
                                            return "fmul. " + i.Fields("D", "A", "C");
                                    case 10:
                                        if (i.Field("Rc") == 0)
                                            return "frsqrte " + i.Fields("D", "B");
                                        else
                                            return "frsqrte. " + i.Fields("D", "B");
                                    case 12:
                                        if (i.Field("Rc") == 0)
                                            return "fmsub " + i.Fields("D", "A", "B", "C");
                                        else
                                            return "fmsub. " + i.Fields("D", "A", "B", "C");
                                    case 13:
                                        if (i.Field("Rc") == 0)
                                            return "fmadd " + i.Fields("D", "A", "B", "C");
                                        else
                                            return "fmadd. " + i.Fields("D", "A", "B", "C");
                                    case 14:
                                        if (i.Field("Rc") == 0)
                                            return "fnmsub " + i.Fields("D", "A", "B", "C");
                                        else
                                            return "fnmsub. " + i.Fields("D", "A", "B", "C");
                                    case 15:
                                        if (i.Field("Rc") == 0)
                                            return "fnmadd " + i.Fields("D", "A", "B", "C");
                                        else
                                            return "fnmadd. " + i.Fields("D", "A", "B", "C");
                                    default:
                                        throw new Exception("Unknown instruction.");
                                }
                            }
                            else
                                throw new Exception("Unknown instruction.");
                    }
                default:
                    throw new Exception("Unknown instruction.");
            }
        }
    }
}
