namespace TSB.Operations
{
    using System;
    using System.Threading;

    public class Operation
    {
        public string TicketId { get; set; }
        public object[] Parameters { get; set; }
        public Action<object[]> Callback { get; set; }
    }

    public class OperatorParameter
    {
        public string Name { get; set; }
        public OperatorType OperatorType { get; }
        public int Sleep { get; set; }
        public ThreadPriority Priority { get; set; }
        public OperatorParameter(string name, OperatorType operatorType = OperatorType.Worker, int sleep = 1, ThreadPriority priority = ThreadPriority.Normal)
        {
            Name = name;
            OperatorType = operatorType;
            Sleep = sleep;
            Priority = priority;
        }
    }

    public enum OperatorType
    {
        Worker,
        FlowController,
        Messenger
    }

    public class OperationResult : IOperationResult
    {
        public Operation Operation { get; set; }
        public object ResultData { get; set; }
        public bool HasError { get => Error != null; }
        public Exception Error { get; set; }
        public bool IsOK { get; set; }
        public string Message { get; set; }
    }

    public interface IOperationResult : IResult
    {
        Operation Operation { get; set; }
        object ResultData { get; set; }

        bool HasError { get; }
        Exception Error { get; set; }
    }

    public interface IResult
    {
        bool IsOK { get; set; }
        string Message { get; set; }
    }

    public class OperationEventArgs : EventArgs
    {
        public DateTime Time { get; }
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
