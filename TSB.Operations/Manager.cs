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

        public Manager(Parameter parameter)
        {
            Inbox = new ConcurrentDictionary<string, object>();
            Timeout = parameter.Timeout;

            _messenger = new Operator(parameter.MessengerParam);
#if DEBUG
            _messenger.OperationTriggered += OnMessengerOperationTriggered;
#endif
            _flowController = new Operator(parameter.FlowControllerParam);
            _flowController.OperationTriggered += OnFlowControllerOperationTriggered;

            _operators = new Dictionary<string, Operator>();
            parameter.OperatorParams.ForEach(o =>
            {
                var op = new Operator(o);
                op.OperationTriggered += OnWorkerOperationTriggered;
                _operators.Add(op.Parameter.Name, op);
            });
        }

        protected virtual void DoWork(Operator.Operation operation)
        {
            _flowController.Enqueue(new Operator.Operation()
            {
                Parameters = new object[] { operation },
                Callback = (objs) =>
                {
                    index = (++index) % _operators.Count;
                    var worker = _operators.Values.ToList()[index];
                    worker.Enqueue((Operator.Operation)objs[0]);
                    return $"[{worker.Parameter.Name}] start working...";
                }
            });
        }

        protected virtual void DoWork(Operator.Operation operation, string workerName)
        {
            if (_operators.TryGetValue(workerName, out Operator worker))
            {
                worker.Enqueue(operation);
            }
            else
            {
                //TODO:
            }
        }

        protected virtual void OnWorkerOperationTriggered(object sender, OperationEventArgs e)
        {
            _flowController.Enqueue(new Operator.Operation()
            {
                Parameters = new object[] { e },
                Callback = objs =>
                {
                    _messenger.Enqueue(new Operator.Operation()
                    {
                        Parameters = new object[] { objs[0] },
                        Callback = SendMessage
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
        protected virtual void OnMessengerOperationTriggered(object sender, OperationEventArgs e)
        {

        }
#endif

        protected virtual void OnFlowControllerOperationTriggered(object sender, OperationEventArgs e)
        {
            _messenger.Enqueue(new Operator.Operation()
            {
                Parameters = new object[] { e },
                Callback = SendMessage
            });
        }

        protected virtual object SendMessage(object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
