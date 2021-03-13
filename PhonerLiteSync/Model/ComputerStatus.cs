using System;
using System.Globalization;

namespace PhonerLiteSync.Model
{
    public class ComputerStatus
    {
        public ComputerStatus(){}

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
               LastChange.ToString(MainManager.DateTimeFormat) +
               Status switch
               {
                   Status.NewEntry => "+",
                   Status.Edited => "*",
                   Status.Removed => "-",
                   _ => "",
               };
    }
}