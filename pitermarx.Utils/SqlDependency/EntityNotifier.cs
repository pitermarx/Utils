using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace pitermarx.SqlDependency
{
    public interface INotifier<TEntity> : IDisposable
    {
        IDisposable OnChange(Action<IQueryable<TEntity>> args);
    }

    /// <summary>
    /// Based on the implementation of https://github.com/rickbassham/ef-change-notify
    /// Uses an sql dependency to monitor changes on an IQueriable
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type
    /// </typeparam>
    /// <typeparam name="TContext">
    /// The type of the context
    /// </typeparam>
    internal class EntityNotifier<TEntity, TContext> : INotifier<TEntity>
        where TContext : DbContext
        where TEntity : class
    {
        /// <summary> A way to get the query that will be monitored from the DbContext </summary>
        private readonly Func<TContext, IQueryable<TEntity>> query;

        /// <summary> The connection string </summary>
        private readonly string connectionString;

        /// <summary> The monitored query in plain SQL </summary>
        private readonly string commandText;

        /// <summary> The event that will be called when the monitored query changes </summary>
        private Action<IQueryable<TEntity>> changed;

        /// <summary> A way to instanciate a new DbContext </summary>
        private Func<TContext> contextMaker;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotifier{TEntity,TContext}"/> class.
        /// </summary>
        internal EntityNotifier(Func<TContext> ctxMaker, Func<TContext, IQueryable<TEntity>> queriable)
        {
            // save funcs
            contextMaker = ctxMaker;
            query = queriable;

            // on a new context
            using (var ctx = ctxMaker())
            {
                // save the connection string
                connectionString = ctx.Database.GetConnectionString();

                // Save the command text
                commandText = (query(ctx) as DbSet<TEntity>).ToString();
            }

            // Start an SQL dependency for the connection string
            SqlDependenciesManager.AddConnectionString(connectionString);

            // start the monitoring
            RegisterNotification();
        }

        private void RegisterNotification()
        {
            // test if it was disposed
            if (contextMaker == null)
                return;

            // create a new command
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand { CommandText = commandText, Connection = connection };
            connection.Open();

            new Microsoft.Data.SqlClient.SqlDependency(command).OnChange += (s, e) =>
                {
                        // test if it was disposed
                        if (contextMaker == null)
                    {
                        return;
                    }

                        // continue monitoring
                        RegisterNotification();

                        // call the changed event on a new thread
                        if (e.Type == SqlNotificationType.Change && changed != null)
                    {
                        Task.Run(() =>
                            {
                                using var ctx = contextMaker();
                                changed(query(ctx));
                            });
                    }
                };

            // NOTE: You have to execute the command, or the notification will never fire.
            using (command.ExecuteReader()) { }
        }

        public IDisposable OnChange(Action<IQueryable<TEntity>> handler)
        {
            changed = handler;
            return this;
        }

        public void Dispose()
        {
            if (contextMaker == null)
            {
                return;
            }

            contextMaker = null;

            SqlDependenciesManager.RemoveConnectionString(connectionString);
        }
    }
}