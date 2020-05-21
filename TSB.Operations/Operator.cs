namespace TSB.Operations
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class Operator : IDisposable
    {
        private readonly AutoResetEvent _resetEvent;
        private readonly Thread _thread;
        private bool _disposedValue;

        private ConcurrentQueue<Operation> OperationQueue { get; set; }

        public string Id { get; private set; }
        public int Sleep { get; private set; }

        public event EventHandler<OperationEventArgs> OperationTriggered;

        public Operator(string id = "", int sleep = 100)
        {
            Id = id;
            Sleep = sleep;
            OperationQueue = new ConcurrentQueue<Operation>();
            _resetEvent = new AutoResetEvent(false);
            _thread = new Thread(Start)
            {
                Name = id,
                Priority = ThreadPriority.Normal,
                IsBackground = true
            };

            _thread.Start();
        }

        public void Enqueue(Operation operation)
        {
            if (!(operation is Operation) || operation.Callback is null)
            {
                var result = new OperationResult()
                {
                    Operation = operation
                };
                OnOperationTriggered(result, new ArgumentException($"{nameof(operation)} is not valid or callback is null"));
            }

            try
            {
                OperationQueue?.Enqueue(operation);
                _resetEvent.Set();
            }
            catch (Exception ex)
            {
                OnOperationTriggered(new OperationResult() { Operation = operation }, ex);
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
                            var result = operation.Callback?.Invoke(operation.Parameters);
                            OnOperationTriggered(new OperationResult()
                            {
                                Operation = operation,
                                Result = result
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        OnOperationTriggered(new OperationResult() { Operation = operation }, ex);
                    }
                }
                else
                {
                    _resetEvent.WaitOne();
                }

                _resetEvent.WaitOne(Sleep);
            }
        }

        protected virtual void OnOperationTriggered(OperationResult payload, Exception error = null)
        {
            OperationTriggered?.Invoke(this, new OperationEventArgs() { Error = error, Payload = payload });
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
