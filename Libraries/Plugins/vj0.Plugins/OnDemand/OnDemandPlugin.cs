using vj0.Core.Framework.Base;

namespace vj0.Plugins.OnDemand;

public interface IOnDemandPlugin : IPlugin
{
    public void PreInitialize();
    public Task Initialize(BaseProfile Profile);
}
