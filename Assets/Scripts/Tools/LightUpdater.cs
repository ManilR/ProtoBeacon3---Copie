using System.Collections;
using UnityEngine;
using Kryz.Tweening;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Beacon
{
    public class LightUpdater : MonoBehaviour
    {
        private EntityManager entityManager;
        private World defaultWorld;

        private Entity beaconEntity;
        private Beacon beacon;
        private EntityQuery beaconQuery;

        public float goal;
        public float currentAngle;
        public float currentGoal;
        public bool moving = false;

        [SerializeField, Range(1, 100)] private float multiplier;

        private void Start()
        {
            defaultWorld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultWorld.EntityManager;
            beaconQuery = entityManager.CreateEntityQuery(new ComponentType[] { typeof(Beacon) });
        }

        private void Update()
        {
            foreach (var entity in beaconQuery.ToEntityArray(Allocator.Temp))
                beaconEntity = entity;

            if (beaconEntity != Entity.Null)
            {
                beacon = entityManager.GetComponentData<Beacon>(beaconEntity);

                goal = Mathf.Atan((beacon.lightLevel / multiplier) / 100) * 2 * Mathf.Rad2Deg;

                if ((!moving || currentGoal != goal) && currentAngle != goal)
                {
                    currentAngle = gameObject.GetComponent<Light>().spotAngle;
                    StartCoroutine(MyBeaconLightCouroutine(gameObject.GetComponent<Light>(), currentAngle, goal, 1, EasingFunctions.InOutQuad));
                }
            }
        }

        public IEnumerator MyBeaconLightCouroutine(Light light, float startValue, float endValue, float duration, EasingDelegate easingFunc)
        {
            float elapsedTime = 0;
            moving = true;
            currentGoal = endValue;

            while (elapsedTime < duration && light)
            {
                float k = elapsedTime / duration;
                light.innerSpotAngle = Mathf.Lerp(startValue - 5, endValue - 5, easingFunc(k));
                light.spotAngle = Mathf.Lerp(startValue, endValue, easingFunc(k));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (light)
            {
                light.innerSpotAngle = (endValue - 5);
                light.spotAngle = (endValue);
            }

            currentAngle = endValue;
            moving = false;
        }

        public static float Round(float value, int digits)
        {
            float mult = Mathf.Pow(10.0f, (float)digits);
            return Mathf.Round(value * mult) / mult;
        }
    }
}


