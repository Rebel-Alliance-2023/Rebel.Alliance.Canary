
### `Rebel.Alliance.Canary.SystemProviders` Overview

The `Rebel.Alliance.Canary.SystemProviders` namespace is designed to implement an in-memory actor system for the Canary architecture. It provides the necessary infrastructure to create, manage, and communicate with actors in a memory-based environment without relying on external services or frameworks. This setup is particularly useful for testing and development purposes.

### Key Components

#### 1. `InMemoryActorSystem`

- **Purpose**: Manages the lifecycle and interactions of in-memory actors.
- **Implements**: `IActorSystem` interface.
- **Details**:
  - Maintains a collection of actors using a `ConcurrentDictionary`.
  - Provides methods to create (`CreateActorAsync`), retrieve (`GetActorRefAsync`), activate (`ActivateActorAsync`), and deactivate actors (`DeactivateActorAsync`).
  - Utilizes `IActorStateManager` for state management and `IMediator` for handling messages.
  - **Usage**:
    - When an actor is created or activated, its state manager and mediator are initialized, and the actor's `OnActivateAsync` method is called.
    - Messages are sent to actors through the `SendMessageAsync` method, leveraging MediatR for message handling.

#### 2. `InMemoryActorRef<TActor>`

- **Purpose**: Provides a reference to an in-memory actor.
- **Implements**: `IActorRef` interface.
- **Details**:
  - Contains the actor instance and its unique identifier (`ActorId`).
  - Offers a `SendAsync` method to send messages to the actor by invoking its `ReceiveAsync` method.
- **Usage**:
  - Used by the `InMemoryActorSystem` to provide a reference to created or retrieved actors.

#### 3. `InMemoryActorStateManager`

- **Purpose**: Manages the state of actors in-memory.
- **Implements**: `IActorStateManager` interface.
- **Details**:
  - Provides methods to get (`GetStateAsync`), set (`SetStateAsync`), and clear (`ClearStateAsync`) the state of actors.
- **Usage**:
  - Used by actors to maintain and manipulate their state during their lifecycle.

#### 4. `InMemorySystemProvider`

- **Purpose**: Provides the core infrastructure for managing the in-memory actor system.
- **Implements**: `IActorSystemProvider` interface.
- **Details**:
  - Manages the creation, retrieval, and removal of actors.
  - Creates an instance of `InMemoryActorSystem` using the `CreateActorSystem` method.
  - Provides methods to create (`CreateActor`) and remove (`RemoveActor`) actors, initializing and activating them as necessary.
- **Usage**:
  - Serves as the entry point for initializing and interacting with the in-memory actor system.

#### 5. `InMemoryMessageRouter`

- **Purpose**: Facilitates message routing between actors within the in-memory system.
- **Details**:
  - Uses MediatR to manage and route messages between actors.
  - Ensures that messages reach their intended actor instances and are handled correctly.

### How the Components Work Together

1. **Actor Creation and Activation**:
   - When an actor is requested (`CreateActor`), `InMemorySystemProvider` checks if the actor already exists. If not, it creates and activates a new actor using `InMemoryActorSystem`.
   - The actor is initialized with a state manager (`InMemoryActorStateManager`) and a mediator (`IMediator`).

2. **Message Handling**:
   - Messages are sent to actors using `InMemoryActorSystem`, which utilizes MediatR to publish and handle these messages.
   - Each actor has a `ReceiveAsync` method that processes incoming messages.

3. **State Management**:
   - Actors use `InMemoryActorStateManager` to maintain their state. The state can be retrieved, updated, or cleared as needed.

4. **Actor References**:
   - `InMemoryActorRef` provides a reference to actors, allowing other components to send messages to them.

### Conclusion

The `Rebel.Alliance.Canary.SystemProviders` namespace provides a complete in-memory actor framework that is flexible, extensible, and integrates seamlessly with MediatR for message handling. This setup enables rapid development and testing of the Canary architecture without the need for external dependencies, making it ideal for development and local testing environments.