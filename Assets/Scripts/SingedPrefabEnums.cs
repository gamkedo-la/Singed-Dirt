using System;

/// <summary>
/// The following sets of enums represent different prefabs under the Resources directory.
/// Each Enum represents a separate subdirectory, and the naming of the subdirectory must
/// match the name of the Enum minus "Kind" (e.g.: ProjectileKind expects there to be a
/// Resources/Projectile/ subdirectory.  Under the subdirectory, the names of the prefabs
/// must match the Enum values (e.g.: For ProjectileKind.cannonBall, there should be a
/// prefab named Resources/Projectile/cannonBall.prefab

/// NOTE: for prefabs that must be spawned over the network,
/// ensure that NetRegistry.spawnableEnums is updated appropriately
/// </summary>

public enum ProjectileKind {
    cannonBall = 0,
    acorn,
    artilleryShell,
    sharkToothCluster,
    sharkToothBomblet
}

public enum ExplosionKind {
    fire = 0,
	virus,
    cluster,
    bomblet
}

public enum DeformationKind {
    shotCrater = 0,
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
