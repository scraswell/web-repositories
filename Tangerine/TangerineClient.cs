using System;
using System.Collections.Generic;
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
            { "saveStatementLink", "[href*='FORMAT=PDF']" }
        };

        /// <summary>
        /// Indicates whether the client is logged in.
        /// </summary>
        private bool isLoggedIn;

        /// <summary>
        /// The client.
        /// </summary>
        private HttpClient client;

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
        /// <param name="clientConfiguration">The client configuration.</param>
        /// <param name="objectFactory">The object factory.</param> 
        public TangerineClient(
            HttpClient client,
            IWebRepositoryConfiguration clientConfiguration,
            TangerineObjectFactory objectFactory)
        {
            if (client == null)
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

            this.client = client;
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
                this.client.GetElementsText(selectorMapping["chequingAccounts"]));

            accountInfo.AddRange(
                this.client.GetElementsText(selectorMapping["savingsAccounts"]));

            tangerineAccounts.AddRange(this.objectFactory.BuildAccountList(accountInfo));

            foreach (TangerineAccount account in tangerineAccounts)
            {
                this.ClickViewAccountsLink();
                this.ClickViewAccountDetail(account);

                List<string> transactionInfo = new List<string>();
                transactionInfo.AddRange(this.client
                    .GetElementsText(selectorMapping["accountTransactionDetail"])
                    .ToList<string>());

                IList<IAccountTransaction> transactions = this.objectFactory
                    .BuildTransactionList(transactionInfo);

                account.Transactions = transactions;
            }

            return tangerineAccounts
                .ToList<IAccount>();
        }

        /// <summary>
        /// Gets a list of all bank accounts and recent transactions.
        /// </summary>
        public void GetStatement()
        {
            if (!this.isLoggedIn)
            {
                this.Login();
            }

            this.ClickViewAccountsLink();
            this.GoToStatements();

            this.client.ClickElement(
                selectorMapping["statementLinks"]);
            this.client.FocusLastOpenedWindow();

            string downloadUrl = this.client.GetElementAttributeValue(
                selectorMapping["saveStatementLink"],
                "href");

            this.client.DownloadFile(downloadUrl);

            this.client.CloseActiveWindow();
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

                if (this.client != null)
                {
                    this.client.Dispose();
                    this.client = null;
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
            this.client.OpenUrl(
                this.clientConfiguration.Address.ToString(),
                selectorMapping["loginLink"]);

            this.client.ClickElementAndWaitForSelector(
                selectorMapping["loginLink"],
                selectorMapping["clientNumberInput"]);
        }

        /// <summary>
        /// Enters the client number.
        /// </summary>
        private void EnterClientNumber()
        {
            this.client.EnterInput(
                selectorMapping["clientNumberInput"],
                this.clientConfiguration.Username);

            this.client.ClickElementAndWaitForSelector(
                selectorMapping["clientNumberGoButton"],
                selectorMapping["challengeInput"]);
        }

        /// <summary>
        /// Completes the challenge question form.
        /// </summary>
        private void CompleteChallenge()
        {
            string challengeQuestion = this.client
                .GetElementText(selectorMapping["challengeQuestion"]);

            this.client.EnterInput(
                selectorMapping["challengeInput"],
                this.clientConfiguration.SecurityQuestions[challengeQuestion]);

            this.client.ClickElementAndWaitForSelector(
                selectorMapping["challengeNextButton"],
                selectorMapping["pinInput"]);
        }

        /// <summary>
        /// Enters the pin.
        /// </summary>
        private void EnterPin()
        {
            this.client.EnterInput(
                selectorMapping["pinInput"],
                this.clientConfiguration.Password);

            this.client.ClickElementAndWaitForSelector(
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

            this.client.ClickElementAndWaitForSelector(
                selectorMapping["viewAccountsLink"],
                selectorMapping["viewAccountSummaryLink"]);
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

            this.client.ClickElementAndWaitForSelector(
                accountLinkSelector,
                selectorMapping["accountTransactionDetail"]);
        }

        /// <summary>
        /// Follows links to statements.
        /// </summary>
        private void GoToStatements()
        {
            this.client.ClickElementAndWaitForSelector(
                selectorMapping["displayMyDocuments"],
                selectorMapping["statements"]);

            this.client.ClickElementAndWaitForSelector(
                selectorMapping["statements"],
                selectorMapping["statementLinks"]);
        }

        /// <summary>
        /// Logout of Tangerine banking.
        /// </summary>
        private void Logout()
        {
            this.client.ClickElementAndWaitForSelector(
                selectorMapping["logoutLink"],
                selectorMapping["loginLink"]);

            this.isLoggedIn = false;
        }
    }
}

