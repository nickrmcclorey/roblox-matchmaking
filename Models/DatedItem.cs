using System;

namespace Matchmaking.Models
{
    public class DatedItem<T>
    {
        public T Value { get; set; }
        public DateTime Date { get; set; }

        public DatedItem(T value)
        {
            Value = value;
            Date = DateTime.Now;
        }
    }
}
