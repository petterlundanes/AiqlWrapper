using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiqlWrapper.Helper;
using Newtonsoft.Json;

namespace AiqlWrapper.Tables
{
    public interface ICustomDimensions
    {
        object this[string s] { get; }
    }
    internal class CustomDimensions : ICustomDimensions
    {
        public object this[string s]
        {
            get
            {
                if (_dict.TryGetValue(s, out object obj))
                    return obj;
                return null;
            }
            set { throw new NotImplementedException();}
        }

        private Dictionary<string, object> _dict = new Dictionary<string, object>();

        public override string ToString()
        {
            if (!_dict.Any())
                return "{}";
            var sb = new StringBuilder();
            sb.Append('{');
            foreach (var kvp in _dict)
            {
                sb.Append($"{kvp.Key}: ");
                if (kvp.Value is string s)
                    sb.Append(JsonConvert.SerializeObject(s));
                else
                    sb.Append(kvp.Value);
                
                sb.Append(", ");
            }
            sb.Length -= 2;
            sb.Append("}");
            return sb.ToString();
        }

        public static ICustomDimensions Load(string str)
        {
            var customDimensions = new CustomDimensions();
            if (string.IsNullOrWhiteSpace(str)) return customDimensions;

            customDimensions._dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(str);

            return customDimensions;
        }
    }
}
