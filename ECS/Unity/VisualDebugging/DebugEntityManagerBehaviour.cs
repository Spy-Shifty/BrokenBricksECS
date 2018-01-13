using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS.VisualDebugging {
    public class DebugEntityManagerBehaviour : MonoBehaviour {
        public UnityEntityManager EntityManager { get { return _entityManager; } }
        private UnityEntityManager _entityManager;
        public void Init(UnityEntityManager entityManager) {
            _entityManager = entityManager;
        }
    }
}
