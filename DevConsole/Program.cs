using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiqlWrapper;

namespace DevConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Test("DEMO_APP", "DEMO_KEY");
            Console.ReadKey();
        }

        public static void Test(string appid, string apiKey)
        {
            var cli = new ApplicationInsightsClient(appid, apiKey);
            var time = DateTime.UtcNow.AddDays(-2);
            var strings = new[] { "foo", "bar" };
            var res = cli.Requests
                //.Where(r => r.Timestamp > time && r.Name.StartsWith("GET feed/get"))
                //.Where(r => r.Timestamp > AiqlFuncs.Ago(TimeSpan.FromDays(3)))
                .Where(r => r.Timestamp > AiqlFuncs.Ago(TimeSpan.FromMinutes(10)))
                //.Where(r => !(!r.Name.Contains("swagger") && r.Name != "abc"))
                //.Where(r => r.Name == strings.Single(s => s.StartsWith("ba")))
                //.Top(20, r => r.Timestamp).ThenByDescending(r => r.Name)
                //.Where(r => r.CustomDimensions["userid"] == null)
                //.Select(r => new { url = r.Url, comp = r.Timestamp, VistorType = AiqlFuncs.Iff(r.CustomDimensions["userid"] != null, "Registered", "Guest") })
                .Select(r => new { url = r.Url, comp = r.Timestamp, VisitorType = r.CustomDimensions["userid"] != null ? "Registered" : "Guest" })
                //.Select(r => r.Timestamp)
                //.Select(r => new TargetClass(r.Url) {Timestamp = r.Timestamp, VisitorType = r.CustomDimensions["userid"] != null ? "Registered" : "Guest" })
                //.Select(tc => new {tc.Url, tc.Timestamp, tc.VisitorType})
                //.Select(r => new { Count = AiqlFuncs.Count(), name = r.Name, time = AiqlFuncs.Bin(r.Timestamp, TimeSpan.FromDays(1))})
                //.Where(r => r.Timestamp > AiqlFuncs.Ago(TimeSpan.FromDays(90)))
                //.Select(r => new { s = AiqlFuncs.Sum(r.ItemCount), c = AiqlFuncs.Count(), average = AiqlFuncs.Avg(r.ItemCount), comp = AiqlFuncs.ToDouble(AiqlFuncs.Sum(r.ItemCount))/AiqlFuncs.ToDouble(AiqlFuncs.Count()), timestamp = AiqlFuncs.Bin(r.Timestamp, TimeSpan.FromDays(10)) })
                //.OrderByDescending(a => a.Count).Take(20)
                .ToList();
            //.LongCount(r => r.comp > AiqlFuncs.Ago(TimeSpan.FromMinutes(10)));
            //.FirstOrDefault(r => r.Name == "abc");
            //Console.WriteLine(res);
            Console.WriteLine($"Got {res.Count} results");
            foreach (var re in res.Take(10))
            {
                Console.WriteLine(re.ToString());
            }
            //var res = cli.Requests.Where(r => r.Timestamp.Hour > time.Hour + time.Hour).ToList();
        }

        private class TargetClass
        {
            public TargetClass()
            {

            }

            public TargetClass(string url)
            {
                Url = url;
            }
            public DateTime Timestamp { get; set; }
            public string Url { get; set; }

            public string VisitorType { get; set; }

            public override string ToString()
            {
                return $"Timestamp: {Timestamp}, Url: {Url}, VisitorType: {VisitorType}";
            }
        }
    }
}
