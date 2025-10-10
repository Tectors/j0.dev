using CUE4Parse.UE4.Versions;
using vj0.Core.Framework.Base;

namespace vj0.Plugins.Interfaces;

public interface IGameVersionUpdatePlugin : IGameIdPlugin
{
    EGame TargetVersion => EGame.GAME_UE5_LATEST;

    public void Update(BaseProfile Profile)
    {
        Profile.Version = TargetVersion;
    }
}
