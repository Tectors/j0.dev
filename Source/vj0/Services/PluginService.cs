using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Serilog;

using vj0.Framework;
using vj0.Plugins;

namespace vj0.Services;

/* Loads Assemblies and stores Plugins */
public class PluginService : IService
{
    public List<IPlugin> List { get; } = [];
    
    public void Load()
    {
        LoadAssemblies();
        RegisterPlugins();
    }

    private void LoadAssemblies()
    {
        try
        {
            string pluginsPath = Path.Combine(AppContext.BaseDirectory, "Plugins");

            if (Directory.Exists(pluginsPath))
            {
                foreach (var pluginFile in Directory.GetFiles(pluginsPath, "vj0.Plugins.*.dll"))
                {
                    try
                    {
                        Assembly.LoadFrom(pluginFile);
                        Log.Information($"Loaded plugin assembly: {Path.GetFileName(pluginFile)}");
                    }
                    catch (Exception innerEx)
                    {
                        Log.Warning($"Failed to load plugin assembly {pluginFile}: {innerEx.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Failed to load plugin assemblies: {ex.Message}");
        }
    }

    private void RegisterPlugins()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                if (!assembly.GetName().Name?.StartsWith("vj0.Plugins.") == true)
                {
                    continue;
                }
                
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && 
                                    !t.IsAbstract && 
                                    !t.IsInterface &&
                                    t.GetConstructor(Type.EmptyTypes) != null);

                    foreach (var Plugin in types.Select(type => (IPlugin)Activator.CreateInstance(type)!))
                    {
                        RegisterPlugin(Plugin);
                        
                        Log.Information($"Registered plugin: {Plugin.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"Failed to scan assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Failed to discover plugins: {ex.Message}");
        }
        
        Log.Information($"Registered {List.Count} plugins");
    }
    
    private void RegisterPlugin(IPlugin plugin)
    {
        List.Add(plugin);
    }
}
