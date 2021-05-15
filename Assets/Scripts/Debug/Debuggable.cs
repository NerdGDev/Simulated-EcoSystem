using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Kit;

public class Debuggable : MonoBehaviour
{
    private StringBuilder debugReport;
    private static GUIStyle InfoStyle;

    private void Awake()
    {
        if (InfoStyle == null)
        {
            Texture2D grayTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false, false);
            grayTexture.SetPixel(1, 1, Color.gray.CloneAlpha(0.5f));
            grayTexture.alphaIsTransparency = true;
            grayTexture.anisoLevel = 0;
            grayTexture.Apply();
            InfoStyle = new GUIStyle()
            {
                alignment = TextAnchor.UpperLeft,
                normal = new GUIStyleState()
                {
                    textColor = Color.cyan,
                    background = grayTexture,
                },
                padding = new RectOffset(3, 3, 3, 3),
                border = new RectOffset(2, 2, 2, 2),
            };
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
