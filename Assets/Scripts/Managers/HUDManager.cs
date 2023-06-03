using Kryz.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UI;

namespace Beacon
{ 
    public class HUDManager : MonoBehaviour
    {
        [Header("Root HUD Elements")]
        [SerializeField] private GameObject MenuHUD;
        [SerializeField] private GameObject GameHUD;
        [SerializeField] private GameObject PauseHUD;
        [SerializeField] private GameObject VictoryHUD;
        [SerializeField] private GameObject GameOverHUD;
        [SerializeField] private GameObject LoadingHUD;

        [Header("HUD Elements")]
        [SerializeField] private GameObject FinalChallengeHUD;

        [Header("Button Elements")]
        [SerializeField] private GameObject PlayButtons;
        [SerializeField] private GameObject ResumeButtons;
        [SerializeField] private GameObject EndDayButton;
        [SerializeField] private GameObject FinalChallengeButton;

        [Header("Icon Elements")]
        [SerializeField] private GameObject DayIconGO;
        [SerializeField] private GameObject NightIconGO;

        private RectTransform endDayPanel;
        private RectTransform finalChallengePanel;
        private Image DayIcon;
        private Image NightIcon;

        private Color32 FadedColor = new Color32(255, 255, 255, 0);
        private Color32 NormalColor = new Color32(255, 255, 255, 255);

        public static HUDManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            ActivateRootHUD(MenuHUD);

            endDayPanel = EndDayButton.transform.GetChild(0).gameObject.GetComponent<RectTransform>();
            finalChallengePanel = FinalChallengeButton.transform.GetChild(0).gameObject.GetComponent<RectTransform>();
            DayIcon = DayIconGO.GetComponent<Image>();
            NightIcon = NightIconGO.GetComponent<Image>();
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener<GamePlayDayEvent>(onDayStarted);

            EventManager.Instance.AddListener<SaveDataEvent>(onSaveDataEvent);
            EventManager.Instance.AddListener<LoadDataEvent>(onLoadDataEvent);
            EventManager.Instance.AddListener<ApplyDataEvent>(onApplyDataEvent);

            EventManager.Instance.AddListener<DataSavedEvent>(onDataSavedEvent);
            EventManager.Instance.AddListener<DataLoadedEvent>(onDataLoadedEvent);
            EventManager.Instance.AddListener<DataAppliedEvent>(onDataAppliedEvent);

            EventManager.Instance.AddListener<GamePlayDayEvent>(onGamePlayDayEvent);
            EventManager.Instance.AddListener<GamePlayNightEvent>(onGamePlayNightEvent);
            EventManager.Instance.AddListener<GamePauseEvent>(onGamePauseEvent);
            EventManager.Instance.AddListener<GameVictoryEvent>(onGameVictoryEvent);
            EventManager.Instance.AddListener<GameOverEvent>(onGameOverEvent);

            EventManager.Instance.AddListener<AllBuildingsBuiltEvent>(onAllBuildingsBuiltEvent);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<GamePlayDayEvent>(onDayStarted);

            EventManager.Instance.RemoveListener<SaveDataEvent>(onSaveDataEvent);
            EventManager.Instance.RemoveListener<LoadDataEvent>(onLoadDataEvent);
            EventManager.Instance.RemoveListener<ApplyDataEvent>(onApplyDataEvent);

            EventManager.Instance.RemoveListener<DataSavedEvent>(onDataSavedEvent);
            EventManager.Instance.RemoveListener<DataLoadedEvent>(onDataLoadedEvent);
            EventManager.Instance.RemoveListener<DataAppliedEvent>(onDataAppliedEvent);

            EventManager.Instance.RemoveListener<GamePlayDayEvent>(onGamePlayDayEvent);
            EventManager.Instance.RemoveListener<GamePlayNightEvent>(onGamePlayNightEvent);
            EventManager.Instance.RemoveListener<GamePauseEvent>(onGamePauseEvent);
            EventManager.Instance.RemoveListener<GameVictoryEvent>(onGameVictoryEvent);
            EventManager.Instance.RemoveListener<GameOverEvent>(onGameOverEvent);

            EventManager.Instance.RemoveListener<AllBuildingsBuiltEvent>(onAllBuildingsBuiltEvent);
        }

        private void ActivateRootHUD(GameObject HUD) {
            MenuHUD.SetActive(false);
            GameHUD.SetActive(false);
            PauseHUD.SetActive(false);
            VictoryHUD.SetActive(false);
            GameOverHUD.SetActive(false);

            HUD.SetActive(true);
        }

        #region Button Callbacks
        public void onPlayButtonClicked(bool newSave)
        {
            ActivateRootHUD(GameHUD);

            EventManager.Instance.Raise(new PlayButtonClickedEvent() {
                newSave = newSave,
                unpause = false
            });
        }

        public void onResumeButtonClicked()
        {
            ActivateRootHUD(GameHUD);

            EventManager.Instance.Raise(new PlayButtonClickedEvent()
            {
                newSave = false,
                unpause = true
            });
        }

        public void onMenuButtonClicked(bool GameOver = false)
        {
            ActivateRootHUD(MenuHUD);

            EventManager.Instance.Raise(new MenuButtonClickedEvent() { });
        }

        public void onQuitButtonClicked()
        {
            Application.Quit();
        }

        public void onDayEnded()
        {
            EventManager.Instance.Raise(new DayEndedEvent() { });
            StartCoroutine(Coroutines.MyImageFadeCouroutine(DayIcon, DayIcon.color, FadedColor, 1, EasingFunctions.OutCubic));
            StartCoroutine(Coroutines.MyImageFadeCouroutine(NightIcon, NightIcon.color, NormalColor, 1, EasingFunctions.OutCubic));
        }

        public void onFinalChallengeHUDOK()
        {
            FinalChallengeHUD.SetActive(false);
        }

        public void onFinalChallenge()
        {
            EventManager.Instance.Raise(new FinalChallengeEvent() { });
            onDayEnded();
        }

        public void onSpeedButtonClicked(int speed)
        {
            EventManager.Instance.Raise(new SpeedChangedEvent() { speed = speed });
        }
        #endregion

        #region Events Callbacks
        private void onGamePlayDayEvent(GamePlayDayEvent e)
        {
            ActivateRootHUD(GameHUD);
            NightIcon.color = FadedColor;
            DayIcon.color = NormalColor;
            StartCoroutine(Coroutines.MyMenuOpenCouroutine(EndDayButton, endDayPanel, new Vector2(-55, 20), new Vector2(-55, 0), 2, EasingFunctions.OutCubic));

            if (GameManager.instance.isFinalChallenge)
                StartCoroutine(Coroutines.MyMenuOpenCouroutine(FinalChallengeButton, finalChallengePanel, new Vector2(-114, 20), new Vector2(-114, 0), 2, EasingFunctions.OutCubic));
            else
                finalChallengePanel.anchoredPosition = new Vector2(-114, 20);
        }

        private void onGamePlayNightEvent(GamePlayNightEvent e)
        {
            ActivateRootHUD(GameHUD);
            DayIcon.color = FadedColor;
            NightIcon.color = NormalColor;
            StartCoroutine(Coroutines.MyMenuCloseCouroutine(EndDayButton, endDayPanel, new Vector2(-55, 0), new Vector2(-55, 20), 2, EasingFunctions.OutCubic));

            if (GameManager.instance.isFinalChallenge)
                StartCoroutine(Coroutines.MyMenuCloseCouroutine(FinalChallengeButton, finalChallengePanel, new Vector2(-114, 0), new Vector2(-114, 20), 2, EasingFunctions.OutCubic));
            else
                finalChallengePanel.anchoredPosition = new Vector2(-114, 20);
        }

        private void onGamePauseEvent(GamePauseEvent e)
        {
            ActivateRootHUD(PauseHUD);
        }

        private void onGameVictoryEvent(GameVictoryEvent e)
        {
            ActivateRootHUD(VictoryHUD);
        }

        private void onGameOverEvent(GameOverEvent e)
        {
            ActivateRootHUD(GameOverHUD);
        }

        private void onDayStarted(GamePlayDayEvent e)
        {
            StartCoroutine(Coroutines.MyImageFadeCouroutine(NightIcon, NightIcon.color, FadedColor, 1, EasingFunctions.OutCubic));
            StartCoroutine(Coroutines.MyImageFadeCouroutine(DayIcon, DayIcon.color, NormalColor, 1, EasingFunctions.OutCubic));
        }

        private void onAllBuildingsBuiltEvent(AllBuildingsBuiltEvent e) {
            FinalChallengeHUD.SetActive(true);
            StartCoroutine(Coroutines.MyMenuOpenCouroutine(FinalChallengeButton, finalChallengePanel, new Vector2(-114, 20), new Vector2(-114, 0), 2, EasingFunctions.OutCubic));
        }

        private void onSaveDataEvent(SaveDataEvent e)
        {
            LoadingHUD.SetActive(true);
        }

        private void onLoadDataEvent(LoadDataEvent e)
        {
            LoadingHUD.SetActive(true);
        }

        private void onApplyDataEvent(ApplyDataEvent e)
        {
            LoadingHUD.SetActive(true);
        }

        private void onDataSavedEvent(DataSavedEvent e)
        {
            LoadingHUD.SetActive(false);
        }

        private void onDataLoadedEvent(DataLoadedEvent e)
        {
            LoadingHUD.SetActive(false);
        }

        private void onDataAppliedEvent(DataAppliedEvent e)
        {
            LoadingHUD.SetActive(false);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Menu")
            {
                PlayButtons.SetActive(!PreferenceManager.instance.isSaved());
                ResumeButtons.SetActive(PreferenceManager.instance.isSaved());
            }
        }
        #endregion
    }
}
