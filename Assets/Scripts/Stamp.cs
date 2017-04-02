using UnityEngine;

public class Stamp
{
	public Stamp(Texture2D stampTexture)
    {
		Width = stampTexture.width;
		Height = stampTexture.height;

		Pixels = new float[Width * Height];

		for(int x = 0; x < Width; x++)
		{
			for(int y = 0; y < Height; y++)
			{
				Pixels[x + y * Width] = stampTexture.GetPixel(x, y).a;
			}
		}
    }

    public float[] Pixels { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }
}
