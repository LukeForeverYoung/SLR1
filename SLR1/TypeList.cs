using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLR1
{
    public class TypeList
    {
        public List<TypePtr> list;
        public TypeList()
        {
            list = new List<TypePtr>();
            list.Add(new TypePtr("int"));
            list.Add(new TypePtr("char"));
            list.Add(new TypePtr("float"));
            list.Add(new TypePtr("void"));
        }
        public TypePtr FindType(String kindStr)
        {
            foreach(var item in list)
            {
                if(item.typeName==kindStr)
                {
                    return item;
                }
            }
            return null;
        }
        //基础数据类型的typeKind直接写typeName，其他则调用TypePtr参数
        public class TypePtr
        {
            public int size;
            public String typeName;
            public TypePtr(String typeName)
            {
                size = 1;
                this.typeName = typeName;
            }
            public override string ToString()
            {
                return size + "\t" + typeName;
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
        public class RecordTypePtr : TypePtr
        {
            public class RecordBody
            {
                String name;
                public TypePtr typePtr;
                int offset;
                
                public RecordBody(String name,TypePtr typePtr,int offset)
                {
                    this.name = name;
                    this.typePtr = typePtr;
                    this.offset = offset;
                }
                public override string ToString()
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(name+"\t");
                    builder.Append(typePtr.typeName + "\t");
                    builder.Append(offset);
                    return builder.ToString();
                }
            }
            public void AddBody(RecordBody body)
            {
                records.Add(body);
                size += body.typePtr.size;
            }
            public List<RecordBody> records;
            public RecordTypePtr(String structName) : base(structName)
            {
                records = new List<RecordBody>();
                size = 0;
            }
            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("" + size + "\t" + typeName + "\n");
                foreach(var item in records)
                {
                    builder.Append("\t");
                    builder.Append(item.ToString());
                    builder.Append("\n");
                }
                return builder.ToString();
            }
        }
        
        public class InnerExpress
        {
            List<Express> list;
            public int size;
            public InnerExpress()
            {
                list = new List<Express>();
            }
            public void Add(Express express)
            {
                list.Add(express);
            }
            public override String ToString()
            {
                StringBuilder builder = new StringBuilder();
                foreach(var item in list)
                {
                    builder.Append("\t");
                    builder.Append(item.ToString());
                    builder.Append("\n");
                }
                return builder.ToString();
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
            public override String ToString()
            {
                return "" + type.typeName + " " + iden;
            }
        }
    }
    
}
