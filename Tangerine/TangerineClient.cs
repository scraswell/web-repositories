using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Craswell.Automation.DataAccess;
using Craswell.WebScraping;

using log4net;

namespace Craswell.WebRepositories.Tangerine
{
    /// <summary>
    /// Models the tangerine client.
    /// </summary>
    public class TangerineClient : IDisposable
    {
        /// <summary>
        /// Dictionary of strings to selectors for use in the tangerine client.
        /// </summary>
        private static readonly Dictionary<string, string> selectorMapping = new Dictionary<string, string>()
        {
            { "loginLink", "div.header-top-menu > a[href*='command=displayLogin']" },
            { "logoutLink", "section.visible-desktop a[href*='command=displayLogout']" },
            { "clientNumberInput", "input#ACN" },
            { "clientNumberGoButton", "button#GoBtn" },
            { "challengeInput", "input#Answer" },
            { "challengeQuestion", "form#ChallengeQuestion input + div > h2 + p" },
            { "challengeNextButton", "button#Next" },
            { "pinInput", "input#PIN" },
            { "pinGoButton", "button[name='Go']" },
            { "viewAccountsLink", "a[href*='command=displayAccountSummary']" },
            { "viewAccountSummaryLink", "a[href*='command=goToAccount&account=']" },
            { "chequingAccounts", ".chequing table tbody tr:not(.final) td:not([data-title='USD:'])" },
            { "savingsAccounts", ".savings table tbody tr:not(.final) td:not([data-title='USD:'])" },
            { "accountTransactionDetail", "div.account-details-history tr[data-page] td" },
            { "displayMyDocuments", "a[href*='/web/Tangerine.html?command=displayMyDocuments']" },
            { "statements", "a[href*='/web/Tangerine.html?command=gotoEstmtList']" },
            { "statementLinks", "[data-popupwin='true']" },
            { "statementSelect", ".dropdown-menu [data-value]" },
            { "refreshStatementList", "a[href*='refreshEStmtList()']" },
            { "saveStatementLink", "[href*='FORMAT=PDF']" },
            { "statementAccountDetails", "div.orange-key + div.eStatement-section .scrollable-table-inner tbody tr td:nth-child(2)" },
            { "statementDateInformation", "div.account-details-header p:nth-child(1)" },
            { "statementDropdown", "a[data-name='oldEstatmentList']" },
            { "statementDateSelector", "li > a[data-value]" },
            { "statementDate", "li > a[data-value='{0}']" },
            { "selectDate" , "div.btn-group + a" }
        };

        /// <summary>
        /// The logger used by the tangerine client.
        /// </summary>
        private ILog logger;

        /// <summary>
        /// Indicates whether the client is logged in.
        /// </summary>
        private bool isLoggedIn;

        /// <summary>
        /// The client.
        /// </summary>
        private HttpClient httpClient;

        /// <summary>
        /// The client configuration.
        /// </summary>
        private IWebRepositoryConfiguration clientConfiguration;

        /// <summary>
        /// The object factory.
        /// </summary>
        private TangerineObjectFactory objectFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Craswell.WebRepositories.Tangerine.TangerineClient"/> class.
        /// </summary>
        /// <param name="logger">The logger for the tangerine client.</param>
        /// <param name="httpClient">The HTTP client used by the TangerineClient.</param>
        /// <param name="clientConfiguration">The client configuration.</param>
        /// <param name="objectFactory">The object factory.</param> 
        public TangerineClient(
            ILog logger,
            HttpClient httpClient,
            IWebRepositoryConfiguration clientConfiguration,
            TangerineObjectFactory objectFactory)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("tangerineClientLogger");
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException("client");
            }

            if (clientConfiguration == null)
            {
                throw new ArgumentNullException("clientConfiguration");
            }

            if (objectFactory == null)
            {
                throw new ArgumentNullException("objectFactory");
            }

            this.logger = logger;
            this.httpClient = httpClient;
            this.clientConfiguration = clientConfiguration;
            this.objectFactory = objectFactory;
        }

        /// <summary>
        /// Releases all resource used by the <see cref="Craswell.WebRepositories.Tangerine.TangerineClient"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="Craswell.WebRepositories.Tangerine.TangerineClient"/>. The <see cref="Dispose"/> method leaves
        /// the <see cref="Craswell.WebRepositories.Tangerine.TangerineClient"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="Craswell.WebRepositories.Tangerine.TangerineClient"/> so the garbage collector can reclaim the
        /// memory that the <see cref="Craswell.WebRepositories.Tangerine.TangerineClient"/> was occupying.</remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the session.
        /// </summary>
        public void StartSession()
        {
            this.Login();
        }

        /// <summary>
        /// Gets a list of all bank accounts and recent transactions.
        /// </summary>
        public IList<IAccount> GetAccounts()
        {
            if (!this.isLoggedIn)
            {
                this.Login();
            }

            this.ClickViewAccountsLink();

            var tangerineAccounts = new List<TangerineAccount>();
            var accountInfo = new List<string>();

            accountInfo.AddRange(
                this.httpClient.GetElementsText(selectorMapping["chequingAccounts"]));

            accountInfo.AddRange(
                this.httpClient.GetElementsText(selectorMapping["savingsAccounts"]));

            this.LogList("list of accounts", accountInfo);

            tangerineAccounts.AddRange(this.objectFactory.BuildAccountList(accountInfo));

            ////foreach (TangerineAccount account in tangerineAccounts)
            ////{
            ////    this.ClickViewAccountsLink();
            ////    this.ClickViewAccountDetail(account);

            ////    List<string> transactionInfo = new List<string>();
            ////    transactionInfo.AddRange(this.httpClient
            ////        .GetElementsText(selectorMapping["accountTransactionDetail"])
            ////        .ToList<string>());

            ////    this.LogList(string.Format(
            ////        "list of transactions for account {0}",
            ////        account.Number),
            ////        transactionInfo);

            ////    IList<IAccountTransaction> transactions = this.objectFactory
            ////        .BuildTransactionList(transactionInfo);

            ////    account.Transactions = transactions;
            ////}

            return tangerineAccounts
                .ToList<IAccount>();
        }

        /// <summary>
        /// Gets all available statements.
        /// </summary>
        /// <returns>A list of statement details.</returns>
        public IList<TangerineStatement> GetAllStatements()
        {
            if (!this.isLoggedIn)
            {
                this.Login();
            }

            List<TangerineStatement> statements = new List<TangerineStatement>();

            this.ClickViewAccountsLink();

            this.GoToStatements();

            IDictionary<DateTime, int> statementDateMapping = this.BuildStatementDateMapping();

            foreach (DateTime key in statementDateMapping.Keys)
            {
                this.ClickStatementDateDropdown();

                this.ChooseStatementDate(statementDateMapping[key]);

                this.ClickView();

                IList<string> statementLinks = this.httpClient
                    .GetElementsAttributeValue(selectorMapping["statementLinks"], "href");

                foreach (string link in statementLinks)
                {
                    this.httpClient.ClickElement(string.Format("a[href='{0}']", new Uri(link).PathAndQuery));

                    this.httpClient.FocusLastOpenedWindow();

                    string statementDateInfo = this.httpClient
                        .GetElementText(selectorMapping["statementDateInformation"]);

                    this.logger.DebugFormat("Statement Date Information: {0}", statementDateInfo);

                    string statementAccountInfo = this.httpClient
                        .GetElementText(selectorMapping["statementAccountDetails"]);

                    this.logger.DebugFormat("Statement Account Information: {0}", statementAccountInfo);

                    TangerineStatement statement = this.objectFactory
                        .BuildStatement(statementDateInfo, statementAccountInfo);

                    string downloadUrl = this.httpClient.GetElementAttributeValue(
                        selectorMapping["saveStatementLink"],
                        "href");

                    string filePath = this.httpClient.DownloadFile(downloadUrl);

                    ////string fileName = Path.GetFileName(filePath);
                    string directoryName = Path.GetDirectoryName(filePath);

                    File.Move(filePath, Path.Combine(directoryName, statement.FileName));

                    this.httpClient.CloseActiveWindow();

                    statements.Add(statement);
                }
            }

            return statements;
        }


        /// <summary>
        /// Gets a list of all bank accounts and recent transactions.
        /// </summary>
        public TangerineStatement GetStatement(IAccount account, int year, int month)
        {
            if (!this.isLoggedIn)
            {
                this.Login();
            }

            DateTime statementDate = new DateTime(year, month, 1);

            TangerineStatement statement = null;

            this.ClickViewAccountsLink();

            this.GoToStatements();

            IDictionary<DateTime, int> statementDateMapping = this.BuildStatementDateMapping();

            this.ClickStatementDateDropdown();

            this.ChooseStatementDate(statementDateMapping[statementDate]);

            this.ClickView();

            IList<string> statementLinks = this.httpClient
                .GetElementsAttributeValue(selectorMapping["statementLinks"], "href");

            foreach (string link in statementLinks)
            {
                this.httpClient.ClickElement(string.Format("a[href='{0}']", new Uri(link).PathAndQuery));

                this.httpClient.FocusLastOpenedWindow();

                string statementDateInfo = this.httpClient
                    .GetElementText(selectorMapping["statementDateInformation"]);

                this.logger.DebugFormat("Statement Date Information: {0}", statementDateInfo);

                string statementAccountInfo = this.httpClient
                    .GetElementText(selectorMapping["statementAccountDetails"]);

                this.logger.DebugFormat("Statement Account Information: {0}", statementAccountInfo);

                TangerineStatement thisStatement = this.objectFactory
                    .BuildStatement(statementDateInfo, statementAccountInfo);

                if (thisStatement.AccountNumber == account.Number)
                {
                    statement = thisStatement;
                    break;
                }
                else
                {
                    this.httpClient.CloseActiveWindow();
                }
            }

            if (statement == null)
            {
                return statement;
            }

            string downloadUrl = this.httpClient.GetElementAttributeValue(
                selectorMapping["saveStatementLink"],
                "href");

            string filePath = this.httpClient.DownloadFile(downloadUrl);

            ////string fileName = Path.GetFileName(filePath);
            string directoryName = Path.GetDirectoryName(filePath);

            File.Move(filePath, Path.Combine(directoryName, statement.FileName));

            this.httpClient.CloseActiveWindow();

            return statement;
        }

        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Logout();

                if (this.httpClient != null)
                {
                    this.httpClient.Dispose();
                    this.httpClient = null;
                }
            }
        }

        /// <summary>
        /// Login to Tangerine banking.
        /// </summary>
        private void Login()
        {
            this.ClickLoginLink();
            this.EnterClientNumber();
            this.CompleteChallenge();
            this.EnterPin();

            this.isLoggedIn = true;
        }

        /// <summary>
        /// Clicks the login link.
        /// </summary>
        private void ClickLoginLink()
        {
            this.httpClient.OpenUrl(
                this.clientConfiguration.Address.ToString(),
                selectorMapping["loginLink"]);

            this.httpClient.ClickElementAndWaitForSelector(
                selectorMapping["loginLink"],
                selectorMapping["clientNumberInput"]);
        }

        /// <summary>
        /// Enters the client number.
        /// </summary>
        private void EnterClientNumber()
        {
            this.httpClient.EnterInput(
                selectorMapping["clientNumberInput"],
                this.clientConfiguration.Username);

            this.httpClient.ClickElementAndWaitForSelector(
                selectorMapping["clientNumberGoButton"],
                selectorMapping["challengeInput"]);
        }

        /// <summary>
        /// Completes the challenge question form.
        /// </summary>
        private void CompleteChallenge()
        {
            string challengeQuestion = this.httpClient
                .GetElementText(selectorMapping["challengeQuestion"]);

            this.httpClient.EnterInput(
                selectorMapping["challengeInput"],
                this.clientConfiguration.SecurityQuestions[challengeQuestion]);

            this.httpClient.ClickElementAndWaitForSelector(
                selectorMapping["challengeNextButton"],
                selectorMapping["pinInput"]);
        }

        /// <summary>
        /// Enters the pin.
        /// </summary>
        private void EnterPin()
        {
            this.httpClient.EnterInput(
                selectorMapping["pinInput"],
                this.clientConfiguration.Password);

            this.httpClient.ClickElementAndWaitForSelector(
                selectorMapping["pinGoButton"],
                selectorMapping["logoutLink"]);
        }

        /// <summary>
        /// Clicks the view accounts link.
        /// </summary>
        private void ClickViewAccountsLink()
        {
//            string href = this.client
//                .GetElementAttributeValue(selectorMapping["viewAccountsLink"], "href");
//
//            this.client.OpenUrl(
//                href,
//                selectorMapping["viewAccountSummaryLink"]);

            this.httpClient.ClickElementAndWaitForSelector(
                selectorMapping["viewAccountsLink"],
                selectorMapping["viewAccountSummaryLink"]);
        }

        /// <summary>
        /// Clicks the view accounts link.
        /// </summary>
        private void ClickStatementDateDropdown()
        {
            this.httpClient.ClickElement(
                selectorMapping["statementDropdown"]);
        }

        /// <summary>
        /// Clicks the view accounts link.
        /// </summary>
        private void ChooseStatementDate(int dataValue)
        {
            this.httpClient.ClickElement(string.Format(
                selectorMapping["statementDate"],
                dataValue));
        }

        /// <summary>
        /// Clicks the view accounts link.
        /// </summary>
        private void ClickView()
        {
            this.httpClient.ClickElement(
                selectorMapping["selectDate"]);
        }

        /// <summary>
        /// Views the account detail.
        /// </summary>
        /// <param name="account">Account.</param>
        private void ClickViewAccountDetail(TangerineAccount account)
        {
            string accountLinkSelector = selectorMapping["viewAccountSummaryLink"]
                .Replace(
                     "account=",
                     string.Concat(
                         "account=",
                         account.AccountIndex));

//            string href = this.client
//                .GetElementAttributeValue(accountLinkSelector, "href");
//
//            this.client.OpenUrl(
//                href,
//                selectorMapping["viewAccountSummaryLink"]);

            this.httpClient.ClickElementAndWaitForSelector(
                accountLinkSelector,
                selectorMapping["accountTransactionDetail"]);
        }

        /// <summary>
        /// Follows links to statements.
        /// </summary>
        private void GoToStatements()
        {
            this.httpClient.ClickElementAndWaitForSelector(
                selectorMapping["displayMyDocuments"],
                selectorMapping["statements"]);

            this.httpClient.ClickElementAndWaitForSelector(
                selectorMapping["statements"],
                selectorMapping["statementLinks"]);
        }

        /// <summary>
        /// Logout of Tangerine banking.
        /// </summary>
        private void Logout()
        {
            this.httpClient.ClickElementAndWaitForSelector(
                selectorMapping["logoutLink"],
                selectorMapping["loginLink"]);

            this.isLoggedIn = false;
        }

        /// <summary>
        /// Logs a list of strings to the logger.
        /// </summary>
        /// <param name="description">A description of the listed items.</param>
        /// <param name="listOfStrings">Logs the list of strings to the logger.</param>
        private void LogList(string description, IList<string> listOfStrings)
        {
            this.logger.DebugFormat("Logging {0}", description);

            foreach(string stringInstance in listOfStrings)
            {
                this.logger.Debug(stringInstance);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IDictionary<DateTime, int> BuildStatementDateMapping()
        {
            Dictionary<DateTime, int> statementDates = new Dictionary<DateTime, int>();

            var statementDateMapping = this.httpClient
                .EnumerateSelect(selectorMapping["statementDateSelector"], "data-value");

            foreach(string date in statementDateMapping.Keys)
            {
                DateTime parsedDate;
                int parsedValue;

                if (DateTime.TryParse(date, out parsedDate)
                    && int.TryParse(statementDateMapping[date], out parsedValue))
                {
                    statementDates.Add(parsedDate, parsedValue);
                }
            }

            return statementDates;
        }
    }
}

