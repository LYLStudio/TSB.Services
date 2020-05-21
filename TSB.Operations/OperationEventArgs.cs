using System;

namespace TSB.Operations
{
    public class OperationEventArgs : EventArgs
    {
        public DateTime Time { get; }
        public Exception Error { get; set; }
        public bool HasError { get => Error != null; }
        public OperationResult Payload { get; set; }

        public OperationEventArgs()
        {
            Time = DateTime.Now;
        }

        public OperationEventArgs(OperationResult payload) : this()
        {
            Payload = payload;
        }
    }
}
