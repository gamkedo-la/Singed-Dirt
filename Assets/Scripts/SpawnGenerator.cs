using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using TyVoronoi;

public interface ISpawnGenerator {
    Vector3[] Generate(int numSpawns);
}

/// <summary>
/// Class used to create player spawn points
/// This implementation takes a fixed set of spawn points which are split across two sides of the map
/// and assigns spawn locations based on even/odd index of expected player.  This should have the
/// effect that player 1 and player 2 should always be on opposite sides of the map
/// </summary>
public class FixedSpawnGenerator : ISpawnGenerator {
    Vector3[] oddSpawns = new Vector3[] {
        new Vector3(23.5f, 0f, 102.3f),
        new Vector3(30.5f, 0f, 82.3f),
        new Vector3(67.4f, 0f, 56.5f),
        new Vector3(37.7f, 0f, 59.7f),
        new Vector3(70.5f, 0f, 42.7f),
        new Vector3(102.1f, 0f, 24.9f),
    };
    Vector3[] evenSpawns = new Vector3[] {
        new Vector3(176.3f, 0f, 88.5f),
        new Vector3(163.3f, 0f, 115.5f),
        new Vector3(137.2f, 0f, 139.4f),
        new Vector3(157.3f, 0f, 151.6f),
        new Vector3(125.3f, 0f, 166.6f),
        new Vector3(93.2f, 0f, 178.5f),
    };

    public Vector3[] Generate(int numSpawns) {
        var oddOpen = new List<int>();
        var evenOpen = new List<int>();
        for (var i=0; i<oddSpawns.Length; i++) {
            oddOpen.Add(i);
        }
        for (var i=0; i<evenSpawns.Length; i++) {
            evenOpen.Add(i);
        }
        var spawnPoints = new Vector3[numSpawns];
        for (var i=0; i<numSpawns; i++) {
            if (i%2 == 0) {
                var choice = UnityEngine.Random.Range(0, evenOpen.Count);
                spawnPoints[i] = evenSpawns[evenOpen[choice]];
                evenOpen.RemoveAt(choice);
            } else {
                var choice = UnityEngine.Random.Range(0, oddOpen.Count);
                spawnPoints[i] = oddSpawns[oddOpen[choice]];
                oddOpen.RemoveAt(choice);
            }
        }
        return spawnPoints;
    }

}

/// <summary>
/// Class used to create player spawn points
/// This implementation randomizes the spawn locations but guarantees that:
/// * each player is not within a minimal distance of another player
/// * each player is within a computed max distance from the center of the map, based on passed
/// X/Z values for terrain.
/// </summary>
public class RandomSpawnGenerator : ISpawnGenerator{
    static Vector2[] quadOffsets = {
        Vector2.zero,
        Vector2.up,
        Vector2.up + Vector2.right,
        Vector2.right,
    };
    int[] quadOrder;
    float minSpacing;
    float maxX;
    float maxZ;

    public RandomSpawnGenerator(float minSpacing, float maxX, float maxZ) {
        this.minSpacing = minSpacing;
        this.maxX = maxX;
        this.maxZ = maxZ;
        // generate random quad order
        var quadList = new List<int>();
        for (var i=0; i<4; i++) {
            quadList.Add(i);
        }
        quadOrder = new int[4];
        for (var i=0; i<4; i++) {
            quadOrder[i] = quadList[UnityEngine.Random.Range(0,quadList.Count)];
            quadList.Remove(quadOrder[i]);
        }
        //Debug.Log("quadOrder: " + String.Join(",", quadOrder.Select(v=>v.ToString()).ToArray()));
    }
    public Vector3[] Generate(int numSpawns) {
        var center = new Vector3(maxX/2f, 0, maxZ/2);
        var maxFromCenter = maxX/2f - 20f;
        var spawnPoints = new Vector3[numSpawns];

        var quadIndex = UnityEngine.Random.Range(0,quadOrder.Length);

        for (var i=0; i<numSpawns; i++) {
            // attempt to find a candidate point within maxFromCenter distance from center of map
            // and not within minSpacing from another point
            // try 20 times before giving up
            var candidatePoint = Vector3.zero;
            for (var attempts=0; attempts<20; attempts++) {
                // start w/ random 2d point inside 0,0 to center
                var candidate2D = new Vector2(
                    UnityEngine.Random.Range(0f, center.x),
                    UnityEngine.Random.Range(0f, center.z)
                );
                // offset to put point in current quadrant
                var offset = quadOffsets[quadOrder[quadIndex]];
                candidate2D += new Vector2(center.x*offset.x, center.z*offset.y);
                candidatePoint = new Vector3(candidate2D.x, 0, candidate2D.y);
                // point is invalid if greater than maxFromCenter away from center
                var candidateOK = (center-candidatePoint).magnitude < maxFromCenter;
                // point is invalid if within minSpacing of another spawn point
                for (var j=0; j<i && candidateOK; j++) {
                    if ((spawnPoints[j] - candidatePoint).magnitude < minSpacing) {
                        candidateOK = false;
                    }
                }
                if (candidateOK) break;
            }
            spawnPoints[i] = candidatePoint;
            //Debug.Log("quadIndex: " + quadOrder[quadIndex] + " spawnPoint: " + candidatePoint);
            quadIndex++;
            if (quadIndex >= 4) quadIndex = 0;
        }

        return spawnPoints;
    }
}

/// <summary>
/// Class used to create terrain spawn points by finding voronoi graph edges around player
/// spawn locations.
/// This implementation randomizes the spawn locations but guarantees that:
/// * each spawn is within maxDrift of a voronoi edge between players.
/// * each player is within a computed max distance from the center of the map, based on passed
/// X/Z values for terrain.
/// </summary>
public class VoronoiSpawnGenerator : ISpawnGenerator{
    Vector3[] playerSpawns;
    float minSpacing;
    float maxDrift;
    float maxX;
    float maxZ;
    Voronoi voronoi;

    public VoronoiSpawnGenerator(Vector3[] playerSpawns, float minSpacing, float maxDrift, float maxX, float maxZ) {
        this.minSpacing = minSpacing;
        this.playerSpawns = playerSpawns;
        this.maxDrift = maxDrift;
        this.maxX = maxX;
        this.maxZ = maxZ;

        // build site list, translating x,z to x,y
        var sites = new Vector2[playerSpawns.Length];
        for (var i=0; i<playerSpawns.Length; i++) {
            sites[i] = new Vector2(playerSpawns[i].x, playerSpawns[i].z);
        }

		var size = new Vector3(maxX, maxZ, 0);
		var bounds = new Bounds(size/2f, size);
        this.voronoi = new Voronoi(bounds, sites);
        voronoi.Compute();
        for (var i=0; i<voronoi.edgeList.Count; i++) {
            var edge = voronoi.edgeList[i];
            Debug.DrawLine(
                new Vector3(edge.vertices[0].x, 100, edge.vertices[0].y),
                new Vector3(edge.vertices[1].x, 100, edge.vertices[1].y),
                Color.red, 300);
        }
    }

    public Vector3[] Generate(int numSpawns) {
        var center = new Vector3(maxX/2f, 0, maxZ/2);
        var maxFromCenter = maxX/2f - 10f;
        var spawnPoints = new Vector3[numSpawns];

        for (var i=0; i<numSpawns; i++) {
            // attempt to find a candidate point within maxFromCenter distance from center of map
            // and not within minSpacing from another point
            // try 20 times before giving up
            var candidatePoint = Vector3.zero;
            for (var attempts=0; attempts<20; attempts++) {
        		// select edge
        		var edge = voronoi.edgeList[UnityEngine.Random.Range(0, voronoi.edgeList.Count)];

        		// select random point on edge, represented by % of length
        		float percent = UnityEngine.Random.Range(0f,1f);

        		// plot point
        		// point on line
        		var v2point = edge.vertices[0] + ((edge.vertices[1]-edge.vertices[0]) * percent);
        		//Debug.Log("point along edge: " + v2point);

        		// add random point within specified drift
        		Vector2 drift = UnityEngine.Random.insideUnitCircle;
        		drift *= maxDrift;
        		candidatePoint = new Vector3(v2point.x + drift.x, 0, v2point.y + drift.y);

                var candidateOK = (candidatePoint - center).magnitude <= maxFromCenter;
                // point is invalid if within minSpacing of player spawn point
                for (var j=0; j<playerSpawns.Length && candidateOK; j++) {
                    if ((playerSpawns[j] - candidatePoint).magnitude < minSpacing) {
                        // Debug.Log("terrain spawn too close to player");
                        candidateOK = false;
                    }
                }
                if (candidateOK) break;
            }
            spawnPoints[i] = candidatePoint;
        }

        return spawnPoints;
    }
}

/// <summary>
/// Class used to create terrain spawn points by finding voronoi graph edges around player
/// spawn locations.  Used specifically to find bisectors between each nearest neighbor.
/// </summary>
public class VoronoiBisectorSpawnGenerator : ISpawnGenerator{
    public Voronoi voronoi;

    public VoronoiBisectorSpawnGenerator(Vector3[] playerSpawns, float maxX, float maxZ) {
        // build site list, translating x,z to x,y
        var sites = new Vector2[playerSpawns.Length];
        for (var i=0; i<playerSpawns.Length; i++) {
            sites[i] = new Vector2(playerSpawns[i].x, playerSpawns[i].z);
        }

		var size = new Vector3(maxX, maxZ, 0);
		var bounds = new Bounds(size/2f, size);
        this.voronoi = new Voronoi(bounds, sites);
        voronoi.Compute();
    }

    public Vector3[] Generate(int numSpawns) {
        var spawnPoints = new Vector3[numSpawns];
        for (var i=0; i<numSpawns; i++) {
            var edgeIndex = i % voronoi.edgeList.Count;
            var bisector = voronoi.edgeList[edgeIndex].bisector;
            spawnPoints[i] = new Vector3(bisector.x, 0, bisector.y);
        }
        return spawnPoints;
    }
}

/// <summary>
/// Class used to create terrain spawn points within a specified set of spawn boxes
/// This implementation randomizes the spawn locations but guarantees that:
/// * each spawn is within maxDrift of a voronoi edge between players.
/// </summary>
public class SpawnBoxSpawnGenerator: ISpawnGenerator {
    Transform[] spawnBoxes;
    float maxDrift;

    public SpawnBoxSpawnGenerator(Transform[] spawnBoxes) {
        this.spawnBoxes = spawnBoxes;
    }

    public Vector3[] Generate(int numSpawns) {
        var spawnPoints = new Vector3[numSpawns];
        for (var i=0; i<numSpawns; i++) {
    		Vector3 randInSpawnBox;
    		randInSpawnBox = new Vector3(
                UnityEngine.Random.Range (-1.0f, 1.0f),
                UnityEngine.Random.Range (-1.0f, 1.0f),
                UnityEngine.Random.Range (-1.0f, 1.0f));
    		randInSpawnBox = spawnBoxes[UnityEngine.Random.Range(0, spawnBoxes.Length)].TransformPoint (randInSpawnBox * 0.5f);
    		spawnPoints[i] = randInSpawnBox;
        }
        return spawnPoints;
    }
}
