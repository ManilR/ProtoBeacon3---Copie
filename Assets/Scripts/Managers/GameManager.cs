using System.Globalization;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Beacon 
{
    public enum GAMESTATE { menu, play_day, play_night, pause, victory, over };

    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        private GAMESTATE currentState = GAMESTATE.menu;
        private GAMESTATE lastPlayState = GAMESTATE.play_day;

        private EntityManager entityManager;
        private World defaultWorld;

        private int dayCount;
        private float lightLevel;

        private bool finalChallenge;

        public bool isPlaying { get { return (currentState == GAMESTATE.play_day || currentState == GAMESTATE.play_night); } }
        public bool isDay { get { return (currentState == GAMESTATE.play_day); } }
        public bool isNight { get { return (currentState == GAMESTATE.play_night); } }
        public bool isFinalChallenge { get { return finalChallenge; } }

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            EventManager.Instance.Raise(new LoadDataEvent());

            defaultWorld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultWorld.EntityManager;

            Menu();
        }

        private void Update()
        {
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener<WaveEndedEvent>(onWaveEndedEvent);
            EventManager.Instance.AddListener<DayEndedEvent>(onDayEndedEvent);
            EventManager.Instance.AddListener<SpeedChangedEvent>(onSpeedChandedEvent);

            EventManager.Instance.AddListener<PlayButtonClickedEvent>(onPlayButtonClickedEvent);
            EventManager.Instance.AddListener<PauseButtonClickedEvent>(onPauseButtonClickedEvent);
            EventManager.Instance.AddListener<MenuButtonClickedEvent>(onMenuButtonClickedEvent);

            EventManager.Instance.AddListener<FinalChallengeEvent>(onFinalChallengeEvent);

            EventManager.Instance.AddListener<DataSavedEvent>(onDataSavedEvent);
            EventManager.Instance.AddListener<DataLoadedEvent>(onDataLoadedEvent);
            EventManager.Instance.AddListener<DataAppliedEvent>(onDataAppliedEvent);

            EventManager.Instance.AddListener<LightLevelChangedEvent>(onLightLevelChangedEvent);

            EventManager.Instance.AddListener<ForceDayEvent>(onForceDayEvent);
            EventManager.Instance.AddListener<ForceNightEvent>(onForceNightEvent);
            EventManager.Instance.AddListener<ForceEndEvent>(onForceEndEvent);
            EventManager.Instance.AddListener<ForceWinEvent>(onForceWinEvent);
            EventManager.Instance.AddListener<ForceGameOverEvent>(onForceGameOverEvent);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<WaveEndedEvent>(onWaveEndedEvent);
            EventManager.Instance.RemoveListener<DayEndedEvent>(onDayEndedEvent);
            EventManager.Instance.RemoveListener<SpeedChangedEvent>(onSpeedChandedEvent);

            EventManager.Instance.RemoveListener<PlayButtonClickedEvent>(onPlayButtonClickedEvent);
            EventManager.Instance.RemoveListener<PauseButtonClickedEvent>(onPauseButtonClickedEvent);
            EventManager.Instance.RemoveListener<MenuButtonClickedEvent>(onMenuButtonClickedEvent);

            EventManager.Instance.RemoveListener<FinalChallengeEvent>(onFinalChallengeEvent);

            EventManager.Instance.RemoveListener<DataSavedEvent>(onDataSavedEvent);
            EventManager.Instance.RemoveListener<DataLoadedEvent>(onDataLoadedEvent);
            EventManager.Instance.RemoveListener<DataAppliedEvent>(onDataAppliedEvent);

            EventManager.Instance.RemoveListener<LightLevelChangedEvent>(onLightLevelChangedEvent);

            EventManager.Instance.RemoveListener<ForceDayEvent>(onForceDayEvent);
            EventManager.Instance.RemoveListener<ForceNightEvent>(onForceNightEvent);
            EventManager.Instance.RemoveListener<ForceEndEvent>(onForceEndEvent);
            EventManager.Instance.RemoveListener<ForceWinEvent>(onForceWinEvent);
            EventManager.Instance.RemoveListener<ForceGameOverEvent>(onForceGameOverEvent);
        }

        private void setState(GAMESTATE newState)
        {
            lastPlayState = (currentState == GAMESTATE.play_day || currentState == GAMESTATE.play_night) ? currentState : lastPlayState;
            currentState = newState;
            switch (currentState)
            {
                case GAMESTATE.menu:
                    EventManager.Instance.Raise(new GameMenuEvent());
                    break;
                case GAMESTATE.play_day:
                    EventManager.Instance.Raise(new GamePlayDayEvent());
                    break;
                case GAMESTATE.play_night:
                    EventManager.Instance.Raise(new GamePlayNightEvent());
                    break;
                case GAMESTATE.pause:
                    EventManager.Instance.Raise(new GamePauseEvent());
                    break;
                case GAMESTATE.victory:
                    EventManager.Instance.Raise(new GameVictoryEvent());
                    break;
                case GAMESTATE.over:
                    EventManager.Instance.Raise(new GameOverEvent());
                    break;
                default:
                    break;
            }
        }

        private void EndDay()
        {
            Tools.LOG(this, "Day finished, switching to night");
            setState(GAMESTATE.play_night);
            Save();
        }

        private void EndNight()
        {
            Tools.LOG(this, "Night finished, switching to day");
            dayCount++;
            EventManager.Instance.Raise(new SaveDataEvent()
            {
                day = dayCount,
                light = lightLevel,
                currentPlayState = currentState
            });
            setState(GAMESTATE.play_day);
            Save();
        }

        private void Menu()
        {
            Tools.LOG(this, "Menu");

            Time.timeScale = PreferenceManager.instance.GetSettings().gameSpeed;
            setState(GAMESTATE.menu);
        }

        private void Victory()
        {
            Tools.LOG(this, "Victory");

            EventManager.Instance.Raise(new EraseDataEvent());

            Time.timeScale = 0;
            setState(GAMESTATE.victory);
        }
        
        private void GameOver()
        {
            if (!isPlaying || !ScenesManager.instance.isSceneLoaded("ClosestTarget") || PreferenceManager.instance.GetSave().light > 0)
                return;

            Tools.LOG(this, "GameOver : Light is " + PreferenceManager.instance.GetSave().light);

            EventManager.Instance.Raise(new EraseDataEvent());

            Time.timeScale = 0;
            setState(GAMESTATE.over);
        }

        private void Save()
        {
            GetLightLevel();
            EventManager.Instance.Raise(new SaveDataEvent()
            {
                day = dayCount,
                light = lightLevel,
                currentPlayState = currentState,
                finalChallenge = finalChallenge
            });
        }

        private void GetLightLevel()
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Beacon>();
            EntityQuery beaconQuery = entityManager.CreateEntityQuery(builder);
            Beacon beacon;
            beaconQuery.TryGetSingleton<Beacon>(out beacon);

            if (beacon.lightLevel != -1 && isPlaying)
                lightLevel = beacon.lightLevel;

            if (lightLevel <= 0 && lightLevel != -1 && isPlaying)
                GameOver();
        }

        #region Event Callbacks
        private void onWaveEndedEvent(WaveEndedEvent e)
        {
            if (finalChallenge)
                Victory();
            else
                EndNight();
        }

        private void onDayEndedEvent(DayEndedEvent e)
        {
            EndDay();
        }

        private void onSpeedChandedEvent(SpeedChangedEvent e)
        {
            if (isPlaying)
            {
                Time.timeScale = e.speed;
            }
        }

        private void onLightLevelChangedEvent(LightLevelChangedEvent e)
        {
            if (!isPlaying)
                return;

            lightLevel = e.lightLevel;
            if (lightLevel <= 0 && lightLevel != -1)
                GameOver();
        }

        private void onPlayButtonClickedEvent(PlayButtonClickedEvent e)
        {
            if (e.newSave)
            {
                EventManager.Instance.Raise(new EraseDataEvent());
                EventManager.Instance.Raise(new GamePlayEvent());
                Time.timeScale = PreferenceManager.instance.GetSettings().gameSpeed;
                setState(GAMESTATE.play_day);
            }
            else
            {
                if (!e.unpause)
                {
                    EventManager.Instance.Raise(new ApplyDataEvent());
                    EventManager.Instance.Raise(new GamePlayEvent());
                }
                else if (!isPlaying)
                {
                    Time.timeScale = PreferenceManager.instance.GetSettings().gameSpeed;
                    setState(PreferenceManager.instance.GetSave().lastPlayState);
                }
            }
        }

        private void onPauseButtonClickedEvent(PauseButtonClickedEvent e)
        {
            if (!isPlaying)
                return;

            Tools.LOG(this, "Pause");

            Time.timeScale = 0;
            Save();
            setState(GAMESTATE.pause);
        }

        private void onMenuButtonClickedEvent(MenuButtonClickedEvent e)
        {
            Menu();
        }

        private void onFinalChallengeEvent(FinalChallengeEvent e)
        {
            finalChallenge = true;
        }

        private void onDataLoadedEvent(DataLoadedEvent e)
        {
            dayCount = e.save.day;
            lightLevel = e.save.light;
            finalChallenge = e.save.finalChallenge;
        }

        private void onDataAppliedEvent(DataAppliedEvent e)
        {
            finalChallenge = PreferenceManager.instance.GetSave().finalChallenge;
            dayCount = PreferenceManager.instance.GetSave().day;
            lightLevel = PreferenceManager.instance.GetSave().light;
            Time.timeScale = PreferenceManager.instance.GetSettings().gameSpeed;
            setState(PreferenceManager.instance.GetSave().lastPlayState);
        }

        private void onDataSavedEvent(DataSavedEvent e)
        {
        }

        private void onForceDayEvent(ForceDayEvent e)
        {
            EntityQuery entityQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Enemy>());
            NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.TempJob);
            for (int i = 0; i < entities.Length; i++)
            {
                entityManager.DestroyEntity(entities[i]);
            }
            entities.Dispose();
            EventManager.Instance.Raise(new WaveEndedEvent());
            EventManager.Instance.Raise(new NightEndedEvent());
            EventManager.Instance.Raise(new GamePlayDayEvent());
        }

        private void onForceNightEvent(ForceNightEvent e)
        {
            EventManager.Instance.Raise(new DayEndedEvent());
            EventManager.Instance.Raise(new GamePlayNightEvent());
        }

        private void onForceEndEvent(ForceEndEvent e)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Building>();
            EntityQuery buildingQuery = entityManager.CreateEntityQuery(builder);

            NativeArray<Entity> entities = buildingQuery.ToEntityArray(Allocator.TempJob);

            if (entities.Length != 0)
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    Building building = entityManager.GetComponentData<Building>(entities[i]);
                    Health health = entityManager.GetComponentData<Health>(entities[i]);

                    health.maxHealth = 120;

                    building.nbSoldierMAX = 13;

                    building.mode = Mode.attack;
                    building.lvlAttack = 11;
                    building.lvlDefense = 11;
                    building.lvlProduction = 11;

                    building.buildingPrice = 0;

                    entityManager.SetComponentData<Building>(entities[i], building);
                    entityManager.SetComponentData<Health>(entities[i], health);
                    entityManager.SetComponentEnabled<InConstruction>(entities[i], true);
                }
            }

            EventManager.Instance.Raise(new AllBuildingsBuiltEvent() { });
        }

        private void onForceWinEvent(ForceWinEvent e)
        {
            Victory();
        }

        private void onForceGameOverEvent(ForceGameOverEvent e)
        {
            EntityQuery beaconQuery = entityManager.CreateEntityQuery(new ComponentType[] { typeof(Beacon) });
            Beacon beaconData;
            Entity beacon = Entity.Null;

            foreach (var entity in beaconQuery.ToEntityArray(Allocator.Temp))
                beacon = entity;

            if (beacon != Entity.Null)
            {
                PreferenceManager.instance.GetSave().light = 0;

                beaconData = entityManager.GetComponentData<Beacon>(beacon);
                beaconData.lightLevel = 0;
                entityManager.SetComponentData<Beacon>(beacon, beaconData);
            }
            GameOver();
        }
        #endregion
    }

}
