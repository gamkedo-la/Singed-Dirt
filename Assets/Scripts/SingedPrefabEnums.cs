using System;

/// <summary>
/// The following sets of enums represent different prefabs under the Resources directory.
/// Each Enum represents a separate subdirectory, and the naming of the subdirectory must
/// match the name of the Enum minus "Kind" (e.g.: ProjectileKind expects there to be a
/// Resources/Projectile/ subdirectory.  Under the subdirectory, the names of the prefabs
/// must match the Enum values (e.g.: For ProjectileKind.cannonBall, there should be a
/// prefab named Resources/Projectile/cannonBall.prefab

/// NOTE: if prefabs must be spawned over the network, their enum type should be added to the
/// NetRegistry.spawnableEnums definition.  This only needs to be updated for adding new enum kinds,
/// not adding individual values to existing enums.
/// </summary>

public enum ProjectileKind {
    cannonBall = 0,
    acorn,
    artilleryShell,
    beetMissile,
    projectX,
    sharkToothCluster,
    sharkToothBomblet,
    pillarShot
}

public enum OneLinersKind {
    voice_grumpy01 = 0,
    voice_grumpy02,
    voice_grumpy03,
    voice_grumpy04,
    voice_grumpy05,
    voice_grumpy06,
    voice_grumpy07,
    voice_grumpy08,
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
    voice_grumpy19,
    voice_grumpy20,
    voice_grumpy21,
    voice_grumpy22,
    voice_grumpy23,
    voice_grumpy24,
    voice_grumpy25,
    voice_grumpy26,
    voice_grumpy27,
    voice_grumpy28,
    voice_grumpy29,
    voice_meanie01,
    voice_meanie02,
    voice_meanie03,
    voice_meanie04,
    voice_meanie05,
    voice_meanie06,
    voice_meanie07,
    voice_meanie08,
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
    voice_meanie19,
    voice_meanie20,
    voice_meanie21,
    voice_meanie22,
    voice_meanie23,
    voice_meanie24,
    voice_meanie25,
    voice_meanie26,
    voice_meanie27,
    voice_meanie28,
    voice_meanie29
}

public enum ExplosionKind {
    fire = 0,
	virus,
    cluster,
    bomblet
}

public enum DeformationKind {
    shotCrater = 0,
    bombletCrater,
    pillarDeformer
}

public enum TankBaseKind {
    standard = 0,
    crocodile,
    squirrel,
}

public enum TankTurretBaseKind {
    standard = 0,
    crocodile,
    squirrel,
}

public enum TankTurretKind {
    standard = 0,
    crocodile,
    squirrel,
}

public enum TankHatKind {
    sunBlack = 0,
    sunBlue,
    sunGreen,
    sunRed,
    sunYellow,
    sunWhite,
	horn,
}
