namespace TSB.Services.F2DService.Operations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using TSB.Operations;
    using TSB.Services.F2DService.Models;

    public class DbImportOperatorManager : Manager
    {
        public DbImportOperatorManager(ManagerParameter parameter) : base(parameter)
        {
        }

        public async Task<object> SetDataAsync(string ticketId, string sql, params object[] sqlParameters)
        {
            object result = null;
            var cts = new CancellationTokenSource();
            try
            {
                var op = new Operation()
                {
                    TicketId = ticketId,
                    Parameters = new object[] { ticketId, cts.Token, sql, sqlParameters }
                };

                op.Callback = objs =>
                {
                    var ticket = objs[0].ToString();
                    var token = (CancellationToken)objs[1];

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    object dbResult = null; ;
                    using (var db = new RateInfoEntities())
                    {
                        var cmd = objs[2].ToString();
                        var sqlParams = (object[])objs[3];
                        dbResult = $"{DateTime.Now:O}";// db.Database.ExecuteSqlCommand(cmd, sqlParams);
                    }

                    var opResult = new OperationResult()
                    {
                        IsOK = true,
                        Operation = op,
                        ResultData = dbResult
                    };

                    Inbox.TryAdd(ticket, opResult);
                };

                DoWork(op);

                cts.CancelAfter(Timeout);

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


    }
}
