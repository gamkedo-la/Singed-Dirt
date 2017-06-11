using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HudController: MonoBehaviour {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text hudStatus;
	public Transform healthBar;
	public InputField powerValue;
	public Transform projectileModelWorldPosition;
	public Camera projectileViewCamera;
	public RectTransform projectileModelScreenPosition;
	public RectTransform shotPowerBar;
	public AmmoListController ammoListPanel;
	private Dictionary<ProjectileKind, GameObject> projetileModels;

	TankController activeTank;
	ProjectileKind selectedProjectile;
	bool forceAmmoUpdate = false;

	void Awake() {
		selectedProjectile = ProjectileKind.cannonBall;
		projetileModels = new Dictionary<ProjectileKind, GameObject>();
		projetileModels[selectedProjectile] = getProjectileModel(selectedProjectile);
		UpdateSelectedShot();
		forceAmmoUpdate = true;
		adjustProjectileModelPosition();
  }

	private void adjustProjectileModelPosition()
	{
		// Debug.Log("Projectile pivot: " + projectileModelScreenPosition.position);
		projectileModelWorldPosition.position = projectileViewCamera.ScreenToWorldPoint(new Vector3(projectileModelScreenPosition.position.x, projectileModelScreenPosition.position.y, 1f));
	}

    public void AssignTank(TankController tank) {
        if (tank.isLocalPlayer) {
            activeTank = tank;
            activeTank.shotInventory.onModifyEvent.RemoveAllListeners();
            activeTank.shotInventory.onModifyEvent.AddListener(OnInventoryModify);
            ammoListPanel.AssignInventory(activeTank.shotInventory);
        }
    }

    public void OnInventoryModify(object inventory) {
        // update current inventory se
        if (Object.ReferenceEquals((object) activeTank.shotInventory, inventory)) {
            ammoListPanel.AssignInventory(activeTank.shotInventory);
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
        if(forceAmmoUpdate || (activeTank != null && selectedProjectile != activeTank.selectedShot)) {
            forceAmmoUpdate = false;
            projetileModels[selectedProjectile].SetActive(false);
            if(!projetileModels.ContainsKey(activeTank.selectedShot)) {
    			projetileModels[activeTank.selectedShot] = getProjectileModel(activeTank.selectedShot);
            }

            projetileModels[activeTank.selectedShot].SetActive(true);

			selectedProjectile = activeTank.selectedShot;
            // update ammo list panel to reference selected projectile
            ammoListPanel.SetSelected(selectedProjectile);
        }
    }

    public void ToggleModelView(bool enabled) {
        projetileModels[selectedProjectile].GetComponent<MeshRenderer>().enabled = enabled;
    }

    public GameObject getProjectileModel(ProjectileKind shotToShow) {
		GameObject prefab = PrefabRegistry.singleton.GetPrefab<ProjectileKind>(shotToShow);
		GameObject shotModelPrefab = prefab.transform.Find("Model").gameObject;
		GameObject shotModel = (GameObject)GameObject.Instantiate(shotModelPrefab, projectileModelWorldPosition.position, projectileModelWorldPosition.rotation);

		shotModel.transform.parent = projectileModelWorldPosition;
		SetLayer(shotModel.transform, projectileModelWorldPosition.gameObject.layer);

		Vector3 scale = shotModel.transform.localScale;

		scale.x *= prefab.transform.localScale.x;
		scale.y *= prefab.transform.localScale.y;
		scale.z *= prefab.transform.localScale.z;

		scale.x *= projectileModelWorldPosition.localScale.x;
		scale.y *= projectileModelWorldPosition.localScale.y;
		scale.z *= projectileModelWorldPosition.localScale.z;

		shotModel.transform.localScale = scale;

		shotModel.transform.localRotation = shotModelPrefab.transform.localRotation;

		return shotModel;
	}

	//Recursively set the layer of a transform and all children
	private void SetLayer(Transform root, int layer) {
		root.gameObject.layer = layer;
		foreach(Transform child in root) {
			SetLayer(child, layer);
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
		float shotPower = activeTank.shotPower;
        var health = activeTank.GetComponent<Health>();
        var tankHitPoints =  (health != null) ? health.health : 100;
        string ammoCount = activeTank.AmmoDisplayCountText();
		hudStatus.text =
			// "Heading: " + horizontalTurret + " degrees\n" +
			// "Elevation: " + verticalTurret + " degrees\n" +
			// "Muzzle Velocity: " + shotPower + " m/s\n" +
			// "HitPoints: " + tankHitPoints + "\n" +
            // "Ammo remaining for selected shot type: " + ammoCount + "\n" +
            "Press H for help!"; // + "m/s\n" +
			// "projectile: " + selectedProjectile;
		// powerValue.text = "" + shotPower;

		float power = shotPower / activeTank.maxShotPower;
		shotPowerBar.localScale = new Vector3(power, 1f, 1f);
		//shotPowerBar.localPosition = new Vector3(power, 0, 0);
	}

	void Update() {
        if (activeTank != null) {
            UpdateSelectedShot();
            UpdateHealthBar();
            UpdateHudStatus();
        }
    }
}
