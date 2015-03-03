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
                cnx.Create("table:test", "key_format=q,value_format=u");
                using(var crc = cnx.OpenCursorLK("table:test"))
                {
                    crc.SetKey(10);
                    crc.SetValue(Encoding.ASCII.GetBytes("Hello"));
                    crc.Insert();

                    crc.SetKey(11);
                    crc.SetValue(Encoding.ASCII.GetBytes("World"));
                    crc.Insert();

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
