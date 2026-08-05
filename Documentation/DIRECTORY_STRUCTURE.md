# Directory Structure

**Complete Project Liberty Directory Structure**

---

## Root Directory

```
ProjectLiberty/
├── src/                          # Source code
├── SDK/                          # Software Development Kit
├── Documentation/                # Documentation files
├── Tests/                        # Test projects
├── Build/                        # Build scripts and tools
├── Languages/                   # Localization files
├── Config/                       # Configuration files
├── Plugins/                      # Installed plugins (runtime)
├── Scripts/                      # Installed scripts (runtime)
├── Saves/                        # Save data (runtime)
├── Logs/                         # Log files (runtime)
├── .windsurf/                    # Windsurf configuration
├── .gitignore                    # Git ignore file
├── README.md                     # Project README
├── LICENSE.md                    # License file
├── ProjectLiberty.sln            # Solution file
└── global.json                   # Global configuration
```

---

## Source Directory (src/)

```
src/
├── Core/                         # Core system
│   ├── ProjectLiberty.Core.csproj
│   ├── LibertyCore.cs            # Main entry point
│   ├── ServiceContainer.cs       # DI container
│   └── Application.cs            # Application lifecycle
│
├── Abstractions/                 # Interface definitions
│   ├── ProjectLiberty.Abstractions.csproj
│   ├── IGameHost.cs              # Game host interface
│   ├── INativeBridge.cs          # Native bridge interface
│   ├── IGameLoop.cs              # Game loop interface
│   ├── IEntityManager.cs         # Entity manager interface
│   ├── IPlayerManager.cs         # Player manager interface
│   ├── IVehicleManager.cs        # Vehicle manager interface
│   ├── IPedManager.cs            # Ped manager interface
│   ├── IWorldManager.cs          # World manager interface
│   ├── INativeInvoker.cs         # Native invoker interface
│   ├── IEventBus.cs              # Event bus interface
│   ├── IPlugin.cs                # Plugin interface
│   ├── IModule.cs                # Module interface
│   ├── ILocalization.cs          # Localization interface
│   ├── IConfiguration.cs         # Configuration interface
│   ├── ISaveSystem.cs            # Save system interface
│   ├── ILogger.cs                # Logger interface
│   └── Entities/                 # Entity interfaces
│       ├── IEntity.cs
│       ├── IPlayer.cs
│       ├── IVehicle.cs
│       ├── IPed.cs
│       └── IBlip.cs
│
├── GameHost/                     # Game host implementation
│   ├── ProjectLiberty.GameHost.csproj
│   ├── GameHost.cs               # Main game host
│   ├── ScriptHookHost.cs         # ScriptHookVDotNet implementation
│   ├── NativeBridge.cs           # Native bridge implementation
│   ├── GameLoop.cs               # Game loop implementation
│   └── NativeInvoker.cs          # Native invoker implementation
│
├── PluginSystem/                 # Plugin system
│   ├── ProjectLiberty.PluginSystem.csproj
│   ├── PluginLoader.cs           # Plugin loader
│   ├── PluginManager.cs          # Plugin manager
│   ├── PluginManifest.cs        # Plugin manifest
│   ├── PluginDependency.cs      # Plugin dependency
│   ├── DependencyResolver.cs     # Dependency resolver
│   ├── VersionChecker.cs         # Version compatibility
│   └── Lifecycle/
│       ├── PluginLifecycle.cs
│       └── LifecycleState.cs
│
├── EventBus/                     # Event system
│   ├── ProjectLiberty.EventBus.csproj
│   ├── EventBus.cs               # Event bus implementation
│   ├── EventSubscription.cs      # Event subscription
│   ├── EventHandler.cs          # Event handler base
│   ├── Events/                   # Event definitions
│   │   ├── CoreEvents.cs
│   │   ├── PlayerEvents.cs
│   │   ├── VehicleEvents.cs
│   │   ├── CalloutEvents.cs
│   │   ├── DispatchEvents.cs
│   │   ├── TrafficStopEvents.cs
│   │   └── ArrestEvents.cs
│   └── Filters/                  # Event filters
│       ├── EventFilter.cs
│       └── PriorityFilter.cs
│
├── Localization/                 # Localization system
│   ├── ProjectLiberty.Localization.csproj
│   ├── LocalizationService.cs    # Main localization service
│   ├── LanguageManager.cs        # Language manager
│   ├── TranslationLoader.cs      # Translation loader
│   ├── TranslationCache.cs       # Translation cache
│   └── Formatters/               # String formatters
│       ├── NumberFormatter.cs
│       ├── DateFormatter.cs
│       └── CurrencyFormatter.cs
│
├── Configuration/                # Configuration system
│   ├── ProjectLiberty.Configuration.csproj
│   ├── ConfigurationService.cs  # Main configuration service
│   ├── ConfigurationLoader.cs   # Configuration loader
│   ├── ConfigurationValidator.cs # Configuration validator
│   ├── ConfigurationWatcher.cs   # File watcher for hot reload
│   └── Schemas/                  # JSON schemas
│       ├── CoreSchema.json
│       ├── PluginSchema.json
│       └── SystemSchema.json
│
├── SaveSystem/                   # Save system
│   ├── ProjectLiberty.SaveSystem.csproj
│   ├── SaveService.cs            # Main save service
│   ├── SaveManager.cs            # Save manager
│   ├── SaveSerializer.cs         # Save serializer
│   ├── SaveValidator.cs          # Save validator
│   └── Slots/                    # Save slot management
│       ├── SaveSlot.cs
│       └── SlotManager.cs
│
├── Logging/                      # Logging system
│   ├── ProjectLiberty.Logging.csproj
│   ├── Logger.cs                 # Main logger
│   ├── LogEntry.cs               # Log entry
│   ├── LogWriter.cs              # Log writer
│   ├── LogFormatter.cs          # Log formatter
│   ├── LogLevel.cs               # Log levels
│   └── Outputs/                  # Log outputs
│       ├── FileOutput.cs
│       ├── ConsoleOutput.cs
│       └── DebugOutput.cs
│
├── Dispatch/                     # Dispatch system
│   ├── ProjectLiberty.Dispatch.csproj
│   ├── DispatchService.cs        # Main dispatch service
│   ├── DispatchManager.cs       # Dispatch manager
│   ├── RadioSystem.cs            # Radio system
│   ├── UnitManager.cs            # Unit management
│   ├── CallCodes/                # Dispatch codes
│   │   ├── CallCode.cs
│   │   ├── CallCodeRegistry.cs
│   │   └── DefaultCodes.cs
│   └── Audio/                    # Radio audio
│       ├── RadioAudio.cs
│       └── SoundManager.cs
│
├── TrafficStops/                 # Traffic stop system
│   ├── ProjectLiberty.TrafficStops.csproj
│   ├── TrafficStopService.cs     # Main traffic stop service
│   ├── TrafficStopManager.cs    # Traffic stop manager
│   ├── TrafficStop.cs            # Traffic stop entity
│   ├── Stages/                   # Traffic stop stages
│   │   ├── ObserveStage.cs
│   │   ├── PullOverStage.cs
│   │   ├── GreetingStage.cs
│   │   ├── DocumentsStage.cs
│   │   ├── QuestioningStage.cs
│   │   ├── DatabaseStage.cs
│   │   ├── CitationStage.cs
│   │   ├── SearchStage.cs
│   │   ├── ArrestStage.cs
│   │   └── ReleaseStage.cs
│   └── AI/                       # Traffic stop AI
│       ├── DriverBehavior.cs
│       └── StopAI.cs
│
├── Callouts/                     # Callout system
│   ├── ProjectLiberty.Callouts.csproj
│   ├── CalloutService.cs         # Main callout service
│   ├── CalloutManager.cs        # Callout manager
│   ├── Callout.cs                # Callout base class
│   ├── CalloutInfo.cs            # Callout metadata
│   ├── CalloutArea.cs            # Callout spawn area
│   ├── CalloutScanner.cs         # Callout scanner
│   └── Base/                     # Base callout implementations
│       ├── PursuitCallout.cs
│       ├── CrimeCallout.cs
│       └── AssistanceCallout.cs
│
├── ArrestSystem/                 # Arrest system
│   ├── ProjectLiberty.ArrestSystem.csproj
│   ├── ArrestService.cs          # Main arrest service
│   ├── ArrestManager.cs          # Arrest manager
│   ├── Arrest.cs                 # Arrest entity
│   ├── ArrestProcedure.cs        # Arrest procedure
│   ├── HandcuffSystem.cs         # Handcuff system
│   ├── TransportSystem.cs        # Prisoner transport
│   └── Booking/                  # Booking system
│       ├── BookingProcess.cs
│       └── BookingStation.cs
│
├── AI/                           # AI system
│   ├── ProjectLiberty.AI.csproj
│   ├── AIService.cs              # Main AI service
│   ├── BehaviorTree/             # Behavior tree implementation
│   │   ├── BehaviorTree.cs
│   │   ├── Node.cs
│   │   ├── CompositeNode.cs
│   │   ├── DecoratorNode.cs
│   │   ├── LeafNode.cs
│   │   └── Nodes/
│   │       ├── SequenceNode.cs
│   │       ├── SelectorNode.cs
│   │       ├── ParallelNode.cs
│   │       ├── ConditionNode.cs
│   │       └── ActionNode.cs
│   ├── StateMachine/             # State machine
│   │   ├── StateMachine.cs
│   │   ├── State.cs
│   │   ├── Transition.cs
│   │   └── Condition.cs
│   ├── Blackboard/               # Blackboard for AI memory
│   │   ├── Blackboard.cs
│   │   ├── BlackboardKey.cs
│   │   └── Memory.cs
│   ├── Perception/               # AI perception
│   │   ├── PerceptionSystem.cs
│   │   ├── SightSensor.cs
│   │   ├── HearingSensor.cs
│   │   └── MemorySystem.cs
│   ├── DecisionMaking/           # Decision making
│   │   ├── DecisionMaker.cs
│   │   ├── UtilitySystem.cs
│   │   └── GoalSystem.cs
│   └── Goals/                    # AI goals
│       ├── Goal.cs
│       ├── GoalQueue.cs
│       └── Actions/
│           ├── Action.cs
│           └── ActionQueue.cs
│
├── EntityManager/                # Entity manager
│   ├── ProjectLiberty.EntityManager.csproj
│   ├── EntityManager.cs          # Entity manager implementation
│   ├── EntityPool.cs             # Entity pooling
│   ├── EntityTracker.cs          # Entity tracking
│   └── Components/               # Entity components
│       ├── TransformComponent.cs
│       ├── HealthComponent.cs
│       └── MetadataComponent.cs
│
├── PlayerManager/                # Player manager
│   ├── ProjectLiberty.PlayerManager.csproj
│   ├── PlayerManager.cs          # Player manager implementation
│   ├── Player.cs                 # Player entity
│   ├── PlayerState.cs            # Player state
│   ├── PlayerActions.cs         # Player actions
│   └── LocalPlayer.cs            # Local player
│
├── VehicleManager/               # Vehicle manager
│   ├── ProjectLiberty.VehicleManager.csproj
│   ├── VehicleManager.cs         # Vehicle manager implementation
│   ├── Vehicle.cs                # Vehicle entity
│   ├── VehicleState.cs           # Vehicle state
│   ├── VehiclePool.cs            # Vehicle pooling
│   └── Models/                   # Vehicle models
│       ├── VehicleModel.cs
│       └── ModelLoader.cs
│
├── PedManager/                   # Ped manager
│   ├── ProjectLiberty.PedManager.csproj
│   ├── PedManager.cs             # Ped manager implementation
│   ├── Ped.cs                    # Ped entity
│   ├── PedState.cs               # Ped state
│   ├── PedPool.cs                # Ped pooling
│   └── Models/                   # Ped models
│       ├── PedModel.cs
│       └── ModelLoader.cs
│
├── WorldManager/                 # World manager
│   ├── ProjectLiberty.WorldManager.csproj
│   ├── WorldManager.cs           # World manager implementation
│   ├── WaypointSystem.cs         # Waypoint system
│   ├── BlipSystem.cs             # Blip system
│   ├── WeatherSystem.cs          # Weather system
│   ├── TimeSystem.cs             # Time system
│   └── ZoneSystem.cs             # Zone system
│
├── NativeInvoker/                # Native invoker
│   ├── ProjectLiberty.NativeInvoker.csproj
│   ├── NativeInvoker.cs          # Native invoker implementation
│   ├── NativeCache.cs            # Native hash cache
│   ├── NativePool.cs             # Native call pooling
│   └── Hashes/                   # Native hashes
│       ├── NativeHash.cs
│       └── HashGenerator.cs
│
├── Scripting/                    # Scripting system
│   ├── ProjectLiberty.Scripting.csproj
│   ├── ScriptEngine.cs           # Script engine
│   ├── ScriptLoader.cs           # Script loader
│   ├── ScriptCompiler.cs         # Script compiler
│   ├── ScriptContext.cs          # Script execution context
│   └── API/                      # Scripting API
│       ├── ScriptAPI.cs
│       ├── PlayerAPI.cs
│       ├── VehicleAPI.cs
│       ├── WorldAPI.cs
│       └── DispatchAPI.cs
│
├── HUD/                          # HUD system
│   ├── ProjectLiberty.HUD.csproj
│   ├── HUDService.cs             # Main HUD service
│   ├── HUDRenderer.cs            # HUD renderer
│   ├── Elements/                 # HUD elements
│   │   ├── TextElement.cs
│   │   ├── ProgressBar.cs
│   │   ├── SpriteElement.cs
│   │   └── ContainerElement.cs
│   └── Layouts/                  # HUD layouts
│       ├── DefaultLayout.cs
│       └── MinimalLayout.cs
│
├── Menus/                        # Menu system
│   ├── ProjectLiberty.Menus.csproj
│   ├── MenuService.cs            # Main menu service
│   ├── Menu.cs                   # Menu base class
│   ├── MenuItem.cs               # Menu item
│   ├── MenuRenderer.cs           # Menu renderer
│   └── Types/                    # Menu types
│       ├── UIMenu.cs
│       ├── ContextMenu.cs
│       └── SubMenu.cs
│
├── Commands/                     # Command system
│   ├── ProjectLiberty.Commands.csproj
│   ├── CommandService.cs         # Main command service
│   ├── CommandParser.cs          # Command parser
│   ├── CommandRegistry.cs        # Command registry
│   ├── Command.cs                # Command base class
│   └── BuiltIn/                  # Built-in commands
│       ├── HelpCommand.cs
│       ├── ReloadCommand.cs
│       └── StatusCommand.cs
│
├── Evidence/                     # Evidence system
│   ├── ProjectLiberty.Evidence.csproj
│   ├── EvidenceService.cs       # Main evidence service
│   ├── EvidenceManager.cs        # Evidence manager
│   ├── Evidence.cs               # Evidence entity
│   ├── EvidenceCollector.cs      # Evidence collection
│   └── Types/                    # Evidence types
│       ├── PhysicalEvidence.cs
│       ├── DigitalEvidence.cs
│       └── WitnessEvidence.cs
│
├── Crimes/                       # Crime system
│   ├── ProjectLiberty.Crimes.csproj
│   ├── CrimeService.cs           # Main crime service
│   ├── CrimeManager.cs           # Crime manager
│   ├── Crime.cs                  # Crime entity
│   ├── CrimeDetection.cs         # Crime detection
│   └── Types/                    # Crime types
│       ├── ViolentCrime.cs
│       ├── PropertyCrime.cs
│       └── TrafficCrime.cs
│
├── Missions/                     # Mission system
│   ├── ProjectLiberty.Missions.csproj
│   ├── MissionService.cs        # Main mission service
│   ├── MissionManager.cs        # Mission manager
│   ├── Mission.cs                # Mission base class
│   ├── Objective.cs              # Mission objective
│   └── Types/                    # Mission types
│       ├── PatrolMission.cs
│       ├── InvestigationMission.cs
│       └── TacticalMission.cs
│
├── Scenarios/                    # Scenario system
│   ├── ProjectLiberty.Scenarios.csproj
│   ├── ScenarioService.cs        # Main scenario service
│   ├── ScenarioManager.cs       # Scenario manager
│   ├── Scenario.cs               # Scenario base class
│   ├── ScenarioNode.cs           # Scenario node
│   └── Conditions/               # Scenario conditions
│       ├── TimeCondition.cs
│       ├── LocationCondition.cs
│       └── StateCondition.cs
│
└── UI/                           # UI system
    ├── ProjectLiberty.UI.csproj
    ├── UIService.cs              # Main UI service
    ├── UIManager.cs              # UI manager
    ├── Window.cs                 # Window base class
    ├── Controls/                 # UI controls
    │   ├── Button.cs
    │   ├── TextBox.cs
    │   ├── CheckBox.cs
    │   ├── Slider.cs
    │   └── DropDownList.cs
    └── Themes/                   # UI themes
        ├── Theme.cs
        └── DefaultTheme.cs
```

---

## SDK Directory (SDK/)

```
SDK/
├── ProjectLiberty.SDK.csproj     # SDK project
├── Plugins/                      # Plugin SDK
│   ├── IPlugin.cs                # Plugin interface
│   ├── PluginBase.cs             # Plugin base class
│   ├── PluginContext.cs          # Plugin context
│   ├── PluginAttributes.cs       # Plugin attributes
│   └── Examples/
│       ├── BasicPlugin.cs
│       ├── CalloutPlugin.cs
│       └── SystemPlugin.cs
├── Callouts/                     # Callout SDK
│   ├── ICallout.cs               # Callout interface
│   ├── CalloutBase.cs            # Callout base class
│   ├── CalloutContext.cs         # Callout context
│   ├── CalloutAttributes.cs      # Callout attributes
│   └── Examples/
│       ├── PursuitCallout.cs
│       ├── RobberyCallout.cs
│       └── TrafficStopCallout.cs
├── Scripts/                      # Scripting SDK
│   ├── ScriptAPI.cs              # Script API reference
│   ├── ScriptContext.cs          # Script context
│   ├── ScriptTypes.cs            # Script types
│   └── Examples/
│       ├── BasicScript.cs
│       ├── PatrolScript.cs
│       └── ScenarioScript.cs
├── Templates/                    # Project templates
│   ├── PluginTemplate/
│   │   ├── PluginTemplate.csproj
│   │   └── MyPlugin.cs
│   ├── CalloutTemplate/
│   │   ├── CalloutTemplate.csproj
│   │   └── MyCallout.cs
│   └── ScriptTemplate/
│       ├── ScriptTemplate.csproj
│       └── MyScript.cs
└── Documentation/                 # SDK documentation
    ├── GettingStarted.md
    ├── PluginDevelopment.md
    ├── CalloutDevelopment.md
    ├── ScriptingGuide.md
    └── APIReference.md
```

---

## Documentation Directory (Documentation/)

```
Documentation/
├── README.md                     # This file
├── ARCHITECTURE.md               # Architecture documentation
├── PROJECT_RULES.md              # Project rules
├── CODING_STANDARDS.md           # Coding standards
├── DIRECTORY_STRUCTURE.md        # Directory structure
├── EVENT_BUS.md                  # Event bus documentation
├── PLUGIN_SDK.md                 # Plugin SDK documentation
├── SCRIPTING_API.md              # Scripting API documentation
├── LOCALIZATION.md               # Localization documentation
├── CONFIGURATION.md              # Configuration documentation
├── SAVE_SYSTEM.md                # Save system documentation
├── DISPATCH.md                   # Dispatch system documentation
├── TRAFFIC_STOPS.md              # Traffic stops documentation
├── CALL_OUTS.md                  # Callouts documentation
├── ARREST_SYSTEM.md              # Arrest system documentation
├── AI_SYSTEM.md                  # AI system documentation
├── PERFORMANCE.md                # Performance guidelines
├── ROADMAP.md                    # Development roadmap
├── AGENT.md                      # AI agent documentation
├── CONTRIBUTING.md               # Contributing guidelines
└── LICENSE.md                    # License information
```

---

## Tests Directory (Tests/)

```
Tests/
├── ProjectLiberty.Tests.csproj   # Test project
├── Unit/                         # Unit tests
│   ├── Core/
│   │   ├── ServiceContainerTests.cs
│   │   └── ApplicationTests.cs
│   ├── EventBus/
│   │   ├── EventBusTests.cs
│   │   └── EventSubscriptionTests.cs
│   ├── PluginSystem/
│   │   ├── PluginLoaderTests.cs
│   │   └── DependencyResolverTests.cs
│   ├── Localization/
│   │   ├── LocalizationServiceTests.cs
│   │   └── TranslationLoaderTests.cs
│   └── Configuration/
│       ├── ConfigurationServiceTests.cs
│       └── ConfigurationValidatorTests.cs
├── Integration/                  # Integration tests
│   ├── ManagerIntegration/
│   │   ├── PlayerManagerIntegrationTests.cs
│   │   └── VehicleManagerIntegrationTests.cs
│   ├── PluginIntegration/
│   │   ├── PluginLifecycleTests.cs
│   │   └── PluginCommunicationTests.cs
│   └── SystemIntegration/
│       ├── DispatchIntegrationTests.cs
│       └── CalloutIntegrationTests.cs
└── System/                      # System tests
    ├── EndToEnd/
    │   ├── FullScenarioTests.cs
    │   └── PerformanceTests.cs
    └── Stress/
        ├── LoadTests.cs
        └── MemoryTests.cs
```

---

## Build Directory (Build/)

```
Build/
├── Scripts/                      # Build scripts
│   ├── build.ps1                 # Main build script
│   ├── clean.ps1                 # Clean script
│   ├── test.ps1                  # Test script
│   ├── package.ps1               # Package script
│   └── release.ps1              # Release script
├── Tools/                        # Build tools
│   ├── dotnet-format             # Code formatter
│   └── csharpier                 # Style formatter
└── Configuration/                # Build configuration
    ├── Debug.json
    ├── Release.json
    └── Package.json
```

---

## Languages Directory (Languages/)

```
Languages/
├── en/                           # English
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── ru/                           # Russian
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── fr/                           # French
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── de/                           # German
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── es/                           # Spanish
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── pt/                           # Portuguese
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── it/                           # Italian
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── pl/                           # Polish
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── uk/                           # Ukrainian
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── zh/                           # Chinese
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
├── ja/                           # Japanese
│   ├── core.json
│   ├── dispatch.json
│   ├── trafficstops.json
│   ├── callouts.json
│   ├── arrests.json
│   └── ui.json
└── ko/                           # Korean
    ├── core.json
    ├── dispatch.json
    ├── trafficstops.json
    ├── callouts.json
    ├── arrests.json
    └── ui.json
```

---

## Config Directory (Config/)

```
Config/
├── core.json                     # Core configuration
├── plugins.json                  # Plugin configuration
├── dispatch.json                 # Dispatch configuration
├── trafficstops.json             # Traffic stops configuration
├── callouts.json                 # Callouts configuration
├── arrests.json                 # Arrest system configuration
├── ai.json                       # AI configuration
├── performance.json              # Performance settings
└── logging.json                  # Logging configuration
```

---

## Runtime Directories

These directories are created at runtime and should not be committed to version control.

```
Plugins/                          # Installed plugins (runtime)
├── MyPlugin.dll
├── MyCallout.dll
└── MyScript.dll

Scripts/                          # Installed scripts (runtime)
├── patrol.lua
├── scenario.lua
└── custom.lua

Saves/                           # Save data (runtime)
├── slot1.json
├── slot2.json
└── slot3.json

Logs/                            # Log files (runtime)
├── liberty_2024-01-01.log
├── liberty_2024-01-02.log
└── liberty_error.log
```

---

## .windsurf Directory

```
.windsurf/
└── workflows/                    # Windsurf workflows
    ├── build.md
    ├── test.md
    └── release.md
```

---

## File Naming Conventions

### Source Files
- Classes: `PascalCase.cs` (e.g., `PlayerManager.cs`)
- Interfaces: `IPascalCase.cs` (e.g., `IPlayerManager.cs`)
- Enums: `PascalCase.cs` (e.g., `VehicleModel.cs`)

### Documentation Files
- All documentation: `UPPER_CASE.md` (e.g., `ARCHITECTURE.md`)

### Configuration Files
- All configuration: `lowercase.json` (e.g., `core.json`)

### Localization Files
- Language code: `lowercase/` (e.g., `en/`, `ru/`)
- Files: `lowercase.json` (e.g., `core.json`)

---

## Summary

This directory structure provides:

- **Clear Separation**: Each system has its own directory
- **Scalability**: Easy to add new systems
- **Maintainability**: Easy to find and modify code
- **Modularity**: Each module is self-contained
- **SDK Ready**: Clear SDK structure for developers
- **Test Ready**: Comprehensive test structure
- **Documentation**: Complete documentation organization

All directories follow consistent naming conventions and are organized by responsibility.
