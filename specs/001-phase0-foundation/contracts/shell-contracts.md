# Shell Contracts

Clean Architecture interfaces for the Phase 0 application shell.

## INavigationService

Provides navigation between views in the multi-tab workspace.

| Method | Description |
|--------|-------------|
| `NavigateTo(string targetId)` | Opens or activates a tab for the given navigation target |
| `RegisterTarget(string id, string displayName, Func<object> viewFactory)` | Registers a navigation target with its view factory |
| `GetActiveTarget()` | Returns the currently active target identifier |

## IThemeService

Manages application theme switching.

| Method | Description |
|--------|-------------|
| `SetTheme(string themeName)` | Switches to "Dark" or "Light" theme |
| `GetCurrentTheme()` | Returns the active theme name |
| `ThemeChanged` | Event raised when theme changes |

## ILocalizationService

Manages runtime language switching and resource lookup.

| Method | Description |
|--------|-------------|
| `SetLanguage(string cultureCode)` | Switches to "en" or "ar" |
| `GetString(string key)` | Returns localized string for the given key |
| `GetCurrentLanguage()` | Returns the active culture code |
| `IsRtl()` | Returns true if current language is right-to-left |
| `LanguageChanged` | Event raised when language changes |

## ISettingsService

Persists and retrieves user preferences.

| Method | Description |
|--------|-------------|
| `Load()` | Loads saved preferences from storage |
| `Save(UserPreferences preferences)` | Persists preferences to storage |
| `Get<T>(string key)` | Retrieves a typed setting value |
| `Set<T>(string key, T value)` | Stores a typed setting value |

## ILoggingService

Application-wide structured logging.

| Method | Description |
|--------|-------------|
| `Info(string message, params object[] args)` | Log at info level |
| `Error(string message, Exception ex, params object[] args)` | Log exception with context |
| `Warning(string message, params object[] args)` | Log at warning level |

## IDatabaseService

Database lifecycle management.

| Method | Description |
|--------|-------------|
| `InitializeAsync(CancellationToken ct)` | Creates/opens database and applies pending migrations |
| `GetConnection()` | Returns the active database connection factory |
| `IsInitialized()` | Returns true if database is ready for use |
