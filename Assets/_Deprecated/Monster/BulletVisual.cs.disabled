using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BulletVisual : MonoBehaviour
{
    [SerializeField] private float bulletLength = 0.6f;   // 탄두 길이
    [SerializeField] private float bulletWidth = 0.4f;    // 탄두 두께
    [SerializeField] private Color bulletColor = new Color(2f, 0.7f, 0.15f); // 주황 (밝게)

    void Awake()
    {
        var lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 2;
        lr.widthMultiplier = bulletWidth;
        lr.numCapVertices = 8;              // 끝 둥글게 = 캡슐
        lr.startColor = lr.endColor = bulletColor;
        lr.SetPosition(0, new Vector3(-bulletLength / 2f, 0f, 0f));
        lr.SetPosition(1, new Vector3(bulletLength / 2f, 0f, 0f));
    }
}