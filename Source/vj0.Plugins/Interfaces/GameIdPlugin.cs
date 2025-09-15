using vj0.Core.Framework.Base;

namespace vj0.Plugins.Interfaces;

public interface IGameIdPlugin : IGamePlugin
{
    EDetectedGameId GameId => EDetectedGameId.None;

    bool IGamePlugin.DoesInherentlyMatch(BaseProfile profile) => profile.AutoDetectedGameId == GameId;
}
