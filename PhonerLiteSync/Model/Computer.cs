namespace PhonerLiteSync.Model
{
    public class Computer
    {
        public Computer()
        {
        }

        public Computer(string name)
        {
            if (name.Length <= 1)
            {
                return;
            }

            Id = int.Parse(name[0].ToString());
            Name = name.Substring(1);
        }

        public int Id { get; set; }
       
        public string Name { get; set; }

        public override string ToString()
            => Id + Name;
    }
}
