using System;

/// <summary>
/// The following sets of enums represent different prefabs under the Resources directory.
/// Each Enum represents a separate subdirectory, and the naming of the subdirectory must
/// match the name of the Enum minus "Kind" (e.g.: ProjectileKind expects there to be a
/// Resources/Projectile/ subdirectory.  Under the subdirectory, the names of the prefabs
/// must match the Enum values (e.g.: For ProjectileKind.cannonBall, there should be a
/// prefab named Resources/Projectile/cannonBall.prefab (See BarkManager.cs for notes on 
/// adding ammo-specific one-liners.)

/// NOTE: if prefabs must be spawned over the network, their enum type should be added to the
/// NetRegistry.spawnableEnums definition.  This only needs to be updated for adding new enum kinds,
/// not adding individual values to existing enums.
/// </summary>

public enum ProjectileKind {
    cannonBall = 0,
    acorn,
    artilleryShell,
    sharkToothCluster,
    sharkToothBomblet,
    pillarShot,
    beetMissile,
    mushboom,
    teleportBall
}

public enum MenuSoundKind {
    menuSelect = 0,
    menuBack,
    playerConnect,
    ui_tank_option,
    ui_tank_rotate
}

public enum MusicKind {
    mainMenuMusic = 0,
    gameplayMusic
}

public enum TankSoundKind {
    canonFire1 = 0,
    canonFire2,
    tank_movement_LeftRight_LOOP_01,
    tank_movement_UpDown_LOOP_01,
    tank_power_UpDown_LOOP
}

public enum ProjectileSoundKind {
    groundHit = 0,
    projectile_explo,
    tank_hit
}

public enum ExplosionKind {
    fire = 0,
    virus,
    cluster,
    bomblet,
    molasses,
    mushCloud,
    peeShooterFire,
    hacking
}

public enum DeformationKind {
    shotCrater = 0,
    bombletCrater,
    pillarDeformer,
    molPuddle,
    groundZero,
    peeCrater,
    teleportScar
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

public enum SpawnKind {
    lootbox = 0,
    glowBall,
    InfectedParticles
}