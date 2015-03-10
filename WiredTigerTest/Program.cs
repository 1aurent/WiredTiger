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
            using (var tst = WiredTigerNet.Connection.Open(@"C:\_perso\test\DB\", 
                "create,cache_size=10M,eviction=(threads_max=4)"
                //+ ",log=(compressor=snappy,enabled,prealloc)"
                ))
            using (var cnx = tst.OpenSession(""))
            {
                const string tableConfig =
                                       "key_format=q,value_format=u"    // Key format
                                     + ",block_compressor=snappy"       // Enable Google Snappy
                                     + ",memory_page_max=10m"           // From MongoDB - Speed
                                     + ",split_pct=90"                  // From MongoDB - Insert optimized
                                     + ",leaf_value_max=1MB";           // From MongoDB - Insert optimized

                cnx.Create("table:testA", tableConfig);
                cnx.Create("table:testB", tableConfig);
                using (var crcA = cnx.OpenCursorLK("table:testA"))
                using (var crcB = cnx.OpenCursorLK("table:testB"))
                {
                    for (var j = 0L; j <= 1000L; ++j)
                    {
                        var o = j * 1000000L;
                        for (var i = 0L; i <= 1000000L; ++i)
                        {
                            if((i%1000)==0) Console.Write("\r{0}  A {1}  ", j, i+o);
                            crcA.SetKey(i+o);
                            crcA.SetValue(Encoding.ASCII.GetBytes("A Hello world " + i + " Awesome Music Edited Automatically to Fit Your YouTube Videos."));
                            crcA.Insert();
                        }
                        for (var i = 0L; i <= 1000000L; ++i)
                        {
                            if ((i % 1000) == 0) Console.Write("\r{0}  B {1}  ", j, i+o);
                            crcB.SetKey(i + o);
                            crcB.SetValue(Encoding.ASCII.GetBytes("B Hello world " + i + " Awesome Music Edited Automatically to Fit Your YouTube Videos."));
                            crcB.Insert();
                        }
                    }
                }
            }
            Console.WriteLine(" ... Completed");
            
        }
    }
}
