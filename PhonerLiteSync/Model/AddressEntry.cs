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
    }
}