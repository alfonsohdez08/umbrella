using System;
using System.Collections.Generic;
using System.Text;

namespace Umbrella
{
    public class Column
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public bool IsNullable { get; set; }
        public Delegate Mapper { get; set; }
    }
}
