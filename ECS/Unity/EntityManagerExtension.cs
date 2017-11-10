using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ECS {
    public class InstantiateGameObjectException : Exception {
        public InstantiateGameObjectException(string message) : base(message) {
        }
    }
    public static class EntityManagerExtension {

        /// <summary>
        /// Instantiates only Entities with there Components from Prefab! 
        /// Wont Instantiate GameObject itself!
        /// Use InstantiateWithGameObject instead.
        /// </summary>
        public static void Instantiate(this EntityManager entityManager, GameObject prefab, NativeArray<Entity> entityArray) {
            var gameObjectEntity = prefab.GetComponent<GameObjectEntity>();
            if (!gameObjectEntity) {
                throw new Exception(prefab.name + " cant be instantiate without " + typeof(GameObjectEntity).Name + " component");
            }

            ComponentWrapper[] components = gameObjectEntity.GetComponents();

            for (int i = 0; i < entityArray.Length; i++) {
                entityArray[i] = entityManager.CreateEntity();
            }

            for (int i = 0; i < components.Length; i++) {
                for (int j = 0; j < entityArray.Length; j++) {
                    components[i].AddComponentToEntity(entityArray[j], entityManager);
                }
            }
        }

        /// <summary>
        /// Instantiates only Entity with it's Components from Prefab! 
        /// Wont Instantiate GameObject itself!
        /// Use InstantiateWithGameObject instead.
        /// </summary>
        public static Entity Instantiate(this EntityManager entityManager, GameObject prefab) {
            var gameObjectEntity = prefab.GetComponent<GameObjectEntity>();
            if (!gameObjectEntity) {
                throw new Exception(prefab.name + " cant be instantiate without " + typeof(GameObjectEntity).Name + " component");
            }
            Entity entity = entityManager.CreateEntity();
            ComponentWrapper[] components = gameObjectEntity.GetComponents();
            for (int i = 0; i < components.Length; i++) {
                components[i].AddComponentToEntity(entity, entityManager);
            }
            return entity;
        }

        /// <summary>
        /// Instantiates Entity and GameObject with it's Components from Prefab!
        /// </summary>
        public static GameObject InstantiateWithGameObject(this EntityManager entityManager, GameObject prefab) {
            Entity entity = entityManager.Instantiate(prefab);
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
            gameObject.GetComponent<GameObjectEntity>().SetEntity(entity, entityManager);
            return gameObject;
        }

        /// <summary>
        /// Instantiates Entity and GameObject with it's Components from Prefab!
        /// </summary>
        public static GameObject InstantiateWithGameObject(this EntityManager entityManager, GameObject prefab, Transform parent, bool instantiateInWorldSpace = false) {
            Entity entity = entityManager.Instantiate(prefab);
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab, parent, instantiateInWorldSpace);
            gameObject.GetComponent<GameObjectEntity>().SetEntity(entity, entityManager);
            return gameObject;
        }

        /// <summary>
        /// Instantiates Entity and GameObject with it's Components from Prefab!
        /// </summary>
        public static GameObject InstantiateWithGameObject(this EntityManager entityManager, GameObject prefab, Vector3 position, Quaternion rotation) {
            Entity entity = entityManager.Instantiate(prefab);
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation);
            gameObject.GetComponent<GameObjectEntity>().SetEntity(entity, entityManager);
            return gameObject;
        }

        /// <summary>
        /// Instantiates Entity and GameObject with it's Components from Prefab!
        /// </summary>
        public static GameObject InstantiateWithGameObject(this EntityManager entityManager, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent) {
            Entity entity = entityManager.Instantiate(prefab);
            GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            gameObject.GetComponent<GameObjectEntity>().SetEntity(entity, entityManager);
            return gameObject;
        }



        /// <summary>
        /// Instantiates only Entities with there Components from Prefab! 
        /// Wont Instantiate GameObject itself!
        /// Use InstantiateWithGameObject instead.
        /// </summary>
        public static void InstantiateWithGameObject(this EntityManager entityManager, GameObject prefab, NativeArray<Entity> entityArray, NativeArray<GameObject> gameObjectArray) {
            if(gameObjectArray.Length != entityArray.Length) {
                throw new InstantiateGameObjectException("entityArray and gameObjectArray must be the same size");
            }

            entityManager.Instantiate(prefab, entityArray);

            for (int i = 0; i < entityArray.Length; i++) {
                gameObjectArray[i] = UnityEngine.Object.Instantiate(prefab);
                gameObjectArray[i].GetComponent<GameObjectEntity>().SetEntity(entityArray[i], entityManager);
            }

        }
    }
}
