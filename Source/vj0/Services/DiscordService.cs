using System;
using System.Threading.Tasks;
using DiscordRPC;
using Serilog;
using vj0.Application;
using vj0.Framework;

namespace vj0.Services;

public class DiscordService : IService
{
    private DiscordRpcClient? _client;
    private bool _isInitialized;
    
    private readonly RichPresence DefaultPresence = new()
    {
        Timestamps = new Timestamps { Start = DateTime.UtcNow },
        Assets = new Assets 
        {
            LargeImageKey = "logo",
            
            SmallImageText = $"v{Globals.VERSION} ({Globals.COMMIT})",
            SmallImageKey = $"small_image",
        },
        Buttons =
        [
            new Button 
            { 
                Label = "Join",
                Url = Globals.DISCORD_LINK 
            },
            new Button 
            { 
                Label = "Support us",
                Url = Globals.DONATE_LINK 
            }
        ]
    };
    
    public async void Initialize()
    {
        if (_isInitialized) return;

        await Task.Delay(1000);
        
        try
        {
            _client = new DiscordRpcClient(Globals.DISCORD_ACTIVITY_ID);
            
            _isInitialized = true;
            
            _client.OnReady += (_, args) =>
            {
                Log.Information("Discord Rich Presence Started for {Username} ({ID})", args.User.Username, args.User.ID);
                
                Log.Information("Setting presence with {ButtonCount} buttons", DefaultPresence.Buttons?.Length ?? 0);
            };
            
            _client.OnError += (_, args) => 
            {
                Log.Error("Discord Rich Presence Error {Type}: {Message}", 
                    args.Type.ToString(), args.Message);
            };

            _client.OnPresenceUpdate += (_, _) =>
            {
                Log.Information("Presence updated successfully");
            };

            _client.Initialize();
            
            await Task.Delay(500);
            _client.SetPresence(DefaultPresence);
            
            Log.Information("Button URL: {Url}", Globals.DISCORD_LINK);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize Discord Rich Presence");
        }
    }

    public void Deinitialize()
    {
        if (!_isInitialized || _client is null) return;

        try
        {
            var user = _client.CurrentUser;
            if (user is not null)
            {
                Log.Information("Discord Rich Presence Stopped for {Username} ({ID})", user.Username, user.ID);
            }

            _client.Deinitialize();
            _client.Dispose();
            _isInitialized = false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during Discord Rich Presence deinitialization");
        }
    }
    
    public void UpdateDetails(string Details)
    {
        if (!_isInitialized || _client is null) return;
        
        var presence = DefaultPresence;
        presence.Details = Details;
        
        Log.Information($"Changing presence details to {presence.Details}");
        
        _client.SetPresence(presence);
    }
    
    public void UpdateState(string State)
    {
        if (!_isInitialized || _client is null) return;
        
        var presence = DefaultPresence;
        presence.State = State;
        
        Log.Information($"Changing presence state to {presence.State}");
        
        _client.SetPresence(presence);
    }
}
