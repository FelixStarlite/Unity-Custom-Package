using UnityEngine;

/// <summary>
/// 此腳本負責處理紋理相關的一些操作
/// </summary>
public static class TextureManipulator
{
    /// <summary>
    /// 將紋理旋轉90度
    /// </summary>
    /// <param name="originalTexture">原始紋理</param>
    public static Texture2D RotateTexture90Degrees(Texture2D originalTexture)
    {
        int width = originalTexture.width;
        int height = originalTexture.height;
        Texture2D rotatedTexture = new Texture2D(height, width);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                rotatedTexture.SetPixel(j, width - 1 - i, originalTexture.GetPixel(i, j));
            }
        }

        rotatedTexture.Apply();
        return rotatedTexture;
    }
}
