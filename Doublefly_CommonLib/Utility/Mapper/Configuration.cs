using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility.Mapper.Adapters;

namespace Utility.Mapper
{
    public class Configuration
    {
        public static void Initialize(Assembly[] assemblys)
        {
            AutoMapperAdapter.InitProfile(assemblys);
        }
    }
}
