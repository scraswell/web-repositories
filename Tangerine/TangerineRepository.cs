using System;
using System.Collections.Generic;

using Craswell.Automation.DataAccess;
using NHibernate;

namespace Craswell.WebRepositories.Tangerine
{
    /// <summary>
    /// Tangerine repository.
    /// </summary>
    public class TangerineRepository : IConnectedWebRepository
    {
        /// <summary>
        /// The type.
        /// </summary>
        private const WebRepositoryType type = WebRepositoryType.Tangerine;

        /// <summary>
        /// The data access layer.
        /// </summary>
        private DataAccessLayer dal = new DataAccessLayer();

        /// <summary>
        /// The tangerine client.
        /// </summary>
        private TangerineClient tangerineClient;

        /// <summary>
        /// The repository identifier.
        /// </summary>
        private int id;

        /// <summary>
        /// The repository name.
        /// </summary>
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="Craswell.WebRepositories.Tangerine.TangerineRepository"/> class.
        /// </summary>
        /// <param name="name">The repository name.</param>
        /// <param name="tangerineClient>The tangerine client.</param>
        public TangerineRepository(
            int id,
            string name,
            TangerineClient tangerineClient)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (tangerineClient == null)
            {
                throw new ArgumentNullException("tangerineClient");
            }

            this.id = id;
            this.name = name;
            this.tangerineClient = tangerineClient;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Craswell.WebRepositories.Tangerine.TangerineRepository"/> is reclaimed by garbage collection.
        /// </summary>
        ~TangerineRepository()
        {
            this.Dispose(false);
        }

        #region IWebRepository implementation
        /// <summary>
        /// Gets the repository identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id
        {
            get
            {
                return this.id;
            }
        }

        /// <summary>
        /// Gets the name of the repository.
        /// </summary>
        /// <value>The name of the repository.</value>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the web repository type.
        /// </summary>
        /// <value>The web repository type.</value>
        public WebRepositoryType Type
        {
            get
            {
                return type;
            }
        }
        #endregion

        #region IConnectedWebRepository implementation
        /// <summary>
        /// Gets the accounts.
        /// </summary>
        /// <returns>The accounts.</returns>
        public IList<IAccount> GetAccounts()
        {
            return this.tangerineClient.GetAccounts();
        }

        /// <summary>
        /// Gets the statement.
        /// </summary>
        public void GetStatement(IAccount account, int year, int month)
        {
            this.tangerineClient.GetStatement(account, year, month);
        }

        /// <summary>
        /// Gets the statement.
        /// </summary>
        public void GetAllStatements()
        {
            this.tangerineClient.GetAllStatements();
        }
        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="Craswell.WebRepositories.Tangerine.TangerineRepository"/>. The <see cref="Dispose"/> method
        /// leaves the <see cref="Craswell.WebRepositories.Tangerine.TangerineRepository"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="Craswell.WebRepositories.Tangerine.TangerineRepository"/> so the garbage collector can reclaim
        /// the memory that the <see cref="Craswell.WebRepositories.Tangerine.TangerineRepository"/> was occupying.</remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.tangerineClient != null)
                {
                    this.tangerineClient.Dispose();
                    this.tangerineClient = null;
                }
            }
        }
    }
}

