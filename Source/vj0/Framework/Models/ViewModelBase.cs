using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace vj0.Framework.Models;

public abstract class ViewModelBase : ObservableValidator
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public virtual async Task Initialize() { }  
    public virtual async Task OnViewOpened() { }  
    public virtual async Task OnViewExited() { }  
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}