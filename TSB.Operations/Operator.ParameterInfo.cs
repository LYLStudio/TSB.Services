namespace TSB.Operations
{
    using System.Threading;

    public partial class Operator
    {
        public class ParameterInfo
        {
            public string Name { get; set; }
            public int Sleep { get; set; }
            public ThreadPriority Priority { get; set; }
            public ParameterInfo(string name, int sleep = 1, ThreadPriority priority = ThreadPriority.Normal)
            {
                Name = name;
                Sleep = sleep;
                Priority = priority;
            }
        }
    }
}
