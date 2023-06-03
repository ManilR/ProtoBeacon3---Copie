using System.Collections;
using System.Collections.Generic;
using Kryz.Tweening;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

namespace Beacon
{
    public class LightManager : MonoBehaviour
    {
        private EntityManager entityManager;
        private World defaultWorld;

        public GameObject lightElement;

        private void Start()
        {
            defaultWorld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultWorld.EntityManager;
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener<GamePlayDayEvent>(onGamePlayDayEvent);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<GamePlayDayEvent>(onGamePlayDayEvent);
        }

        private void onGamePlayDayEvent(GamePlayDayEvent e)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Building, Health, LocalTransform>();
            EntityQuery buildingQuery = entityManager.CreateEntityQuery(builder);

            NativeArray<Entity> entities = buildingQuery.ToEntityArray(Allocator.TempJob);

            if (entities.Length != 0)
            {
                Tools.LOG(this, "Found " + entities.Length + " entities");
                for (int i = 0; i < entities.Length; i++)
                {
                    Building building = entityManager.GetComponentData<Building>(entities[i]);
                    Health health = entityManager.GetComponentData<Health>(entities[i]);
                    LocalTransform position = entityManager.GetComponentData<LocalTransform>(entities[i]);

                    float lightValue = building.lvlProduction * health.health * 0.01f * (building.mode == Mode.production ? 2 : 1);

                    if (lightValue > 0)
                    {
                        GameObject LightElement = Instantiate(lightElement, position.Position, Quaternion.identity);
                        LightElement.GetComponent<Light>().intensity = lightValue;

                        StartCoroutine(MyLightUpCouroutine(LightElement, position.Position, 1, EasingFunctions.OutCubic));
                    }

                    StartCoroutine(MyFunctionLoadWithDelay());
                }
            }
            else
            {
                Tools.LOG(this, "Found no entities");

            }
        }

        IEnumerator MyFunctionLoadWithDelay()
        {
            yield return new WaitForSecondsRealtime(1f);
        }

        public IEnumerator MyLightUpCouroutine(GameObject lightElement, Vector3 startValue, float duration, EasingDelegate easingFunc)
        {
            float elapsedTime = 0;
            Vector3 endPos = new Vector3(startValue.x, startValue.y + 10, startValue.z);
            float lightIntensity = lightElement.GetComponent<Light>().intensity;

            while (elapsedTime < duration && lightElement)
            {
                float k = elapsedTime / duration;
                lightElement.transform.position = Vector3.Lerp(startValue, endPos, easingFunc(k));
                lightElement.GetComponent<Light>().intensity = Mathf.Lerp(lightIntensity, lightIntensity * 100, easingFunc(k));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (lightElement)
                lightElement.transform.position = endPos;

            StartCoroutine(MyLightToBeaconCouroutine(lightElement, endPos, 1, EasingFunctions.OutCubic));
        }

        public IEnumerator MyLightToBeaconCouroutine(GameObject lightElement, Vector3 startValue, float duration, EasingDelegate easingFunc)
        {
            float elapsedTime = 0;
            Vector3 endPos = new Vector3(0, 33, 0);
            float lightIntensity = lightElement.GetComponent<Light>().intensity;

            while (elapsedTime < duration && lightElement)
            {
                float k = elapsedTime / duration;
                lightElement.transform.position = Vector3.Lerp(startValue, endPos, easingFunc(k));
                //lightElement.GetComponent<Light>().intensity = Mathf.Lerp(lightIntensity, lightIntensity * 100, easingFunc(k));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (lightElement)
                lightElement.transform.position = endPos;

            StartCoroutine(MyLightConversionCouroutine(lightElement, 0.5f, EasingFunctions.OutExpo)) ;
        }

        public IEnumerator MyLightConversionCouroutine(GameObject lightElement, float duration, EasingDelegate easingFunc)
        {
            float elapsedTime = 0;
            float lightIntensity = lightElement.GetComponent<Light>().intensity;
            float lightRange = lightElement.GetComponent<Light>().range;

            while (elapsedTime < duration && lightElement)
            {
                float k = elapsedTime / duration;
                lightElement.transform.position = Vector3.Lerp(new Vector3(0, 33, 0), new Vector3(0, 0, 0), easingFunc(k));
                lightElement.GetComponent<Light>().intensity = Mathf.Lerp(lightIntensity, lightIntensity * 1000, easingFunc(k));
                lightElement.GetComponent<Light>().range = Mathf.Lerp(lightRange, lightRange/10, easingFunc(k));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            EntityQuery beaconQuery = entityManager.CreateEntityQuery(new ComponentType[] { typeof(Beacon) });
            Beacon beaconData;
            Entity beacon = Entity.Null;

            foreach (var entity in beaconQuery.ToEntityArray(Allocator.Temp))
                beacon = entity;

            if (beacon != Entity.Null && lightElement)
            {
                beaconData = entityManager.GetComponentData<Beacon>(beacon);
                float lightToAdd = lightIntensity / 100;
                beaconData.lightLevel = Mathf.Clamp(beaconData.lightLevel + lightToAdd, 0f, beaconData.MAX_LIGHT_LEVEL);
                entityManager.SetComponentData<Beacon>(beacon, beaconData);

                Tools.LOG(this, "Light added by building is " + lightToAdd);
            }

            Destroy(lightElement);
        }
    }
}