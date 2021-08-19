using System;

namespace NXPorts
{
    public class SourcePosition
    {
        public string FilePath { get; }
        public int? Line { get; }
        public int? Column { get; }

        public SourcePosition(string filePath)
        {
            this.FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public SourcePosition(string filePath, int line, int column) : this(filePath)
        {
            Line = line;
            Column = column;
        }
    }
}
