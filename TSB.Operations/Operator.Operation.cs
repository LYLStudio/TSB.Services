namespace TSB.Operations
{
    using System;

    public partial class Operator
    {
        public class Operation
        {
            public string TicketId { get; set; }
            public object[] Parameters { get; set; }
            public Func<object[], object> Callback { get; set; }
        }
    }
}
