using System;

/// <summary>
/// The following sets of enums represent different prefabs under the Resources directory.
/// Each Enum represents a separate subdirectory, and the naming of the subdirectory must
/// match the name of the Enum minus "Kind" (e.g.: ProjectileKind expects there to be a
/// Resources/Projectile/ subdirectory.  Under the subdirectory, the names of the prefabs
/// must match the Enum values (e.g.: For ProjectileKind.cannonBall, there should be a
/// prefab named Resources/Projectile/cannonBall.prefab

/// NOTE: The one-liner randomization system requires a different version of every line for each 
/// character voice. If there are 4 characters, 4 versions of any new lines are required. There are 
/// unique Enums for ammo-specific lines while the AnyOneLinersKind can be used for any ammo 
/// type.

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
    mushboom
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
    tank_movement_UpDown_LOOP_01
}

public enum ProjectileSoundKind {
    groundHit = 0,
	projectile_explo,
	tank_hit
}

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
    voice_aggro56,       // move to teleport when implemented

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
    voice_cheery56,  // move to teleport when implemented

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
    voice_grumpy56,     // move to teleport when implemented

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
    voice_meanie29,
    voice_meanie56      // move to teleport when implemented
}

public enum AcornOneLinersKind {
    voice_aggro02,
    voice_aggro30,
    voice_aggro31,
    voice_aggro32,

    voice_cheery02,
    voice_cheery30,
    voice_cheery31,
    voice_cheery32,

    voice_grumpy02,
    voice_grumpy30,
    voice_grumpy31,
    voice_grumpy32,

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

    voice_cheery33,
    voice_cheery34,
    voice_cheery35,
    voice_cheery36,

    voice_grumpy33,
    voice_grumpy34,
    voice_grumpy35,
    voice_grumpy36,

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

    voice_cheery08,
    voice_cheery19,
    voice_cheery22,
    voice_cheery37,

    voice_grumpy08,
    voice_grumpy19,
    voice_grumpy22,
    voice_grumpy37,

    voice_meanie08,
    voice_meanie19,
    voice_meanie22,
    voice_meanie37
}

public enum MissileOneLinersKind {
    voice_aggro05,
    voice_aggro26,
    voice_aggro28,
    voice_aggro38,

    voice_cheery05,
    voice_cheery26,
    voice_cheery28,
    voice_cheery38,

    voice_grumpy05,
    voice_grumpy26,
    voice_grumpy28,
    voice_grumpy38,

    voice_meanie05,
    voice_meanie26,
    voice_meanie28,
    voice_meanie38
}

public enum MushboomOneLinersKind {
    voice_aggro45,
    voice_aggro46,
    voice_aggro47,
    voice_aggro48,

    voice_cheery45,
    voice_cheery46,
    voice_cheery47,
    voice_cheery48,

    voice_grumpy44,
    voice_grumpy45,
    voice_grumpy47,
    voice_grumpy48,

    voice_meanie44,
    voice_meanie45,
    voice_meanie47,
    voice_meanie48
}

public enum PillarOneLinersKind {
    voice_aggro39,
    voice_aggro41,
    voice_aggro42,
    voice_aggro43,

    voice_cheery40,
    voice_cheery41,
    voice_cheery42,
    voice_cheery43,

    voice_grumpy39,
    voice_grumpy40,
    voice_grumpy41,
    voice_grumpy42,

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

    voice_cheery03,
    voice_cheery49,
    voice_cheery50,
    voice_cheery51,

    voice_grumpy03,
    voice_grumpy49,
    voice_grumpy50,
    voice_grumpy51,

    voice_meanie03,
    voice_meanie49,
    voice_meanie50,
    voice_meanie51
}

public enum TeleportOneLinersKind {
    voice_aggro52,
    voice_aggro54,
    voice_aggro55,

    voice_cheery53,
    voice_cheery54,
    voice_cheery55,

    voice_grumpy52,
    voice_grumpy54,
    voice_grumpy55,

    voice_meanie53,
    voice_meanie54,
    voice_meanie55
}

public enum ExplosionKind {
    fire = 0,
	virus,
    cluster,
    bomblet,
    molasses
}

public enum DeformationKind {
    shotCrater = 0,
    bombletCrater,
    pillarDeformer,
    molPuddle
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
}
