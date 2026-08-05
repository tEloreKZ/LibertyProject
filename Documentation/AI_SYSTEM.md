# AI System

**AI Architecture Documentation**

---

## Table of Contents

1. [Overview](#overview)
2. [AI Architecture](#ai-architecture)
3. [Behavior Trees](#behavior-trees)
4. [State Machines](#state-machines)
5. [Blackboard](#blackboard)
6. [Perception System](#perception-system)
7. [Decision Making](#decision-making)
8. [Goals and Actions](#goals-and-actions)
9. [Best Practices](#best-practices)

---

## Overview

The AI System provides a modern, extensible AI architecture using behavior trees, state machines, blackboards, and decision-making systems. The AI is designed for performance and flexibility.

### Architecture

```
┌─────────────────────────────────────────┐
│         AI Service                      │
│  (Main AI coordinator)                 │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Behavior Trees                 │
│  (Decision logic)                      │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         State Machines                 │
│  (State management)                    │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Blackboard                      │
│  (AI memory)                            │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Perception System               │
│  (Sensory input)                        │
└─────────────────────────────────────────┘
```

---

## AI Architecture

### Design Principles

1. **Modular**: Each AI component is independent
2. **Performance-Optimized**: Designed for game environments
3. **Data-Driven**: AI behavior defined in data, not code
4. **Reusable**: Components can be shared across entities
5. **Extensible**: Easy to add new behaviors

### System Boundaries

```
AI System
├── Knows: Behavior trees, state machines, perception
├── Doesn't Know: Game-specific logic (traffic stops, callouts)
├── Receives: Perception data, blackboard updates
└── Sends: Action requests, state changes
```

---

## Behavior Trees

### Behavior Tree Interface

```csharp
namespace ProjectLiberty.AI.BehaviorTree
{
    /// <summary>
    /// Behavior tree interface.
    /// </summary>
    public interface IBehaviorTree
    {
        /// <summary>
        /// Gets the root node.
        /// </summary>
        INode RootNode { get; }
        
        /// <summary>
        /// Gets the blackboard.
        /// </summary>
        IBlackboard Blackboard { get; }
        
        /// <summary>
        /// Executes the behavior tree.
        /// </summary>
        /// <returns>Execution result.</returns>
        NodeResult Execute();
        
        /// <summary>
        /// Resets the behavior tree.
        /// </summary>
        void Reset();
    }
}
```

### Node Interface

```csharp
namespace ProjectLiberty.AI.BehaviorTree
{
    /// <summary>
    /// Node interface.
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// Gets the node name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Executes the node.
        /// </summary>
        /// <param name="blackboard">AI blackboard.</param>
        /// <returns>Execution result.</returns>
        NodeResult Execute(IBlackboard blackboard);
        
        /// <summary>
        /// Resets the node.
        /// </summary>
        void Reset();
    }
}
```

### Node Result

```csharp
namespace ProjectLiberty.AI.BehaviorTree
{
    /// <summary>
    /// Node execution result.
    /// </summary>
    public enum NodeResult
    {
        Success,
        Failure,
        Running
    }
}
```

### Node Types

#### Composite Nodes

```csharp
namespace ProjectLiberty.AI.BehaviorTree.Nodes
{
    /// <summary>
    /// Sequence node - executes children in order until one fails.
    /// </summary>
    public class SequenceNode : CompositeNode
    {
        public override NodeResult Execute(IBlackboard blackboard)
        {
            foreach (var child in Children)
            {
                var result = child.Execute(blackboard);
                
                if (result == NodeResult.Failure)
                    return NodeResult.Failure;
                
                if (result == NodeResult.Running)
                    return NodeResult.Running;
            }
            
            return NodeResult.Success;
        }
    }
    
    /// <summary>
    /// Selector node - executes children in order until one succeeds.
    /// </summary>
    public class SelectorNode : CompositeNode
    {
        public override NodeResult Execute(IBlackboard blackboard)
        {
            foreach (var child in Children)
            {
                var result = child.Execute(blackboard);
                
                if (result == NodeResult.Success)
                    return NodeResult.Success;
                
                if (result == NodeResult.Running)
                    return NodeResult.Running;
            }
            
            return NodeResult.Failure;
        }
    }
}
```

#### Leaf Nodes

```csharp
namespace ProjectLiberty.AI.BehaviorTree.Nodes
{
    /// <summary>
    /// Condition node - checks a condition.
    /// </summary>
    public class ConditionNode : LeafNode
    {
        private readonly Func<IBlackboard, bool> _condition;
        
        public ConditionNode(string name, Func<IBlackboard, bool> condition)
        {
            Name = name;
            _condition = condition;
        }
        
        public override NodeResult Execute(IBlackboard blackboard)
        {
            return _condition(blackboard) ? NodeResult.Success : NodeResult.Failure;
        }
    }
    
    /// <summary>
    /// Action node - executes an action.
    /// </summary>
    public class ActionNode : LeafNode
    {
        private readonly Func<IBlackboard, NodeResult> _action;
        
        public ActionNode(string name, Func<IBlackboard, NodeResult> action)
        {
            Name = name;
            _action = action;
        }
        
        public override NodeResult Execute(IBlackboard blackboard)
        {
            return _action(blackboard);
        }
    }
}
```

---

## State Machines

### State Machine Interface

```csharp
namespace ProjectLiberty.AI.StateMachine
{
    /// <summary>
    /// State machine interface.
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>
        /// Gets the current state.
        /// </summary>
        IState CurrentState { get; }
        
        /// <summary>
        /// Adds a state.
        /// </summary>
        /// <param name="state">State to add.</param>
        void AddState(IState state);
        
        /// <summary>
        /// Adds a transition.
        /// </summary>
        /// <param name="fromState">From state.</param>
        /// <param name="toState">To state.</param>
        /// <param name="condition">Transition condition.</param>
        void AddTransition(string fromState, string toState, Func<IBlackboard, bool> condition);
        
        /// <summary>
        /// Sets the initial state.
        /// </summary>
        /// <param name="stateName">State name.</param>
        void SetInitialState(string stateName);
        
        /// <summary>
        /// Updates the state machine.
        /// </summary>
        /// <param name="blackboard">AI blackboard.</param>
        void Update(IBlackboard blackboard);
    }
}
```

### State Interface

```csharp
namespace ProjectLiberty.AI.StateMachine
{
    /// <summary>
    /// State interface.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Gets the state name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Called when entering the state.
        /// </summary>
        /// <param name="blackboard">AI blackboard.</param>
        void OnEnter(IBlackboard blackboard);
        
        /// <summary>
        /// Called when updating the state.
        /// </summary>
        /// <param name="blackboard">AI blackboard.</param>
        void OnUpdate(IBlackboard blackboard);
        
        /// <summary>
        /// Called when exiting the state.
        /// </summary>
        /// <param name="blackboard">AI blackboard.</param>
        void OnExit(IBlackboard blackboard);
    }
}
```

---

## Blackboard

### Blackboard Interface

```csharp
namespace ProjectLiberty.AI.Blackboard
{
    /// <summary>
    /// Blackboard interface for AI memory.
    /// </summary>
    public interface IBlackboard
    {
        /// <summary>
        /// Gets a value.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Key.</param>
        /// <returns>Value.</returns>
        T GetValue<T>(BlackboardKey<T> key);
        
        /// <summary>
        /// Sets a value.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        void SetValue<T>(BlackboardKey<T> key, T value);
        
        /// <summary>
        /// Checks if a key exists.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Key.</param>
        /// <returns>True if exists, false otherwise.</returns>
        bool HasKey<T>(BlackboardKey<T> key);
        
        /// <summary>
        /// Clears all values.
        /// </summary>
        void Clear();
    }
}
```

### Blackboard Key

```csharp
namespace ProjectLiberty.AI.Blackboard
{
    /// <summary>
    /// Blackboard key.
    /// </summary>
    public struct BlackboardKey<T>
    {
        public int Id { get; }
        
        public BlackboardKey(int id)
        {
            Id = id;
        }
    }
    
    /// <summary>
    /// Common blackboard keys.
    /// </summary>
    public static class BlackboardKeys
    {
        public static readonly BlackboardKey<Vector3> TargetPosition = new BlackboardKey<Vector3>(1);
        public static readonly BlackboardKey<int> TargetEntity = new BlackboardKey<int>(2);
        public static readonly BlackboardKey<bool> IsAlerted = new BlackboardKey<bool>(3);
        public static readonly BlackboardKey<float> AlertLevel = new BlackboardKey<float>(4);
        public static readonly BlackboardKey<DateTime> LastSeenTime = new BlackboardKey<DateTime>(5);
    }
}
```

---

## Perception System

### Perception Interface

```csharp
namespace ProjectLiberty.AI.Perception
{
    /// <summary>
    /// Perception system interface.
    /// </summary>
    public interface IPerceptionSystem
    {
        /// <summary>
        /// Gets the perception range.
        /// </summary>
        float PerceptionRange { get; }
        
        /// <summary>
        /// Checks if an entity is visible.
        /// </summary>
        /// <param name="observer">Observer entity.</param>
        /// <param name="target">Target entity.</param>
        /// <returns>True if visible, false otherwise.</returns>
        bool IsVisible(IPed observer, IPed target);
        
        /// <summary>
        /// Gets visible entities.
        /// </summary>
        /// <param name="observer">Observer entity.</param>
        /// <returns>Visible entities.</returns>
        IEnumerable<IPed> GetVisibleEntities(IPed observer);
        
        /// <summary>
        /// Checks if a sound is audible.
        /// </summary>
        /// <param name="observer">Observer entity.</param>
        /// <param name="soundPosition">Sound position.</param>
        /// <param name="soundVolume">Sound volume.</param>
        /// <returns>True if audible, false otherwise.</returns>
        bool IsAudible(IPed observer, Vector3 soundPosition, float soundVolume);
    }
}
```

---

## Decision Making

### Decision Maker Interface

```csharp
namespace ProjectLiberty.AI.DecisionMaking
{
    /// <summary>
    /// Decision maker interface.
    /// </summary>
    public interface IDecisionMaker
    {
        /// <summary>
        /// Makes a decision based on the blackboard.
        /// </summary>
        /// <param name="blackboard">AI blackboard.</param>
        /// <returns>Decision result.</returns>
        DecisionResult MakeDecision(IBlackboard blackboard);
    }
}
```

### Utility System

```csharp
namespace ProjectLiberty.AI.DecisionMaking
{
    /// <summary>
    /// Utility system for decision making.
    /// </summary>
    public class UtilitySystem
    {
        private readonly List<UtilityAction> _actions = new List<UtilityAction>();
        
        public void AddAction(UtilityAction action)
        {
            _actions.Add(action);
        }
        
        public UtilityAction SelectBestAction(IBlackboard blackboard)
        {
            UtilityAction bestAction = null;
            float bestScore = 0f;
            
            foreach (var action in _actions)
            {
                var score = action.CalculateUtility(blackboard);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestAction = action;
                }
            }
            
            return bestAction;
        }
    }
    
    /// <summary>
    /// Utility action.
    /// </summary>
    public class UtilityAction
    {
        public string Name { get; set; }
        public Func<IBlackboard, float> UtilityFunction { get; set; }
        public Action<IBlackboard> ExecuteAction { get; set; }
        
        public float CalculateUtility(IBlackboard blackboard)
        {
            return UtilityFunction(blackboard);
        }
    }
}
```

---

## Goals and Actions

### Goal Interface

```csharp
namespace ProjectLiberty.AI.Goals
{
    /// <summary>
    /// Goal interface.
    /// </summary>
    public interface IGoal
    {
        /// <summary>
        /// Gets the goal name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the goal priority.
        /// </summary>
        float Priority { get; }
        
        /// <summary>
        /// Checks if the goal is achieved.
        /// </summary>
        /// <param name="blackboard">AI blackboard.</param>
        /// <returns>True if achieved, false otherwise.</returns>
        bool IsAchieved(IBlackboard blackboard);
        
        /// <summary>
        /// Gets the actions to achieve the goal.
        /// </summary>
        /// <param name="blackboard">AI blackboard.</param>
        /// <returns>Actions to achieve goal.</returns>
        IEnumerable<IAction> GetActions(IBlackboard blackboard);
    }
}
```

### Action Interface

```csharp
namespace ProjectLiberty.AI.Goals
{
    /// <summary>
    /// Action interface.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Gets the action name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="blackboard">AI blackboard.</param>
        /// <returns>Action result.</returns>
        ActionResult Execute(IBlackboard blackboard);
        
        /// <summary>
        /// Checks if the action can be executed.
        /// </summary>
        /// <param name="blackboard">AI blackboard.</param>
        /// <returns>True if can execute, false otherwise.</returns>
        bool CanExecute(IBlackboard blackboard);
    }
}
```

### Action Result

```csharp
namespace ProjectLiberty.AI.Goals
{
    /// <summary>
    /// Action result.
    /// </summary>
    public enum ActionResult
    {
        Success,
        Failure,
        InProgress
    }
}
```

---

## Best Practices

### 1. Use Behavior Trees for Complex Logic

```csharp
// GOOD - Behavior tree for complex decisions
var tree = new BehaviorTree();
tree.RootNode = new SequenceNode
{
    Children = new List<INode>
    {
        new ConditionNode("IsEnemyVisible", bb => bb.GetValue<bool>(IsVisible)),
        new ActionNode("Attack", bb => Attack(bb))
    }
};

// BAD - Nested if statements
if (IsVisible)
{
    if (IsInRange)
    {
        if (HasAmmo)
        {
            Attack();
        }
    }
}
```

### 2. Use State Machines for Stateful Behavior

```csharp
// GOOD - State machine for states
var stateMachine = new StateMachine();
stateMachine.AddState(new IdleState());
stateMachine.AddState(new PatrolState());
stateMachine.AddState(new CombatState());

// BAD - Manual state tracking
switch (currentState)
{
    case "Idle":
        // Handle idle
        break;
    case "Patrol":
        // Handle patrol
        break;
}
```

### 3. Use Blackboard for Memory

```csharp
// GOOD - Blackboard for memory
_blackboard.SetValue(BlackboardKeys.LastSeenTime, DateTime.UtcNow);
var lastSeen = _blackboard.GetValue<DateTime>(BlackboardKeys.LastSeenTime);

// BAD - Individual fields
private DateTime _lastSeenTime;
```

### 4. Limit AI Update Frequency

```csharp
// GOOD - Reduced update frequency
private float _aiUpdateTimer;

public void Update(float deltaTime)
{
    _aiUpdateTimer += deltaTime;
    if (_aiUpdateTimer >= 0.1f) // 10 times per second
    {
        _aiUpdateTimer = 0;
        _ai.Update();
    }
}

// BAD - Update every frame
public void Update(float deltaTime)
{
    _ai.Update(); // Expensive
}
```

### 5. Use Spatial Partitioning for Perception

```csharp
// GOOD - Spatial partitioning
var visibleEntities = _spatialGrid.GetEntitiesInRadius(observer.Position, perceptionRange);

// BAD - Check all entities
var visibleEntities = _allEntities.Where(e => Vector3.Distance(e.Position, observer.Position) < perceptionRange);
```

### 6. Cache Behavior Trees

```csharp
// GOOD - Cache behavior tree
private static readonly IBehaviorTree _cachedTree = BuildBehaviorTree();

// BAD - Rebuild every time
public IBehaviorTree GetBehaviorTree()
{
    return BuildBehaviorTree(); // Expensive
}
```

### 7. Use Object Pooling for AI Components

```csharp
// GOOD - Object pooling
var blackboard = _blackboardPool.Get();
try
{
    // Use blackboard
}
finally
{
    _blackboardPool.Return(blackboard);
}

// BAD - Create new every time
var blackboard = new Blackboard();
```

---

## Summary

The AI System provides:

- **Behavior trees** for complex decision logic
- **State machines** for stateful behavior
- **Blackboard** for AI memory
- **Perception system** for sensory input
- **Decision making** for utility-based AI
- **Goals and actions** for goal-oriented behavior
- **Performance optimization** for game environments

The AI system is modular, performant, and extensible.
