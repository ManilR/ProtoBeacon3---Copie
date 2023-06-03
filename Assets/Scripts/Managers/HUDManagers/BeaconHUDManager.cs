using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.UI;
using TMPro;
using Kryz.Tweening;

namespace Beacon
{
    public class BeaconHUDManager : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private GameObject BeaconLife;
        [SerializeField] private GameObject BeaconText;

        private RectTransform BeaconLifeTransform;
        private TextMeshProUGUI BeaconLifeTransformText;

        private float lastLightLevel;

        private EntityManager entityManager;
        private World defaultWorld;

        public static BeaconHUDManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            BeaconLifeTransform = BeaconLife.GetComponent<RectTransform>();
            BeaconLifeTransformText = BeaconText.GetComponent<TextMeshProUGUI>();

            defaultWorld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultWorld.EntityManager;
        }

        private void FixedUpdate()
        {
            if (!GameManager.instance.isPlaying)
                return;

            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Beacon>();
            EntityQuery beaconQuery = entityManager.CreateEntityQuery(builder);
            Beacon beacon;
            beaconQuery.TryGetSingleton<Beacon>(out beacon);

            if (beacon.lightLevel != -1 && beacon.lightLevel != lastLightLevel)
            {
                EventManager.Instance.Raise(new LightLevelChangedEvent()
                {
                    lightLevel = beacon.lightLevel
                });

                float health = beacon.lightLevel;
                float maxhealth = beacon.MAX_LIGHT_LEVEL;

                float life = health * 263.0f / maxhealth;
                life = Mathf.Clamp(Round(life, 2), 0.0f, 263.0f);

                StartCoroutine(Coroutines.MyLightLevelChangeCouroutine(BeaconLifeTransform, BeaconLifeTransform.sizeDelta, new Vector2(life, 7), 1, EasingFunctions.OutCubic));
                StartCoroutine(Coroutines.MyLightTextChangeCouroutine(BeaconLifeTransformText, Round(lastLightLevel, 1), Round(health, 1), (" / " + beacon.MAX_LIGHT_LEVEL), 1, EasingFunctions.OutCubic));


                lastLightLevel = beacon.lightLevel;
            }                
        }

        public static float Round(float value, int digits)
        {
            float mult = Mathf.Pow(10.0f, (float)digits);
            return Mathf.Round(value * mult) / mult;
        }
    }
}
