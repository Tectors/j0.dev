﻿using vj0.Framework.Models;
using vj0.Services.Framework;
using vj0.ViewModels.Settings;

namespace vj0.Views.Settings;

public partial class ModelSettingsView : ViewBase<ModelSettingsViewModel>
{
    public ModelSettingsView() : base(AppServices.Settings.Model)
    {
        InitializeComponent();
    }
}