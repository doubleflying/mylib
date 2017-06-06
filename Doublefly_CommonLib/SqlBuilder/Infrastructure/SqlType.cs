using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuilder.Infrastructure
{
    public enum SqlType
    {
        Select,
        Insert,
        Update,
        Delete,
        Count,
        Page
    }
}
