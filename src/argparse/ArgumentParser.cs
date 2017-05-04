using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ppcg.argparse {
    public class ArgumentParser<T> where T: new() {

        private static Regex optionregex = new Regex(@"^--(.+?)(?:=(.+))?$|^-(.)(.+)?$", RegexOptions.Compiled);

        private IEnumerable<PropertyInfo> arguments;
        private IEnumerable<PropertyInfo> options;

        public ArgumentParser() {
            this.arguments = typeof(T).GetProperties().Where(
                prop => prop.GetCustomAttributes(typeof(ArgumentAttribute), true).Length == 1
            );

            this.options = typeof(T).GetProperties().Where(
                prop => prop.GetCustomAttributes(typeof(OptionAttribute), true).Length == 1
            );
        }

        public T Parse(string[] args) {
            T output = new T();

            // set default values
            foreach(PropertyInfo propinfo in typeof(T).GetProperties().Where(
                prop => prop.GetCustomAttributes(typeof(DefaultValueAttribute), true).Length == 1
            )) {
                DefaultValueAttribute dvattr = (DefaultValueAttribute)propinfo.GetCustomAttributes(typeof(DefaultValueAttribute), true).Single();
                propinfo.SetValue(output, dvattr.Value, null);
            }

            foreach(string arg in args.Where(arg => arg.StartsWith("-"))) {
                Match optionmatch = optionregex.Match(arg);
                PropertyInfo propinfo = null;
                string value = null;
                if(!string.IsNullOrEmpty(optionmatch.Groups[1].Value)) {
                    try {
                        propinfo = options.Where(
                            prop => ((OptionAttribute)prop.GetCustomAttributes(typeof(OptionAttribute), true).Single()).Name == optionmatch.Groups[1].Value
                        ).Single();
                        value = optionmatch.Groups[2].Value;
                    } catch {
                        ShowUsage(string.Format("Unrecognized option --{0}", optionmatch.Groups[1].Value));
                    }
                } else {
                    try {
                        propinfo = options.Where(
                            prop => ((OptionAttribute)prop.GetCustomAttributes(typeof(OptionAttribute), true).Single()).ShortName == optionmatch.Groups[3].Value[0]
                        ).Single();
                        value = optionmatch.Groups[4].Value;
                    } catch {
                        ShowUsage(string.Format("Unrecognized option -{0}", optionmatch.Groups[3].Value));
                    }
                }

                if(propinfo.PropertyType == typeof(bool)) {
                    propinfo.SetValue(output, true, null);
                } else {
                    TypeConverter converter = TypeDescriptor.GetConverter(propinfo.PropertyType);
                    try {
                        propinfo.SetValue(output, converter.ConvertFromString(value), null);
                    } catch {
                        OptionAttribute optattr = (OptionAttribute)propinfo.GetCustomAttributes(typeof(OptionAttribute), true).Single();
                        ShowUsage(string.Format("Invalid type for option --{0}.", optattr.Name));
                    }
                }
            }

            PropertyInfo help = typeof(T).GetProperties().Where(
                prop => prop.GetCustomAttributes(typeof(HelpOptionAttribute), true).Length == 1
            ).FirstOrDefault();

            if(help is PropertyInfo && (bool)help.GetValue(output, null)) {
                ShowUsage();
            }

            int num_reqargs = arguments.Where(prop => ((ArgumentAttribute)prop.GetCustomAttributes(typeof(ArgumentAttribute), true).Single()).Required).Count();
            Queue<string> passedargs = new Queue<string>(args.Where(arg => !arg.StartsWith("-")));
            Queue<PropertyInfo> argqueue = new Queue<PropertyInfo>(arguments);

            if(passedargs.Count() < num_reqargs) {
                ShowUsage("Not enough arguments.");
            }

            while(passedargs.Count() > num_reqargs) {
                PropertyInfo propinfo = argqueue.Dequeue();
                string arg = passedargs.Dequeue();
                TypeConverter converter = TypeDescriptor.GetConverter(propinfo.PropertyType);
                try {
                    propinfo.SetValue(output, converter.ConvertFromString(arg), null);
                } catch {
                    ArgumentAttribute argattr = (ArgumentAttribute)propinfo.GetCustomAttributes(typeof(ArgumentAttribute), true).Single();
                    ShowUsage(string.Format("Invalid type for argument {0}.", argattr.Name));
                }
                if(((ArgumentAttribute)propinfo.GetCustomAttributes(typeof(ArgumentAttribute), true).Single()).Required) {
                    num_reqargs -= 1;
                }
            }

            foreach(PropertyInfo propinfo in argqueue.Where(
                prop => ((ArgumentAttribute)prop.GetCustomAttributes(typeof(ArgumentAttribute), true).Single()).Required
            )) {
                string arg = passedargs.Dequeue();
                TypeConverter converter = TypeDescriptor.GetConverter(propinfo.PropertyType);
                try {
                    propinfo.SetValue(output, converter.ConvertFromString(arg), null);
                } catch {
                    ArgumentAttribute argattr = (ArgumentAttribute)propinfo.GetCustomAttributes(typeof(ArgumentAttribute), true).Single();
                    ShowUsage(string.Format("Invalid type for argument {0}.", argattr.Name));
                }
            }

            return output;
        }

        public void ShowUsage(string error = null) {
            if(!string.IsNullOrEmpty(error)) {
                Console.WriteLine(string.Format("Error: {0}", error));
            }
            string usage = string.Format("Usage: {0}", Assembly.GetExecutingAssembly().GetName().Name);
            if(options.Count() > 0) {
                usage += " [--options]";
            }

            foreach(PropertyInfo prop in arguments) {
                ArgumentAttribute argattr = (ArgumentAttribute)prop.GetCustomAttributes(typeof(ArgumentAttribute), true).Single();
                if(argattr.Required) {
                    usage += string.Format(" {0}", argattr.Name);
                } else {
                    usage += string.Format(" [{0}]", argattr.Name);
                }
            }

            Console.WriteLine(usage);
            Console.WriteLine();

            object[] descattr = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), true);
            if(descattr.Length == 1) {
                Console.WriteLine(((DescriptionAttribute)descattr[0]).Description);
                Console.WriteLine();
            }

            if(arguments.Count() > 0) {
                Console.WriteLine("Arguments");
                Console.WriteLine(new string('-', Math.Min(60, Console.WindowWidth - 1)));
                foreach(PropertyInfo prop in arguments) {
                    ArgumentAttribute argattr = (ArgumentAttribute)prop.GetCustomAttributes(typeof(ArgumentAttribute), true).Single();
                    Console.WriteLine(argattr.ToString(prop));
                }
                Console.WriteLine();
            }

            if(options.Count() > 0) {
                Console.WriteLine("Options");
                Console.WriteLine(new string('-', Math.Min(60, Console.WindowWidth - 1)));
                foreach(PropertyInfo prop in options) {
                    OptionAttribute optattr = (OptionAttribute)prop.GetCustomAttributes(typeof(OptionAttribute), true).Single();
                    Console.WriteLine(optattr.ToString(prop));
                }
            }

            Environment.Exit(0);
        }

    }
}
