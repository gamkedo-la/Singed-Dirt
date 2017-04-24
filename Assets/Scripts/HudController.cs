using UnityEngine;
using UnityEngine.UI;

public class HudController: MonoBehaviour {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text hudStatus;
	public Transform healthBar;
	public InputField powerValue;
    public GameObject[] selectedProjectileModels;

    TankController activeTank;
	ProjectileKind selectedProjectile;

    void Awake() {
		selectedProjectile = ProjectileKind.acorn;
    }

    public void AssignTank(TankController tank) {
        if (tank.isLocalPlayer) {
            activeTank = tank;
        }
    }

    // function for use with buttons in HUD to increase/decrease tank powerValue
    public void OnClickAdjustPower(int power){
        activeTank.DialAdjustPower(power);
    }

    public void OnClickAdjustHeading(int power){
        activeTank.DialAdjustHeading(power);
    }

    public void OnClickAdjustElevation(int power){
        activeTank.DialAdjustElevation(power);
    }

    void UpdateSelectedShot() {
        if(selectedProjectile != activeTank.selectedShot){
            selectedProjectile = activeTank.selectedShot;
            for(int i =0; i < selectedProjectileModels.Length;i++){
                selectedProjectileModels[i].SetActive((int)selectedProjectile == i);
            }
        }
    }

    void UpdateHealthBar() {
        // calculate health
        var health = activeTank.GetComponent<Health>();
        if (health != null) {
            var healthScale = (float) health.health/(float)Health.maxHealth;
            healthBar.localScale = new Vector3(healthScale,1f,1f);
        }
    }

    void UpdateHudStatus() {
		var horizontalTurret = activeTank.model.tankRotation;
		var verticalTurret = activeTank.model.turretElevation;
		var shotPower = activeTank.shotPower;
        var health = activeTank.GetComponent<Health>();
        var tankHitPoints =  (health != null) ? health.health : 100;
        string ammoCount = activeTank.AmmoDisplayCountText();
		hudStatus.text =
			"Heading: " + horizontalTurret + " degrees\n" +
			"Elevation: " + verticalTurret + " degrees\n" +
			"Muzzle Velocity: " + shotPower + " m/s\n" +
			"HitPoints: " + tankHitPoints + "\n" +
            "Ammo remaining for selected shot type: " + ammoCount; // + "m/s\n" +
			// "projectile: " + selectedProjectile;
		powerValue.text = "" + shotPower;
    }

    void Update() {
        if (activeTank != null) {
            UpdateSelectedShot();
            UpdateHealthBar();
            UpdateHudStatus();
        }
    }
}
