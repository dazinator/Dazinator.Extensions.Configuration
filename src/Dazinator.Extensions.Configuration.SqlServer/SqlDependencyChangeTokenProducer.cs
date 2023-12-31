namespace Dazinator.Extensions.Configuration.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;

public class SqlDependencyChangeTokenProducer : IChangeTokenProducer, IDisposable
{

    private readonly SqlDependency _sqlDependency;
    private readonly bool _shouldDispose;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly SqlConnection _connection;
    private readonly SqlCommand _command;


    public SqlDependencyChangeTokenProducer(SqlConnection connection, string commandText, bool shouldDispose = true) : this(connection, null, commandText, shouldDispose)
    {
    }

    public SqlDependencyChangeTokenProducer(SqlConnection connection, SqlCommand? command, string commandText = "", bool shouldDispose = true)
    {
        _connection = connection;
        // SqlDependency.Start(connection.ConnectionString);
        _command = command ?? connection.CreateCommand();
        if (string.IsNullOrWhiteSpace(_command.CommandText))
        {
            _command.CommandText = commandText;
        }

        _sqlDependency = new SqlDependency(_command);
        _sqlDependency.OnChange += DependencyOnChange;
        _shouldDispose = shouldDispose;
    }


    public IChangeToken Produce()
    {
        using var previousTokenSource = Interlocked.Exchange(ref _cancellationTokenSource, new CancellationTokenSource());
        return new CancellationChangeToken(_cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _sqlDependency.OnChange -= DependencyOnChange;
        _cancellationTokenSource?.Dispose();
        ////  SqlDependency.Stop(_connection.ConnectionString);
        if (_shouldDispose)
        {
            _command.Dispose();
            _connection.Dispose();
        }
    }

    private void DependencyOnChange(object sender, SqlNotificationEventArgs e)
    {
        if (e.Type == SqlNotificationType.Change)
        {
            // signal current token.
            _cancellationTokenSource.Cancel();
        }
    }

}
