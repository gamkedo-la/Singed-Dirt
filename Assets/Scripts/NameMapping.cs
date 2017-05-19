using System.Collections.Generic;
public static class NameMapping {
    static Dictionary<ProjectileKind, string> projectileMapping =
        new Dictionary<ProjectileKind, string>() {
            { ProjectileKind.cannonBall, "Cannon Ball" },
            { ProjectileKind.acorn, "Deez Nutz" },
            { ProjectileKind.artilleryShell, "FUD Maker" },
            { ProjectileKind.sharkToothCluster, "To The Teeth" },
            { ProjectileKind.beetMissile, "Beet Dem Down" },
            { ProjectileKind.pillarShot, "Build Dem Up" },
            { ProjectileKind.mushboom, "MushBoom" },
        };

    public static string ForProjectile(ProjectileKind projectileKind) {
        if (projectileMapping.ContainsKey(projectileKind)) {
            return projectileMapping[projectileKind];
        } else {
            return projectileKind.ToString();
        }
    }
}
