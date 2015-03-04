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
            using(var tst = WiredTigerNet.Connection.Open(@"C:\_perso\test\DB\", "create"))
            using(var cnx = tst.OpenSession(""))
            {
                cnx.Create("table:test", "key_format=q,value_format=u,block_compressor=snappy");
                using(var crc = cnx.OpenCursorLK("table:test"))
                {
                    for (var i = 1000000; i >= 0; --i)
                    {
                        if ((i % 1000) == 0) Console.Write("\r{0}", i);
                        crc.SetKey(i);
                        crc.SetValue(Encoding.ASCII.GetBytes("Hello world "+ i));
                        crc.Insert();
                    }

                    Console.WriteLine(" ... Completed");
                    crc.Reset();
                    while(crc.Next())
                    {
                        Console.WriteLine("{0} : {1}", crc.GetKey(), Encoding.ASCII.GetString(crc.GetValue()));
                    }
                }
            }
            
        }
    }
}
