using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIAnimation : MonoBehaviour
{
    [Serializable]
    private enum TypeAnimation
    {
        Move,
        Size,
        Color
    }
    [SerializeField] private TypeAnimation typeAnim;
    [Header("Move")]
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;
    [Header("Size")]
    [SerializeField] private float sizeStart;
    [SerializeField] private float sizeEnd;
    [Header("Color")]
    [SerializeField] private Color colorEnd;
    [Header("")]
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private bool startOnAwake = false;
    //[SerializeField] private bool destroyAtEnd = false;

    private bool cancelRequested = false;
    private bool animationRunning = false;

    private RectTransform rectTransform;


    private async void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (startOnAwake)
        {
            await AnimateFromStartToEndAsync();
            if (!isDestroy) Destroy(gameObject);
        }
    }
    private bool isDestroy = false;
    private async void OnDestroy()
    {
        isDestroy = true;
        await CancelAnimation();
    }

    public async Task<bool> AnimateFromStartToEndAsync()
    {
        await CancelAnimation();
        switch (typeAnim)
        {
            case TypeAnimation.Move:
                if (rectTransform == null) return false;
                return await AnimateFromToAsync(rectTransform.anchoredPosition3D, endPosition);

            case TypeAnimation.Size:
                return await SizeFromToAsync(rectTransform.localScale.x, sizeEnd);

            default:
                return false;
        }
    }

    public async Task<bool> AnimateFromEndToStartAsync()
    {
        await CancelAnimation();
        switch (typeAnim)
        {
            case TypeAnimation.Move:
                if (rectTransform == null) return false;
                return await AnimateFromToAsync(rectTransform.anchoredPosition3D, startPosition, false);

            case TypeAnimation.Size:
                return await SizeFromToAsync(rectTransform.localScale.x, sizeStart, false);

            default:
                return false;
        }

    }

    private async Task<bool> AnimateFromToAsync(Vector3 from, Vector3 to, bool open = true)
    {
        if (animationCurve.length <= 0 || rectTransform == null)
        {
            return false;
        }

        animationRunning = true;
        float timeElapsed = 0.0f;
        Keyframe lastKeyframe = animationCurve[animationCurve.length - 1];
        float animationTime = lastKeyframe.time;
        while (timeElapsed < animationTime)
        {
            timeElapsed += Time.deltaTime;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition3D = Vector3.Lerp(from, to, animationCurve.Evaluate(timeElapsed));
                if (panelGroup && open) panelGroup.alpha = animationCurve.Evaluate(timeElapsed);
                else if (panelGroup && !open) panelGroup.alpha = 1 - animationCurve.Evaluate(timeElapsed);
            }

            if (cancelRequested == false)
            {
                await Task.Yield();
            }
            else
            {
                cancelRequested = false;
                animationRunning = false;
                return false;
            }
        }
        animationRunning = false;
        return true;
    }

    public void ChangeStartPosition(Vector3 newPos)
    {
        startPosition = newPos;
    }

    private async Task<bool> SizeFromToAsync(float from, float to, bool open = true)
    {
        if (animationCurve.length <= 0 || rectTransform == null)
        {
            return false;
        }

        animationRunning = true;
        float timeElapsed = 0.0f;
        Keyframe lastKeyframe = animationCurve[animationCurve.length - 1];
        float animationTime = lastKeyframe.time;
        while (timeElapsed < animationTime)
        {
            timeElapsed += Time.deltaTime;
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(from * Vector3.one, to * Vector3.one, animationCurve.Evaluate(timeElapsed));
                if (panelGroup && open) panelGroup.alpha = animationCurve.Evaluate(timeElapsed);
                else if (panelGroup && !open) panelGroup.alpha = 1 - animationCurve.Evaluate(timeElapsed);
            }

            if (cancelRequested == false)
            {
                await Task.Yield();
            }
            else
            {
                cancelRequested = false;
                animationRunning = false;
                return false;
            }
        }
        animationRunning = false;
        return true;
    }

    #region Fade
    public async Task<bool> FadeInAsync()
    {
        Image img = GetComponent<Image>();
        if (img == null)
        {
            return false;
        }
        Color opaque = img.color;
        opaque.a = 1f;
        await CancelAnimation();
        return await FadeFromToColorAsync(img, img.color, opaque);
    }

    public async Task<bool> FadeOutAsync()
    {
        Image img = GetComponent<Image>();
        if (img == null)
        {
            return false;
        }
        Color transparent = img.color;
        transparent.a = 0f;
        await CancelAnimation();
        return await FadeFromToColorAsync(img, img.color, transparent);
    }

    public async Task<bool> FlashColor()
    {
        Image img = GetComponent<Image>();
        if (img == null)
        {
            return false;
        }
        Color imgColor = img.color;
        await CancelAnimation();
        await FadeFromToColorAsync(img, imgColor, colorEnd);
        return await FadeFromToColorAsync(img, colorEnd, imgColor);
    }

    private async Task<bool> FadeFromToColorAsync(Image image, Color from, Color to)
    {
        animationRunning = true;

        float timeElapsed = 0.0f;
        Keyframe lastKeyframe = animationCurve[animationCurve.length - 1];
        float animationTime = lastKeyframe.time;
        while (timeElapsed < animationTime)
        {
            timeElapsed += Time.deltaTime;
            image.color = Color.Lerp(from, to, animationCurve.Evaluate(timeElapsed));

            if (cancelRequested == false)
            {
                await Task.Yield();
            }
            else
            {
                cancelRequested = false;
                return false;
            }
        }
        animationRunning = false;
        return true;
    }
    #endregion

    public async Task CancelAnimation()
    {
        if (animationRunning && !cancelRequested)
        {
            cancelRequested = true;
            while (cancelRequested && animationRunning)
            {
                await Task.Yield();
            }
        }
    }
}
