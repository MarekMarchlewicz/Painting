using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CreateSquare : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Texture2D tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);

        for(int x = 0; x < 8; x++)
        {
            for(int y = 0; y < 8; y++)
            {
                Color col;
                if((x < 2 || x > 5) || (y < 2 || y > 5))
                {
                    col = Color.clear;
                } 
                else
                {
                    col = Color.red;
                }

                tex.SetPixel(x, y, col);
            }
        }

        tex.Apply();

        File.WriteAllBytes(Application.dataPath + "/Tex1.png", tex.EncodeToPNG());
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
