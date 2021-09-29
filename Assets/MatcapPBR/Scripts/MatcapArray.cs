using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatcapArray : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Texture> textureList;
    void Start()
    {
        SetTexture();
    }

    void SetTexture()
    {
        if (textureList != null && textureList.Count > 0)
        {
            Texture2DArray  array = new Texture2DArray(textureList[0].width,textureList[0].height,textureList.Count,TextureFormat.DXT1,1,false);
            for (int i = 0; i < textureList.Count; ++i)
            {
                Graphics.CopyTexture(textureList[i],0,0,array,i,0);    
            }
            Shader.SetGlobalTexture("matcap_maps",array);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
