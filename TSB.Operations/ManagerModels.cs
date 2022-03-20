namespace TSB.Operations
{
    using System.Collections.Generic;

    public class ManagerParameter
    {
        public OperatorParameter FlowControllerParam { get; set; }
        public OperatorParameter MessengerParam { get; set; }
        public List<OperatorParameter> OperatorParams { get; set; }
        public int Timeout { get; set; }
    }
}
