using vj0.Shared.Framework.Base;

namespace vj0.Plugins.Resolvers;

/* Interface responsible for handling game-specific APIs and installations,
   used to retrieve AES keys, mappings, or other required data for each game. */
public interface IArchiveResolverPlugin : IPlugin
{
    /* Resolves data for a specific Profile */
    public async Task Resolve(BaseProfile profile)
    {
        await ResolveKeys(profile);
        await ResolveMappings(profile);
    }

    /* Resolves keys for a specific Profile */
    public Task ResolveKeys(BaseProfile profile);

    /* Resolves mappings for a specific Profile */
    public Task ResolveMappings(BaseProfile profile);
}