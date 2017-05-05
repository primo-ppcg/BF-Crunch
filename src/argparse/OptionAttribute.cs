using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

using ppcg.math;
using ppcg.text;

namespace ppcg.argparse {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OptionAttribute : Attribute {

        public string Name { get; set; }

        public char ShortName { get; set; }

        public string Placeholder { get; set; }

        public bool Required { get; set; }

        public string HelpText { get; set; }

        public string ToString(PropertyInfo prop) {
            StringBuilder sb = new StringBuilder();

            string usage = string.Format("--{0}", Name);
            if(MyMath.IsNumeric(prop.PropertyType)) {
                usage = string.Format("{0}={1}", usage, Placeholder ?? "#");
            } else if(prop.PropertyType != typeof(bool)) {
                usage = string.Format("{0}={1}", usage, Placeholder ?? "value");
            }
            if(ShortName != default(char)) {
                usage = string.Format("-{0}, {1}", ShortName, usage);
            }

            sb.Append(string.Format("  {0,-16}  ", usage));

            if(usage.Length > 16) {
                sb.Append(Environment.NewLine);
                sb.Append(' ', 20);
            }

            string helptext = HelpText;
            DefaultValueAttribute dvattr = (DefaultValueAttribute)prop.GetCustomAttributes(typeof(DefaultValueAttribute), true).FirstOrDefault();
            if(dvattr is DefaultValueAttribute) {
                helptext = string.Format("{0} Default = {1}", helptext, dvattr.Value);
            }

            sb.Append(string.Join(Environment.NewLine + new string(' ', 20), helptext.WordWrap(Console.WindowWidth - 20)));
            return sb.ToString();
        }

    }
}
