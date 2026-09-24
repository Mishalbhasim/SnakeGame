using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;


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


    public async Task EnsureSignedIn()
    {
        if (signInTask == null)
        {
            signInTask = SignInInternal();
        }

        Task thisAttempt = signInTask;

        try
        {
            await thisAttempt;
        }
        catch
        {
            // Only clear if no newer attempt has already replaced it.
            if (signInTask == thisAttempt)
            {
                signInTask = null;
            }
            throw;
        }
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