namespace TSB.Operations
{
    using System.Collections.Generic;

    public partial class Manager
    {
        public class Parameter
        {
            public Operator.ParameterInfo FlowControllerParam { get; set; }
            public Operator.ParameterInfo MessengerParam { get; set; }
            public List<Operator.ParameterInfo> OperatorParams { get; set; }
            public int Timeout { get; set; }
        }
    }
}
