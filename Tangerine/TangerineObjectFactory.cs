using System;
using System.Collections.Generic;
using System.Linq;

using Craswell.Automation.DataAccess;

namespace Craswell.WebRepositories.Tangerine
{
    /// <summary>
    /// A factory used to create objects from data gathered using the
    /// web repository.
    /// </summary>
    public class TangerineObjectFactory
    {
        /// <summary>
        /// Creates a statement from collected information.
        /// </summary>
        /// <param name="statementAccountInformation">The statement account information.</param>
        /// <param name="statementDateInformation">The statement date information.</param>
        /// <returns>The statement information.</returns>
        public TangerineStatement BuildStatement(
            string statementDateInformation,
            string statementAccountInformation)
        {
            string[] statementInfo = statementDateInformation
                .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string date = statementInfo[0]
                .Split(new string[] { " to " }, StringSplitOptions.RemoveEmptyEntries)
                .Last()
                .Trim();

            string clientNumber = statementInfo[1]
                .Split(new string[] { "#:" }, StringSplitOptions.RemoveEmptyEntries)
                .Last()
                .Trim();

            string[] accountInfo = statementAccountInformation
                .Split();

            string accountNumber = accountInfo
                .Last()
                .Trim();

            string fileName = string.Format(
                "{0}.pdf",
                Guid.NewGuid());

            DateTime statementTimestamp = DateTime.Parse(date);

            return new TangerineStatement()
            {
                Timestamp = DateTime.Parse(date),
                AccountNumber = accountNumber,
                FileName = fileName
            };
        }

        /// <summary>
        /// Creates accounts from the gathered account data.
        /// </summary>
        /// <returns>A list of accounts.</returns>
        /// <param name="accountData">Data scraped from the web repository.</param>
        public IList<TangerineAccount> BuildAccountList(IList<string> accountData)
        {
            List<TangerineAccount> accountList = new List<TangerineAccount>();

            for (var i = 0; i < accountData.Count; i += 2)
            {
                TangerineAccount account = new TangerineAccount()
                {
                    AccountIndex = (i) / 2,
                    Name = this.ParseName(accountData[i]),
                    Number = this.ParseNumber(accountData[i]),
                    Balance = this.ParseBalance(accountData[i + 1])
                };

                accountList.Add(account);
            }

            return accountList;
        }

        /// <summary>
        /// Builds the transaction list.
        /// </summary>
        /// <returns>The transaction list.</returns>
        /// <param name="transactionData">Transaction data.</param>
        public IList<IAccountTransaction> BuildTransactionList(IList<string> transactionData)
        {
            List<IAccountTransaction> transactionList = new List<IAccountTransaction>();

            for (var i = 0; i < transactionData.Count; i += 5)
            {
                double amount;
                double.TryParse(transactionData[i + 3], out amount);

                TangerineTransaction tx = new TangerineTransaction()
                {
                    Timestamp = DateTime.Parse(transactionData[i]),
                    Subject = transactionData[i+1],
                    Type = amount < 0 ? TransactionType.Debit : TransactionType.Credit,
                    Amount = amount
                };

                transactionList.Add(tx);
            }

            return transactionList;
        }

        /// <summary>
        /// Parses the name of the account.
        /// </summary>
        /// <returns>The account name.</returns>
        /// <param name="accountInfo">Account info.</param>
        private string ParseName(string accountInfo)
        {
            IEnumerable<string> parts = accountInfo.Split(
                                 new string[] { " - " },
                                 StringSplitOptions.RemoveEmptyEntries);

            parts = parts
                .Select(p => p.Trim());

            return parts.First();
        }

        /// <summary>
        /// Parses the name of the account.
        /// </summary>
        /// <returns>The account name.</returns>
        /// <param name="accountInfo">Account info.</param>
        private string ParseNumber(string accountInfo)
        {
            IEnumerable<string> parts = accountInfo.Split(
                new string[] { " - " },
                StringSplitOptions.RemoveEmptyEntries);

            parts = parts
                .Select(p => p.Trim());

            return parts.Last();
        }

        /// <summary>
        /// Parses the balance.
        /// </summary>
        /// <returns>The balance.</returns>
        /// <param name="balanceInfo">Account info.</param>
        private double ParseBalance(string balanceInfo)
        {
            balanceInfo = balanceInfo
                .Replace(",", string.Empty)
                .Replace("$", string.Empty);

            double balance;
            if (!double.TryParse(balanceInfo, out balance))
            {
                throw new InvalidOperationException(string.Format(
                    "Could not parse balance from {0}",
                    balanceInfo));
            }

            return balance;
        }
    }
}

