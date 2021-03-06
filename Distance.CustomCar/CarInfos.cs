﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomCar
{
    public class MaterialInfos
    {
        public int diffuseIndex = -1;
        public int emitIndex = -1;
        public Material material;
        public int normalIndex = -1;

        public void ReplaceMaterialInRenderer(Renderer r, int materialIndex)
        {
            if (material == null || r == null || materialIndex >= r.materials.Length)
            {
                return;
            }

            Material m = r.materials[materialIndex];

            Material mat = Object.Instantiate(material);
            if (diffuseIndex >= 0)
            {
                mat.SetTexture(diffuseIndex, m.GetTexture("_MainTex")); //diffuse
            }

            if (emitIndex >= 0)
            {
                mat.SetTexture(emitIndex, m.GetTexture("_EmissionMap")); //emissive
            }

            if (normalIndex >= 0)
            {
                mat.SetTexture(normalIndex, m.GetTexture("_BumpMap")); //normal
            }

            r.materials[materialIndex] = mat;
        }
    }

    public class MaterialPropertyInfo
    {
        public int diffuseIndex = -1;
        public int emitIndex = -1;
        public string name;
        public int normalIndex = -1;
        public string shaderName;

        public MaterialPropertyInfo(string _shaderName, string _name, int _diffuseIndex, int _normalIndex, int _emitIndex)
        {
            shaderName = _shaderName;
            name = _name;
            diffuseIndex = _diffuseIndex;
            normalIndex = _normalIndex;
            emitIndex = _emitIndex;
        }
    }

    public class CarInfos
    {
        public GameObject baseCar;
        public GameObject boostJet;
        public CarColors defaultColors;
        public Dictionary<string, MaterialInfos> materials = new Dictionary<string, MaterialInfos>();
        public GameObject rotationJet;
        public GameObject wingJet;
        public GameObject wingTrail;

        public void CollectInfos()
        {
            GetBaseCar();
            GetJetsAndTrail();
            GetMaterials();
        }

        private void GetBaseCar()
        {
            var prefab = G.Sys.ProfileManager_.carInfos_[0].prefabs_.carPrefab_;
            if (prefab == null)
            {
                ErrorList.Add("Can't find the refractor base car prefab");
                return;
            }

            baseCar = prefab;
            defaultColors = G.Sys.ProfileManager_.carInfos_[0].colors_;
        }

        private void GetJetsAndTrail()
        {
            if (baseCar == null)
            {
                return;
            }

            foreach (JetFlame j in baseCar.GetComponentsInChildren<JetFlame>())
            {
                var name = j.gameObject.name;
                if (name == "BoostJetFlameCenter")
                {
                    boostJet = j.gameObject;
                }
                else if (name == "JetFlameBackLeft")
                {
                    rotationJet = j.gameObject;
                }
                else if (name == "WingJetFlameLeft1")
                {
                    wingJet = j.gameObject;
                }
            }

            wingTrail = baseCar.GetComponentInChildren<WingTrail>().gameObject;

            if (boostJet == null)
            {
                ErrorList.Add("No valid BoostJet found on Refractor");
            }

            if (rotationJet == null)
            {
                ErrorList.Add("No valid RotationJet found on Refractor");
            }

            if (wingJet == null)
            {
                ErrorList.Add("No valid WingJet found on Refractor");
            }

            if (wingTrail == null)
            {
                ErrorList.Add("No valid WingTrail found on Refractor");
            }
        }

        private void GetMaterials()
        {
            List<MaterialPropertyInfo> materialsNames = new List<MaterialPropertyInfo>
            {
                new MaterialPropertyInfo("Custom/LaserCut/CarPaint", "carpaint", 5, -1, -1),
                new MaterialPropertyInfo("Custom/LaserCut/CarWindow", "carwindow", -1, 218, 219),
                new MaterialPropertyInfo("Custom/Reflective/Bump Glow LaserCut", "wheel", 5, 218, 255),
                new MaterialPropertyInfo("Custom/LaserCut/CarPaintBump", "carpaintbump", 5, 218, -1),
                new MaterialPropertyInfo("Custom/Reflective/Bump Glow Interceptor Special", "interceptor", 5, 218, 255),
                new MaterialPropertyInfo("Custom/LaserCut/CarWindowTrans2Sided", "transparentglow", -1, 218, 219)
            };

            foreach (var c in G.Sys.ProfileManager_.carInfos_)
            {
                GameObject prefab = c.prefabs_.carPrefab_;
                foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>())
                {
                    foreach (Material mat in renderer.materials)
                    {
                        foreach (MaterialPropertyInfo key in materialsNames)
                        {
                            if (materials.ContainsKey(key.name))
                            {
                                continue;
                            }

                            if (mat.shader.name == key.shaderName)
                            {
                                MaterialInfos m = new MaterialInfos
                                {
                                    material = mat,
                                    diffuseIndex = key.diffuseIndex,
                                    normalIndex = key.normalIndex,
                                    emitIndex = key.emitIndex
                                };
                                materials.Add(key.name, m);
                            }
                        }
                    }
                }
            }

            foreach (MaterialPropertyInfo mat in materialsNames.Where(mat => !materials.ContainsKey(mat.name)))
            {
                ErrorList.Add("Can't find the material: " + mat.name + " - shader: " + mat.shaderName);
            }

            materials.Add("donotreplace", new MaterialInfos());
        }
    }
}