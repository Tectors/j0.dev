using System;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using vj0.Framework;
using vj0.Framework.Models;

namespace vj0.Services.Framework;

public abstract class TransientAttribute : Attribute;

public static class ServiceExtensions
{
    public static void AddCommonServices(this ServiceCollection collection)
    {
        var serviceTypes = Assembly.GetAssembly(typeof(IService))?.GetTypes().Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IService))) ?? [];

        foreach (var serviceType in serviceTypes)
        {
            collection.AddSingleton(serviceType);
        }
    }
    
    public static void AddViewModels(this ServiceCollection collection)
    {
        var viewModelTypes = Assembly.GetAssembly(typeof(ViewModelBase))?.GetTypes().Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(ViewModelBase))) ?? [];

        foreach (var viewModelType in viewModelTypes)
        {
            if (viewModelType.GetCustomAttribute<TransientAttribute>() is not null)
            {
                collection.AddTransient(viewModelType);
            }
            else
            {
                collection.AddSingleton(viewModelType);
            }
        }
    }
}