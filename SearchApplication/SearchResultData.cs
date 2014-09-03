using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchApplication
{
    class SearchResultData : IEnumerable
    {
        public String FileNameAndPath { get; set; }
        public String LineNumber { get; set; }

        public IEnumerator GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
