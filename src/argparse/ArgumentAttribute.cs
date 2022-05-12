using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

using ppcg.text;

namespace ppcg.argparse {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ArgumentAttribute : Attribute {

        public string Name { get; set; }

        public bool Required { get; set; }

        public string HelpText { get; set; }

        public string ToString(PropertyInfo prop) {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("  {0,-16}  ", Name));
            if(Name.Length > 16) {
                sb.Append(Environment.NewLine);
                sb.Append(' ', 20);
            }

            string helptext = HelpText;
            DefaultValueAttribute dvattr = (DefaultValueAttribute)prop.GetCustomAttributes(typeof(DefaultValueAttribute), true).FirstOrDefault();
            if(dvattr is DefaultValueAttribute) {
                helptext = string.Format("{0} Default = {1}", helptext, dvattr.Value ?? "(empty)");
            }

            sb.Append(string.Join(Environment.NewLine + new string(' ', 20), helptext.WordWrap(Console.WindowWidth - 20)));
            return sb.ToString();
        }

    }
}
