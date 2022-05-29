using System;
using System.Collections.Generic;

namespace Minibank.Data.Domains.Currencies
{
    public class CourseResponseModel
    {
        public DateTime Date { get; set; }
        public Dictionary<string, ValueItem> Valute { get; set; }
    }

    public class ValueItem
    {
        public double Value { get; set; }
    }
}
