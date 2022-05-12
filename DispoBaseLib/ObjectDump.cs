using System;
using System.Text;
using System.Reflection;

namespace DispoBaseLib
{
    public static class ObjectDump
    {
        public static string DumpProperties(object obj)
        {
            StringBuilder sb = new StringBuilder(5000);

            Type t = obj.GetType();
            sb.AppendLine($"Typ {t.FullName}\n======================================");

            foreach (PropertyInfo prop in t.GetProperties())
            {
                object value = null;
                if (prop.CanRead)
                {
                    value = prop.GetValue(obj);
                }
                sb.AppendLine($"{prop.Name}\t({prop.PropertyType.Name})\t{value}");
            }
            return sb.ToString();
        }
    }
}
