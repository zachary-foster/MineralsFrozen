﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 

namespace MineralsFrozen
{
    /// <summary>
    /// FrozenWallBase class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class FrozenWallBase : Building
    {

        // Cache for texture indexes
        protected int textureIndex = -100;


        public virtual ThingDef_FrozenWallBase attributes
        {
            get
            {
                return def as ThingDef_FrozenWallBase;
            }
        }

        public virtual bool isMelting
        {
            get
            {
                float temp = Position.GetTemperature(Map);
                return temp > 0;
            }
        }

        public virtual float meltRate
        {
            get
            {
                if (isMelting)
                {
                    float temp = Position.GetTemperature(Map);
                    return temp / 30;
                }
                else
                {
                    return 0f;
                }
            }
        }

        public override void TickRare()
        {
            // Melt if hot
            if (isMelting)
            {
                float meltDamage = meltRate;
                if (meltDamage < 1 & Rand.Range(0f, 1f) < meltDamage)
                {
                    TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1, 0, -1, null, null, null));
                }
                else
                {
                    TakeDamage(new DamageInfo(DamageDefOf.Deterioration, (int) Math.Floor(meltDamage), 0, -1, null, null, null));
        
                }
            }
        
            base.TickRare();
        }


        public virtual void initializeTextures() {
            Rand.PushState();
            Rand.Seed = Position.GetHashCode();
            textureIndex = Rand.Range(0, attributes.getTexturePaths().Count);
            Rand.PopState();
        }

        public virtual string getTexturePath()
        {
            // initalize the array if it has not already been initalized
            if (textureIndex == -100)
            {
                initializeTextures();
            }

            return(attributes.getTexturePaths()[textureIndex]);
        }

        public override void Print(SectionLayer layer)
        {
             Rand.PushState();
             Rand.Seed = Position.GetHashCode() + attributes.defName.GetHashCode();
             base.Print(layer);
             Rand.PopState();
        }

        public override Graphic Graphic
        {
            get
            {
                // Pick a random path 
                string printedTexturePath = getTexturePath();
                Graphic printedTexture = GraphicDatabase.Get<Graphic_Single>(printedTexturePath, attributes.graphicData.shaderType.Shader);

                // convert to corner filler
                printedTexture = GraphicDatabase.Get<Graphic_Single>(printedTexture.path, printedTexture.Shader, printedTexture.drawSize, DrawColor, DrawColorTwo, printedTexture.data);
                return new Graphic_LinkedCornerFiller(printedTexture);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (DebugSettings.godMode)
            {
                stringBuilder.AppendLine("Melt Rate: " + meltRate);
            }
            else
            {
                if (isMelting)
                {
                    stringBuilder.AppendLine("Melting.");
                }
                else
                {
                    stringBuilder.AppendLine("Frozen.");
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }


    }       


    /// <summary>
    /// ThingDef_FrozenWallBase class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_FrozenWallBase : ThingDef
    {
        public List<string> texturePaths;

        public virtual void initTexturePaths()
        {
            // Get paths to textures
            string textureName = System.IO.Path.GetFileName(graphicData.texPath);
            texturePaths = new List<string> { };
            List<string> versions = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
            foreach (string letter in versions)
            {
                string a_path = graphicData.texPath + "/" + textureName + letter;
                if (ContentFinder<Texture2D>.Get(a_path, false) != null)
                {
                    texturePaths.Add(a_path);
                }
            }

        }

        public virtual List<string> getTexturePaths()
        {
            if (texturePaths == null)
            {
                initTexturePaths();
            }
            return texturePaths;
        }


    }
}
