using System;
using System.Linq;

namespace PhonerLiteSync.Model
{
    public class AddressEntry
    {
        public AddressEntry()
        { }

        public AddressEntry(string[] fields)
        {
            if (fields.Length != 4)
            {
                Console.WriteLine("Length not ok");
                return;
            }

            try
            {
                Number = fields[0];
                Name = fields[1];
                Comment = fields[3];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public AddressEntry(string[] fields, PhoneBook exFile)
        {
            if (fields.Length != 4 + exFile.Devices.Length && fields.Length != 4 + exFile.Devices.Length - 1)
            {
                Console.WriteLine("Length not ok");
                return;
            }

            try
            {
                Number = fields[0];
                Name = fields[1];
                Comment = fields[3];

                AllComputers = fields[Range.StartAt(4)].Select(m => new ComputerStatus(m)).ToArray();
                if (AllComputers.Length == exFile.MyId)
                {
                    AllComputers = AllComputers.Append(new ComputerStatus(exFile.MyId, DateTime.MinValue, Status.NewEntry)).ToArray();
                }

                MyStatus = AllComputers[exFile.MyId];

                var changerList = AllComputers.Where(m => m.Status != Status.UpToDate).ToList();

                if (changerList.Count > 0)
                {
                    LastChanger = changerList.OrderBy(m => m.LastChange).First();
                }

                Console.WriteLine(this.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public string Name { get; set; }
        public string Number { get; set; }
        public string Comment { get; set; }

        public ComputerStatus[] AllComputers { get; set; }

        public ComputerStatus LastChanger { get; set; }
        public ComputerStatus MyStatus { get; set; }

        public sealed override string ToString()
            => $"{Name} - {Number} - {Comment} - {LastChanger} - {MyStatus}";

        public bool Equal(AddressEntry b)
            => Number == b.Number && Name == b.Name && Comment == b.Comment;

        public string ToLocalString()
            => $"{Number};{Name};;{Comment}";

        public string ToExternString(int myPosition, string dateTimeFormatter)
        {
            if (AllComputers.Count(m => m.Status == Status.Removed) == AllComputers.Length)
            {
                return string.Empty;
            }

            // All PC are newer than last Change && there is a change
            if (AllComputers.Count(m => LastChanger == null || m.LastChange < LastChanger.LastChange) == 0 && AllComputers.Count(m => m.Status != Status.UpToDate) != 0)
            {
                // Set Changer UpToDate
                AllComputers.Where(m => m.LastChange == LastChanger.LastChange).ToList().ForEach(m => m.Status = Status.UpToDate);
            }

            var endString = AllComputers.Aggregate($"{Number};{Name};;{Comment};", (current, t) => current + $"{t};");
            return endString.Substring(0, endString.Length - 1);
        }
    }
}