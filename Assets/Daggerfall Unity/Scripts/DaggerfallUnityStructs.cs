﻿using System;
using DaggerfallConnect;
using UnityEngine;
using DaggerfallConnect.Utility;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Some information about a climate texture, returned by climate parser.
    /// </summary>
    [Serializable]
    public struct ClimateTextureInfo
    {
        public DFLocation.ClimateTextureGroup textureGroup;
        public DFLocation.ClimateTextureSet textureSet;
        public bool supportsWinter;
        public bool supportsRain;
    }

    /// <summary>
    /// Defines a single cached material.
    /// </summary>
    [Serializable]
    public struct CachedMaterial
    {
        public int key;                         // Key of this material
        public int keyGroup;                    // Group of this material
        public Material material;               // Shared material
        public Rect singleRect;                 // Rect for single material
        public Rect[] atlasRects;               // Array of rects for atlased materials
        public RecordIndex[] indices;           // Array of record indices into atlas rect array
        public FilterMode filterMode;           // Filter mode of this material
        public bool isWindow;                   // True if this is a window material
        public Color windowColor;               // Colour of this window
        public float windowIntensity;           // Intensity of this window
        public DFSize recordSize;               // Size of texture record
        public DFSize recordScale;              // Scale of texture record
        public int recordFrameCount;            // Number of frames in this textures record
    }

    /// <summary>
    /// Defines animation setup for mobile enemies.
    /// </summary>
    [Serializable]
    public struct MobileAnimation
    {
        public int Record;                      // Index of this animation
        public int NumFrames;                   // Number of frames in this animation
        public int FramePerSecond;              // Speed at which this animation plays
        public bool FlipLeftRight;              // True if animation flipped left-to-right
    }

    /// <summary>
    /// Defines basic properties of mobile enemies.
    /// </summary>
    [Serializable]
    public struct MobileEnemy
    {
        public int ID;                          // ID of this mobile
        public string Name;                     // In-game name of this mobile
        public MobileBehaviour Behaviour;       // Behaviour of mobile
        public MobileAffinity Affinity;         // Affinity of mobile
        public MobileGender Gender;             // Gender of mobile
        public MobileReactions Reactions;       // Mobile reaction setting
        public MobileCombatFlags CombatFlags;   // Mobile combat flags
        public int MaleTexture;                 // Texture archive index for male sprite
        public int FemaleTexture;               // Texture archive index for female sprite
        public int CorpseTexture;               // Corpse texture archive:record bits
        public bool HasIdle;                    // Has standard Idle animation group
        public bool HasRangedAttack1;           // Has RangedAttack1 animation group
        public bool HasRangedAttack2;           // Has RangedAttack2 animation group
        public float Health;                    // Enemy health pool
        public bool CanOpenDoors;               // Enemy can open doors to pursue player
        public bool PrefersRanged;              // Enemy prefers ranged attacks and spells over melee
        public int BloodIndex;                  // Index in TEXTURE.380 for blood splash 
        public int MoveSound;                   // Index of enemy "moving around" sound
        public int BarkSound;                   // Index of enemy "bark" or "shout" sound
        public int AttackSound;                 // Index of enemy "attack" sound
        public int SightModifier;               // +/- range of vision for acute/impaired sight
        public int HearingModifier;             // +/- range of hearing for acute/impaired hearing
    }

    /// <summary>
    /// Defines a list of random encounters based on dungeon type (Crypt, Orc Stronghold, etc.).
    /// </summary>
    [Serializable]
    public struct RandomEncounterTable
    {
        public DFRegion.DungeonTypes DungeonType;
        public MobileTypes[] Enemies;
    }

    /// <summary>
    /// A record index for cached materials.
    /// Used to align indices in atlased textures.
    /// </summary>
    [Serializable]
    public struct RecordIndex
    {
        public int startIndex;                  // Index of first frame in atlas rect array
        public int frameCount;                  // Number of frames in this record
        public int width;                       // Width in pixels of this record, excluding border and padding
        public int height;                      // Height in pixels of this record, excluding border and padding
    }

    /// <summary>
    /// Defines model data.
    /// </summary>
    [Serializable]
    public struct ModelData
    {
        public DFMesh DFMesh;                   // Native ngon geometry as read from ARCH3D.BSA
        public Vector3[] Vertices;              // Vector3 array containing position data
        public Vector3[] Normals;               // Vector3 array containing normal data
        public Vector2[] UVs;                   // Vector2 array containing UV data
        public int[] Indices;                   // Index array describing the triangles of this mesh
        public SubMeshData[] SubMeshes;         // Data for each SubMesh, grouped by texture
        public ModelDoor[] Doors;               // Doors found in this model

        /// <summary>
        /// Defines submesh data.
        /// </summary>
        [Serializable]
        public struct SubMeshData
        {
            public int StartIndex;              // Location in the index array at which to start reading vertices
            public int PrimitiveCount;          // Number of primitives in this submesh
            public int TextureArchive;          // Texture archive index
            public int TextureRecord;           // Texture record index
        }
    }

    /// <summary>
    /// Defines a door found in model data.
    /// </summary>
    [Serializable]
    public struct ModelDoor
    {
        public int Index;                       // Index of this door in model data
        public DoorTypes Type;                  // Type of door found (building, dungeon, etc.)
        public Vector3 Vert0;                   // Vertex 0
        public Vector3 Vert1;                   // Vertex 1
        public Vector3 Vert2;                   // Vertex 2
        public Vector3 Normal;                  // Normal facing away from door
    }

    /// <summary>
    /// Defines animation setup for weapons.
    /// </summary>
    [Serializable]
    public struct WeaponAnimation
    {
        public int Record;                      // Index of this animation
        public int NumFrames;                   // Number of frames in this animation
        public int FramePerSecond;              // Speed at which this animation plays
        public WeaponAlignment Alignment;       // Side of screen to align animation
        public float Offset;                    // Offset from edge of screen in 0-1 range, ignored for WeaponAlignment.Center
    }

    /// <summary>
    /// Defines a static door inside a scene.
    /// </summary>
    [Serializable]
    public struct StaticDoor
    {
        public Matrix4x4 buildingMatrix;        // Matrix of individual building owning this door
        public DoorTypes doorType;              // Type of door
        public int blockIndex;                  // Block index in BLOCKS.BSA
        public int recordIndex;                 // Record index of interior
        public int doorIndex;                   // Door index for individual building/record (most buildings have only 1-2 doors)
        public Vector3 centre;                  // Door centre in model space
        public Vector3 size;                    // Door size in model space
        public Vector3 normal;                  // Normal pointing away from door
    }

    /// <summary>
    /// Information about a single map pixel for streaming world.
    /// </summary>
    public struct MapPixelData
    {
        public int mapPixelX;                   // Map pixel X coordinate
        public int mapPixelY;                   // Map pixel Y coordinate
        public int worldHeight;                 // Height of this pixel
        public int worldClimate;                // Climate of this pixel
        public int worldPolitic;                // Politics of this pixel
        public bool hasLocation;                // True if location present
        public int regionIndex;                 // Region index
        public int mapIndex;                    // Location map index (if present)
        public int locationID;                  // Location ID (if present)
        public string locationName;             // Location name (if present)
        public float locationHeight;            // Average height of terrain
        public GameObject gameObject;           // GameObject with terrain component
    }

    /// <summary>
    /// Height, normal, and texture information for any point in world.
    /// </summary>
    public struct WorldSample
    {
        public float height;                    // Height of this sample
        public Vector3 normal;                  // Normal of sample
        public WorldTile tile;                  // Texture and nature of sample
    }

    /// <summary>
    /// Tile texture information.
    /// </summary>
    public struct WorldTile
    {
        public int record;                      // Record index into texture atlas
        public bool flip;                       // Flip texture UVs
        public bool rotate;                     // Rotate texture UVs
        public bool location;                   // True if location present
        public int nature;                      // Index of nature flat at this point (0 is nothing)
    }

    /// <summary>
    /// Data for a single terrain chunk.
    /// </summary>
    public struct TerrainChunkData
    {
        //public Mesh renderMesh;                 // Mesh for rendering this chunk
        //public Mesh colliderMesh;               // Mesh for colliding with this chunk
        //public Material material;               // Shared terrain material atlas for this chunk
        public Vector3[] vertices;              // Vertex array
        public Vector3[] normals;               // Normal array
        public Vector2[] uvs;                   // UV array
        public int[] indices;                   // Mesh indices
        //public Vector3[] colliderVerts;         // Collider vertices
        //public int[] colliderIndices;           // Collider indices;
    }
}