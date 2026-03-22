using System.Reflection;

namespace SURIMI.Common.gRPC.Services
{
    public class ProtocolVersionService
    {
        public string LoadVersion()
        {
            var assembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "BSR.Surimi.Surimi-Protocol.Grpc.Csharp");

            if (assembly == null)
            {
                return "unknown";
            }

            var info = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (info == null)
            {
                return "unknown";
            }
            else
            {
                var plusIndex = info.IndexOf('+');
                return plusIndex >= 0 ? info.Substring(plusIndex + 1) : "unknown";
            }
        }
    }
}
