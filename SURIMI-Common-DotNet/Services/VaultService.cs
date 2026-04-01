using VaultSharp;

namespace SURIMI.Common.Services
{
    public class VaultService
    {
        public void LoadVaultSecretsInEnvironmentVariables()
        {
            var vaultAddr = Environment.GetEnvironmentVariable("VAULT_ADDR");
            var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");
            var vaultTopDir = Environment.GetEnvironmentVariable("VAULT_TOP_DIR");
            var vaultRelativePath = Environment.GetEnvironmentVariable("VAULT_RELATIVE_PATH");
            var vaultMount = Environment.GetEnvironmentVariable("VAULT_MOUNT");
            if (string.IsNullOrEmpty(vaultAddr) || string.IsNullOrEmpty(vaultToken) || string.IsNullOrEmpty(vaultTopDir) || string.IsNullOrEmpty(vaultRelativePath) || string.IsNullOrEmpty(vaultMount))
            {
                Console.WriteLine("Vault Addr, Token, Top Dir, Relative Path, or Mount not set in environment variables. Skipping Vault loading.");
                return;
            }
            var vaultClient = new VaultClient(new VaultClientSettings(vaultAddr, new VaultSharp.V1.AuthMethods.Token.TokenAuthMethodInfo(vaultToken)));
            // Assuming secrets are stored under "secret/data/surimi"
            var secretPath = $"{vaultTopDir}/{vaultRelativePath}";
            try
            {
                var secret = vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretPath, mountPoint: vaultMount).Result;
                foreach (var kv in secret.Data.Data)
                {
                    Environment.SetEnvironmentVariable(kv.Key, kv.Value.ToString());
                    Console.WriteLine($"Loaded secret '{kv.Key}' from Vault into environment variables.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading secrets from Vault: {ex.Message}");
            }
        }
    }
}
