using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Kubernetes;
using VaultSharp.V1.Commons;

namespace Ship.Ses.Extractor.Infrastructure.Services
{

    public class VaultService
    {
        private readonly IVaultClient _vaultClient;
        private readonly string _secretsPath;

        public VaultService(string vaultUri, string role, string mountPath, string secretsPath)
        {
            if (string.IsNullOrWhiteSpace(vaultUri))
                throw new ArgumentException("Vault URI is required.");
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Vault role is required.");
            if (string.IsNullOrWhiteSpace(mountPath))
                throw new ArgumentException("Vault mount path is required.");
            if (string.IsNullOrWhiteSpace(secretsPath))
                throw new ArgumentException("Vault secrets path is required.");

            _secretsPath = secretsPath;

            var jwt = File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");

            IAuthMethodInfo authMethod = new KubernetesAuthMethodInfo(role, jwt, mountPath);
            var vaultClientSettings = new VaultClientSettings(vaultUri, authMethod);

            _vaultClient = new VaultClient(vaultClientSettings);
        }

        public async Task<Dictionary<string, string>> GetSecretsAsync()
        {
            Secret<SecretData> secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(_secretsPath);
            return secret.Data?.Data?.ToDictionary(k => k.Key, v => v.Value?.ToString()) ?? new Dictionary<string, string>();
        }
    }

}
