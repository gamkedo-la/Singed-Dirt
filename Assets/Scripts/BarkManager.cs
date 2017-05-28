using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// General criteria for adding new barks and character voices:
///         1. New ammo/bark types can have more or fewer barks than the other ammo/bark types.
///         (Example: if you want a new bark triggered on Death, you might only have 1 one-liner.)
///         2. BUT any new one-liner must have 1 version/file for each character voice.
///         (Example: the Death one-liner might have 4 different files: meanie, grumpy, aggro, and cheery.)
///         3. Any new character voice must match the other characters in the number of one-
///         liners for every ammo/bark type. (Example: If the other characters have 4 barks for Acorn and
///         1 for Death, then the new character must have 4 barks for Acorn and 1 for Death.)
///
/// To add a new type of ammo/bark one-liner:
///         1. Add a new Enum below and add a reference in the NetRegistry.cs file.
///         2. Group the one-liners by character voice using the same voice sequence as the other Enums.
///         (Example: If meanie is the first group in one Enum, it should be first in all Enums.)
///         3. Declare a currentIndex property and totalLines property for your the bark type.
///         4. Add an indexQueue for your new bark type.
///         5. Declare a kind variable for the bark type. (Example: deathOneLinersKind deathOneLiner)
///         6. Assign the totalLines value in Awake.
///         7. Assign the reset value for currentIndex and build the indexQueue in the Reset method.
///         8. Add the appropriate condition and triggered code in GetTheShotOneLiner method, or
///         write a new method specific to your bark.
///
/// To add one-liners to already-existing ammo/bark types:
///         1. Add each version's filename to the appropriate Enum, grouping it with its appropriate
///         character voice.
///
/// To add a new character voice:
///         1. Add each filename to its appropriate Enum below, grouping the references by character
///         voice.
///         2. Always add your voice group at the same position within each Enum.
///         (Example: If the new character's lines are put 2nd in one Enum, it should be 2nd in all the other
///         Enums.)
///         3. Increase the totalVoices property by the number of new character voices added.

public enum AnyOneLinersKind {
    voice_aggro01 = 0,
    voice_aggro04,
    voice_aggro06,
    voice_aggro07,
    voice_aggro09,
    voice_aggro10,
    voice_aggro11,
    voice_aggro12,
    voice_aggro13,
    voice_aggro14,
    voice_aggro15,
    voice_aggro16,
    voice_aggro17,
    voice_aggro18,
    voice_aggro20,
    voice_aggro21,
    voice_aggro23,
    voice_aggro24,
    voice_aggro25,
    voice_aggro27,
    voice_aggro29,

    voice_grumpy01,
    voice_grumpy04,
    voice_grumpy06,
    voice_grumpy07,
    voice_grumpy09,
    voice_grumpy10,
    voice_grumpy11,
    voice_grumpy12,
    voice_grumpy13,
    voice_grumpy14,
    voice_grumpy15,
    voice_grumpy16,
    voice_grumpy17,
    voice_grumpy18,
    voice_grumpy20,
    voice_grumpy21,
    voice_grumpy23,
    voice_grumpy24,
    voice_grumpy25,
    voice_grumpy27,
    voice_grumpy29,

    voice_cheery01,
    voice_cheery04,
    voice_cheery06,
    voice_cheery07,
    voice_cheery09,
    voice_cheery10,
    voice_cheery11,
    voice_cheery12,
    voice_cheery13,
    voice_cheery14,
    voice_cheery15,
    voice_cheery16,
    voice_cheery17,
    voice_cheery18,
    voice_cheery20,
    voice_cheery21,
    voice_cheery23,
    voice_cheery24,
    voice_cheery25,
    voice_cheery27,
    voice_cheery29,

    voice_meanie01,
    voice_meanie04,
    voice_meanie06,
    voice_meanie07,
    voice_meanie09,
    voice_meanie10,
    voice_meanie11,
    voice_meanie12,
    voice_meanie13,
    voice_meanie14,
    voice_meanie15,
    voice_meanie16,
    voice_meanie17,
    voice_meanie18,
    voice_meanie20,
    voice_meanie21,
    voice_meanie23,
    voice_meanie24,
    voice_meanie25,
    voice_meanie27,
    voice_meanie29
}

public enum AcornOneLinersKind {
    voice_aggro02,
    voice_aggro30,
    voice_aggro31,
    voice_aggro32,

    voice_grumpy02,
    voice_grumpy30,
    voice_grumpy31,
    voice_grumpy32,

    voice_cheery02,
    voice_cheery30,
    voice_cheery31,
    voice_cheery32,

    voice_meanie02,
    voice_meanie30,
    voice_meanie31,
    voice_meanie32
}


public enum BeetOneLinersKind {
    voice_aggro33,
    voice_aggro34,
    voice_aggro35,
    voice_aggro36,

    voice_grumpy33,
    voice_grumpy34,
    voice_grumpy35,
    voice_grumpy36,

    voice_cheery33,
    voice_cheery34,
    voice_cheery35,
    voice_cheery36,

    voice_meanie33,
    voice_meanie34,
    voice_meanie35,
    voice_meanie36
}

public enum CannonOneLinersKind {
    voice_aggro08,
    voice_aggro19,
    voice_aggro22,
    voice_aggro37,

    voice_grumpy08,
    voice_grumpy19,
    voice_grumpy22,
    voice_grumpy37,

    voice_cheery08,
    voice_cheery19,
    voice_cheery22,
    voice_cheery37,

    voice_meanie08,
    voice_meanie19,
    voice_meanie22,
    voice_meanie37
}

public enum MissileOneLinersKind {
    voice_aggro05,
    voice_aggro26,
    voice_aggro28,
    voice_aggro48,

    voice_grumpy05,
    voice_grumpy26,
    voice_grumpy28,
    voice_grumpy48,

    voice_cheery05,
    voice_cheery26,
    voice_cheery28,
    voice_cheery48,

    voice_meanie05,
    voice_meanie26,
    voice_meanie28,
    voice_meanie48
}

public enum MushboomOneLinersKind {
    voice_aggro45,
    voice_aggro46,
    voice_aggro47,
    voice_aggro38,


    voice_grumpy44,
    voice_grumpy45,
    voice_grumpy47,
    voice_grumpy38,


    voice_cheery45,
    voice_cheery46,
    voice_cheery47,
    voice_cheery38,


    voice_meanie44,
    voice_meanie45,
    voice_meanie47,
    voice_meanie38
}

public enum PillarOneLinersKind {
    voice_aggro39,
    voice_aggro41,
    voice_aggro42,
    voice_aggro43,

    voice_grumpy39,
    voice_grumpy40,
    voice_grumpy41,
    voice_grumpy42,

    voice_cheery40,
    voice_cheery41,
    voice_cheery42,
    voice_cheery43,

    voice_meanie39,
    voice_meanie41,
    voice_meanie42,
    voice_meanie43
}

public enum SharktoothOneLinersKind {
    voice_aggro03,
    voice_aggro49,
    voice_aggro50,
    voice_aggro51,

    voice_grumpy03,
    voice_grumpy49,
    voice_grumpy50,
    voice_grumpy51,

    voice_cheery03,
    voice_cheery49,
    voice_cheery50,
    voice_cheery51,

    voice_meanie03,
    voice_meanie49,
    voice_meanie50,
    voice_meanie51
}

public enum TeleportOneLinersKind {
    voice_aggro52,
    voice_aggro54,
    voice_aggro55,
    voice_aggro56,

    voice_grumpy52,
    voice_grumpy54,
    voice_grumpy55,
    voice_grumpy56,

    voice_cheery53,
    voice_cheery54,
    voice_cheery55,
    voice_cheery56,

    voice_meanie53,
    voice_meanie54,
    voice_meanie55,
    voice_meanie56
}

public class BarkManager : MonoBehaviour {
    public static BarkManager self;

    private AnyOneLinersKind anyOneLiner;
    private AcornOneLinersKind acornOneLiner;
    private BeetOneLinersKind beetOneLiner;
    private CannonOneLinersKind cannonOneLiner;
    private MissileOneLinersKind missileOneLiner;
    private MushboomOneLinersKind mushboomOneLiner;
    private PillarOneLinersKind pillarOneLiner;
    private SharktoothOneLinersKind sharktoothOneLiner;
    private TeleportOneLinersKind teleportOneLiner;

    private int totalVoices = 4,
        currentVoiceIndex,
        currentAnyLineIndex,
        currentAcornLineIndex,
        currentBeetLineIndex,
        currentCannonLineIndex,
        currentMissileLineIndex,
        currentMushboomLineIndex,
        currentPillarLineIndex,
        currentSharktoothLineIndex,
        currentTeleportLineIndex,
        totalAnyLines,
        totalAcornLines,
        totalBeetLines,
        totalCannonLines,
        totalMissileLines,
        totalMushboomLines,
        totalPillarLines,
        totalSharktoothLines,
        totalTeleportLines;

    private List<int> voiceIndexQueue = new List<int>(),
        anyIndexQueue = new List<int>(),
        acornIndexQueue = new List<int>(),
        beetIndexQueue = new List<int>(),
        cannonIndexQueue = new List<int>(),
        missileIndexQueue = new List<int>(),
        mushboomIndexQueue = new List<int>(),
        pillarIndexQueue = new List<int>(),
        sharktoothIndexQueue = new List<int>(),
        teleportIndexQueue = new List<int>();

    // Use this for initialization
    void Awake() {
        if (self == null) {
            DontDestroyOnLoad(gameObject);
            self = this;
        }
        else Destroy(gameObject);

        totalAnyLines = System.Enum.GetValues(typeof(AnyOneLinersKind)).Length / totalVoices;
        totalAcornLines = System.Enum.GetValues(typeof(AcornOneLinersKind)).Length / totalVoices;
        totalBeetLines = System.Enum.GetValues(typeof(BeetOneLinersKind)).Length / totalVoices;
        totalCannonLines = System.Enum.GetValues(typeof(CannonOneLinersKind)).Length / totalVoices;
        totalMissileLines = System.Enum.GetValues(typeof(MissileOneLinersKind)).Length / totalVoices;
        totalMushboomLines = System.Enum.GetValues(typeof(MushboomOneLinersKind)).Length / totalVoices;
        totalPillarLines = System.Enum.GetValues(typeof(PillarOneLinersKind)).Length / totalVoices;
        totalSharktoothLines = System.Enum.GetValues(typeof(SharktoothOneLinersKind)).Length / totalVoices;
        totalTeleportLines = System.Enum.GetValues(typeof(TeleportOneLinersKind)).Length / totalVoices;

        Reset();
    }

    private void BuildQueue(List<int> queue, int max) {
        queue.Clear();
        for (int i = 0; i < max; i++) {
            queue.Add(i);
        }
        RandomizeQueue(queue);
    }

    private void RandomizeQueue(List<int> queue) {
        List<int> randomized = queue.OrderBy(i => UnityEngine.Random.value).ToList();
        queue.Clear();
        queue.AddRange(randomized);
    }

    // Public Methods
    public int AssignCharVoice() {
        if (currentVoiceIndex >= totalVoices) {
            currentVoiceIndex = 0;
            RandomizeQueue(voiceIndexQueue);
        }
        int voice = voiceIndexQueue[currentVoiceIndex];
        currentVoiceIndex++;
        return voice;
    }

    public AudioClip GetTheShotOneLiner(ProjectileKind shot, int voice) {
        return Resources.Load<AudioClip>(GetTheShotOneLinerPath(shot, voice));
    }

    public string GetTheShotOneLinerPath(ProjectileKind shot, int voice) {
        int anyNumber = UnityEngine.Random.Range(0, 10),
            lineIndex,
            voiceIndex;
        string theLine;

        if (anyNumber > 5) {
            switch (shot) {
                case (ProjectileKind)1:
                    if (currentAcornLineIndex >= totalAcornLines) {
                        currentAcornLineIndex = 0;
                        RandomizeQueue(acornIndexQueue);
                    }
                    lineIndex = acornIndexQueue[currentAcornLineIndex];
                    voiceIndex = totalAcornLines * voice;
                    acornOneLiner = (AcornOneLinersKind)(voiceIndex + lineIndex);
                    theLine = "OneLiners/" + acornOneLiner;

                    currentAcornLineIndex++;
                    return theLine;

                case (ProjectileKind)2:
                    if (currentMissileLineIndex >= totalMissileLines) {
                        currentMissileLineIndex = 0;
                        RandomizeQueue(missileIndexQueue);
                    }
                    lineIndex = missileIndexQueue[currentMissileLineIndex];
                    voiceIndex = totalMissileLines * voice;
                    missileOneLiner = (MissileOneLinersKind)(voiceIndex + lineIndex);
                    theLine = "OneLiners/" + missileOneLiner;

                    currentMissileLineIndex++;
                    return theLine;

                case (ProjectileKind)3:
                    if (currentSharktoothLineIndex >= totalSharktoothLines) {
                        currentSharktoothLineIndex = 0;
                        RandomizeQueue(sharktoothIndexQueue);
                    }
                    lineIndex = sharktoothIndexQueue[currentSharktoothLineIndex];
                    voiceIndex = totalSharktoothLines * voice;
                    sharktoothOneLiner = (SharktoothOneLinersKind)(voiceIndex + lineIndex);
                    theLine = "OneLiners/" + sharktoothOneLiner;

                    currentSharktoothLineIndex++;
                    return theLine;

                case (ProjectileKind)5:
                    if (currentPillarLineIndex >= totalPillarLines) {
                        currentPillarLineIndex = 0;
                        RandomizeQueue(pillarIndexQueue);
                    }
                    lineIndex = pillarIndexQueue[currentPillarLineIndex];
                    voiceIndex = totalPillarLines * voice;
                    pillarOneLiner = (PillarOneLinersKind)(voiceIndex + lineIndex);
                    theLine = "OneLiners/" + pillarOneLiner;

                    currentPillarLineIndex++;
                    return theLine;

                case (ProjectileKind)6:
                    if (currentBeetLineIndex >= totalBeetLines) {
                        currentBeetLineIndex = 0;
                        RandomizeQueue(beetIndexQueue);
                    }
                    lineIndex = beetIndexQueue[currentBeetLineIndex];
                    voiceIndex = totalBeetLines * voice;
                    beetOneLiner = (BeetOneLinersKind)(voiceIndex + currentBeetLineIndex);
                    theLine = "OneLiners/" + beetOneLiner;

                    currentBeetLineIndex++;
                    return theLine;

                case (ProjectileKind)7:
                    if (currentMushboomLineIndex >= totalMushboomLines) {
                        currentMushboomLineIndex = 0;
                        RandomizeQueue(mushboomIndexQueue);
                    }
                    lineIndex = mushboomIndexQueue[currentMushboomLineIndex];
                    voiceIndex = totalMushboomLines * voice;
                    mushboomOneLiner = (MushboomOneLinersKind)(voiceIndex + currentMushboomLineIndex);
                    theLine = "OneLiners/" + mushboomOneLiner;

                    currentMushboomLineIndex++;
                    return theLine;

                case (ProjectileKind)8:
                    if (currentTeleportLineIndex >= totalTeleportLines) {
                        currentTeleportLineIndex = 0;
                        RandomizeQueue(teleportIndexQueue);
                    }
                    lineIndex = teleportIndexQueue[currentTeleportLineIndex];
                    voiceIndex = totalTeleportLines * voice;
                    teleportOneLiner = (TeleportOneLinersKind)(voiceIndex + currentTeleportLineIndex);
                    theLine = "OneLiners/" + teleportOneLiner;

                    currentTeleportLineIndex++;
                    return theLine;

                default:
                    if (currentCannonLineIndex >= totalCannonLines) {
                        currentCannonLineIndex = 0;
                        RandomizeQueue(cannonIndexQueue);
                    }
                    lineIndex = cannonIndexQueue[currentCannonLineIndex];
                    voiceIndex = totalCannonLines * voice;
                    cannonOneLiner = (CannonOneLinersKind)(voiceIndex + currentCannonLineIndex);
                    theLine = "OneLiners/" + cannonOneLiner;

                    currentCannonLineIndex++;
                    return theLine;
            }
        }
        else {
            if (currentAnyLineIndex >= totalAnyLines) {
                currentAnyLineIndex = 0;
                RandomizeQueue(anyIndexQueue);
            }
            lineIndex = anyIndexQueue[currentAnyLineIndex];
            voiceIndex = totalAnyLines * voice;
            anyOneLiner = (AnyOneLinersKind)(voiceIndex + currentAnyLineIndex);
            theLine = "OneLiners/" + anyOneLiner;

            currentAnyLineIndex++;
            return theLine;
        }
    }

    public void Reset() {
        currentVoiceIndex = 0;
        currentAnyLineIndex = 0;
        currentAcornLineIndex = 0;
        currentBeetLineIndex = 0;
        currentCannonLineIndex = 0;
        currentMissileLineIndex = 0;
        currentMushboomLineIndex = 0;
        currentPillarLineIndex = 0;
        currentSharktoothLineIndex = 0;
        currentTeleportLineIndex = 0;

        BuildQueue(voiceIndexQueue, totalVoices);
        BuildQueue(anyIndexQueue, totalAnyLines);
        BuildQueue(acornIndexQueue, totalAcornLines);
        BuildQueue(beetIndexQueue, totalBeetLines);
        BuildQueue(cannonIndexQueue, totalCannonLines);
        BuildQueue(missileIndexQueue, totalMissileLines);
        BuildQueue(mushboomIndexQueue, totalMushboomLines);
        BuildQueue(pillarIndexQueue, totalPillarLines);
        BuildQueue(sharktoothIndexQueue, totalSharktoothLines);
        BuildQueue(teleportIndexQueue, totalTeleportLines);
    }
}
