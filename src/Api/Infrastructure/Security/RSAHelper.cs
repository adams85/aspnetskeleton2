using System;
using System.IO;
using System.Security.Cryptography;
using ProtoBuf.Meta;

namespace WebApp.Api.Infrastructure.Security
{
    public static class RSAHelper
    {
        private static RuntimeTypeModel Configure(this RuntimeTypeModel typeModel)
        {
            typeModel.Add(typeof(RSAParameters), applyDefaultBehaviour: false)
                .Add(1, nameof(RSAParameters.D))
                .Add(2, nameof(RSAParameters.DP))
                .Add(3, nameof(RSAParameters.DQ))
                .Add(4, nameof(RSAParameters.Exponent))
                .Add(5, nameof(RSAParameters.InverseQ))
                .Add(6, nameof(RSAParameters.Modulus))
                .Add(7, nameof(RSAParameters.P))
                .Add(8, nameof(RSAParameters.Q));

            return typeModel;
        }

        private static readonly RuntimeTypeModel s_protoBufTypeModel = RuntimeTypeModel.Create().Configure();

        public static RSAParameters GenerateParameters()
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            return rsa.ExportParameters(includePrivateParameters: true);
        }

        public static string SerializeParameters(RSAParameters rsaParams)
        {
            using var ms = new MemoryStream();
            s_protoBufTypeModel.Serialize(ms, rsaParams);
            return Convert.ToBase64String(ms.GetBuffer().AsSpan(0, (int)ms.Length));
        }

        public static RSAParameters DeserializeParameters(string value)
        {
            using var ms = new MemoryStream(Convert.FromBase64String(value));
            return (RSAParameters)s_protoBufTypeModel.Deserialize(ms, null, typeof(RSAParameters));
        }
    }
}
