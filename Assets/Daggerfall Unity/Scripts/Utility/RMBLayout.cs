﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop.Utility
{
    /// <summary>
    /// Helper for laying out RMB (city block) data in scene.
    /// </summary>
    public static class RMBLayout
    {
        public static float RMBSide = 4096f * MeshReader.GlobalScale;

        const float PropsOffsetY = -4f;
        const float BlockFlatsOffsetY = -6f;
        const float NatureFlatsOffsetY = -2f;
        const uint cityGateOpenId = 446;
        const uint cityGateClosedId = 447;

        #region New Layout Methods

        public static GameObject CreateGameObject(DaggerfallUnity dfUnity, string blockName)
        {
            // Validate
            if (string.IsNullOrEmpty(blockName))
                return null;
            if (!blockName.ToUpper().EndsWith(".RMB"))
                return null;

            // Get block data
            DFBlock blockData = dfUnity.ContentReader.BlockFileReader.GetBlock(blockName);

            return CreateGameObject(dfUnity, ref blockData);
        }

        public static GameObject CreateGameObject(DaggerfallUnity dfUnity, ref DFBlock blockData)
        {
            // Create gameobject
            GameObject go = new GameObject(string.Format("DaggerfallBlock [Name={0}]", blockData.Name));
            go.AddComponent<DaggerfallBlock>();

            // Setup combiner
            ModelCombiner combiner = null;
            if (dfUnity.Option_CombineRMB)
                combiner = new ModelCombiner();

            // Add models and doors
            GameObject modelsNode = new GameObject("Models");
            modelsNode.transform.parent = go.transform;
            StaticDoor[] doors = AddModels(dfUnity, ref blockData, combiner, modelsNode.transform);
            AddProps(dfUnity, ref blockData, combiner, modelsNode.transform);
            AddDoors(doors, go);

            // Add flats
            GameObject flatsNode = new GameObject("Flats");
            flatsNode.transform.parent = go.transform;
            AddBlockFlats(dfUnity, ref blockData, flatsNode.transform);
            AddNatureFlats(dfUnity, ref blockData, flatsNode.transform);

            // Add ground plane
            if (dfUnity.Option_SimpleGroundPlane)
                AddSimpleGroundPlane(dfUnity, ref blockData, go.transform);

            // Apply combiner
            if (combiner != null)
            {
                if (combiner.VertexCount > 0)
                {
                    combiner.Apply();
                    GameObjectHelper.CreateCombinedMeshGameObject(
                        dfUnity,
                        combiner,
                        "CombinedModels",
                        modelsNode.transform,
                        dfUnity.Option_SetStaticFlags);
                }
            }

            return go;
        }

        public static StaticDoor[] AddModels(
            DaggerfallUnity dfUnity,
            ref DFBlock blockData,
            ModelCombiner combiner = null,
            Transform parent = null)
        {
            List<StaticDoor> doors = new List<StaticDoor>();

            // Iterate through all subrecords
            int recordCount = 0;
            foreach (DFBlock.RmbSubRecord subRecord in blockData.RmbBlock.SubRecords)
            {
                // Get subrecord transform
                Vector3 subRecordPosition = new Vector3(subRecord.XPos, 0, BlocksFile.RMBDimension - subRecord.ZPos) * MeshReader.GlobalScale;
                Vector3 subRecordRotation = new Vector3(0, -subRecord.YRotation / BlocksFile.RotationDivisor, 0);
                Matrix4x4 subRecordMatrix = Matrix4x4.TRS(subRecordPosition, Quaternion.Euler(subRecordRotation), Vector3.one);

                // Iterate through models in this subrecord
                foreach (DFBlock.RmbBlock3dObjectRecord obj in subRecord.Exterior.Block3dObjectRecords)
                {
                    // Get model transform
                    Vector3 modelPosition = new Vector3(obj.XPos, -obj.YPos, obj.ZPos) * MeshReader.GlobalScale;
                    Vector3 modelRotation = new Vector3(0, -obj.YRotation / BlocksFile.RotationDivisor, 0);
                    Matrix4x4 modelMatrix = subRecordMatrix * Matrix4x4.TRS(modelPosition, Quaternion.Euler(modelRotation), Vector3.one);

                    // Get model data
                    ModelData modelData;
                    dfUnity.MeshReader.GetModelData(obj.ModelIdNum, out modelData);

                    // Does this model have doors?
                    if (modelData.Doors != null)
                        doors.AddRange(GameObjectHelper.GetStaticDoors(ref modelData, blockData.Index, recordCount, modelMatrix));

                    // Add or combine
                    if (combiner == null || IsCityGate(obj.ModelIdNum))
                        AddStandaloneModel(dfUnity, ref modelData, modelMatrix, parent);
                    else
                        combiner.Add(ref modelData, modelMatrix);
                }

                // Increment record count
                recordCount++;
            }

            return doors.ToArray();
        }

        public static void AddProps(
            DaggerfallUnity dfUnity,
            ref DFBlock blockData,
            ModelCombiner combiner = null,
            Transform parent = null)
        {
            // Iterate through all misc records
            foreach (DFBlock.RmbBlock3dObjectRecord obj in blockData.RmbBlock.Misc3dObjectRecords)
            {
                // Get model transform
                Vector3 modelPosition = new Vector3(obj.XPos, -obj.YPos + PropsOffsetY, obj.ZPos + BlocksFile.RMBDimension) * MeshReader.GlobalScale;
                Vector3 modelRotation = new Vector3(0, -obj.YRotation / BlocksFile.RotationDivisor, 0);
                Matrix4x4 modelMatrix = Matrix4x4.TRS(modelPosition, Quaternion.Euler(modelRotation), Vector3.one);

                // Get model data
                ModelData modelData;
                dfUnity.MeshReader.GetModelData(obj.ModelIdNum, out modelData);

                // Add or combine
                if (combiner == null)
                    AddStandaloneModel(dfUnity, ref modelData, modelMatrix, parent);
                else
                    combiner.Add(ref modelData, modelMatrix);
            }
        }

        public static void AddBlockFlats(
            DaggerfallUnity dfUnity,
            ref DFBlock blockData,
            Transform parent = null)
        {
            // Add block flats
            foreach (DFBlock.RmbBlockFlatObjectRecord obj in blockData.RmbBlock.MiscFlatObjectRecords)
            {
                // Spawn billboard gameobject
                GameObject go = GameObjectHelper.CreateDaggerfallBillboardGameObject(dfUnity, obj.TextureArchive, obj.TextureRecord, parent);
                go.transform.position = new Vector3(
                    obj.XPos,
                    -obj.YPos + BlockFlatsOffsetY,
                    obj.ZPos + BlocksFile.RMBDimension) * MeshReader.GlobalScale;

                // Add lights
                if (obj.TextureArchive == 210 && dfUnity.Option_ImportPointLights)
                {
                    // Spawn light gameobject
                    Vector2 size = dfUnity.MeshReader.GetScaledBillboardSize(210, obj.TextureRecord);
                    GameObject lightgo = GameObjectHelper.CreateDaggerfallRMBPointLight(dfUnity, go.transform);
                    lightgo.transform.position = new Vector3(
                        obj.XPos,
                        -obj.YPos + size.y,
                        obj.ZPos + BlocksFile.RMBDimension) * MeshReader.GlobalScale;

                    // Animate light
                    DaggerfallLight c = lightgo.AddComponent<DaggerfallLight>();
                    c.ParentBillboard = go.GetComponent<DaggerfallBillboard>();
                    if (dfUnity.Option_AnimatedPointLights)
                    {
                        c.Animate = true;
                    }
                }
            }
        }

        public static void AddNatureFlats(
            DaggerfallUnity dfUnity,
            ref DFBlock blockData,
            Transform parent = null,
            ClimateNatureSets climateNature = ClimateNatureSets.SubTropical,
            ClimateSeason climateSeason = ClimateSeason.Summer)
        {
            int archive = ClimateSwaps.GetNatureArchive(climateNature, climateSeason);

            // Add block scenery
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    // Get scenery item
                    DFBlock.RmbGroundScenery scenery = blockData.RmbBlock.FldHeader.GroundData.GroundScenery[x, 15 - y];

                    // Ignore 0 as this appears to be a marker/waypoint of some kind
                    if (scenery.TextureRecord > 0)
                    {
                        // Spawn billboard gameobject
                        GameObject go = GameObjectHelper.CreateDaggerfallBillboardGameObject(dfUnity, archive, scenery.TextureRecord, parent);
                        Vector3 billboardPosition = new Vector3(
                            x * BlocksFile.TileDimension,
                            NatureFlatsOffsetY,
                            y * BlocksFile.TileDimension + BlocksFile.TileDimension) * MeshReader.GlobalScale;

                        // Set transform
                        go.transform.position = billboardPosition;
                    }
                }
            }
        }

        public static GameObject AddSimpleGroundPlane(
            DaggerfallUnity dfUnity,
            ref DFBlock blockData,
            Transform parent = null,
            ClimateBases climateBase = ClimateBases.Temperate,
            ClimateSeason climateSeason = ClimateSeason.Summer)
        {
            GameObject go = new GameObject("Ground");
            if (parent != null)
                go.transform.parent = parent;

            // Assign components
            DaggerfallGroundPlane dfGround = go.AddComponent<DaggerfallGroundPlane>();
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();

            // Assign climate and mesh
            Color32[] tileMap;
            Mesh mesh = dfUnity.MeshReader.GetSimpleGroundPlaneMesh(
                ref blockData,
                out tileMap,
                dfUnity.MeshReader.AddMeshTangents,
                dfUnity.MeshReader.AddMeshLightmapUVs);
            if (mesh)
            {
                meshFilter.sharedMesh = mesh;
            }

            // Assign tileMap and climate
            dfGround.tileMap = tileMap;
            dfGround.SetClimate(dfUnity, climateBase, climateSeason);

            // Assign collider
            if (dfUnity.Option_AddMeshColliders)
                go.AddComponent<BoxCollider>();

            // Assign static
            if (dfUnity.Option_SetStaticFlags)
                go.isStatic = true;

            return go;
        }

        #endregion

        #region Private Methods

        private static GameObject AddStandaloneModel(
            DaggerfallUnity dfUnity,
            ref ModelData modelData,
            Matrix4x4 matrix,
            Transform parent)
        {
            uint modelID = (uint)modelData.DFMesh.ObjectId;

            // Add GameObject
            GameObject go = GameObjectHelper.CreateDaggerfallMeshGameObject(dfUnity, modelID, parent, dfUnity.Option_SetStaticFlags);
            go.transform.position = matrix.GetColumn(3);
            go.transform.rotation = GameObjectHelper.QuaternionFromMatrix(matrix);

            // Is this a city gate?
            if (IsCityGate(modelID))
            {
                DaggerfallCityGate gate = go.AddComponent<DaggerfallCityGate>();
                gate.SetOpen(!dfUnity.Option_CloseCityGates);
            }

            return go;
        }

        private static void AddDoors(StaticDoor[] doors, GameObject target)
        {
            if (doors != null && target != null)
            {
                DaggerfallStaticDoors c = target.AddComponent<DaggerfallStaticDoors>();
                c.Doors = doors;
            }
        }

        private static bool IsCityGate(uint modelID)
        {
            if (modelID == cityGateOpenId || modelID == cityGateClosedId)
                return true;
            else
                return false;
        }

        #endregion
    }
}