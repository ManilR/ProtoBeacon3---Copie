using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuRotateLight : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI titre;
    private Color32 fadedColor = new Color32(0, 0, 0, 0);
    private Color32 fullColor = new Color32(0, 0, 0, 255);


    private void OnEnable()
    {
        StartCoroutine(Rotate(5.0f));
    }

    IEnumerator Rotate(float duration)
    {
        float startRotation = transform.eulerAngles.y;
        float endRotation = startRotation + 120.0f;
        float t = 0.0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float yRotation = Mathf.Lerp(startRotation, endRotation, t / duration) % 360.0f;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation,
            transform.eulerAngles.z);
            yield return null;
        }

        StartCoroutine(TextFade(1.0f));
    }

    IEnumerator TextFade(float duration)
    {
        float t = 0.0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            titre.color = Color32.Lerp(fadedColor, fullColor, t / duration);
            yield return null;
        }
    }
}

