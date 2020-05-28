using ProtoBuf.Meta;

namespace WebApp.Service.Host
{
    public partial class ServiceHostContractSerializer
    {
        public static RuntimeTypeModel ConfigureServiceHostDefaults(this RuntimeTypeModel typeModel)
        {
            // register external types, etc.

            return typeModel;
        }
    }
}

