using System.Collections;
using UnityEngine;
using Kryz.Tweening;

namespace Beacon
{
    public class LightMovement : MonoBehaviour
    {
        public delegate float EasingDelegate(float t);

        public float currentLightLevel;
        public float currentGoal;
        public bool moving;

        void OnEnable()
        {
            EventManager.Instance.AddListener<LightLevelChangedEvent>(onLightLevelChangedEvent);
            EventManager.Instance.AddListener<DataAppliedEvent>(onDataAppliedEvent);

            currentLightLevel = gameObject.transform.position.y;
        }

        void OnDisable()
        {
            EventManager.Instance.RemoveListener<LightLevelChangedEvent>(onLightLevelChangedEvent);
            EventManager.Instance.RemoveListener<DataAppliedEvent>(onDataAppliedEvent);

        }

        public static float Round(float value, int digits)
        {
            float mult = Mathf.Pow(10.0f, (float)digits);
            return Mathf.Round(value * mult) / mult;
        }

        public IEnumerator MyBeaconLightCouroutine(GameObject light, float startValue, float endValue, float duration, EasingDelegate easingFunc)
        {
            float elapsedTime = 0;
            moving = true;
            currentGoal = endValue;

            while (elapsedTime < duration)
            {
                float k = elapsedTime / duration;
                light.transform.position = Vector3.Lerp(new Vector3(0.0f, startValue, 0.0f), new Vector3(0.0f, endValue, 0.0f), easingFunc(k));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            light.transform.position = new Vector3(0.0f, endValue, 0.0f);
            currentLightLevel = endValue;
            moving = false;
        }

        #region Event Callbacks
        void onLightLevelChangedEvent(LightLevelChangedEvent e)
        {
            float factor = 0.05f;
            float goal = Round(Mathf.Clamp(e.lightLevel * factor - 9.0f, -9.0f, 16.0f), 2);
            if ((!moving || currentGoal != goal) && currentLightLevel != goal)
            {
                currentLightLevel = gameObject.transform.position.y;
                StartCoroutine(MyBeaconLightCouroutine(gameObject, currentLightLevel, goal, 2.0f, EasingFunctions.InOutQuad));
            }
        }

        void onDataAppliedEvent(DataAppliedEvent e)
        {
            float factor = 0.05f;
            float goal = Round(Mathf.Clamp(PreferenceManager.instance.GetSave().light * factor - 9.0f, -9.0f, 16.0f), 2);
            if ((!moving || currentGoal != goal) && currentLightLevel != goal)
            {
                currentLightLevel = gameObject.transform.position.y;
                StartCoroutine(MyBeaconLightCouroutine(gameObject, currentLightLevel, goal, 2.0f, EasingFunctions.InOutQuad));
            }
        }
        #endregion
    }
}


