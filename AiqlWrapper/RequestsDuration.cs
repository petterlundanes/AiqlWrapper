using System;
using System.Text;
using Newtonsoft.Json;

namespace AiqlWrapper
{
    public class RequestsDuration
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Interval { get; set; }
        public Segment[] Segments { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Start} to {End}\n{Interval}\nSegments:\n");
            foreach (var segment in Segments)
            {
                sb.Append(segment);
                sb.Append("\n");
            }
            return sb.ToString(0, sb.Length - 1);
        }
    }

    public class Segment
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        //[DataMember(Name = "requests/duration")]
        [JsonProperty(PropertyName = "requests/count")]
        public Item RequestsDuration { get; set; }

        public override string ToString()
        {
            return $"{Start} to {End}: Reqs/dur: {RequestsDuration}";
        }
    }

    public class Item
    {
        [JsonProperty(PropertyName = "sum")]
        public double Avg { get; set; }
        public override string ToString()
        {
            return $"Avg={Avg}";
        }
    }
}
