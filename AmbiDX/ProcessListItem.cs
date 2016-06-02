using System.Diagnostics;

namespace AmbiDX
{
    public class ProcessListItem
    {
        public string Name { get; }
        public int Id { get; }

        public ProcessListItem(string name, int id)
        {
            Name = name;
            Id = id;
        }

        public ProcessListItem(Process process)
        {
            Name = process.ProcessName;
            Id = process.Id;
        }

        public override string ToString()
        {
            return Name + " - " + Id;
        }
    }
}