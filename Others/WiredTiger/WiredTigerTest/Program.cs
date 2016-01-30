using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace WiredTigerTest
{
    class Program
    {
        struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [DllImport("kernel32.dll")]
        static extern bool GetProcessIoCounters(IntPtr ProcessHandle, out IO_COUNTERS IoCounters);

        static ulong LastIORead=0;

        static ulong IoReads()
        {
            IO_COUNTERS counters;

            GetProcessIoCounters(System.Diagnostics.Process.GetCurrentProcess().Handle, out counters);
            if(LastIORead==0)
            {
                LastIORead = counters.ReadTransferCount;
                return 0;
            }

            var r = counters.ReadTransferCount - LastIORead;
            LastIORead = counters.ReadTransferCount;
            return r;
        }

        static void Main(string[] args)
        {
            using (var tst = WiredTigerNet.Connection.Open(@"D:\_dev\WiredTiger\_Test\", 
                "create"
                + ",cache_size=5G,eviction=(threads_max=4)"
                //+ ",log=(compressor=snappy,enabled,prealloc)"
                ))
            using (var cnx = tst.OpenSession(""))
            {
                const string tableConfig =
                                        "key_format=q,value_format=u"    // Key format
                                        + ",prefix_compression"
                                        + ",block_compressor=snappy"       // Enable Google Snappy
                                        + ",memory_page_max=10m"           // From MongoDB - Speed
                                        + ",split_pct=90"                  // From MongoDB - Insert optimized
                                        + ",leaf_value_max=1MB";           // From MongoDB - Insert optimized

                cnx.Create("table:testA", tableConfig);
                cnx.Create("table:testB", tableConfig);

                var sw = new System.Diagnostics.Stopwatch();
                using (var crcA = cnx.OpenCursorLK("table:testA"))
                using (var crcB = cnx.OpenCursorLK("table:testB"))
                {
                    var r = new Random(108); //< Make it repeatable
                    //const long grandCount = 30L;
                    for (var j = 0L; j <= 50; ++j)
                    {
                        for (var i = 0L; i < 1000000L; ++i)
                        {
                            if ((i % 1000) == 0) Console.Write("\r{0} ", i);
                            sw.Start();
                            var k = (long)(r.NextDouble() * long.MaxValue);
                            crcA.SetKey(k);
                            crcA.SetValue(Encoding.ASCII.GetBytes(k + "%A%Random filler:" + (new string('*', r.Next(1, 40)))));
                            crcA.Insert();
                            
                            sw.Stop();
                        }
                        Console.WriteLine("\r{0},{1},{2}                              ", j, sw.ElapsedMilliseconds, IoReads());
                        sw.Reset();
                    }


                    //IoReads();
                    //for (var j = 0L; j <= grandCount; ++j)
                    //{
                    //    var o = j * 1000000L;
                    //    for (var i = 0L; i < 1000000L; ++i)
                    //    {
                    //        if ((i % 1000) == 0) Console.Write("\r{0}  A {1}  ", j, i + o);
                    //        sw.Start();
                    //        crcA.SetKey(i + o);
                    //        crcA.SetValue(Encoding.ASCII.GetBytes(i + "%A%Random filler:" + (new string('*', r.Next(1, 40)))));
                    //        crcA.Insert();
                    //        sw.Stop();
                    //    }
                    //    for (var i = 0L; i < 1000000L; ++i)
                    //    {
                    //        if ((i % 1000) == 0) Console.Write("\r{0}  B {1}  ", j, i + o);
                    //        sw.Start();
                    //        crcB.SetKey(i + o);
                    //        crcB.SetValue(Encoding.ASCII.GetBytes(i + "%B%Random filler:" + (new string('*', r.Next(1, 40)))));
                    //        crcB.Insert();
                    //        sw.Stop();
                    //    }
                    //    Console.WriteLine("\r{0},{1},{2}                              ", j, sw.ElapsedMilliseconds,IoReads());
                    //    sw.Reset();
                    //}
                    //Console.WriteLine(" Upload Completed");

                    //Console.WriteLine(" Scan test");
                    //for (var j = 0L; j < 2; ++j)
                    //{
                    //    var crc = (j & 1) == 0 ? crcA : crcB;
                    //    crc.Reset();
                    //    sw.Start();
                    //    IoReads();
                    //    while (crc.Next())
                    //    {
                    //        var k = crc.GetKey();
                    //        var s = Encoding.ASCII.GetString(crc.GetValue()).Split('%');
                    //        if (long.Parse(s[0]) != (k % 1000000L)) throw new Exception("Invalid Key!");
                    //        if (k == 0) continue;
                    //        if ((k % 1000) == 0) Console.Write("\r{0} {1}  ", j, k);
                    //        if ((k % 1000000) == 0)
                    //        {
                    //            sw.Stop();
                    //            Console.WriteLine("\r{0},{1},{2}                             ", k / 1000000L, sw.ElapsedMilliseconds, IoReads());
                    //            sw.Reset();
                    //            sw.Start();
                    //        }
                    //    }
                    //    sw.Stop();
                    //    sw.Reset();
                    //}

                    //Console.WriteLine("\rRandom key test    ");
                    //const long maxKey = grandCount * 1000000L;
                    //IoReads(); //< reset stats
                    //for (var j = 0L; j <= 5; ++j)
                    //{
                    //    for (var i = 0L; i <= 1000000L; ++i)
                    //    {
                    //        if ((i % 1000) == 0) Console.Write("\r{0} {1}  ", j, i);
                    //        var k = (long)(r.NextDouble() * maxKey);
                    //        var crc = (i & 1) == 0 ? crcA : crcB;
                    //        sw.Start();
                    //        crc.SetKey(k);
                    //        if (!crc.Search()) throw new Exception("Not found!");
                    //        sw.Stop();
                    //        var s = Encoding.ASCII.GetString(crc.GetValue()).Split('%');
                    //        if (long.Parse(s[0]) != (k%1000000L)) throw new Exception("Invalid Key!");
                    //    }
                    //    Console.WriteLine("\r{0},{1},{2}                              ", j, sw.ElapsedMilliseconds, IoReads());
                    //    sw.Reset();
                    //}
                }

            }
            
        }
    }
}
