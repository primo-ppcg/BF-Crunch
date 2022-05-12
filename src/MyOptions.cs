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
            HelpText = "The maximum BF program length to search for. If empty, the length of the shortest program found so far will be used (-r)."
        )]
        [DefaultValue(null)]
        public int? Limit { get; set; }

        [Option(
            Name = "max-init",
            ShortName = 'i',
            HelpText = "The maximum length of the initialization segment. If empty, the program will run indefinitely."
        )]
        [DefaultValue(null)]
        public int? MaxInit { get; set; }

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
            Name = "max-node-cost",
            ShortName = 'n',
            HelpText = "The maximum cost for any node."
        )]
        [DefaultValue(20)]
        public int MaxNodeCost { get; set; }

        [Option(
            Name = "max-loops",
            ShortName = 'l',
            HelpText = "The maximum number of iterations of the main loop."
        )]
        [DefaultValue(30000)]
        public int MaxLoops { get; set; }

        [Option(
            Name = "max-slen",
            ShortName = 's',
            HelpText = "The maximum length of the s-segment."
        )]
        [DefaultValue(null)]
        public int? MaxSLen { get; set; }

        [Option(
            Name = "min-slen",
            ShortName = 'S',
            HelpText = "The minimum length of the s-segment."
        )]
        [DefaultValue(1)]
        public int MinSLen { get; set; }

        [Option(
            Name = "max-clen",
            ShortName = 'c',
            HelpText = "The maximum length of the c-segment."
        )]
        [DefaultValue(null)]
        public int? MaxCLen { get; set; }

        [Option(
            Name = "min-clen",
            ShortName = 'C',
            HelpText = "The minimum length of the c-segment."
        )]
        [DefaultValue(1)]
        public int MinCLen { get; set; }

        [Option(
            Name = "rolling-limit",
            ShortName = 'r',
            HelpText = "If set, the limit will be adjusted whenever a shorter program is found."
        )]
        public bool RollingLimit { get; set; }

        [Option(
            Name = "unique-cells",
            ShortName = 'u',
            HelpText = "If set, each used cell used for output will be unique."
        )]
        public bool UniqueCells { get; set; }

        [HelpOption(
            Name = "help",
            ShortName ='?',
            HelpText = "Display this help text."
        )]
        public bool Help { get; set; }

    }
}
