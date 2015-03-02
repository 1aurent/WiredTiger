using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiredTigerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var tst = WiredTigerNet.Connection.Open("D:\\_dev\\test\\", "");
        }
    }
}
