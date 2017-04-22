using UnityEngine;

public class Painter : MonoBehaviour
{
    [SerializeField] private PaintMode paintMode;

    [SerializeField] private PaintReceiver paintReceiver;

    [SerializeField] private Transform paintingTransform;

    [SerializeField] private Texture2D brush;

    [SerializeField] private Color color;

    [SerializeField] private MeshRenderer[] colouredParts;

    [SerializeField] private float spacing = 1f;
    
    private float currentAngle = 0f;
    private float lastAngle = 0f;

    private Stamp stamp;

	private Vector2? lastDrawPosition = null;

    private void Awake()
    {
        stamp = new Stamp(brush);
        stamp.mode = paintMode;

        ChangeColour(color);

        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material.renderQueue -= 1;
        }
    }

    private void OnTriggerStay(Collider otherCollider)
    {
        if (otherCollider.GetComponent<PaintReceiver>() != null)
        {
            Ray ray = new Ray(paintingTransform.position - paintingTransform.forward, paintingTransform.forward);
            RaycastHit hit;

            if (otherCollider.Raycast(ray, out hit, 10f))
            {
                if (lastDrawPosition.HasValue && lastDrawPosition.Value != hit.textureCoord)
                {
                    Debug.Log("Draw Line: " + Time.time.ToString());
                    paintReceiver.CreateSplash(hit.textureCoord, stamp, color, currentAngle);
                    paintReceiver.DrawLine(stamp, lastDrawPosition.Value, hit.textureCoord, lastAngle, currentAngle, color, spacing);
                }
                else
                {
                    paintReceiver.CreateSplash(hit.textureCoord, stamp, color, currentAngle);

                    Debug.Log("Draw Splash: " + Time.time.ToString());
                }

                lastAngle = currentAngle;

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

    public void SetRotation(float newAngle)
    {
        currentAngle = newAngle;
    }
}
