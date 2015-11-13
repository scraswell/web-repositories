using System;

using Craswell.Automation.DataAccess;

namespace Craswell.WebRepositories.Tangerine
{
    /// <summary>
    /// Tangerine transaction.
    /// </summary>
    public class TangerineTransaction : IAccountTransaction
    {
        #region IAccountTransaction implementation
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets the transaction type.
        /// </summary>
        /// <value>The type.</value>
        public TransactionType Type { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>The subject.</value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        public double Amount { get; set; }
        #endregion
    }
}

