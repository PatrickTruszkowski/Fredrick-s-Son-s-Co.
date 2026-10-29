using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BasementGenerator : NetworkBehaviour
{
    public static BasementGenerator Singleton { get; private set; }

    [SerializeField] private Transform[] tileArray;
    [SerializeField] private Transform[] decorationArray;

    public NetworkVariable<int> networkSeed = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Tile IDs.
    private const int TILE_EMPTY = 0;
    private const int TILE_ONE_WALL = 1;
    private const int TILE_TWO_WALL = 2;
    private const int TILE_CORNER = 3;
    private const int TILE_ALL_WAY_INTERSECTION = 4;
    private const int TILE_ONE_WALL_INTERSECTION = 5;
    private const int TILE_TWO_WALL_INTERSECTION = 6;

    // Decoration IDs.
    private const int METAL_SHELF = 0;

    // Basement spacing/dimensions.
    private const int MAX_SIZE = 10; // Floor 1: 10, Floor 2: 20, Floor 3: 30
    private const int TILE_SPACING = 8;

    //private int[,] tilePositionArray = new int[MAX_SIZE, MAX_SIZE];
    private readonly int[] edgeTileIDArray = { 1, 2, 3, 5, 6 };
    private readonly Dictionary<int, int[]> leftEdgeTileIDRotationTickDictionary = new()
    {
        {TILE_ONE_WALL, new int[] {2}}, {TILE_TWO_WALL, new int[] {0}}, {TILE_CORNER, new int[] {0, 1}}, {TILE_ONE_WALL_INTERSECTION, new int[] {1}}, {TILE_TWO_WALL_INTERSECTION, new int[] {0, 1}}
    };

    private void Awake()
    {
        Singleton = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            networkSeed.Value = Random.Range(int.MinValue, int.MaxValue);
        }

        Random.InitState(networkSeed.Value);

        Generate();
    }

    private void Generate()
    {
        int maxPosition = MAX_SIZE * TILE_SPACING / 2 - 4;

        for (int row = 0; row < MAX_SIZE; row++)
        {
            float positionZ = maxPosition - (row * TILE_SPACING);

            for (int col = 0; col < MAX_SIZE; col++)
            {
                float positionX = -maxPosition + (col * TILE_SPACING);

                // There is a 1/3 chance to spawn a decoration object.
                if (Random.Range(0, 3) == 0)
                {
                    Transform decorationTransform = Instantiate(decorationArray[METAL_SHELF]);
                    decorationTransform.SetParent(transform);
                    decorationTransform.SetLocalPositionAndRotation(new Vector3(positionX + Random.Range(-1f, 1f), 2f, positionZ + Random.Range(-1f, 1f)), Quaternion.Euler(Vector3.up * Random.Range(0f, 359f)));
                }

                // Check for corner piece.
                if (row == 0 && col == 0)
                {
                    float rotation = 90f;
                    SpawnTile(TILE_CORNER, rotation, positionX, positionZ);
                    continue;
                }

                if (row == 0 && col == MAX_SIZE - 1)
                {
                    float rotation = 180f;
                    SpawnTile(TILE_CORNER, rotation, positionX, positionZ);
                    continue;
                }

                if (row == MAX_SIZE - 1 && col == 0)
                {
                    float rotation = 0f;
                    SpawnTile(TILE_CORNER, rotation, positionX, positionZ);
                    continue;
                }

                if (row == MAX_SIZE - 1 && col == MAX_SIZE - 1)
                {
                    float rotation = 270f;
                    SpawnTile(TILE_CORNER, rotation, positionX, positionZ);
                    continue;
                }

                int randomTileID;
                float randomRotation;

                // Check for left edge.
                if (col == 0)
                {
                    randomTileID = edgeTileIDArray[Random.Range(0, edgeTileIDArray.Length)];
                    int[] rotationTickArray = leftEdgeTileIDRotationTickDictionary.GetValueOrDefault(randomTileID);
                    randomRotation = 90f * rotationTickArray[Random.Range(0, rotationTickArray.Length)];

                    SpawnTile(randomTileID, randomRotation, positionX, positionZ);
                    continue;
                }

                // Check for top edge.
                if (row == 0)
                {
                    randomTileID = edgeTileIDArray[Random.Range(0, edgeTileIDArray.Length)];
                    int[] rotationTickArray = leftEdgeTileIDRotationTickDictionary.GetValueOrDefault(randomTileID);
                    randomRotation = 90f * (rotationTickArray[Random.Range(0, rotationTickArray.Length)] + 1);

                    SpawnTile(randomTileID, randomRotation, positionX, positionZ);
                    continue;
                }

                // Check for right edge.
                if (col == MAX_SIZE - 1)
                {
                    randomTileID = edgeTileIDArray[Random.Range(0, edgeTileIDArray.Length)];
                    int[] rotationTickArray = leftEdgeTileIDRotationTickDictionary.GetValueOrDefault(randomTileID);
                    randomRotation = 90f * (rotationTickArray[Random.Range(0, rotationTickArray.Length)] + 2);

                    SpawnTile(randomTileID, randomRotation, positionX, positionZ);
                    continue;
                }

                // Check for bottom edge.
                if (row == MAX_SIZE - 1)
                {
                    randomTileID = edgeTileIDArray[Random.Range(0, edgeTileIDArray.Length)];
                    int[] rotationTickArray = leftEdgeTileIDRotationTickDictionary.GetValueOrDefault(randomTileID);
                    randomRotation = 90f * (rotationTickArray[Random.Range(0, rotationTickArray.Length)] + 3);

                    SpawnTile(randomTileID, randomRotation, positionX, positionZ);
                    continue;
                }

                // Check for center.
                if (row == MAX_SIZE / 2 && col == MAX_SIZE / 2)
                {
                    float rotation = 0f;
                    SpawnTile(TILE_EMPTY, rotation, positionX, positionZ);
                    continue;
                }

                // Else we are in the middle therefore generate a random tile.
                randomTileID = Random.Range(0, 7);
                randomRotation = 90f * Random.Range(0, 4);

                SpawnTile(randomTileID, randomRotation, positionX, positionZ);
            }
        }
    }

    private void SpawnTile(int tileID, float rotation, float positionX, float positionZ)
    {
        Transform tileTransform = Instantiate(tileArray[tileID]);
        tileTransform.SetParent(transform);
        tileTransform.SetLocalPositionAndRotation(new Vector3(positionX, 0f, positionZ), Quaternion.Euler(new Vector3(0f, rotation, 0f)));
    }
}