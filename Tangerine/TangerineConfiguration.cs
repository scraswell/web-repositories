using System;
using System.Collections.Generic;
using System.Linq;

using Craswell.Automation.DataAccess;

namespace Craswell.WebRepositories.Tangerine
{
    /// <summary>
    /// Tangerine configuration.
    /// </summary>
    public class TangerineConfiguration : IWebRepositoryConfiguration
    {
        /// <summary>
        /// The web site address.
        /// </summary>
        private Uri webSiteAddress;

        /// <summary>
        /// The acn.
        /// </summary>
        private readonly string acn;

        /// <summary>
        /// The pin.
        /// </summary>
        private readonly string pin;

        /// <summary>
        /// The security questions.
        /// </summary>
        private readonly Dictionary<string, string> securityQuestions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Craswell.WebRepositories.Tangerine.TangerineConfiguration"/> class.
        /// </summary>
        /// <param name="webSiteAddress">Web site address.</param>
        /// <param name="acn">The account client number.</param>
        /// <param name="pin">The client pin.</param>
        /// <param name="securityQuestions">The client security questions.</param>
        public TangerineConfiguration(
            Uri webSiteAddress,
            string acn,
            string pin,
            IDictionary<string, string> securityQuestions)
        {
            if (string.IsNullOrEmpty(acn))
            {
                throw new ArgumentNullException("acn");
            }

            if (string.IsNullOrEmpty(pin))
            {
                throw new ArgumentNullException("pin");
            }

            if (webSiteAddress == null)
            {
                throw new ArgumentNullException("webSiteAddress");
            }

            if (securityQuestions == null)
            {
                throw new ArgumentNullException("securityQuestions");
            }

            this.acn = acn;
            this.pin = pin;
            this.webSiteAddress = webSiteAddress;

            this.securityQuestions = securityQuestions
                .ToDictionary(d => d.Key, d => d.Value);
        }

        #region IWebRepositoryConfiguration implementation
        /// <summary>
        /// Gets the address.
        /// </summary>
        /// <value>The address.</value>
        public Uri Address
        {
            get
            {
                return this.webSiteAddress;
            }
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username
        {
            get
            {
                return this.acn;
            }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get
            {
                return this.pin;
            }
        }

        /// <summary>
        /// Gets the security questions.
        /// </summary>
        /// <value>The security questions.</value>
        public IDictionary<string, string> SecurityQuestions
        {
            get
            {
                return this.securityQuestions;
            }
        }
        #endregion
    }
}

