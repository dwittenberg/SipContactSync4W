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

        public AddressEntry(string[] fields, PhonebookStruct exFile)
        {
            if (fields.Length != 4 + exFile.Devices.Length)
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
                MyStatus = AllComputers[exFile.MyPosition];

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
                return "";
            }


            if (AllComputers.Count(m => m.LastChange < ToDoLastChange) == 0 && AllComputers.Count(m => m.Status != Status.UpToDate) != 0)
            {
                AllComputers.Where(m => m.LastChange == ToDoLastChange).ToList().ForEach(m => m.Status = Status.UpToDate);
            }

            var endString = $"{Number};{Name};;{Comment};";
            for (int i = 0; i < AllComputers.Length; i++)
            {
                endString += $"{AllComputers[i]};";
            }

            return endString.Substring(0, endString.Length - 1);
        }
    }
}