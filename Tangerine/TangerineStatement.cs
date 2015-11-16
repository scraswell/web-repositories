using System;

using Craswell.Automation.DataAccess;

namespace Craswell.WebRepositories.Tangerine
{
    /// <summary>
    /// Models a tangerine statement.
    /// </summary>
    public class TangerineStatement : IAccountStatement
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the Account Number.
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the FileName.
        /// </summary>
        public string FileName { get; set; }
    }
}
