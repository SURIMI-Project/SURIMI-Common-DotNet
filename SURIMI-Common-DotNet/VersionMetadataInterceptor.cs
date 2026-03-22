using Grpc.Core;
using Grpc.Core.Interceptors;
using SURIMI.Common.gRPC.Services;
using System.Reflection;

namespace SURIMI.Common.gRPC
{
    public class VersionMetadataInterceptor : Interceptor
    {
        private readonly string _version;

        public VersionMetadataInterceptor(ProtocolVersionService protocolVersionService)
        {
            _version = protocolVersionService.LoadVersion();
            // Try to load the Surimi gRPC protocol assembly
            var assembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "BSR.Surimi.Surimi-Protocol.Grpc.Csharp");

            if (assembly == null)
            {
                _version = "unknown";
                return;
            }

            // Retrieve InformationalVersion (contains the +commit hash)
            var info = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (info == null)
            {
                _version = "unknown";
            }
            else
            {
                var plusIndex = info.IndexOf('+');
                _version = plusIndex >= 0 ? info.Substring(plusIndex + 1) : "unknown";
            }
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            context.ResponseTrailers.Add("protocol-version", _version);
            return await continuation(request, context);
        }
    }
}
