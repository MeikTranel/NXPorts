namespace NXPorts
{
    public class SourcePosition {
        public string FilePath { get; }
        public int? Line { get; }
        public int? Column { get; }

        public SourcePosition(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new System.ArgumentException("message", nameof(filePath));

            this.FilePath = filePath;
        }

        public SourcePosition(string filePath, int line, int column) : this(filePath)
        {
            Line = line;
            Column = column;
        }
    }
}