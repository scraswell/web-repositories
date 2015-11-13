using System;
using System.Collections.Generic;
using System.Linq;

using Craswell.Automation.DataAccess;
using Craswell.Encryption;
using Craswell.WebRepositories.Tangerine;
using Craswell.WebScraping;

using log4net;
using NHibernate;

namespace Craswell.WebRepositories
{
    public class WebRepositoryFactory : IDisposable
    {
        /// <summary>
        /// The dal.
        /// </summary>
        private DataAccessLayer dal = new DataAccessLayer();

        /// <summary>
        /// The aes tool.
        /// </summary>
        private AesEncryptionTool aesTool = new AesEncryptionTool();

        /// <summary>
        /// The passphrase used for encryption and decryption.
        /// </summary>
        private string passphrase;

        /// <summary>
        /// Initializes a new instance of the <see cref="Craswell.WebRepositories.WebRepositoryFactory"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase used for encryption and decryption.</param>
        public WebRepositoryFactory(string passphrase)
        {
            if (string.IsNullOrEmpty(passphrase))
            {
                throw new ArgumentNullException("passphrase");
            }

            this.passphrase = passphrase;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Craswell.WebRepositories.WebRepositoryFactory"/> is reclaimed by garbage collection.
        /// </summary>
        ~WebRepositoryFactory()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="Craswell.WebRepositories.WebRepositoryFactory"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="Craswell.WebRepositories.WebRepositoryFactory"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="Craswell.WebRepositories.WebRepositoryFactory"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="Craswell.WebRepositories.WebRepositoryFactory"/> so the garbage collector can reclaim the memory
        /// that the <see cref="Craswell.WebRepositories.WebRepositoryFactory"/> was occupying.</remarks>
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
                if (this.dal != null)
                {
                    this.dal.Dispose();
                    this.dal = null;
                }
            }
        }

        /// <summary>
        /// Creates the web repository and returns its identifier.
        /// </summary>
        /// <param name="repositoryType">The type of web repository.</param>
        /// <param name="repositoryName">The repository name.</param>
        /// <param name="webAddress">Web address.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="securityQuestions">Security questions.</param>
        public int CreateWebRepository(
            WebRepositoryType repositoryType,
            string repositoryName,
            string webAddress,
            string username,
            string password,
            IDictionary<string, string> securityQuestions)
        {
            securityQuestions = securityQuestions
                .ToDictionary(
                sq => this.aesTool.EncryptText(sq.Key, this.passphrase),
                sq => this.aesTool.EncryptText(sq.Value, this.passphrase));

            WebRepositoryConfigurationData config = new WebRepositoryConfigurationData()
            {
                Address = new Uri(webAddress),
                Username = this.aesTool.EncryptText(username, this.passphrase),
                Password = this.aesTool.EncryptText(password, this.passphrase),
                SecurityQuestions = securityQuestions
            };

            var repo = new WebRepositoryData()
            {
                Type = repositoryType,
                Name = repositoryName,
                Configuration = config
            };

            using (ISession session = this.dal.OpenSession())
            using (ITransaction tx = session.BeginTransaction())
            {
                session.Save(repo);
                tx.Commit();
            }

            return repo.Id;
        }

        /// <summary>
        /// Gets the web repositories.
        /// </summary>
        /// <returns>The web repositories.</returns>
        public IList<IConnectedWebRepository> GetWebRepositories()
        {
            List<IConnectedWebRepository> repositoryList = new List<IConnectedWebRepository>();

            using (ISession session = this.dal.OpenSession())
            {
                IList<WebRepositoryData> webRepositories = session
                    .CreateCriteria<WebRepositoryData>()
                    .List<WebRepositoryData>();

                foreach (WebRepositoryData repository in webRepositories)
                {
                    switch (repository.Type)
                    {
                        case WebRepositoryType.Tangerine:
                            ILog tangerineLogger = LogManager.GetLogger("TangerineClientLogger");

                            TangerineConfiguration config = new TangerineConfiguration(
                                repository.Configuration.Address,
                                this.aesTool.DecryptText(repository.Configuration.Username, this.passphrase),
                                this.aesTool.DecryptText(repository.Configuration.Password, this.passphrase),
                                repository.Configuration.SecurityQuestions
                                .ToDictionary(
                                    sq => this.aesTool.DecryptText(sq.Key, this.passphrase),
                                    sq => this.aesTool.DecryptText(sq.Value, this.passphrase)));

                            HttpClient httpClient = new HttpClient(tangerineLogger);

                            TangerineObjectFactory objectFactory = new TangerineObjectFactory();

                            TangerineClient client = new TangerineClient(
                                httpClient,
                                config,
                                objectFactory);

                            IConnectedWebRepository repo = new TangerineRepository(
                                repository.Id,
                                repository.Name,
                                client);

                            repositoryList.Add(repo);
                            break;
                    }
                }
            }

            return repositoryList;
        }
    }
}

