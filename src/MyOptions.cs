using System;
using System.ComponentModel;

using ppcg.argparse;

namespace ppcg {
    [Description("Crunches BF programs to produce a given text.")]
    public class MyOptions {

        [Argument(
            Name = "text",
            Required = true,
            HelpText = "The text to produce."
        )]
        public string Text { get; set; }

        [Argument(
            Name = "limit",
            HelpText = "The maximum BF program length to search for. If zero, the length of the shortest program found so far will be used (-r)."
        )]
        [DefaultValue(0)]
        public int Limit { get; set; }

        [Option(
            Name = "max-init",
            ShortName = 'i',
            HelpText = "The maximum length of the initialization segment. If excluded, the program will run indefinitely."
        )]
        public int MaxInit { get; set; }

        [Option(
            Name = "min-init",
            ShortName = 'I',
            HelpText = "The minimum length of the initialization segment."
        )]
        [DefaultValue(14)]
        public int MinInit { get; set; }

        [Option(
            Name = "max-tape",
            ShortName = 't',
            HelpText = "The maximum tape size to consider. Programs that utilize more tape than this will be ignored."
        )]
        [DefaultValue(1250)]
        public int MaxTape { get; set; }

        [Option(
            Name = "min-tape",
            ShortName = 'T',
            HelpText = "The minimum tape size to consider. Programs that utilize less tape than this will be ignored."
        )]
        [DefaultValue(1)]
        public int MinTape { get; set; }

        [Option(
            Name = "rolling-limit",
            ShortName = 'r',
            HelpText = "If set, the limit will be adjusted whenever a shorter program is found."
        )]
        public bool RollingLimit { get; set; }

        [HelpOption(
            Name = "help",
            ShortName ='?',
            HelpText = "Display this help text."
        )]
        public bool Help { get; set; }

    }
}
