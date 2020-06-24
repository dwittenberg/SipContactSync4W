using System;
using System.Globalization;

namespace PhonerLiteSync.Model
{
    public class ComputerStatus
    {
        public ComputerStatus(string m)
        {
            Id = int.Parse(m[0].ToString());

            LastChange = DateTime.ParseExact(
                m.Substring(1, CsvHandler.DateTimeFormat.Length),
                CsvHandler.DateTimeFormat,
                CultureInfo.InvariantCulture);

            if (m.Length == CsvHandler.DateTimeFormat.Length + 1)
            {
                Status = Status.UpToDate;
                return;
            }

            Status = m[CsvHandler.DateTimeFormat.Length + 1] switch
            {
                '+' => Status.NewEntry,
                '*' => Status.Edited,
                '-' => Status.Removed,
                _ => Status.Undefined,
            };
        }

        public ComputerStatus(int id, DateTime lastChange, Status status)
        {
            Id = id;
            LastChange = lastChange;
            Status = status;
        }

        public int Id { get; set; }
        public Status Status { get; set; }
        public DateTime LastChange { get; set; }

        public override string ToString()
            => Id +
               LastChange.ToString(CsvHandler.DateTimeFormat) +
               Status switch
               {
                   Status.NewEntry => "+",
                   Status.Edited => "*",
                   Status.Removed => "-",
                   _ => "",
               };
    }
}