# Snake Game (Unity, Android)

A 2D Snake game for Android, built in Unity with C#. Portrait orientation, on-screen D-pad controls, and a win condition (fill the whole 20x20 grid). It includes **Ads**, an online **Leaderboard**, **Achievements** and **Analytics**.

## Play it

**Live version (Android APK):** [PASTE ITCH.IO LINK HERE]

Download the APK from the itch.io page, allow "install from unknown sources" on your phone if asked, and install it.

## Features

| Feature | How it works |
|---|---|
| **Gameplay** | 20x20 grid, on-screen D-pad, +10 points per food, high score saved on the device. Filling the entire grid wins the game ("You Win!"). |
| **Ads (Google AdMob)** | A **rewarded ad** lets the player revive once per game after dying. The revived snake keeps its length and score and turns away from the obstacle. An **interstitial ad** shows on game over, but not right after the player declines a revive offer. |
| **Leaderboard (Unity Leaderboards)** | Score is submitted automatically at the end of every game. Shows the top 20 players and the player's own rank, pinned at the bottom if they are outside the top 20. Players get an auto-generated name through anonymous sign-in. The board resets monthly. |
| **Achievements (Unity Cloud Save)** | Achievements are stored in Cloud Save, tied to the anonymous player account. There is a popup when one unlocks, and an Achievements screen showing locked and unlocked ones. |
| **Analytics (Unity Analytics)** | Gameplay events are recorded for the developer. They are viewed on the Unity Dashboard and are not visible to players. |

### Achievements

- **First Bite**: eat your first food
- **Score tiers**: 50, then every 100 points (100, 200, 300, and so on, with no upper limit)
- **Length tiers**: every 10 segments (10, 20, 30, and so on, with no upper limit)
- **Perfectionist**: fill the entire grid

Achievement titles and descriptions are generated from the achievement ID (for example `score_200`), so any tier works without a hardcoded list. The Achievements screen always shows a core set, plus any higher tiers the player has earned.

### Analytics events

`game_start`, `food_eaten`, `revive_offer_shown`, `revive_ad_accepted`, `revive_ad_declined`, `game_over` (with `final_score`, `is_win`, `game_duration_seconds`), `new_high_score` (with `final_score`).

## Controls

- **On a phone:** on-screen D-pad.
- **In the Unity Editor (for testing):** WASD or arrow keys.

## Running the project locally

### Requirements

- **Unity 2022.3.62f3** (installed through Unity Hub, with the **Android Build Support** module including the Android SDK & NDK tools)
- A free **Unity account** (needed for Unity Gaming Services)
- Optional: an Android phone with USB debugging, to test the build

### Setup

1. **Clone the repository**
   ```
   git clone [PASTE REPOSITORY URL HERE]
   ```
2. **Open the project** in Unity Hub (**Add → Add project from disk**, select the cloned folder) using Unity **2022.3.62f3**. The first open takes a few minutes while packages import.
3. **Link your own Unity project ID:** go to **Edit → Project Settings → Services** and link the project to a Unity Cloud project you own. The original project ID in the repository will not work for you.
4. **Enable and set up Unity Gaming Services** on the [Unity Dashboard](https://cloud.unity.com) for your project:
   - **Authentication:** anonymous sign-in must be enabled (it is on by default).
   - **Leaderboards:** create a leaderboard with the ID **`snake_highscore`**, update strategy **Best score**. The original uses a monthly reset with archiving turned on, but that is optional.
   - **Cloud Save** and **Analytics:** enable both for the project.
5. **Google Mobile Ads plugin:** the project uses the Google Mobile Ads Unity Plugin (v11.3.0), included in the repository. If it's missing, import it from the [official GitHub releases](https://github.com/googleads/googleads-mobile-unity/releases). Set your AdMob App ID under **Assets → Google Mobile Ads → Settings**. The repository uses Google's sample test app and ad unit IDs, so the game runs without an AdMob account.
6. **Open the game scene** from `Assets/Scenes` and press **Play** to test in the Editor.

### Building the Android APK

1. **File → Build Settings**, select **Android**, and click **Switch Platform**.
2. Make sure the game scene is ticked in **Scenes In Build** and **Development Build** is off for a release build.
3. In **Project Settings → Player → Other Settings**: package name `com.MishalBhasim.SnakeGame`, **Minimum API Level 24**.
4. Click **Build**, or **Build And Run** with a phone connected by USB.

## Design patterns

- **Singleton:** every manager (`GameManager`, `UIManager`, `AdsManager`, `AuthManager`, `AchievementManager`, `LeaderboardManager`, `AnalyticsManager`, and the grid, food and snake controllers) exposes a static `Instance` with a duplicate guard in `Awake()`.
- **Observer / Event:** `GameManager` never calls the UI or services directly. It broadcasts events (`OnScoreChanged`, `OnGameOver`, `OnGameStarted`, `OnReviveOfferShown` and others), and `UIManager`, `AnalyticsManager`, `AchievementManager` and `LeaderboardManager` subscribe to them. Likewise `GameManager` listens to events from `SnakeController`, `FoodSpawner` and `AdsManager`. This keeps each feature decoupled: Analytics, Achievements and the Leaderboard were added without changing the game logic.
- **State:** `GameManager` uses an explicit `GameState` enum (`MainMenu`, `Playing`, `ReviveOffer`, `GameOver`) rather than loose booleans, and each event handler checks the state before acting.

Each script also has a single responsibility (SRP): one for input and movement, one for food, one for the grid, one per online service, and so on.

## Design decisions and tradeoffs

- **Singletons over dependency injection.** Using about ten Singletons is a deviation from the Dependency Inversion principle, because managers depend on concrete classes. For a solo, short-deadline project, this was a deliberate tradeoff against the extra complexity of a full dependency-injection setup in Unity.
- **Cloud Save instead of PlayerPrefs for achievements.** PlayerPrefs is easy for a player to edit and doesn't follow the player to a new device. Google Play Games achievements would be the usual choice, but they need a published Play Store listing, which this project doesn't have. Cloud Save with anonymous authentication gave server-side storage without that requirement.
- **Shared sign-in (`AuthManager`).** Achievements and the Leaderboard both need the player signed in. Letting each one sign in separately caused a race condition ("player is already signing in"), so a single `AuthManager` owns sign-in. It caches the in-flight task so concurrent callers share one attempt, and clears it on failure so a later call can retry.
- **AdMob instead of Unity LevelPlay.** LevelPlay initialization failed with a persistent error that could not be resolved, so the project switched to Google AdMob.
- **Test ad IDs on purpose.** The project uses Google's official test ad unit IDs. Clicking your own live ads violates AdMob policy and risks an account suspension, so test ads are the correct choice for development and demos. The real ad unit IDs are set up and are kept as comments in `AdsManager.cs`, ready to swap in for a real production release.
- **Revive keeps your snake.** After a revive, the snake keeps its length and score, and turns toward a free direction so it does not die again on the next move.
- **Leaderboard uses request/response, not events.** Submitting a score reacts to `OnGameOver` (event-driven), but fetching the leaderboard is a direct async call from the UI, since nothing else needs to react to a fetch.

## Offline behavior

- The local high score is stored on the device and always works.
- **Leaderboard:** if a score can't be sent (for example, no internet), it's kept as a "pending score" on the device and sent automatically on the next launch or game over. Only scores that failed to send are retried, and the local high score is never resubmitted, so the leaderboard's monthly reset stays correct.
- **Sign-in:** a failed sign-in is retried on the next request instead of failing until the app restarts.

## Known limitations

- Achievements unlocked while offline are not queued. If the app is closed before the connection returns, that unlock can be lost.
- A pending leaderboard score is sent on the next launch or game over, not in the background.
- The Leaderboard and Achievements screens are functional but minimally styled.
- Live ad revenue is not enabled, since the project uses test ad IDs.

## Tech stack

- Unity 2022.3.62f3, C#
- Google Mobile Ads Unity Plugin v11.3.0 (AdMob)
- Unity Gaming Services: Authentication, Cloud Save, Leaderboards, Analytics

## Author

Mishal Bhasim
