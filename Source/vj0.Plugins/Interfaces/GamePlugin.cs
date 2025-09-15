using vj0.Shared.Framework.Base;

namespace vj0.Plugins.Interfaces;

public interface IGamePlugin : IPlugin
{
    /* Does it match at all? */
    public bool Match(BaseProfile profile, out string reason)
    {
        reason = string.Empty;

        if (DoesCharacteristicsMatch(profile))
        {
            reason = "Characteristics matched";
            return true;
        }

        if (DoesInherentlyMatch(profile))
        {
            reason = "Inherently matched";
            return true;
        }

        return false;
    }
    
    /* Checks if the profile isn't auto-detected but has the characteristics of one. */
    public bool DoesCharacteristicsMatch(BaseProfile Profile);
    
    /* Checks if the profile matches inherently (e.g., auto-detected). */
    public bool DoesInherentlyMatch(BaseProfile Profile);
}