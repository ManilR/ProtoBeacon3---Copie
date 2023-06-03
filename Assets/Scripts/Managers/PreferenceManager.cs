using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System.Net.Security;
using Unity.Transforms;

namespace Beacon 
{
    public class PreferenceManager : MonoBehaviour
    {
        private Data gameData;
        private static string dataFilePath;

        public bool overwriteData = false;
        private int test = 0;

        private EntityManager entityManager;
        private World defaultWorld;
        private Entity SetLightValue;

        private bool lightLoaded = false;
        private bool buildingLoaded = false;

        private bool loadingComplete { get { return (lightLoaded && buildingLoaded); } }


        public static PreferenceManager instance;
        private void Awake()
        {
            dataFilePath = Path.Combine(Application.persistentDataPath, "GameData.json");
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (gameData == null)
            {
                gameData = new Data();

                InitializeSaveData();
                InitializeSettingsData();
            }

            if (!File.Exists(dataFilePath) || overwriteData)
            {
                Tools.LOG(this, "File did not exist creating at " + dataFilePath);
                using (StreamWriter writer = new StreamWriter(dataFilePath))
                {
                    string dataToWrite = JsonUtility.ToJson(gameData);
                    writer.Write(dataToWrite);
                }

            }

            EventManager.Instance.AddListener<SaveDataEvent>(onSaveDataEvent);
            EventManager.Instance.AddListener<LoadDataEvent>(onLoadDataEvent);
            EventManager.Instance.AddListener<EraseDataEvent>(onEraseDataEvent);
            EventManager.Instance.AddListener<ApplyDataEvent>(onApplyDataEvent);

            EventManager.Instance.AddListener<CameraMovedEvent>(onCameraMovedEvent);
            EventManager.Instance.AddListener<SpeedChangedEvent>(onSpeedChandedEvent);

            EventManager.Instance.AddListener<LightLevelChangedEvent>(onLightLevelChangedEvent);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<SaveDataEvent>(onSaveDataEvent);
            EventManager.Instance.RemoveListener<LoadDataEvent>(onLoadDataEvent);
            EventManager.Instance.RemoveListener<EraseDataEvent>(onEraseDataEvent);
            EventManager.Instance.RemoveListener<ApplyDataEvent>(onApplyDataEvent);

            EventManager.Instance.RemoveListener<CameraMovedEvent>(onCameraMovedEvent);
            EventManager.Instance.RemoveListener<SpeedChangedEvent>(onSpeedChandedEvent);

            EventManager.Instance.RemoveListener<LightLevelChangedEvent>(onLightLevelChangedEvent);
        }

        private void Start()
        {
            defaultWorld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultWorld.EntityManager;
        }

        private void Update()
        {
        }

        #region Public Accessors
        public Data GetGameData()
        {
            return gameData;
        }

        public bool isSaved()
        {
            return gameData.savedData;
        }

        public SettingsData GetSettings()
        {
            return gameData.settings;
        }

        public SavedData GetSave()
        {
            return gameData.save;
        }
        #endregion


        private void InitializeSaveData()
        {
            gameData.save = new SavedData();
        }

        private void InitializeSettingsData()
        {
            gameData.settings = new SettingsData();
        }

        private void Save()
        {
            gameData.savedData = true;
            using (StreamWriter writer = new StreamWriter(dataFilePath))
            {
                string dataToWrite = JsonUtility.ToJson(gameData);
                writer.Write(dataToWrite);
            }
            Tools.LOG(this, "Successfully saved data");
            EventManager.Instance.Raise(new DataSavedEvent());
        }

        private void Load()
        {
            using (StreamReader reader = new StreamReader(dataFilePath))
            {
                string dataToLoad = reader.ReadToEnd();
                gameData = JsonUtility.FromJson<Data>(dataToLoad);
            }
            Tools.LOG(this, "Successfully loaded data");
            EventManager.Instance.Raise(new DataLoadedEvent()
            {
                save = gameData.save,
                settings = gameData.settings
            });
        }

        private void ApplyLightData()
        {
            // Tools.LOG(this, "Starting to restore light...");
            EntityQuery beaconQuery = entityManager.CreateEntityQuery(new ComponentType[] { typeof(Beacon) });
            Beacon beaconData;
            Entity beacon = Entity.Null;

            foreach (var entity in beaconQuery.ToEntityArray(Allocator.Temp))
                beacon = entity;

            if (beacon != Entity.Null)
            {
                // Tools.LOG(this, "Restoring light data...");

                gameData.save.light = gameData.save.light <= 0 ? 100 : gameData.save.light;

                beaconData = entityManager.GetComponentData<Beacon>(beacon);
                beaconData.lightLevel = gameData.save.light;
                entityManager.SetComponentData<Beacon>(beacon, beaconData);

                Tools.LOG(this, "Light restored to " + gameData.save.light);

                lightLoaded = true;
                if (loadingComplete)
                {
                    // Time.timeScale = 0;
                    EventManager.Instance.Raise(new DataAppliedEvent());
                }
            }
            else
            {
                test++;
                if (test < 30)
                {
                    // Tools.LOG(this, "Retrying to restore light data");
                    StartCoroutine(MyFunctionLoadWithDelay());
                }
                else 
                {
                    Tools.LOG(this, "Critical error : Loading light data failed after 30 seconds trying to load data");
                    EventManager.Instance.Raise(new GameMenuEvent());
                }
                    
            }

            IEnumerator MyFunctionLoadWithDelay()
            {
                yield return new WaitForSecondsRealtime(1f);
                ApplyLightData();
            }
        }

        private void ApplyBuildingData()
        {
            // Tools.LOG(this, "Starting to restore buildings...");
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Building>();
            EntityQuery buildingQuery = entityManager.CreateEntityQuery(builder);

            NativeArray<Entity> entities = buildingQuery.ToEntityArray(Allocator.TempJob);

            if (entities.Length != 0)
            {
                // Tools.LOG(this, "Restoring " + entities.Length + " buildings...");
                for (int i = 0; i < entities.Length; i++)
                {
                    Building building = entityManager.GetComponentData<Building>(entities[i]);
                    Health health = entityManager.GetComponentData<Health>(entities[i]);
                    HouseData buildingData;

                    if (gameData.save.houseData.TryGetValue(building.ID, out buildingData))
                    {
                        building.isDestroyed = buildingData.dead;

                        health.health = buildingData.dead ? 0.0f : buildingData.health;
                        health.maxHealth = buildingData.maxHealth;

                        building.nbSoldierMAX = buildingData.nbSoldierMAX;

                        building.production = buildingData.production;

                        building.mode = buildingData.mode;
                        building.lvlAttack = buildingData.attackLevel;
                        building.lvlDefense = buildingData.defenseLevel;
                        building.lvlProduction = buildingData.productionLevel;

                        entityManager.SetComponentData<Building>(entities[i], building);
                        entityManager.SetComponentData<Health>(entities[i], health);
                        entityManager.SetComponentEnabled<Dead>(entities[i], buildingData.dead);
                    }
                    else
                    {
                        entityManager.SetComponentEnabled<Dead>(entities[i], true);
                    }
                        
                }
                Tools.LOG(this, entities.Length + " buildings restored");

                buildingLoaded = true;
                if (loadingComplete)
                {
                    EventManager.Instance.Raise(new DataAppliedEvent());
                }
            }
            else
            {
                test++;
                if (test < 30)
                {
                    // Tools.LOG(this, "Retrying to restore building data");
                    StartCoroutine(MyFunctionLoadWithDelay());
                }
                else 
                {
                    Tools.LOG(this, "Critical error : Loading building data failed after 30 seconds trying to load data");
                    EventManager.Instance.Raise(new GameMenuEvent());
                }
            }

            IEnumerator MyFunctionLoadWithDelay()
            {
                yield return new WaitForSecondsRealtime(1f);
                ApplyBuildingData();
            }
        }

        private void SaveBuildingData()
        {
            //Tools.LOG(this, "Commencing save building data");
            gameData.save.houseData.Clear();

            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Building>();
            EntityQuery buildingQuery = entityManager.CreateEntityQuery(builder);

            NativeArray<Entity> entities = buildingQuery.ToEntityArray(Allocator.TempJob);

            for (int i = 0; i < entities.Length; i++)
            {
                Building building = entityManager.GetComponentData<Building>(entities[i]);
                Health health = entityManager.GetComponentData<Health>(entities[i]);

                //Tools.LOG(this, "Saving building ID " + building.ID + " as " + (building.isDestroyed ? "destroyed" : "built"));
                gameData.save.houseData.Add(building.ID, new HouseData()
                {
                    ID = building.ID,
                    dead = building.isDestroyed,

                    health = health.health,
                    maxHealth = health.maxHealth,

                    nbSoldierMAX = building.nbSoldierMAX,

                    production = building.production,

                    mode = building.mode,
                    attackLevel = building.lvlAttack,
                    defenseLevel = building.lvlDefense,
                    productionLevel = building.lvlProduction
                });
            }
        }

        #region Event Callbacks
        private void onSaveDataEvent(SaveDataEvent e)
        {
            Tools.LOG(this, "Saving data");
            gameData.save.day = e.day;
            gameData.save.lastPlayState = GameManager.instance.isPlaying ? e.currentPlayState : GAMESTATE.play_day;
            gameData.save.light = e.light != -1 ? e.light : gameData.save.light;
            gameData.save.finalChallenge = e.finalChallenge;

            SaveBuildingData();
            
            Save();
        }

        private void onLoadDataEvent(LoadDataEvent e)
        {
            Tools.LOG(this, "Loading data");
            Load();
        }

        private void onEraseDataEvent(EraseDataEvent e)
        {
            Tools.LOG(this, "Erasing data");
            InitializeSaveData();
            InitializeSettingsData();
            Save();
            gameData.savedData = false;
            Tools.LOG(this, "Successfully erased data");
            EventManager.Instance.Raise(new DataErasedEvent());
        }

        private void onApplyDataEvent(ApplyDataEvent e)
        {
            Tools.LOG(this, "Applying data, light to apply is " + gameData.save.light);
            if (gameData.save.light == 0)
            {
                InitializeSaveData();
                InitializeSettingsData();
            }
            lightLoaded = false;
            test = 0;
            ApplyLightData();
            buildingLoaded = false;
            test = 0;
            ApplyBuildingData();
        }

        private void onCameraMovedEvent(CameraMovedEvent e)
        {
            gameData.settings.zoomLevel = e.zoomLevel;
            gameData.settings.cameraPos = e.position;
            gameData.settings.cameraRot = e.rotation;
        }

        private void onSpeedChandedEvent(SpeedChangedEvent e)
        {
            gameData.settings.gameSpeed = e.speed;
        }

        private void onLightLevelChangedEvent(LightLevelChangedEvent e)
        {
            gameData.save.light = e.lightLevel != -1 ? e.lightLevel : gameData.save.light;
        }
        #endregion
    }

    [System.Serializable]
    public class Data
    {
        public bool savedData = false;
        public SettingsData settings;
        public SavedData save;
    }

    [System.Serializable]
    public class SettingsData
    {
        public int gameSpeed = 1;
        public float zoomLevel = 35;
        public Vector3 cameraPos = new Vector3(0, 85, -90);
        public Quaternion cameraRot = new Quaternion(0.34202f, 0.0f, 0.0f, 0.93969f);
    }

    [System.Serializable]
    public class SavedData
    {
        public GAMESTATE lastPlayState = GAMESTATE.play_day;
        public float light = 100.0f;
        public int day = 1;
        public bool finalChallenge = false;

        public SerializableDictionary<int, HouseData> houseData = new SerializableDictionary<int, HouseData>();
    }

    [System.Serializable]
    public class HouseData
    {
        public int ID;
        public bool dead;

        public float health;
        public float maxHealth;

        public int nbSoldierMAX;

        public int production;

        public Mode mode;
        public int attackLevel;
        public int defenseLevel;
        public int productionLevel;
    }

    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }
}
