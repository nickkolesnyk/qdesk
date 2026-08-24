using System.Windows;

// Tells WPF where to look for theme resource dictionaries. These values are right for an
// application that ships its own styles rather than a reusable control library: no per-Windows-theme
// dictionaries, and the fallback dictionary lives in this assembly.
[assembly: ThemeInfo(
    themeDictionaryLocation: ResourceDictionaryLocation.None,
    genericDictionaryLocation: ResourceDictionaryLocation.SourceAssembly)]
