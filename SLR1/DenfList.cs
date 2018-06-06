using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SLR1.TypeList;

namespace SLR1
{
    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }
    };
    public class DenfList
    {
        public List<Pair<String,DenfItem> > list;
        public DenfList()
        {
            list = new List<Pair<String, DenfItem> >();
        }
        public bool isRepeat(String name)
        {
            foreach(var item in list)
            {
                if (item.First == name) return true;
            }
            return false;
        }
    }
    public abstract class DenfItem
    {
        public String tokenName;
        public int line;
        public TypePtr typePtr;
        public String kind;
        public DenfItem(String tokenName,TypePtr typePtr,String kind,int line)
        {
            this.tokenName = tokenName;
            this.typePtr = typePtr;
            this.kind = kind;
            this.line = line;
        }
    }
    public class typeKind:DenfItem
    {
        public typeKind(String tokenName, TypePtr typePtr, int line) : base(tokenName, typePtr, "typeKind", line) { }
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(tokenName);
            builder.Append("\t->\t");
            builder.Append(typePtr.typeName + "\t");
            builder.Append(kind);
            return builder.ToString();
        }
    }
    public enum Access { dir, indir };
    public class varKind:DenfItem
    {
        Access access;
        int level;
        int off;
        public varKind(String tokenName, TypePtr typePtr,Access access,int level,int off, int line) :base(tokenName, typePtr,"varKind", line)
        {
            this.access = access;
            this.level = level;
            this.off = off;
        }
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(tokenName);
            builder.Append("\t->\t");
            builder.Append(typePtr.typeName + "\t");
            builder.Append(kind+"\t");
            builder.Append(access+"\t");
            builder.Append(level + "\t");
            builder.Append(off);
            return builder.ToString();
        }
    }
    public class fieldKind:DenfItem //是在struct里的变量，hostType是外部struct
    {
        int off;
        TypePtr hostType;
        public fieldKind(String tokenName, RecordTypePtr typePtr,int off, int line) :base(tokenName, typePtr,"fieldKind", line)
        {
            this.off = off;
            this.hostType = typePtr;
        }
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(tokenName);
            builder.Append("\t->\t");
            builder.Append(typePtr.typeName + "\t");
            builder.Append(kind + "\t");
            builder.Append(hostType.ToString());
            return builder.ToString();
        }
    }
    public enum ClassType { actual,formal}
    public class actualRouteKind : DenfItem
    {
        int level;
        ClassType classType;
        public int parm=-1;
        public int code;
        public int size;
        public bool forward;
        public actualRouteKind(String tokenName, TypePtr typePtr,int level, int line) : base(tokenName, typePtr, "actualRouteKind", line)
        {
            this.level = level;
            classType = ClassType.actual;
            forward = false;
        }
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(tokenName);
            builder.Append("\t->\t");
            builder.Append(typePtr.typeName + "\t");
            builder.Append(kind + "\t");
            builder.Append(level + "\t");
            builder.Append(classType+"\t");
            builder.Append(parm+"\t");
            builder.Append("Leave a blank\t");
            builder.Append("Leave a blank\t");
            builder.Append(forward);
            return builder.ToString();
        }
    }
    public class formalRouteKind:DenfItem
    {
        int level;
        ClassType classType;
        int off;
        public formalRouteKind(String tokenName, TypePtr typePtr, int level,int off, int line) : base(tokenName, typePtr, "actualRouteKind",line)
        {
            this.level = level;
            classType = ClassType.formal;
            this.off = off;
        }
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(tokenName);
            builder.Append("\t->\t");
            builder.Append(typePtr.typeName + "\t");
            builder.Append(kind + "\t");
            builder.Append(level + "\t");
            builder.Append(classType+"\t");
            builder.Append(off);
            return builder.ToString();
        }
    }
}
