# BrokenBricksECS - Entity Component System for C# and Unity3D
This ECS System is a rebuild of Unity3D upcoming Entity Component System.
Special thanks to Joachim Ante from Unity3D and his whole team! They inspired me to rebuild their system.
Because I could not wait to use it!

---

## What's the unique thing to that System and why should you use BrokenBicksECS?
* Huge speedup in relation to the (legacy) Unity System
* Creating entities from Prefabs
* Unity MonoBehaviour integration
* Easy to maintain and to extend
* System debugging like Entitas ECS
* Inversion of control (dependency injection)
* No Garbage Collecting
* Tuple Injection for multiple components in systems (only entities that matches all tuples)
* Easy to use components, supports structs and classes
* Easy to extend for multithreaded Systems (but not yet implemented)
* Fast system and component creation through templates
* Supports data prefetching (fetches data before it is needed)
* Supports different contexts for entity (just by creating new classes of RootSystems and EntityManagers)
* No Object pooling required
* and much more

There is even more!
BrokenBicksECS isn't made just for Unity. The Core Framework is pure C# code without any relation to Unity3d or any other system.
The Unity part is just build on top of it!

---

## Original ECS approach by Unity3d

[![Unite Austin 2017](https://img.youtube.com/vi/tGmnZdY5Y-E/default.jpg)](http://www.youtube.com/watch?v=tGmnZdY5Y-E)

---

## So what is ECS exactly:
ECS stands for Entity Component System. Basically you have entities. An Entity will be defined by its components, which hold the data. And this is important! Components ONLY holding Data, no functions at all!
The functionality comes from the different systems, which will act on the data.
And each system should do only one thing. E.g. update Positions, handle Damages, and so on
Systems always acts over all entities with the desired component on it (e.g. all Entities with the position component.)

By the fact that systems handles the behaviour, you can just add a new system or remove one to add or remove a feature. The same for components. Your player character shouldn't move anymore because of something that happened in the game. Remove isMoveable component from the player and catch it in the PlayerInputSystem. Done.

Something like that.

---

## Quick Start

### Controller Executes all the systems
```csharp
namespace ECS {
 
    // Use this class to control the ECS System
    public class GameController : ECSController<UnityStandardSystemRoot, UnityEntityManager> {
 
        // Use this for initialization
        protected override void Initialize() {
            AddSystem<MySystem>();
 
            // or you can use this for more control over composition of systems
            //AddSystem(new MySystem());
        }
 
        // Override this function if you want to control what Scene Entity should be load in this context
        // The base implementation will add all Scene Entities to the context
        //protected override void AddSceneEntitiesToSystem() {
   
        //}
    }
}
```
It creates the system and the entity manager by dependency injection for you
With the AddSystem methods you can assign your systems to the ECSSystemRoot.
The ECSController also executes the Start, Update, and FixedUpdate Routines of the Systems

### An ECS Component
```csharp
namespace ECSExample {
    [Serializable]
    public struct FloatComponent : IComponent {
        public float value;
 
        public FloatComponent(float value) {
            this.value = value;
        }
    }
 
    public class FloatDataComponent : ComponentDataWrapper<FloatComponent> { }
}
```

### Initialize Entity System
```csharp
namespace ECSExample {
    [DebugSystemGroup("Init")]
    class InitEntitiesSystem : ComponentSystem {
 
        private GameObject _gameObject;
 
        public override void OnStart() {
            _gameObject = new GameObject("Entities");
            for (int i = 0; i < 1000; i++) {
                Entity entity = EntityManager.CreateEntity();
 
                GameObject gameObject = new GameObject("Entity-" + i);
                gameObject.transform.SetParent(_gameObject.transform);
 
                GameObjectEntity goEntity = gameObject.AddComponent<GameObjectEntity>();
                goEntity.SetEntity(entity, EntityManager);
                EntityManager.AddComponent(entity, new FloatComponent(1f));
            }
        }
    }
}
```
### Update float component of all entities 

```csharp
namespace ECSExample {
    [DebugSystemGroup("Update")]
    class UpdateFloatSystem : ComponentSystem {
 
        [InjectTuple]
        private ComponentArray<FloatComponent> floats;
        public override void OnUpdate() {
            float sum = 0;
            for (int i = 0; i < floats.Length; i++) {
                EntityManager.SetComponent(floats.GetEntity(i), new FloatComponent(floats[i].value + 1));
            }
        }
    }
}
```

### Accessing Components in Systems
```csharp
//This class see only Enities with ComponentA and B attached to it
class MySystem : ComponentSystem {
    [InjectTuple]
    ComponentArray<ComponentA> componentA;
 
    [InjectTuple]
    ComponentArray<ComponentB> componentB;
}
 
// if you want to manualy will filter components use the following:
 
ComponentGroup group = m_EntityManager.GetComponentGroup(typeof(ComponentA), typeof(ComponentB),...)
 
ComponentArray<ComponentA> compA = group.GetComponentArray<ComponentA>();
ComponentArray<ComponentA> compB = group.GetComponentArray<ComponentB>();
```

### Instantiate from Prefab without instantiating the GameObject
```csharp
 struct Position : IComponent {
    public Vector3 position;
    public Quaternion rotation;
}
 
PositionComponent : ComponentDataWrapper<Position>{ }
 
class Spawner : Monobehaviour {
    public GameObject prefab;
 
    [InjectDependency]
    UnityEntityManager _entityManager;
 
    void Awake() {
        InjectionManager.ResolveObject(this);
    }
 
   void Start() {
        // Instantiate the prefab with all its components attached to it
        Entity entity = _entityManager.Instantiate(prefab);
 
        // just update the position component
        var position = new Position(Vector3.zero, Quaternion.identity);
        entityManager.SetComponent(entity, position);
   }
 
}
```

### Instantiate Entities from Prefab with instantiating the GameObject
```csharp
 public class PrefabSpawner : ScriptBehaviour {
        public GameObject _prefab;
 
        [InjectDependency]
        private UnityEntityManager _entityManager;
 
        // Use this for initialization
        void Start() {
            GameObject gameObject = _entityManager.InstantiateWithGameObject(_prefab, transform.position, transform.rotation);
        }
    }
```

### Components that support Unity Component
```csharp
    [Serializable]
    public class ECSTransform : IComponent, ICloneable {
        public Transform transform;
 
        public object Clone() {
            return MemberwiseClone();
        }
    }
 
    [HideInInspector] // dont show inner component in Inspector
    public class ECSTransformComponent : ComponentWrapper<ECSTransform> {
        // This will assign the Unity Component to the ECS Component
        public override void Initialize() {
            TypedComponent.transform = gameObject.transform;
        }
    }
```

### Creating of different Contexts
```csharp
//context for Cars
class CarEntityManager : EntityManager{}
class CarRootSystem :  UnityRootSystem<CarEntityManager>{}
 
 
//context for Humans
class HumanEntityManager : EntityManager{}
class HumanRootSystem : UnityRootSystem<HumanEntityManager>{}
 
 
//usage
class Controllers : Monobehaviour {
       CarRootSystem carSystem;
       HumanRootSystem humanSystem;
 
       void Awake() {
            carSystem = new CarRootSystem();
            humanSystem = new HumanRootSystem();
 
            //... add systems to the rootsystems
      }
 
      void Start() {
            carSystem.Start();
            humanSystem.Start();
     }
 
      void Update() {
            carSystem.Update();
            humanSystem.Update();
     }
 
 
      void FixedUpdate() {
            carSystem.FixedUpdate();
            humanSystem.FixedUpdate();
     }
}
```
This will enforce that you will separate entities, components and systems
Systems of context Car only knows entities of that system and so on.
If you want to communicate with the other Context use the EntityManager of that context

### Event System
Events called only for specific subscribers

For example you can listen to component changed events for a specific component on a specific entity:

```csharp

public class MyClass : ComponentSystem, IComponentChangedEventListener<ComponentA>
{
    [InjectTuple]
    ComponentArray<ComponentA> componentAArr;

    public override void OnStart() {
        for(int i=0; i < componentAArr.Length; i++){
            EntityManager.SubscribeComponentChanged(componentAArr.GetEntity(i), this);
        }
    }

    public void OnComponentChanged(object sender, Entity entity, ComponentA component) {
        //...
    }
}
```

There are more events available, check out ComponentGroup, ComponentArray and EntityManager classes for more information.

### IOC:
it works with any class you want.
Just call InjectionManager.Resolve<myClass>() or if the object already exist use InjectionManager.ResolveDependency(my object)

An injectable class should have the [InjectableDependency] Attribute on it
And to inject some object to your Fields and Props use [InjectDependency] Attribute
The system will automatically recognize dependencies of constructors

Example:
```csharp
[InjectableDependency(LifeTime.PerInstance)]
class MyClass {
... some code
}
 
class SecondClass {
    [InjectDependency]
    private MyClass myClassInstance;
}
 
class ThirdClass {
    public ThirdClass(MyClass myClassInstance) {
        ... some code ....
   }
}
 
... somewhere in the programm
 
InjectionManager.Resolve<SecondClass>();
InjectionManager.Resolve<Thirdclass>();
 
// or
SecondClass mySecClassInstance = new SecondClass();
InjectionManager.ResolveDependency(mySecClassInstance);
```

If you want to use Dependency Injection in a Monobehaviour class,
just use ScriptBehaviour instead of Monobehaviour.
### Accessing Components in Systems
```csharp
public class SomeMonoBehaviourClass: ScriptBehaviour {
 
        [InjectDependency]
        private UnityEntityManager _entityManager;
 
}
```
