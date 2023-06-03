using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using TMPro;
using UnityEngine.UIElements;
using Kryz.Tweening;
using static UnityEngine.Rendering.DebugUI;

namespace Beacon
{
    public class WavesManager : MonoBehaviour
    {

        [Header("Waves Data")]
        [SerializeField, Range(1, 20)] private float cooldown = 1;
        [SerializeField, Range(50, 200)] private int startNumberOfEnemies = 100;
        [SerializeField, Range(1, 20)] private int startNumberOfWaves = 10;

        [Header("HUD Elements")]
        [SerializeField] private GameObject WaveHUD;
        [SerializeField] private GameObject WaveNumber;
        [SerializeField] private GameObject EnnemieCount;

        private TextMeshProUGUI WaveNumberText;
        private TextMeshProUGUI EnnemieCountText;
        private RectTransform Panel;

        private int numberOfEnemies = 100;
        private int numberOfWaves = 10;

        private float timer = 0f;
        private int waveNumber = 0;

        private int totalNumberOfEnnemies = 0;
        private int totalNumberOfWaves = 0;

        private int littleLeft = 0;

        private bool finalChallenge;

        private EntityManager entityManager;
        private World defaultWorld;
        private Entity waveEntity;

        public static WavesManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }


        private void OnEnable() {
            EventManager.Instance.AddListener<GamePlayDayEvent>(onDayEvent);
            EventManager.Instance.AddListener<GamePlayNightEvent>(onNightEvent);

            EventManager.Instance.AddListener<DataLoadedEvent>(onDataLoadedEvent);
        }

        private void OnDisable() {
            EventManager.Instance.RemoveListener<GamePlayDayEvent>(onDayEvent);
            EventManager.Instance.RemoveListener<GamePlayNightEvent>(onNightEvent);

            EventManager.Instance.RemoveListener<DataLoadedEvent>(onDataLoadedEvent);
        }

        private void Start()
        {
            defaultWorld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultWorld.EntityManager;

            WaveNumberText = WaveNumber.GetComponent<TextMeshProUGUI>();
            EnnemieCountText = EnnemieCount.GetComponent<TextMeshProUGUI>();
            Panel = WaveHUD.GetComponent<RectTransform>();

            totalNumberOfWaves = 0;
        }

        private void Update()
        {
            if (!GameManager.instance.isNight)
                return;

            if ((timer += Time.deltaTime) < cooldown)
                return;

            timer = 0;

            int enemyCount = CountEntitiesWithEnemyTag();

            if (waveNumber < numberOfWaves) 
            {
                int waveCount = (int)Mathf.Floor(Random.Range(numberOfEnemies * 0.9f, numberOfEnemies * 1.1f));
                Vector3 position = generateRandomPosition();
                bool isBoss = Random.Range(0, 15) == 0;
                Tools.LOG(this, "New Wave #" + (waveNumber + 1) + "/" + numberOfWaves + " at " + position + " with " + waveCount + " enemies.");

                NewWave(position, waveCount, isBoss);
                waveNumber++;
                WaveNumberText.text = "Wave " + waveNumber + "/" + numberOfWaves + (isBoss ? " (boss)" : "");
                EnnemieCountText.text = (enemyCount+ waveCount) + " ennemies left";
            }
            else
            {
                WaveNumberText.text = "No more waves tonight";
                EnnemieCountText.text = enemyCount + " ennemies left";

                if (enemyCount == 0)
                {
                    gainEnemyLight();
                    EventManager.Instance.Raise(new WaveEndedEvent());
                }
                if (enemyCount <= 25)
                {
                    littleLeft++;
                    if (littleLeft > 5)
                    {
                        Tools.LOG(this, "Too few enemies left to kill for too long, proceding to force destroy them");
                        ForceDestroyRemainingEnemies();
                    }
                }
            }
        }



        private Vector3 generateRandomPosition() {

            float angle = Random.Range(0f, Mathf.PI * 2);
            float x = Mathf.Cos(angle) * 150;
            float z = Mathf.Sin(angle) * 150;

            return new Vector3(x, 10.0f, z);
        }

        private void NewWave(Vector3 position, int waveCount, bool isBoss = false)
        {
            totalNumberOfEnnemies += waveCount;
            totalNumberOfWaves++;

            if (defaultWorld.IsCreated && !entityManager.Exists(waveEntity))
            {
                waveEntity = entityManager.CreateEntity();
                entityManager.AddBuffer<WaveData>(waveEntity);
            }
            
            entityManager.GetBuffer<WaveData>(waveEntity).Add(new WaveData { 
                position = position,
                count = waveCount,
                ennemyType = isBoss ? ennemyType.boss : Random.Range(0, 2) == 0 ? ennemyType.normal : ennemyType.speedy
            });

        }

        private int CountEntitiesWithEnemyTag()
        {
            EntityQuery entityQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Enemy>());
            NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.TempJob);
            int enemyCount = entities.Length;
            entities.Dispose();
            return enemyCount;
        }

        private void ForceDestroyRemainingEnemies()
        {
            EntityQuery entityQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Enemy>());
            NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.TempJob);
            for (int i = 0; i < entities.Length; i++)
            {
                entityManager.DestroyEntity(entities[i]);
            }
            entities.Dispose();
        }

        private void gainEnemyLight()
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Beacon, LightModifBuffer>();
            EntityQuery beaconQuery = entityManager.CreateEntityQuery(builder);

            DynamicBuffer<LightModifBuffer> lightModifBuffer;
            if (beaconQuery.TryGetSingletonBuffer<LightModifBuffer>(out lightModifBuffer))
            {
                Entity beaconEntity;
                beaconQuery.TryGetSingletonEntity<Beacon>(out beaconEntity);
                if (entityManager.HasComponent<UpdateLight>(beaconEntity))
                {
                    entityManager.SetComponentEnabled<UpdateLight>(beaconEntity, true);
                }

                lightModifBuffer.Add(new LightModifBuffer
                {
                    Value = Mathf.Ceil(totalNumberOfEnnemies * 0.025f)
                });
                Tools.LOG(this, "Wave ended, killed " + totalNumberOfEnnemies + " and gained " + Mathf.Ceil(totalNumberOfEnnemies * 0.025f));
                EventManager.Instance.Raise(new DataSavedEvent());
            }
        }


        #region Event Callbacks
        private void onDayEvent(GamePlayDayEvent e) 
        {
            numberOfEnemies = Mathf.Clamp((int)Mathf.Ceil(numberOfEnemies * 1.05f), 50, 100);
            numberOfWaves = Mathf.Clamp((int)Mathf.Ceil(numberOfWaves * 1.05f), 1, 20);

            StartCoroutine(Coroutines.MyMenuCloseCouroutine(WaveHUD, Panel, new Vector2(0, 130), new Vector2(123, 130), 2, EasingFunctions.OutCubic));
        }
        private void onNightEvent(GamePlayNightEvent e) 
        {
            timer = 0;
            waveNumber = 0;
            littleLeft = 0;
            totalNumberOfEnnemies = 0;

            if (GameManager.instance.isFinalChallenge)
            {
                numberOfEnemies = 250;
                numberOfWaves = 25;
            }

            WaveNumberText.text = (GameManager.instance.isFinalChallenge ? "Final " : "") + "Night is starting...";
            EnnemieCountText.text = "Ennemies are comming";

            StartCoroutine(Coroutines.MyMenuOpenCouroutine(WaveHUD, Panel, new Vector2(123, 130), new Vector2(0, 130), 2, EasingFunctions.OutCubic));

            EventManager.Instance.Raise(new WaveStartedEvent());
        }

        private void onDataLoadedEvent(DataLoadedEvent e)
        {
            numberOfEnemies = Mathf.Clamp((int)Mathf.Ceil(startNumberOfEnemies * Mathf.Pow(1.1f, e.save.day)), 50, 1000);
            numberOfWaves = Mathf.Clamp((int)Mathf.Ceil(startNumberOfWaves * Mathf.Pow(1.1f, e.save.day)), 1, 25);
        }
        #endregion
    }

    public struct WaveData : IBufferElementData
    {
        public Vector3 position;
        public int count;
        public ennemyType ennemyType;
    }

    public enum ennemyType { normal, speedy, boss }
}
