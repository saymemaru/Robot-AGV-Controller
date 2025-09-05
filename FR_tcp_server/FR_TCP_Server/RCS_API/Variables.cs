using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FR_TCP_Server.RCS_API
{
    //定义向RCS传递的参数
    public class Variables
    {
        public string Code { get; set; }
        public object Value { get; set; }
    }
}
