using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECSExample.GameObjectExample {

    public class PrefabSpawner : ScriptBehaviour {
        public GameObject _prefab;

        public Transform[] spawnPoints;

        [InjectDependency]
        private EntityManager _entityManager;

        // Use this for initialization
        void Start() {
            NativeArray<Entity> entities = new NativeArray<Entity>(spawnPoints.Length);
            NativeArray<GameObject> gameObjects = new NativeArray<GameObject>(spawnPoints.Length);
            _entityManager.InstantiateWithGameObject(_prefab, entities, gameObjects);

            for (int i = 0; i < gameObjects.Length; i++) {
                gameObjects[i].name += ": " + i;
                gameObjects[i].transform.position = spawnPoints[i].position;
            }
        }
    }
}
