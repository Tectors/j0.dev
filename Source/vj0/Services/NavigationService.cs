using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.LogicalTree;

using FluentAvalonia.UI.Controls;

using vj0.Framework;

namespace vj0.Services;

public class NavigationService : IService
{
    public NavigatorContext App = new();
    public NavigatorContext Settings = new();
    
    public NavigatorContext Onboarding = new();
    public NavigatorContext OnboardingTerms = new();
}

public class NavigatorContext
{
    private NavigationView? _navigationView;
    private Frame _contentFrame = null!;
    private readonly Dictionary<Type, Func<object, Type?>> _typeResolvers = new();

    public void Initialize(NavigationView navigationView)
    {
        _navigationView = navigationView;

        _navigationView.ItemInvoked += (a, args) =>
        {
            if (args.InvokedItemContainer.Tag is Type pageType)
            {
                Open(pageType);
            }
        };
        
        _navigationView.Loaded += (_, _) =>
        {
            if (navigationView.SelectedItem is NavigationViewItem selectedItem &&
                selectedItem.Tag is Type type)
            {
                Open(type);
            }
        };
        
        _contentFrame = _navigationView.GetLogicalDescendants().OfType<Frame>().First();
    }

    public void AddTypeResolver<T>(Func<T, Type?> resolver)
    {
        _typeResolvers[typeof(T)] = obj => resolver((T)obj);
    }
    
    public event Action<Type>? OnNavigate;

    public void Open<T>() => Open(typeof(T));

    public void Open(object? obj)
    {
        if (obj is null) return;

        var viewType = ResolveType(obj);
        if (viewType is null || _contentFrame is null || IsTabOpen(viewType)) return;
        
        OnNavigate?.Invoke(viewType);

        _contentFrame.Navigate(viewType, null, Settings.Application.Transition);
        _contentFrame.Focus();

        if (_navigationView is not null)
        {
            _navigationView.SelectedItem = _navigationView.MenuItems
                .Concat(_navigationView.FooterMenuItems)
                .OfType<NavigationViewItem>()
                .FirstOrDefault(item => Equals(item.Tag, obj));
        }
    }

    public bool IsTabOpen<T>() => IsTabOpen(typeof(T));

    private bool IsTabOpen(object? obj)
    {
        if (obj is null) return false;

        var viewType = ResolveType(obj);
        if (_contentFrame is null) return false;
        
        return viewType is not null && _contentFrame.CurrentSourcePageType == viewType;
    }

    private Type? ResolveType(object obj)
    {
        return obj switch
        {
            Type type => type,
            _ => _typeResolvers.TryGetValue(obj.GetType(), out var resolver) ? resolver(obj) : null
        };
    }
}