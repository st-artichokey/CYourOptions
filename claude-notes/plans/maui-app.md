# Plan: CYourOptions — Decision Tree App

## Context
A .NET MAUI decision-tree app ("C-Your-Options" — a pun on C#). Built as both a C# learning project and a real application. The library holds platform-agnostic decision tree logic; the MAUI project provides the mobile/desktop UI.

## Approach

### Architecture
- **CYourOptions.Library** — shared code: models, decision engine, save system, stories
- **CYourOptions.App** — MAUI UI: pages, view models, platform-specific code
- **CYourOptions.Tests** — xUnit tests for the library
- MVVM pattern (Model-View-ViewModel) for clean separation of concerns

### Target platforms
- iOS (iPhone/iPad)
- Android
- Mac Catalyst (desktop)

### Tech stack
- **Runtime:** .NET 10
- **UI Framework:** .NET MAUI
- **Architecture pattern:** MVVM
- **Testing:** xUnit

## Status
- [x] Solution scaffolded
- [x] Class library: models, decision engine, save manager
- [x] Story content: "The Production Incident" (24 nodes, 5 endings)
- [x] Test suite: 27 tests passing
- [x] Project renamed to CYourOptions
- [ ] Xcode installed (needed for iOS/Mac builds)
- [ ] Android SDK installed (needed for Android builds)
- [ ] First successful MAUI build
- [ ] MAUI UI: game page with node display + choice buttons
- [ ] MAUI UI: save/load functionality
