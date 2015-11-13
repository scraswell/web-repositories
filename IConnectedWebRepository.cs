using System;
using System.Collections.Generic;

using Craswell.Automation.DataAccess;

namespace Craswell.WebRepositories
{
    /// <summary>
    /// Defines the web repository actions interface.
    /// </summary>
    public interface IConnectedWebRepository : IWebRepository, IDisposable
    {
        /// <summary>
        /// Gets the accounts.
        /// </summary>
        /// <returns>The accounts.</returns>
        IList<IAccount> GetAccounts();

        /// <summary>
        /// Gets the statement.
        /// </summary>
        void GetStatement();
    }
}

