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

	private float[] originalStampPixels;
    private float[] currentStampPixels;
    private int stampWidth;
	private int stampHeight;

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

	public void SetStamp(Stamp stamp)
    {
        stampWidth = stamp.Width;
        stampHeight = stamp.Height;

        originalStampPixels = new float[stampWidth * stampHeight];
        currentStampPixels = new float[stampWidth * stampHeight];

        stamp.Pixels.CopyTo(originalStampPixels, 0);
        stamp.Pixels.CopyTo(currentStampPixels, 0);
	}

    public void CreateSplash(Vector2 uvPosition, Stamp stamp, Color color, float stampRotation = 0f)
    {
        if (currentStampRotation != stampRotation)
            RotateStamp(stampRotation);

		PaintOver ((Color32)color, uvPosition);
    }

    public void DrawLine(Vector2 startUVPosition, Vector2 endUVPosition, float startStampRotation, float endStampRotation, Color color, float spacing)
    {
        Vector2 uvDistance = endUVPosition - startUVPosition;

        Vector2 pixelDistance = new Vector2(uvDistance.x * textureWidth, uvDistance.y * textureHeight);

        int stampsNo = Mathf.RoundToInt(pixelDistance.magnitude / (new Vector2(stampWidth, stampHeight).magnitude / spacing));

        if (stampsNo > 0)
        {
            for (int i = 0; i <= stampsNo; i++)
            {
                float lerp = i / (float)stampsNo;

                Vector2 uvPosition = Vector2.Lerp(startUVPosition, endUVPosition, lerp);

                if (endStampRotation != startStampRotation)
                    RotateStamp(Mathf.Lerp(startStampRotation, endStampRotation, lerp));

                PaintOver((Color32)color, uvPosition);
            }

            newTexture.SetPixels32(currentTexture);
            newTexture.Apply();
        }
    }

    private void RotateStamp(float stampRotation)
    {
        float sin = Mathf.Sin(Mathf.Deg2Rad * stampRotation);
        float cos = Mathf.Cos(Mathf.Deg2Rad * stampRotation);

        float x0 = stampWidth / 2f;
        float y0 = stampHeight / 2f;

        float deltaX, deltaY;

        int xp, yp;

        float rotatedPixelValue;

        for (int x = 0; x < stampWidth; x++)
        {
            for(int y = 0; y < stampHeight; y++)
            {
                deltaX = x - x0;
                deltaY = y - y0;

                xp = Mathf.RoundToInt(deltaX * cos - deltaY * sin + x0);
                yp = Mathf.RoundToInt(deltaX * sin + deltaY * cos + y0);

                if (xp >= 0 && xp < stampWidth && yp >= 0 && yp < stampHeight)
                    rotatedPixelValue = originalStampPixels[xp + stampWidth * yp];
                else
                    rotatedPixelValue = 0f;

                currentStampPixels[x + stampWidth * y] = rotatedPixelValue;
            }
        }

        currentStampRotation = stampRotation;
    }

    private void PaintOver(Color32 color, Vector2 uvPosition)
    {
        if (currentStampPixels == null)
        {
            Debug.LogError("Stamp is not set");
        }

        int paintStartPositionX = (int)((uvPosition.x * textureWidth) - stampWidth / 2f);
		int paintStartPositionY = (int)((uvPosition.y * textureHeight) - stampHeight / 2f);

		int paintStartPositionXClamped = Mathf.Clamp(paintStartPositionX, 0, textureWidth);
        int paintStartPositionYClamped = Mathf.Clamp(paintStartPositionY, 0, textureHeight);

        int paintEndPositionXClamped = Mathf.Clamp(paintStartPositionX + stampWidth, 0, textureWidth);
        int paintEndPositionYClamped = Mathf.Clamp(paintStartPositionY + stampHeight, 0, textureHeight);

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

				alpha = (int)(currentStampPixels[stampX + stampY * stampWidth] * 255f);

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
