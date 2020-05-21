namespace TSB.Operations
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Manager
    {
        private readonly Operator _flowControlOperator = null;
        private readonly Operator _loggerOperator = null;
        private readonly Operator[] _operators = null;
        private int index = 0;

        protected ConcurrentDictionary<string, object> Inbox { get; private set; }

        public int Sleep { get => _flowControlOperator.Sleep; }

        public Manager(int operatorCount = 2)
        {
            Inbox = new ConcurrentDictionary<string, object>();

            _loggerOperator = new Operator();
#if DEBUG
            _loggerOperator.OperationTriggered += OnLoggerOperatorOperationTriggered;
#endif
            _flowControlOperator = new Operator();
            _flowControlOperator.OperationTriggered += OnFlowControlOperatorOperationTriggered;

            _operators = new Operator[operatorCount];
            for (int i = 0; i < _operators.Length; i++)
            {
                _operators[i] = new Operator($"Worker{i:00}");
                _operators[i].OperationTriggered += OnWorkingOperationTriggered;
            }
        }

        protected void DoWork(Operator.Operation operation)
        {
            _flowControlOperator.Enqueue(new Operator.Operation()
            {
                Parameters = new object[] { operation },
                Callback = (objs) =>
                {
                    index = (++index) % _operators.Length;
                    _operators[index].Enqueue((Operator.Operation)objs[0]);
                    return $"_operator[{index}]({_operators[index].Id}) start working...";
                }
            });
        }

        private void OnWorkingOperationTriggered(object sender, OperationEventArgs e)
        {
            _flowControlOperator.Enqueue(new Operator.Operation()
            {
                Parameters = new object[] { e },
                Callback = objs =>
                {
                    _loggerOperator.Enqueue(new Operator.Operation()
                    {
                        Parameters = new object[] { objs[0] },
                        Callback = Log
                    });

                    object result = null;
                    var args = (OperationEventArgs)objs[0];
                    if (!Inbox.ContainsKey(args.Payload.Operation.TicketId))
                    {
                        if (Inbox.TryAdd(args.Payload.Operation.TicketId, args.Payload.Result))
                            result = args.Payload.Result;
                    }

                    return result;
                }
            });
        }

#if DEBUG
        protected virtual void OnLoggerOperatorOperationTriggered(object sender, OperationEventArgs e)
        {

        }
#endif

        private void OnFlowControlOperatorOperationTriggered(object sender, OperationEventArgs e)
        {
            _loggerOperator.Enqueue(new Operator.Operation()
            {
                Parameters = new object[] { e },
                Callback = Log
            });
        }

        protected virtual object Log(object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
