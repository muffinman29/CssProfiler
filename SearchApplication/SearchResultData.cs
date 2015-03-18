using System;
using System.Collections;
using SearchApplication.Annotations;

namespace SearchApplication
{
    class SearchResultData : IEnumerable
    {
        public String FileNameAndPath { get; set; }
        public String LineNumber { [UsedImplicitly] get; set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) this).GetEnumerator();
        }
    }
}
