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
    /// SaltCrystal class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class SnowDrift : Minerals.DynamicMineral
    {

        public override float GrowthRate
        {
            get
            {
                return attributes.GrowthRateAtPos(Map, Position);
            }
        }

            
    }


    /// <summary>
    /// ThingDef_SnowDrift class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_Snow : Minerals.ThingDef_DynamicMineral
    {
        public override void InitNewMap(Map map, float scaling = 1)
        {
            float snowProb = 1f;
            const float minTemp = 3f;

            // Only spawn snow if it is cold out
            if (map.mapTemperature.SeasonalTemp < minTemp)
            {
                snowProb = snowProb * (float)Math.Sqrt(minTemp - map.mapTemperature.SeasonalTemp) / 5;
            }
            else
            {
                snowProb = 0f;
            }

            // Scale by rain amount
            snowProb = snowProb * map.TileInfo.rainfall / 1000;

//            Log.Message("Minerals: snow scaling due to temp/precip: " + snowProb);
            base.InitNewMap(map, snowProb);
        }

        public virtual float grothRateFactor(IntVec3 aPosition, Map aMap)
        {
            // Melt faster in water
            float rate;
            float baseRate = base.GrowthRateAtPos(aMap, aPosition);
            if (baseRate < 0) {
                if (aMap.terrainGrid.TerrainAt(aPosition).defName.Contains("Water"))
                {
                    if (aMap.terrainGrid.TerrainAt(aPosition).defName.Contains("Moving"))
                    {

                        return Math.Abs(baseRate) * 30; // melt even faster in moving water
                    }
                    else
                    {
                        return Math.Abs(baseRate) * 10;
                    }
                }
                else
                {
                    return Math.Abs(baseRate);
                }
            } else {
                rate = aPosition.GetSnowDepth(aMap);
            }

            // Nearby Buldings slow growth
            const int snowCoverRadius = 1;
            for (int xOffset = -snowCoverRadius; xOffset <= snowCoverRadius; xOffset++)
            {
                for (int zOffset = -snowCoverRadius; zOffset <= snowCoverRadius; zOffset++)
                {
                    IntVec3 checkedPosition = aPosition + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(aMap))
                    {
                        foreach (Thing thing in aMap.thingGrid.ThingsListAt(checkedPosition))
                        {
                            if (thing is Building && thing.def.altitudeLayer == AltitudeLayer.Building)
                            {

                                rate = rate * 0.5f;
                            }

                        }
                    }                    
                }
            }

            // Trees on same tile slow growth
            foreach (Thing thing in aMap.thingGrid.ThingsListAt(aPosition))
            {
                if (thing is Plant && thing.def.altitudeLayer == AltitudeLayer.Building)
                {

                    rate = rate * (1 - ((Plant) thing).Growth) * 0.5f;
                }

            }

            // Factor cannot be greater than one or negative
            if (rate > 1)
            { 
                rate = 1f;
            }
            if (rate < 0)
            { 
                rate = 0f;
            }

            // Melt faster in water
            if (aMap.terrainGrid.TerrainAt(aPosition).defName.Contains("Water") && baseRate < 0.05)
            {
                rate = rate * 5;
            }

            return rate;
        }


        public override float GrowthRateAtPos(Map aMap, IntVec3 aPosition) 
        {
            return base.GrowthRateAtPos(aMap, aPosition) * grothRateFactor(aPosition, aMap);
        }

        public override bool PlaceIsBlocked(Map map, IntVec3 position)
        {
//            Log.Message("PlaceIsBlocked: snow");
            // dont spawn on other types of snow 
            foreach (Thing thing in map.thingGrid.ThingsListAt(position))
            {
                if (thing is SnowDrift)
                {
                    return true;
                }
            }

            return base.PlaceIsBlocked(map, position);
        }
     
    }
        
    public class ThingDef_DeepSnow : ThingDef_Snow
    {

        public override bool PlaceIsBlocked(Map map, IntVec3 position)
        {
//            Log.Message("PlaceIsBlocked: deep");
            if (! position.InBounds(map))
            {
                return true;
            }

            // dont spawn on big things
            if (! position.Standable(map))
            {
                return true;
            }

            // dont spawn  next to stuff
            Predicate<IntVec3> validator = c => c.InBounds(map) && (! c.Standable(map));
            IntVec3 unused;
            if (CellFinder.TryFindRandomCellNear(position, map, 1, validator, out unused))
            {
                return true;
            }

            // dont spawn  next to walls
            Predicate<IntVec3> validator2 = c => c.InBounds(map) && (c.Roofed(map) || c.Impassable(map));
            IntVec3 unused2;
            if (CellFinder.TryFindRandomCellNear(position, map, 2, validator2, out unused2))
            {
                return true;
            }
                
            return base.PlaceIsBlocked(map, position);
        }


//        public override void SpawnCluster(Map map, IntVec3 position)
//        {
//            // Make a cluster center
//            Minerals.StaticMineral mineral = TrySpawnAt(position, map);
//            if (mineral != null)
//            {            
//                mineral.size = Rand.Range(initialSizeMin, initialSizeMax);
//
//                // Add snow depth
//                mineral.size = mineral.size * 0.5f  + mineral.size * position.GetSnowDepth(map) * 0.5f;
//
//            }
//        }


    }  

    public class ThingDef_SnowDrift : ThingDef_Snow
    {

        public override bool PlaceIsBlocked(Map map, IntVec3 position)
        {
//            Log.Message("PlaceIsBlocked: drift");
            if (! position.InBounds(map))
            {
                return true;
            }
//            Log.Message("PlaceIsBlocked: drift in bounds");

            // Cant spawn on water
            if (position.GetTerrain(map).defName.Contains("Water"))
            {
                return true;
            }
//            Log.Message("PlaceIsBlocked: drift not water");

            // dont spawn in the open
            Predicate<IntVec3> validator = c => c.InBounds(map) && (c.Roofed(map) || (! c.Standable(map)));
            IntVec3 unused;
            if (! CellFinder.TryFindRandomCellNear(position, map, 2, validator, out unused))
            {
                return true;
            }
//            Log.Message("PlaceIsBlocked: drift not blocked");

            return base.PlaceIsBlocked(map, position);
        }   

    }  
}


