using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLR1
{
    public class TypeList
    {
        public class TypePtr
        {
            public int size;
            public String typeName;

            public TypePtr(String typeName)
            {
                size = 1;
                this.typeName = typeName;
            }
        }
        public class EnumTypePtr:TypePtr
        {
            InnerExpress express;
            public EnumTypePtr(InnerExpress express):base("Enum")
            {
                this.express = express;
                size = express.size;
            }
        }
        public class ArrayTypePtr:TypePtr
        {
            int length;
            TypePtr elementTy;
            public ArrayTypePtr(int length, TypePtr elementTy):base("Array")
            {
                this.length = length;
                this.elementTy = elementTy;
                size = length * elementTy.size;
            }
        }
        public class StructTypePtr : TypePtr
        {
            InnerExpress express;
            public StructTypePtr(InnerExpress express) : base("Struct")
            {
                this.express = express;
                size = express.size;
            }
        }
        static TypePtr IntPtr=new TypePtr("Int");
        static TypePtr CharPtr = new TypePtr("Char");
        static TypePtr FloatPtr = new TypePtr("Float");

        public class InnerExpress
        {
            List<Express> list;
            public int size;
            public InnerExpress()
            {
                list = new List<Express>();
            }
            public void 
                Add(Express express)
            {
                list.Add(express);
            }
        }
        public class Express
        {
            TypePtr type;
            String iden;
            public Express(TypePtr type,String iden)
            {
                this.type = type;
                this.iden = iden;
            }
        }
    }
    
}
