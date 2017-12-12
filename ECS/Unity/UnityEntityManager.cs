using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;

namespace ECS {
    public class InstantiateGameObjectException : Exception {
        public InstantiateGameObjectException(string message) : base(message) {
        }
    }


    [InjectableDependency(LifeTime.Singleton)]
    public partial class UnityEntityManager : EntityManager {
        /// <summary>
        /// Instantiates only Entities with there Components from Prefab! 
        /// Wont Instantiate GameObject itself!
        /// Use InstantiateWithGameObject instead.
        /// </summary>
        public void Instantiate(GameObject prefab, NativeArray<Entity> entityArray) {
            var gameObjectEntity = prefab.GetComponent<GameObjectEntity>();
            if (!gameObjectEntity) {
                throw new Exception(prefab.name + " cant be instantiate without " + typeof(GameObjectEntity).Name + " component");
            }

            ComponentWrapper[] components = gameObjectEntity.GetComponents();

            for (int i = 0; i < entityArray.Length; i++) {
                entityArray[i] = CreateEntity();
            }

            for (int i = 0; i < components.Length; i++) {
                for (int j = 0; j < entityArray.Length; j++) {
                    components[i].AddComponentToEntity(entityArray[j], this);
                }
            }
        }

        /// <summary>
        /// Instantiates only Entity with it's Components from Prefab! 
        /// Wont Instantiate GameObject itself!
        /// Use InstantiateWithGameObject instead.
        /// </summary>
        public Entity Instantiate(GameObject prefab) {
            var gameObjectEntity = prefab.GetComponent<GameObjectEntity>();
            if (!gameObjectEntity) {
                throw new Exception(prefab.name + " cant be instantiate without " + typeof(GameObjectEntity).Name + " component");
            }
            Entity entity = CreateEntity();
            ComponentWrapper[] components = gameObjectEntity.GetComponents();
            for (int i = 0; i < components.Length; i++) {
                components[i].AddComponentToEntity(entity, this);
            }
            return entity;
        }

        /// <summary>
        /// Instantiates Entity and GameObject with it's Components from Prefab!
        /// </summary>
        public GameObject InstantiateWithGameObject(GameObject prefab) {
            Entity entity = CreateEntity();
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
            gameObject.GetComponent<GameObjectEntity>().SetEntity(entity, this);
            return gameObject;
        }

        /// <summary>
        /// Instantiates Entity and GameObject with it's Components from Prefab!
        /// </summary>
        public GameObject InstantiateWithGameObject(GameObject prefab, Transform parent, bool instantiateInWorldSpace = false) {
            Entity entity = CreateEntity();
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab, parent, instantiateInWorldSpace);
            gameObject.GetComponent<GameObjectEntity>().SetEntity(entity, this);
            return gameObject;
        }

        /// <summary>
        /// Instantiates Entity and GameObject with it's Components from Prefab!
        /// </summary>
        public GameObject InstantiateWithGameObject(GameObject prefab, Vector3 position, Quaternion rotation) {
            Entity entity = CreateEntity();
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation);
            gameObject.GetComponent<GameObjectEntity>().SetEntity(entity, this);
            return gameObject;
        }

        /// <summary>
        /// Instantiates Entity and GameObject with it's Components from Prefab!
        /// </summary>
        public GameObject InstantiateWithGameObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent) {
            Entity entity = CreateEntity();
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            gameObject.GetComponent<GameObjectEntity>().SetEntity(entity, this);
            return gameObject;
        }



        /// <summary>
        /// Instantiates only Entities with there Components from Prefab! 
        /// Wont Instantiate GameObject itself!
        /// Use InstantiateWithGameObject instead.
        /// </summary>
        public void InstantiateWithGameObject(GameObject prefab, NativeArray<Entity> entityArray, NativeArray<GameObject> gameObjectArray) {
            if (gameObjectArray.Length != entityArray.Length) {
                throw new InstantiateGameObjectException("entityArray and gameObjectArray must be the same size");
            }

            for (int i = 0; i < entityArray.Length; i++) {
                entityArray[i] = CreateEntity();
                gameObjectArray[i] = UnityEngine.Object.Instantiate(prefab);
                gameObjectArray[i].GetComponent<GameObjectEntity>().SetEntity(entityArray[i], this);
            }
        }
    }

#if (UNITY_EDITOR && ECS_DEBUG)
    public partial class UnityEntityManager : EntityManager {

        private List<EntityInfo> entityInfoList = new List<EntityInfo>();

        public int TotalEntities { get { return _entities.Count; } }
        public int TotalComponentTypes { get { return _components.Count; } }

        public List<EntityInfo> EntityList { get { return entityInfoList; } }

        public override Entity CreateEntity() {
            Entity entity = base.CreateEntity();
            entityInfoList.Add(entity);
            return entity;
        }
        public override void DestroyEntity(Entity entity) {
            base.DestroyEntity(entity);
            entityInfoList.Remove(entity);
        }
    }
    
    public class EntityInfo : IEquatable<Entity> {
        public static implicit operator EntityInfo(Entity entity) {
            return new EntityInfo(entity);
        }
        public static implicit operator Entity(EntityInfo entityInfo) {
            return entityInfo._entity;
        }

        private readonly Entity _entity;
        public bool expanded;

        public string Name { get; set; }

        public EntityInfo(Entity entity) {
            _entity = entity;
        }

        public bool Equals(Entity other) {
            return _entity.Equals(other);
        }
    }
#endif
}
