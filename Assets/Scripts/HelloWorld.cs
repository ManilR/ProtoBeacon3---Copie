using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beacon
{
    public class HelloWorld : MonoBehaviour
    {
        [Header("Debug Parameters")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool debugGraph = false;
        [SerializeField] private GameObject debugButtons;
        [SerializeField] private GameObject debugGraphs;
        [SerializeField] private bool lightMode = false;

        private World defaultWorld;
        private EntityManager entityManager;

        void Start()
        {
            SceneManager.LoadScene("Managers", LoadSceneMode.Additive);
            PlayerPrefs.SetInt("DebugMode", debugMode ? 1 : 0);

            defaultWorld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultWorld.EntityManager;

            EntityArchetype archetype = entityManager.CreateArchetype(typeof(GameStateComponent));
            Entity e = entityManager.CreateEntity(archetype);

            debugGraphs.SetActive(debugGraph);
        }

        private void Update()
        {
            if (lightMode)
                gameObject.GetComponent<Light>().intensity = 20;
            else
                gameObject.GetComponent<Light>().intensity = 0;

            if (debugMode)
                debugButtons.SetActive(true);
            else
                debugButtons.SetActive(false);
        }

        public void HideShowGraphs()
        {
            debugGraphs.SetActive(!debugGraphs.activeSelf);
        }

        public void ForceDay()
        {
            EventManager.Instance.Raise(new ForceDayEvent());
        }
        public void ForceNight()
        {
            EventManager.Instance.Raise(new ForceNightEvent());
        }
        public void ForceEnd()
        {
            EventManager.Instance.Raise(new ForceEndEvent());
        }
        public void ForceWin()
        {
            EventManager.Instance.Raise(new ForceWinEvent());
        }
        public void ForceLoose()
        {
            EventManager.Instance.Raise(new ForceGameOverEvent());
        }
    }
}