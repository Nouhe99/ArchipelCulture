using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingReward : MonoBehaviour
{
    [SerializeField] private int maximumVisibleObjects = 30;
    [SerializeField] private float moveDuration = 0.75f;
    [SerializeField] private float spawnDuration = 0.25f;
    [SerializeField] private float expandDuration = 1f;
    [SerializeField] private float spawnRadius = 1f;
    [SerializeField] private Vector3 targetScale = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private Vector3 originalScale = Vector3.one;

    [SerializeField] private Image rewardObjectPrefab;
    private Transform startPoint;
    private Transform targetPoint;
    [SerializeField] private int rewardCount = 0;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private TMP_Text quantityField;
    [SerializeField] private AudioClip rewardClip;

    private void Awake()
    {
        quantityField.transform.localScale = Vector3.zero;
        rewardObjectPrefab.gameObject.SetActive(false);
    }

    public void PlayRewardAnimation(Transform startPoint, int rewardQuantity, Transform target, Sprite sprite)
    {
        this.startPoint = startPoint;
        targetPoint = target;
        rewardCount = rewardQuantity;
        quantityField.text = $"+{rewardCount}";
        rewardObjectPrefab.sprite = sprite;
        StopAllCoroutines();
        StartCoroutine(PlayRewardAnimation_Coroutine());
        StartCoroutine(AnimateText_Coroutine());
    }

    private IEnumerator PlayRewardAnimation_Coroutine()
    {
        for (int i = 0; i < Mathf.Min(rewardCount, maximumVisibleObjects); i++)
        {
            GameObject copy = Instantiate(rewardObjectPrefab.gameObject, startPoint.position, transform.rotation, transform);
            copy.SetActive(true);
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            StartCoroutine(ExpandInCircle_Coroutine(copy, randomOffset));
            yield return new WaitForSeconds(Mathf.Min(0.05f, spawnDuration / rewardCount));
        }
        yield return new WaitForSeconds(expandDuration + moveDuration);
        EndRewardAnimation();
    }

    private void EndRewardAnimation()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    private IEnumerator ExpandInCircle_Coroutine(GameObject obj, Vector3 offset)
    {
        // Expand in circle
        float elapsedTimeExpand = 0f;
        Vector3 startPosition = obj.transform.position;
        Vector3 startPositionExpanded = startPosition + offset;
        while (elapsedTimeExpand < expandDuration)
        {
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, animationCurve.Evaluate(Mathf.Clamp01(elapsedTimeExpand / expandDuration)));
            obj.transform.position = Vector3.Lerp(startPosition, startPositionExpanded, animationCurve.Evaluate(Mathf.Clamp01(elapsedTimeExpand / expandDuration)));
            elapsedTimeExpand += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(MoveObject_Coroutine(obj, targetPoint));
    }
    private IEnumerator MoveObject_Coroutine(GameObject obj, Transform target)
    {
        // Move to destination
        float elapsedTime = 0f;
        Vector3 startPosition = obj.transform.position;
        while (elapsedTime < moveDuration)
        {
            yield return null;
            obj.transform.position = Vector3.Lerp(startPosition, target.position, animationCurve.Evaluate(Mathf.Clamp01(elapsedTime / moveDuration)));
            obj.transform.localScale = Vector3.Lerp(originalScale, targetScale, animationCurve.Evaluate(Mathf.Clamp01(elapsedTime / moveDuration)));
            elapsedTime += Time.deltaTime;
        }
        PlayAudio.Instance.PlayOneShot(rewardClip);
        Destroy(obj);
    }

    private IEnumerator AnimateText_Coroutine()
    {
        // Define positions
        Vector3 startPosition = new Vector3(startPoint.position.x, startPoint.position.y + spawnRadius);
        Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y + spawnRadius / 2f);
        quantityField.transform.position = startPosition;

        // Text appearance
        float elapsedTime = 0f;
        while (elapsedTime < spawnDuration)
        {
            yield return null;
            quantityField.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, animationCurve.Evaluate(Mathf.Clamp01(elapsedTime / spawnDuration)));
            elapsedTime += Time.deltaTime;
        }

        // Text moving upward
        elapsedTime = 0f;
        while (elapsedTime < expandDuration)
        {
            yield return null;
            quantityField.transform.position = Vector3.Lerp(startPosition, targetPosition, animationCurve.Evaluate(Mathf.Clamp01(elapsedTime / expandDuration)));
            elapsedTime += Time.deltaTime;
        }

        // Text disappearance
        elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            yield return null;
            quantityField.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, animationCurve.Evaluate(Mathf.Clamp01(elapsedTime / moveDuration)));
            elapsedTime += Time.deltaTime;
        }
    }

    private void OnDisable()
    {
        EndRewardAnimation();
    }
}
