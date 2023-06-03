using UnityEngine;
using UnityEngine.SceneManagement;
using Kryz.Tweening;
using Unity.Entities;

namespace Beacon 
{
    public class ScenesManager : MonoBehaviour
    {
        public static ScenesManager instance;
        private World defaultWorld;
        private EntityManager entityManager;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);

            defaultWorld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultWorld.EntityManager;
        }

        void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;

            EventManager.Instance.AddListener<GamePlayEvent>(onPlayEvent);
            EventManager.Instance.AddListener<GameMenuEvent>(onMenuEvent);

            EventManager.Instance.AddListener<GamePlayDayEvent>(onDayEvent);
            EventManager.Instance.AddListener<GamePlayNightEvent>(onNightEvent);
        }

        void OnDisable() {
            EventManager.Instance.RemoveListener<GamePlayEvent>(onPlayEvent);
            EventManager.Instance.RemoveListener<GameMenuEvent>(onMenuEvent);

            EventManager.Instance.RemoveListener<GamePlayDayEvent>(onDayEvent);
            EventManager.Instance.RemoveListener<GamePlayNightEvent>(onNightEvent);
        }

        public bool isSceneLoaded(string name)
        {
            int countLoaded = SceneManager.sceneCount;
            Scene[] loadedScenes = new Scene[countLoaded];

            for (int i = 0; i < countLoaded; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
                if (loadedScenes[i].name == name)
                    return true;
            }
            return false;
        }

        #region Event Callbacks
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Tools.LOG(this, scene.name + " loaded successfully");
            if (scene.name == "ClosestTarget")
            {
                EventManager.Instance.Raise(new LoadDataEvent());
                EventManager.Instance.Raise(new ApplyDataEvent());
            }                

        }

        private void onPlayEvent(GamePlayEvent e)
        {
            if (isSceneLoaded("Menu"))
                SceneManager.UnloadSceneAsync("Menu");
            SceneManager.LoadSceneAsync("ClosestTarget", LoadSceneMode.Additive);

            Entity gameState;
            EntityQuery GSquery = entityManager.CreateEntityQuery(new ComponentType[] { typeof(GameStateComponent) });
            if (GSquery.TryGetSingletonEntity<GameStateComponent>(out gameState))
                entityManager.SetComponentData<GameStateComponent>(gameState, new GameStateComponent { isPlaying = true });
        }

        private void onMenuEvent(GameMenuEvent e)
        {
            if (isSceneLoaded("ClosestTarget"))
                SceneManager.UnloadSceneAsync("ClosestTarget");
            SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Additive);

            Entity gameState;
            EntityQuery GSquery = entityManager.CreateEntityQuery(new ComponentType[] { typeof(GameStateComponent) });
            if (GSquery.TryGetSingletonEntity<GameStateComponent>(out gameState))
                entityManager.SetComponentData<GameStateComponent>(gameState, new GameStateComponent { isPlaying = false });

        }

        private void onDayEvent(GamePlayDayEvent e)
        {
            GameObject lightObj = GameObject.FindGameObjectWithTag("AmbiantLight");
            if (lightObj)
                StartCoroutine(Coroutines.MySunCouroutine(lightObj.GetComponent<Light>(), 0.0f, 3.0f, 2, EasingFunctions.InOutQuad));
            
        }
        private void onNightEvent(GamePlayNightEvent e)
        {
            GameObject lightObj = GameObject.FindGameObjectWithTag("AmbiantLight");
            if (lightObj)
                StartCoroutine(Coroutines.MySunCouroutine(lightObj.GetComponent<Light>(), 3.0f, 0.0f, 2, EasingFunctions.InOutQuad));
        }

        #endregion
    }
}
