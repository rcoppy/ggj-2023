// using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GGJ2022
{
    public class ScreenUVScroller : MonoBehaviour
    {
        Renderer _rend;

        private void Update()
        {
            UpdateTexture();
        }

        private void OnEnable()
        {
            UpdateTexture();
        }

        private void Awake()
        {
            _rend = GetComponent<Renderer>();
        }

        private void UpdateTexture()
        {
            

            // update UVs 
            try
            {
                //MaterialPropertyBlock props = new MaterialPropertyBlock();

                //var dif = coords.Start - coords.End; 

                //float w, h;
                //w = Mathf.Abs(dif.x);
                //h = Mathf.Abs(dif.y);

                //props.SetVector("_BaseMap_ST", new Vector4(w, h, coords.Start.x, coords.Start.y));
                //props.SetVector("_EmissionMap_ST", new Vector4(w, h, coords.Start.x, coords.Start.y));

                //_rend.SetPropertyBlock(props); 

                // the below updates the material coords globally
                // what we want is the above, per instance via property block

                var offset = new Vector2(Random.value, Random.value);

                if (Application.isEditor)
                {
                    // this will corrupt the material between git branches: 
                    // _rend.sharedMaterial.SetTextureOffset("_MainTex", offset);
                    // _rend.sharedMaterial.SetTextureOffset("_EmissionMap", offset);
                } else
                {
                    _rend.material.SetTextureOffset("_MainTex", offset);
                    _rend.material.SetTextureOffset("_EmissionMap", offset);
                }
            }
            catch
            {
                Debug.LogError("Your atlas data isn't configured properly.");
            }
        }
    }
}
