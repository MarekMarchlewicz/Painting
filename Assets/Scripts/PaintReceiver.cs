using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class PaintReceiver : MonoBehaviour
{
    private Texture2D texture;
    private Texture2D newTexture;
	private Color32[] originalTexture;
	private Color32[] currentTexture;
    private float currentStampRotation = 0f;

	private int textureWidth;
	private int textureHeight;

    private void Awake()
    {
        texture = GetComponent<MeshRenderer>().material.mainTexture as Texture2D;

        textureWidth = texture.width;
        textureHeight = texture.height;

        originalTexture = texture.GetPixels32();
        currentTexture = new Color32[textureWidth * textureHeight];
        originalTexture.CopyTo(currentTexture, 0);

        newTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false, true);
		newTexture.SetPixels32(originalTexture);
        newTexture.Apply();

        GetComponent<MeshRenderer>().material.mainTexture = newTexture;
    }

    public void CreateSplash(Vector2 uvPosition, Stamp stamp, Color color, float stampRotation = 0f)
    {
        stamp.SetRotation(stampRotation);

		PaintOver (stamp, (Color32)color, uvPosition);
    }

    public void DrawLine(Stamp stamp, Vector2 startUVPosition, Vector2 endUVPosition, float startStampRotation, float endStampRotation, Color color, float spacing)
    {
        Vector2 uvDistance = endUVPosition - startUVPosition;

        Vector2 pixelDistance = new Vector2(uvDistance.x * textureWidth, uvDistance.y * textureHeight);

        int stampsNo = Mathf.RoundToInt(pixelDistance.magnitude / (new Vector2(stamp.Width, stamp.Height).magnitude / spacing));

        if (stampsNo > 0)
        {
            for (int i = 0; i <= stampsNo; i++)
            {
                float lerp = i / (float)stampsNo;

                Vector2 uvPosition = Vector2.Lerp(startUVPosition, endUVPosition, lerp);

                if (endStampRotation != startStampRotation)
                    stamp.SetRotation(Mathf.Lerp(startStampRotation, endStampRotation, lerp));

                PaintOver(stamp, (Color32)color, uvPosition);
            }

            newTexture.SetPixels32(currentTexture);
            newTexture.Apply();
        }
    }

    private void PaintOver(Stamp stamp, Color32 color, Vector2 uvPosition)
    {
        int paintStartPositionX = (int)((uvPosition.x * textureWidth) - stamp.Width / 2f);
		int paintStartPositionY = (int)((uvPosition.y * textureHeight) - stamp.Height / 2f);

		int paintStartPositionXClamped = Mathf.Clamp(paintStartPositionX, 0, textureWidth);
        int paintStartPositionYClamped = Mathf.Clamp(paintStartPositionY, 0, textureHeight);

        int paintEndPositionXClamped = Mathf.Clamp(paintStartPositionX + stamp.Width, 0, textureWidth);
        int paintEndPositionYClamped = Mathf.Clamp(paintStartPositionY + stamp.Height, 0, textureHeight);

        int totalWidth = paintEndPositionXClamped - paintStartPositionXClamped;
        int totalHeight = paintEndPositionYClamped - paintStartPositionYClamped;

		Color32 newColor;
        Color32 textureColor;
        int alpha;

        for (int x = 0; x < totalWidth; x++)
        {
            for(int y = 0; y < totalHeight; y++)
            {
                int stampX = paintStartPositionXClamped - paintStartPositionX + x;
                int stampY = paintStartPositionYClamped - paintStartPositionY + y;

				int texturePosition = paintStartPositionXClamped + x + (paintStartPositionYClamped + y) * textureWidth;

                if (stamp.mode == PaintMode.Erase)
                    color = originalTexture[texturePosition];

                alpha = (int)(stamp.CurrentPixels[stampX + stampY * stamp.Width] * 255f);

				textureColor = currentTexture[texturePosition];

                newColor.r = (byte)(color.r * alpha / 255 + textureColor.r * textureColor.a * (255 - alpha) / (255 * 255));
                newColor.g = (byte)(color.g * alpha / 255 + textureColor.g * textureColor.a * (255 - alpha) / (255 * 255));
                newColor.b = (byte)(color.b * alpha / 255 + textureColor.b * textureColor.a * (255 - alpha) / (255 * 255));
                newColor.a = (byte)(alpha + textureColor.a * (255 - alpha) / 255);

                currentTexture[texturePosition] = newColor;
            }
        }
    }
}
