using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ppcg.text {
    public static partial class StringExtensions {

        public static IEnumerable<string> WordWrap(this string source, int maxlen) {
            List<string> chunks = new List<string>();
            while(source.Length > maxlen) {
                int spi = source.Substring(0, maxlen).LastIndexOf(' ');
                if(spi == -1) {
                    spi = maxlen;
                }
                chunks.Add(source.Substring(0, spi).Trim());
                source = source.Substring(spi);
            }
            chunks.Add(source.Trim());
            return chunks;
        }

    }
}
