namespace TSB.Operations
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public partial class Manager
    {
        private readonly Operator _flowController = null;
        private readonly Operator _messenger = null;
        private readonly Dictionary<string, Operator> _operators = null;
        private int index = 0;

        protected ConcurrentDictionary<string, object> Inbox { get; private set; }

        public int Sleep { get => _flowController.Parameter.Sleep; }
        public int Timeout { get; private set; }

        public event EventHandler<OperationEventArgs> OperationOccurred;

        public Manager(ManagerParameter parameter)
        {
            Inbox = new ConcurrentDictionary<string, object>();
            Timeout = parameter.Timeout;

            _messenger = new Operator(parameter.MessengerParam);
            _messenger.OperationOccurred += OnMessengerOperationOccurred; ;

            _flowController = new Operator(parameter.FlowControllerParam);
            _flowController.OperationOccurred += OnOperationOccurred;

            _operators = new Dictionary<string, Operator>();
            parameter.OperatorParams.ForEach(o =>
            {
                var op = new Operator(o);
                op.OperationOccurred += OnOperationOccurred;
                _operators.Add(op.Parameter.Name, op);
            });
        }

        private void OnMessengerOperationOccurred(object sender, OperationEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected virtual void DoWork(Operation operation)
        {
            _flowController.Enqueue(new Operation()
            {
                Parameters = new object[] { operation },
                Callback = (objs) =>
                {
                    index = (++index) % _operators.Count;
                    var worker = _operators.Values.ToList()[index];
                    worker.Enqueue((Operation)objs[0]);
                }
            });
        }

        protected virtual void DoWork(Operation operation, string workerName)
        {
            _flowController.Enqueue(new Operation()
            {
                Parameters = new object[] { operation, workerName },
                Callback = (objs) =>
                {
                    if (_operators.TryGetValue(objs[1].ToString(), out Operator worker))
                    {
                        worker.Enqueue((Operation)objs[0]);
                    }
                    else
                    {
                        throw new ArgumentNullException(nameof(workerName));
                    }
                }
            });
        }

        protected virtual void OnOperationOccurred(object sender, OperationEventArgs e)
        {
            _messenger.Enqueue(new Operation()
            {
                TicketId = e.Payload.Operation.TicketId,
                Parameters = new object[] { e },
                Callback = (m) =>
                {

                }
            });

            if (sender is Operator op)
            {
                _flowController.Enqueue(new Operation()
                {
                    TicketId = e.Payload.Operation.TicketId,
                    Parameters = new object[] { e.Payload },
                    Callback = objs =>
                    {
                        var result = (OperationResult)objs[0];
                        Inbox.TryAdd(result.Operation.TicketId, result);
                    }
                });

            }
        }
    }
}
