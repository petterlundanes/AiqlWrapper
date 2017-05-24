using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AiqlWrapper.Tables;

namespace AiqlWrapper.ObjectBuilder
{
    internal static class DefaultBuilderFactory
    {
        public static IBuilder Get<T>()
        {
            return new DefaultBuilder();
        }
    }

    internal class DefaultBuilder : IBuilder
    {
        public IEnumerable<T> Build<T>(ResultTable rt)
        {
            var type = typeof(T);
            var props = type.GetProperties();//.Select(pi => (Setter: pi.GetSetMethod(), Type: pi.PropertyType));
            var cmp = StringComparer.InvariantCultureIgnoreCase;
            var setters = new List<(MethodInfo, int, MethodInfo)>();
            for (var i = 0; i < rt.Columns.Length; i++)
            {
                var col = rt.Columns[i];
                var prop = props.SingleOrDefault(pi => cmp.Compare(col.ColumnName, pi.Name) == 0);
                var setter = prop?.GetSetMethod(false);
                if (setter == null) continue;

                MethodInfo converter = null;
                if (typeof(ICustomDimensions).IsAssignableFrom(prop.PropertyType))
                {
                    converter = typeof(CustomDimensions).GetMethod(nameof(CustomDimensions.Load),
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                }

                setters.Add((setter, i, converter));
            }
            foreach (var row in rt.Rows)
            {
                var item = (T)Activator.CreateInstance(type, true);
                foreach (var (setter, idx, converter) in setters)
                {
                    var data = row[idx];
                    object val = converter == null ? data : converter.Invoke(null, new[] {data});
                    setter.Invoke(item, new[]{val});
                }
                yield return item;
            }
        }
    }
}
