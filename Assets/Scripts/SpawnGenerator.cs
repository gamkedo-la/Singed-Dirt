using System;
using System.Collections.Generic;
using UnityEngine;

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
    float minSpacing;
    float maxX;
    float maxZ;

    public RandomSpawnGenerator(float minSpacing, float maxX, float maxZ) {
        this.minSpacing = minSpacing;
        this.maxX = maxX;
        this.maxZ = maxZ;
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
                var candidate2D = UnityEngine.Random.insideUnitCircle * maxFromCenter;
                candidatePoint = center + new Vector3(candidate2D.x, 0, candidate2D.y);
                var candidateOK = true;
                for (var j=0; j<i && candidateOK; j++) {
                    if ((spawnPoints[j] - candidatePoint).magnitude < minSpacing) {
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
