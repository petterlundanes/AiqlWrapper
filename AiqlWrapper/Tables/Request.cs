using System;
using System.Globalization;
using System.Linq;
using AiqlWrapper.Helper;

namespace AiqlWrapper.Tables
{
    public class Request
    {
        internal Request()
        {
        }
        [FieldName("timestamp")]
        public DateTime Timestamp { get; set; }

        [FieldName("id")]
        public string Id { get; set; }

        [FieldName("name")]
        public string Name { get; set; }
        [FieldName("url")]
        public string Url { get; set; }

        [FieldName("customDimensions")]
        public ICustomDimensions CustomDimensions { get; set; }

        [FieldName("itemCount")]
        public long ItemCount { get; set; }

        public override string ToString()
        {
            return $"Timestamp: {Timestamp}, Url: {Url}, Id: {Id}, CustomDimensions: {CustomDimensions}";
        }
    }
}
