namespace TSB.Operations
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public partial class Operator : IDisposable
    {
        private readonly AutoResetEvent _resetEvent;
        private readonly Thread _thread;
        private bool _disposedValue;

        private ConcurrentQueue<Operation> OperationQueue { get; set; }

        public OperatorParameter Parameter { get; private set; }

        public event EventHandler<OperationEventArgs> OperationOccurred;

        public Operator(OperatorParameter parameter)
        {
            Parameter = new OperatorParameter(parameter.Name, parameter.OperatorType, parameter.Sleep, parameter.Priority);
            OperationQueue = new ConcurrentQueue<Operation>();
            _resetEvent = new AutoResetEvent(false);
            _thread = new Thread(Start)
            {
                Name = Parameter.Name,
                Priority = Parameter.Priority,
                IsBackground = true
            };

            _thread.Start();
        }

        public void Enqueue(Operation operation)
        {
            if (!(operation is Operation) || operation.Callback is null)
            {
                var msg = $"{nameof(operation)} is not valid or callback is null";
                var result = new OperationResult()
                {
                    Error = new ArgumentException(msg),
                    Operation = operation,
                    Message = msg,
                };
                OnOperationOccurred(result);

                return;
            }

            try
            {
                OperationQueue?.Enqueue(operation);
                _resetEvent.Set();
            }
            catch (Exception ex)
            {
                OnOperationOccurred(new OperationResult() { Operation = operation, Error = ex, Message = ex.Message });
            }
        }

        private void Start()
        {
            while (true && !_disposedValue)
            {
                if (!OperationQueue.IsEmpty)
                {
                    Operation operation = null;
                    try
                    {
                        if (OperationQueue.TryDequeue(out operation))
                        {
                            operation.Callback?.Invoke(operation.Parameters);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnOperationOccurred(new OperationResult() { Operation = operation, Error = ex, Message = ex.Message });
                    }
                }
                else
                {
                    _resetEvent.WaitOne();
                }

                _resetEvent.WaitOne(Parameter.Sleep);
            }
        }

        protected virtual void OnOperationOccurred(OperationResult payload)
        {
            OperationOccurred?.Invoke(this, new OperationEventArgs() { Payload = payload });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                _disposedValue = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~Operator()
        // {
        //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
