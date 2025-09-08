using System;
using System.Text.Json.Serialization;

using CommunityToolkit.Mvvm.ComponentModel;

namespace vj0.Shared.Framework.Base;

public enum EProfileStatus
{
    Idle,
    
    /* Is Running */
    Active,
    
    /* Uncreated yet */
    Uncompleted
}

public class BaseProfileStatus : ObservableObject
{
    [JsonIgnore] public EProfileStatus State;
    
    [JsonIgnore] public string FailureReason = "";

    public Action OnStateChange = null!;

    public void SetState(EProfileStatus state)
    {
        State = state;
        OnStateChange.Invoke();
    }

    public void OnFailure(string reason)
    {
        FailureReason = reason;
        SetState(EProfileStatus.Idle);
    }
}