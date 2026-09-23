using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;

/// <summary>
/// Single shared owner of UGS sign-in. AchievementManager and LeaderboardManager
/// both call EnsureSignedIn() instead of signing in themselves - this caches the
/// actual in-flight Task, so if both call it around the same time, the second
/// caller awaits the same sign-in operation instead of racing to start a new one
/// (which is what was causing "player is already signing in" errors).
/// Attach to an empty GameObject called "AuthManager" in the scene.
/// </summary>
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private Task signInTask;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Task EnsureSignedIn()
    {
        if (signInTask == null)
        {
            signInTask = SignInInternal();
        }
        return signInTask;
    }

    private async Task SignInInternal()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        await AuthenticationService.Instance.GetPlayerNameAsync();

        Debug.Log("AuthManager: signed in as " + AuthenticationService.Instance.PlayerName);
    }
}