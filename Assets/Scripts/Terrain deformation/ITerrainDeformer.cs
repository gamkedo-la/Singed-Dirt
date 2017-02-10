using UnityEngine;
using System.Collections;

public interface ITerrainDeformer
{
    void DeformTerrain(Terrain terrain, Vector3 position, int seed);
}
