using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Beacon
{
    public delegate float EasingDelegate(float t);

    public class Coroutines : MonoBehaviour
    {
        public static IEnumerator MySunCouroutine(Light light, float startValue, float endValue, float duration, EasingDelegate easingFunc)
        {
            float elapsedTime = 0;

            while (elapsedTime < duration && light)
            {
                float k = elapsedTime / duration;
                light.intensity = Mathf.Lerp(startValue, endValue, easingFunc(k));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (light)
                light.intensity = endValue;
        }

        public static IEnumerator MyMenuOpenCouroutine(GameObject parent, RectTransform menu, Vector2 startValue, Vector2 endValue, float duration, EasingDelegate easingFunc)
        {
            if (parent)
                parent.SetActive(true);

            float elapsedTime = 0;

            while (elapsedTime < duration && menu)
            {
                float k = elapsedTime / duration;
                menu.anchoredPosition = Vector2.Lerp(startValue, endValue, easingFunc(k));
                elapsedTime += Time.fixedDeltaTime;
                yield return null;
            }

            if (menu)
                menu.anchoredPosition = endValue;
        }

        public static IEnumerator MyMenuCloseCouroutine(GameObject parent, RectTransform menu, Vector2 startValue, Vector2 endValue, float duration, EasingDelegate easingFunc)
        {
            float elapsedTime = 0;

            while (elapsedTime < duration && menu)
            {
                float k = elapsedTime / duration;
                menu.anchoredPosition = Vector2.Lerp(startValue, endValue, easingFunc(k));
                elapsedTime += Time.fixedDeltaTime;
                yield return null;
            }

            if (menu)
                menu.anchoredPosition = endValue;

            if (parent)
                parent.SetActive(false);
        }

        public static IEnumerator MyImageFadeCouroutine(Image image, Color startValue, Color endValue, float duration, EasingDelegate easingFunc)
        {
            float elapsedTime = 0;

            while (elapsedTime < duration && image)
            {
                float k = elapsedTime / duration;
                image.color = Color.Lerp(startValue, endValue, easingFunc(k));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (image)
                image.color = endValue;
        }

        public static IEnumerator MyLightLevelChangeCouroutine(RectTransform lightLevel, Vector2 startValue, Vector2 endValue, float duration, EasingDelegate easingFunc)
        {
            float elapsedTime = 0;

            while (elapsedTime < duration && lightLevel)
            {
                float k = elapsedTime / duration;
                lightLevel.sizeDelta = Vector2.Lerp(startValue, endValue, easingFunc(k));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (lightLevel)
                lightLevel.sizeDelta = endValue;
        }

        public static IEnumerator MyLightTextChangeCouroutine(TextMeshProUGUI text, float startValue, float endValue, string rightText, float duration, EasingDelegate easingFunc)
        {
            float elapsedTime = 0;

            while (elapsedTime < duration && text)
            {
                float k = elapsedTime / duration;
                text.text = Round(Mathf.Lerp(startValue, endValue, easingFunc(k)), 1).ToString() + rightText;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (text)
                text.text = endValue.ToString() + rightText;
        }

        public static float Round(float value, int digits)
        {
            float mult = Mathf.Pow(10.0f, (float)digits);
            return Mathf.Round(value * mult) / mult;
        }
    }
}
