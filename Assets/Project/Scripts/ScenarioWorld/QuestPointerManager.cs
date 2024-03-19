using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestPointerManager : MonoBehaviour
{
    [SerializeField] private GameObject pointerTemplate;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private float borderSize = 50f;

    private List<QuestPointer> questPointers;

    private void Awake()
    {
        pointerTemplate.SetActive(false);
        questPointers = new();
    }

    private void Update()
    {
        foreach (QuestPointer questPointer in questPointers)
        {
            questPointer.Update();
        }
    }

    public void CreatePointer(Vector3 targetPosition)
    {
        GameObject pointerGameObject = Instantiate(pointerTemplate);
        pointerGameObject.SetActive(true);
        pointerGameObject.transform.SetParent(transform, false);
        QuestPointer questPointer = new QuestPointer(targetPosition, pointerGameObject, uiCamera, borderSize);
        questPointers.Add(questPointer);
    }

    public void DestroyQuestPointer(QuestPointer questPointer)
    {
        questPointers.Remove(questPointer);
        questPointer.DestroySelf();
    }

    public class QuestPointer
    {
        private Vector3 targetPosition;
        private GameObject pointerGameObject;
        private Camera uiCamera;
        private RectTransform pointerRectTransform;
        private Image arrowImage;
        private Image markerImage;
        private float borderSize;

        public QuestPointer(Vector3 targetPosition, GameObject pointerGameObject, Camera uiCamera, float borderSize)
        {
            this.targetPosition = targetPosition;
            this.pointerGameObject = pointerGameObject;
            this.uiCamera = uiCamera;
            this.borderSize = borderSize;
            pointerRectTransform = pointerGameObject.GetComponent<RectTransform>();
            arrowImage = pointerGameObject.transform.Find("Arrow").GetComponent<Image>();
            markerImage = pointerGameObject.transform.Find("Marker").GetComponent<Image>();
        }

        public void Update()
        {
            RotatePointerTowardsTargetPosition();

            borderSize = 50f;
            Vector3 targetPositionScreenPoint = uiCamera.WorldToScreenPoint(targetPosition);
            bool isOffScreen = targetPositionScreenPoint.x <= borderSize || targetPositionScreenPoint.x >= Screen.width - borderSize || targetPositionScreenPoint.y <= borderSize || targetPositionScreenPoint.y >= Screen.height - borderSize;

            if (isOffScreen)
            {
                arrowImage.gameObject.SetActive(true);
                markerImage.gameObject.SetActive(false);
                Vector3 cappedTargetScreenPosition = targetPositionScreenPoint;
                cappedTargetScreenPosition.x = Mathf.Clamp(cappedTargetScreenPosition.x, borderSize, Screen.width - borderSize);
                cappedTargetScreenPosition.y = Mathf.Clamp(cappedTargetScreenPosition.y, borderSize, Screen.height - borderSize);


                Vector3 pointerWorldPosition = uiCamera.ScreenToWorldPoint(cappedTargetScreenPosition);
                pointerRectTransform.position = pointerWorldPosition;
                pointerRectTransform.localPosition = new Vector3(pointerRectTransform.localPosition.x, pointerRectTransform.localPosition.y, 0f);
            }
            else
            {
                arrowImage.gameObject.SetActive(false);
                markerImage.gameObject.SetActive(true);
                Vector3 pointerWorldPosition = uiCamera.ScreenToWorldPoint(targetPositionScreenPoint);
                pointerRectTransform.position = pointerWorldPosition;
                pointerRectTransform.localPosition = new Vector3(pointerRectTransform.localPosition.x, pointerRectTransform.localPosition.y, 0f);

                pointerRectTransform.localEulerAngles = Vector3.zero;
            }
        }

        private void RotatePointerTowardsTargetPosition()
        {
            Vector3 toPosition = targetPosition;
            Vector3 fromPosition = Camera.main.transform.position;
            fromPosition.z = 0f;
            Vector3 dir = (toPosition - fromPosition).normalized;
            float angle = GetAngleFromVectorFloat(dir);
            pointerRectTransform.localEulerAngles = new Vector3(0, 0, angle);
        }

        private float GetAngleFromVectorFloat(Vector3 vector)
        {
            float angle = Mathf.Atan2(vector.y, vector.x);
            float degrees = 180 * angle / Mathf.PI;
            return (360 + Mathf.Round(degrees)) % 360;
        }

        public void DestroySelf()
        {
            Destroy(pointerGameObject);
        }
    }
}
