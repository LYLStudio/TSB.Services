namespace TSB.Services.F2DService.Operations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using TSB.Operations;
    using TSB.Services.F2DService.Models;

    public class DbImportOperatorManager : Manager
    {
        public DbImportOperatorManager(Parameter parameter) : base(parameter)
        {
        }

        public async Task<object> SetDataAsync(string ticketId, string sql, params object[] sqlParameters)
        {
            object result = null;
            var cts = new CancellationTokenSource();
            try
            {
                DoWork(new Operator.Operation()
                {
                    TicketId = ticketId,
                    Parameters = new object[] { sql, sqlParameters },
                    Callback = objs =>
                    {
                        using (var db = new RateInfoEntities())
                        {
                            var cmd = objs[0].ToString();
                            var sqlParams = (object[])objs[1];
                            return db.Database.ExecuteSqlCommand(cmd, sqlParams);
                        }
                    }
                });

                cts.CancelAfter(5000);

                while (true)
                {
                    if (Inbox.TryRemove(ticketId, out result) || cts.IsCancellationRequested)
                    {
                        break;
                    }
                    await Task.Delay(Sleep, cts.Token);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (TaskCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
            }
            finally
            {
            }

            return result;
        }

        protected override object SendMessage(object[] parameters)
        {
            var raw = JsonConvert.SerializeObject(parameters);
            Debug.WriteLine($"[{DateTime.Now:O}]{raw}");
            return raw;
        }

#if DEBUG
        protected override void OnMessengerOperationTriggered(object sender, OperationEventArgs e)
        {
            if (e.HasError)
            {
                Debug.WriteLine(e.Error);
            }
        }
#endif

    }
}
