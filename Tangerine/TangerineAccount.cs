using System;
using System.Collections.Generic;

using Craswell.Automation.DataAccess;

namespace Craswell.WebRepositories.Tangerine
{
    public class TangerineAccount : IAccount
    {
        #region IAccount implementation
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>The number.</value>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the balance.
        /// </summary>
        /// <value>The balance.</value>
        public double Balance { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>The transactions.</value>
        public IList<IAccountTransaction> Transactions { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>The transactions.</value>
        public IList<IAccountStatement> Statements { get; set; }
        #endregion

        /// <summary>
        /// Gets or sets the index of the account.
        /// </summary>
        /// <value>The index of the account.</value>
        public int AccountIndex { get; set; }
    }
}

