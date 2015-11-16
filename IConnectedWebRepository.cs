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
        /// Gets a statement for an account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        void GetStatement(IAccount account, int year, int month);

        /// <summary>
        /// Gets all statement for an account.
        /// </summary>
        void GetAllStatements();
    }
}

