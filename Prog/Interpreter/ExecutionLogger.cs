using System;

namespace Prog
{
    public sealed class ExecutionLogger
    {
        private int _indentationLevel = 0;
        public bool EnableLog { get; set; }

        public void Indent() => _indentationLevel += 1;
        public void Unindent() => _indentationLevel -= 1;

        public void Log(string message)
        {
            if (!EnableLog) return;
            for (int i = 0; i < _indentationLevel; i++)
                Console.Write("  ");
            Console.WriteLine(message);
        }
    }
}
