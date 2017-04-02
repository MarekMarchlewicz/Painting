using UnityEngine;

public class Painter : MonoBehaviour
{
    [SerializeField] private Transform paintingTransform;

    [SerializeField] private Texture2D brush;

    [SerializeField] private PaintReceiver paintReceiver;

    [SerializeField] private Color color;

    [SerializeField] private MeshRenderer[] colouredParts;

    [SerializeField] private float spacing = 1f;

    [SerializeField]
    private float angle = 0;

    private Stamp stamp;

	private Vector2? lastDrawPosition = null;

    private void Awake()
    {
        stamp = new Stamp(brush);

        paintReceiver.SetStamp(stamp);

        ChangeColour(color);

        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material.renderQueue -= 1;
        }
    }

    private void OnTriggerStay(Collider otherCollider)
    {
        if (otherCollider == paintReceiver.GetComponent<Collider>())
        {
            Ray ray = new Ray(paintingTransform.position - paintingTransform.forward, paintingTransform.forward);
            RaycastHit hit;

            if (otherCollider.Raycast(ray, out hit, 10f))
            {
                if (lastDrawPosition.HasValue && lastDrawPosition.Value != hit.textureCoord)
                {
                    paintReceiver.DrawLine(lastDrawPosition.Value, hit.textureCoord, angle, ++angle, color, spacing);
                }
                else
                {
                    paintReceiver.CreateSplash(hit.textureCoord, stamp, color, angle);
                }

                lastDrawPosition = hit.textureCoord;
            }
            else
            {
                lastDrawPosition = null;
            }
        }
    }

    public void ChangeColour(Color newColor)
    {
        color = newColor;

        foreach(MeshRenderer renderer in colouredParts)
        {
            renderer.material.color = color;
        }
    }
}
