using System;

namespace Matchmaking.Models
{
    public class DatedValue<T>
    {
        public T Value { get; set; }
        public DateTime Date { get; set; }

        public DatedValue(T value)
        {
            Value = value;
            Date = DateTime.Now;
        }
    }
}
