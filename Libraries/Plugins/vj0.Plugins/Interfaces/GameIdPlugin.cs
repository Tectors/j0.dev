using vj0.Core.Framework.Base;

namespace vj0.Plugins.Interfaces;

public interface IGameIdPlugin : IGamePlugin
{
    string GameId => string.Empty;

    bool IGamePlugin.DoesInherentlyMatch(BaseProfile profile) => profile.AutoDetectedGameId == GameId;
    bool IGamePlugin.DoesCharacteristicsMatch(BaseProfile profile) => false;
}
