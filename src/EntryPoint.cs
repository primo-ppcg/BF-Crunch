﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using ppcg.argparse;

namespace ppcg {
    public class EntryPoint {

        public static void Main(string[] args) {

            ArgumentParser<MyOptions> argparser = new ArgumentParser<MyOptions>();
            MyOptions options = argparser.Parse(args);

            Cruncher cruncher = new Cruncher(options);

            for(int i = options.MinInit; i != options.MaxInit + 1; i++) {
                cruncher.Crunch(i);
            }

        }
    }
}
