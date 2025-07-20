using vj0.Framework.Models;
using vj0.ViewModels;

namespace vj0.Views;

public partial class ScopeView : ViewBase<ScopeViewModel>
{
    public ScopeView(): base(ScopeVM)
    {
        InitializeComponent();
    }
}