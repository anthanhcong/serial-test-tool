using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    partial class Test_Form
    {
        void Bug_trap(bool cond)
        {
            bool foo = true;
            int x = 0;
            while (foo)
            {
                x++;
            }
        }
    }
}