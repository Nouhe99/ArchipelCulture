using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldMapController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Camera worldMapCamera;
    [SerializeField] private RawImage worldMapRawImage;
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 cursor = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(worldMapRawImage.rectTransform, eventData.pressPosition, eventData.pressEventCamera, out cursor))
        {
            Texture texture = worldMapRawImage.texture;
            Rect rect = worldMapRawImage.rectTransform.rect;

            float coordX = Mathf.Clamp(0, (((cursor.x - rect.x) * texture.width) / rect.width), texture.width);
            float coordY = Mathf.Clamp(0, (((cursor.y - rect.y) * texture.height) / rect.height), texture.height);

            float calX = coordX / texture.width;
            float calY = coordY / texture.height;


            cursor = new Vector2(calX, calY);
            CastRayToWorld(cursor);
        }
    }

    private void CastRayToWorld(Vector2 vec)
    {
        Ray mapRay = worldMapCamera.ScreenPointToRay(new Vector2(vec.x * worldMapCamera.pixelWidth, vec.y * worldMapCamera.pixelHeight));
        RaycastHit worldMapHit;
        Debug.DrawRay(new Vector2(vec.x, vec.y), new Vector2(vec.x * worldMapCamera.pixelWidth, vec.y * worldMapCamera.pixelHeight), Color.red, 10f);
        if (Physics.Raycast(mapRay, out worldMapHit, Mathf.Infinity))
        {
            BoatController.Instance?.GoTo(worldMapHit.point);
        }
    }
}
