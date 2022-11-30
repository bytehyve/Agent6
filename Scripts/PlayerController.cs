using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using JaimesUtilities;
using JaimesUtilities.SaveLoad;

// This script is not refactorized.

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public static UnityEvent<Vector3, int, float> OnExplosion = new UnityEvent<Vector3, int, float>();
    public static float cameraFOV;

    [Header("Callbacks")]
    [SerializeField] public Transform playerBody;
    [SerializeField] public Transform cameraBody;
    [SerializeField] private Transform weaponBody;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas UICanvas;
    [SerializeField] private RectTransform crosshairBody;
    [SerializeField] private GameObject ammoUIParent;
    [SerializeField] private UnityEngine.UI.Text ammoClipTextMesh;
    [SerializeField] private UnityEngine.UI.Text ammoStashTextMesh;
    [SerializeField] private UnityEngine.UI.Text hintTextMesh;
    [SerializeField] private RectTransform displayTextParent;
    [SerializeField] private GameObject displayTextPrefab;
    [SerializeField] private GameObject hudParent;
    [SerializeField] private UnityEngine.UI.Image hpImage;
    [SerializeField] private UnityEngine.UI.Image armorImage;
    [SerializeField] private UnityEngine.UI.Image staticHitImage;
    [SerializeField] private UnityEngine.UI.Image ammoIcon;
    [SerializeField] private List<EquipmentInfo> startEquipments;
    [SerializeField] private AudioClip reloadSound;

    [Header("Spawn")]
    [SerializeField] private float startYRotation;

    [Header("Prefabs")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private GameObject bulletHitPrefab;

    [Header("Stats")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float moveSpeed;
    [SerializeField] private int ammoClipsAtStart;
    [SerializeField] private float hitForce;

    [Header("AutoAnimation")]
    [SerializeField] private float idleFrequency;
    [SerializeField] private float idleAmplitude;
    [SerializeField] private float headbobFrequency;
    [SerializeField] private float headbobAmplitude;
    [SerializeField] private float headbobRotation;
    [SerializeField] private float leanSnappiness;
    [SerializeField] private float leanDistance;
    [SerializeField] private float weaponActionSpeed;

    [Header("AimAssist")]
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float manualAimSensitivity;
    [SerializeField] private float manualAimRotateSpeed;
    [SerializeField] private float maxAimAssistDistance;
    [SerializeField] private float maxAimAssistAngle;
    [SerializeField] private float weaponRotationSnappiness;
    [SerializeField] private float weaponSwayStrength;

    [SerializeField] private float recoilAmplitude;
    [SerializeField] private float recoilSnappiness;

    // Callbacks
    private Rigidbody rb;
    private byte equipmentIndex;
    private List<EquipmentInfo> equipments = new List<EquipmentInfo>();
    private EquipmentInfo currentEquipment => equipments.Count > 0 ? equipments[equipmentIndex] : null;
    private WeaponInfo currentWeapon => currentEquipment as WeaponInfo;
    private Stats stats = new Stats();

    // Inputs
    private bool blocked;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool[] mouseInput = new bool[2];
    private AutoResetBool[] mouseInputDown = new AutoResetBool[] {
        new AutoResetBool(1f),
        new AutoResetBool(1f)
    };
    private AutoResetBool reloadInputDown = new AutoResetBool(1f);
    private AutoResetBool switchInputDown = new AutoResetBool(1f);
    private Vector3 groundNormalLerped;

    // Rotation
    private Vector3 defaultCameraPosition;
    private Vector3 defaultWeaponPosition;
    private Vector3 targetRotation;
    private float headbobValue;
    private float headbobStrength;
    private float leanValue;
    private Vector2 swayValue;

    // Aiming
    [HideInInspector] public List<AimTarget> aimTargets;
    private bool isAiming;
    private Vector3 aimDirection;
    private Vector2 manualAimAngle;
    private float defaultFOV;
    private float savedFOV;

    // Weapon
    private enum EquipmentActionType { Noone, Reload, Switch }
    private EquipmentActionType equipmentAction;
    private bool InAction => EquipmentActionValue > 0.05f;
    private float EquipmentActionValue;
    private float recoilValue;
    private bool recoilOn;
    private float fireWaitTime;
    private float muzzleFlashTime;

    #region Monobehaviour Callbacks
    private void Awake() {
        instance = this;
        rb = GetComponent<Rigidbody>();
        defaultCameraPosition = cameraBody.localPosition;
        defaultWeaponPosition = weaponBody.localPosition;
        defaultFOV = mainCamera.fieldOfView;
        savedFOV = defaultFOV;

        foreach (EquipmentInfo eq in startEquipments) {
            ObtainEquipment(eq, false);
            if (eq as WeaponInfo != null) {
                if (ammoClipsAtStart == 0) ((WeaponInfo)eq).ammoClip = 0;
                else ((WeaponInfo)eq).AddAmmo(ammoClipsAtStart * ((WeaponInfo)eq).clipSize);
            }
        }

        aimTargets = new List<AimTarget>();
        displayTextElements = new List<GameObject>();

        targetRotation.y = startYRotation;
        OnExplosion = new UnityEvent<Vector3, int, float>();
        OnExplosion.AddListener(OnExplode);
    }
    private void Update() {
        if (!WorldManager.Instance.isDead) GetInput();

        if (GameManager.Paused) {
            mainCamera.fieldOfView = defaultFOV;
            UICanvas.GetComponent<RectTransform>().localScale = Vector3.one * Mathf.Pow(defaultFOV * 0.0000066f, 1.075f);
        }
    }
    private void FixedUpdate() {
        if (!WorldManager.Instance.isDead) {
            SetGroundNormal();
            SetVelocity();
            UpdateEquipment();
            ResetInput();
        }

        UpdateUI();
    }
    private void LateUpdate() {
        if (GameManager.Paused) return;

        cameraFOV = mainCamera.fieldOfView;

        if (!WorldManager.Instance.isDead) { 
            SetRotation();
            UpdateAim();
        }
    }
    #endregion

    #region Main Control
    private void GetInput() {
        if (!GameManager.Paused && !blocked) {
            float axisHor = 0;
            float axisVer = 0;

            if (SettingsManager.GetInput("MoveLeft", out KeyCode mLeft) && Input.GetKey(mLeft)) axisHor = -1;
            if (SettingsManager.GetInput("MoveRight", out KeyCode mRight) && Input.GetKey(mRight)) axisHor = 1;
            if (SettingsManager.GetInput("MoveForward", out KeyCode mForward) && Input.GetKey(mForward)) axisVer = 1;
            if (SettingsManager.GetInput("MoveBack", out KeyCode mBack) && Input.GetKey(mBack)) axisVer = -1;

            moveInput = new Vector2(axisHor, axisVer);
            lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            
            if (SettingsManager.GetInput("Fire", out KeyCode fireInput)) {
                mouseInput[0] = Input.GetKey(fireInput);
                if (Input.GetKeyDown(fireInput)) mouseInputDown[0].SetTrue();
            }
            if (SettingsManager.GetInput("Aim", out KeyCode aimInput)) {
                mouseInput[1] = Input.GetKey(aimInput);
                if (Input.GetKeyDown(aimInput)) mouseInputDown[1].SetTrue();
            }

            if (SettingsManager.GetInput("Swap", out KeyCode swapInput) && Input.GetKeyDown(swapInput)) switchInputDown.SetTrue();
            if (SettingsManager.GetInput("Reload", out KeyCode reloadInput) && Input.GetKeyDown(reloadInput)) reloadInputDown.SetTrue();
        }
        else {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            for (byte i = 0; i < 2; i++) {
                mouseInput[i] = false;
            }
            rb.Sleep();
        }
    }
    private void ResetInput() {
        for (byte i = 0; i < 2; i++) {
            mouseInputDown[i].SetFalse();
        }
        switchInputDown.SetFalse();
        reloadInputDown.SetFalse();
    }
    private void SetRotation() {
        // Idle
        float timeStep = Time.time * idleFrequency;
        Vector3 cameraIdleValue = (new Vector3(
            Mathf.PerlinNoise(timeStep, 0),
            Mathf.PerlinNoise(timeStep, 1000),
            Mathf.PerlinNoise(timeStep, 2000)) * 2f - Vector3.one) * idleAmplitude;
        Vector3 weaponIdleValue = (new Vector3(
            Mathf.PerlinNoise(timeStep, 3000),
            Mathf.PerlinNoise(timeStep, 4000),
            Mathf.PerlinNoise(timeStep, 5000)) * 2f - Vector3.one) * idleAmplitude * 3f;

        // Leaning
        float targetLeanValue = 0;
        if (isAiming) {
            targetLeanValue = moveInput.x * leanDistance;
            if (Physics.Raycast(transform.position, playerBody.right * moveInput.x, out RaycastHit hitInfo, leanDistance, WorldManager.Instance.settings.collideMask)) {
                targetLeanValue = moveInput.x * hitInfo.distance / 4f;
            }
        }
        leanValue = Mathf.Lerp(leanValue, targetLeanValue, leanSnappiness * Time.deltaTime);
        playerBody.localPosition = playerBody.right * leanValue;

        // Headbob
        headbobStrength = Mathf.Lerp(headbobStrength, moveInput.sqrMagnitude > 0 && !isAiming ? 1f : 0f, 5f * Time.deltaTime);
        headbobValue += headbobStrength * headbobFrequency * Time.deltaTime;
        Vector3 targetHeadbobPosition = new Vector2(Mathf.Sin(headbobValue), -Mathf.Sin(headbobValue).Abs()) * headbobAmplitude;
        Vector3 headbobPosition = Vector3.Lerp(Vector3.zero, targetHeadbobPosition, headbobStrength);
        
        cameraBody.localPosition = defaultCameraPosition + new Vector3(headbobPosition.x / 2f, headbobPosition.y);

        // Camera
        float sensitivity = 0.5f;
        if (JaimesUtilities.SaveLoad.SettingsManager.GetFloat("MouseSensitivity", out float msresult)) sensitivity += msresult; 
        
        if (!isAiming) { 
            targetRotation.x -= lookInput.y * mouseSensitivity * sensitivity;
            targetRotation.y += lookInput.x * mouseSensitivity * sensitivity;
        }
        else {
            Vector2 aimPerc = manualAimAngle / mainCamera.fieldOfView;
            if (aimPerc.x.Abs() > aimDeadZone) targetRotation.y += aimPerc.x * manualAimRotateSpeed * sensitivity * (1f + cameraFOV / 20f);
            if (aimPerc.y.Abs() > aimDeadZone) targetRotation.x -= aimPerc.y * manualAimRotateSpeed * sensitivity * (1f + cameraFOV / 20f);
        }
        targetRotation.x = Mathf.Clamp(targetRotation.x, -80, 80);

        playerBody.localRotation = Quaternion.Euler(0, targetRotation.y, 0);
        cameraBody.localRotation = Quaternion.Euler(
            targetRotation.x + cameraIdleValue.x, 
            cameraIdleValue.y, 
            headbobPosition.x * headbobRotation + cameraIdleValue.z);

        // Weapon
        Vector2 targetWeaponSway = new Vector2(-lookInput.y.Clamp(-1) / 2f, lookInput.x.Clamp(-1)) * weaponSwayStrength / 2.5f;
        swayValue = Vector2.Lerp(swayValue, targetWeaponSway, 20f * Time.deltaTime);
        Quaternion targetWeaponRotation 
            = Quaternion.LookRotation(aimDirection != Vector3.zero ? aimDirection : cameraBody.up, cameraBody.up) 
            * Quaternion.Euler(swayValue) 
            * Quaternion.Euler(weaponIdleValue) 
            * Quaternion.Euler(-recoilValue + EquipmentActionValue * 60, 0, 0);
        weaponBody.localPosition 
            = defaultWeaponPosition 
            + new Vector3(-headbobPosition.x, headbobPosition.y / 2f) / 6f
            + new Vector3(0, -EquipmentActionValue / 4f, 0);
        weaponBody.rotation = Quaternion.Slerp(weaponBody.rotation, targetWeaponRotation, weaponRotationSnappiness * Time.deltaTime);
    }
    private void SetVelocity() {
        if (!isAiming) { 
            Vector3 groundNormal = groundNormalLerped;
            Vector3 targetVelocity = (-Vector3.Cross(groundNormal, playerBody.right) * moveInput.y + Vector3.Cross(groundNormal, playerBody.forward) * moveInput.x) * moveSpeed;
            rb.AddForce(targetVelocity, ForceMode.VelocityChange);
        }

        rb.AddForce(-groundNormalLerped * WorldManager.Instance.settings.gravity, ForceMode.VelocityChange);
    }
    private void SetGroundNormal() {
        Vector3 result = Vector3.up;
        if (Physics.Raycast(transform.position, Vector3.Lerp(-playerBody.up, 
            playerBody.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)), 
            moveInput.magnitude / 5f), out RaycastHit hitInfo, 2f, WorldManager.Instance.settings.staticCollideMask)) {
            
            float angle = Vector3.Angle(hitInfo.normal, result);
            if (angle < 48) result = hitInfo.normal;
        }

        groundNormalLerped = Vector3.Lerp(groundNormalLerped, result, 15f * Time.deltaTime);
    }
    public void BlockControls() {
        blocked = true;
    }
    #endregion

    #region Equipment Control
    private float aimDeadZone = 0.7f;

    private void UpdateEquipment() {
        if (equipmentAction == EquipmentActionType.Noone) {
            if (currentWeapon != null && ((currentWeapon.ammoClip == 0 && currentWeapon.ammoStash > 0) || (reloadInputDown.value && currentWeapon.ammoClip < currentWeapon.clipSize && currentWeapon.ammoStash > 0))) {
                equipmentAction = EquipmentActionType.Reload;
                reloadInputDown.SetFalse();
            }
            if (switchInputDown.value) {
                equipmentAction = EquipmentActionType.Switch;
                switchInputDown.SetFalse();
            }
        }
        else {
            if (EquipmentActionValue > 0.999f) {
                switch (equipmentAction) {
                    case EquipmentActionType.Reload:
                        OnReload();
                        break;
                    case EquipmentActionType.Switch:
                        OnSwitch();
                        break;
                }
                equipmentAction = EquipmentActionType.Noone;
            }
        }
        EquipmentActionValue = Mathf.Lerp(EquipmentActionValue, equipmentAction != EquipmentActionType.Noone ? 1f : 0f, weaponActionSpeed * Time.deltaTime);

        // Set objs active
        for (byte i = 0; i < equipments.Count; i++) {
            equipments[i].ingamePrefab.SetActive(i == equipmentIndex);
        }
        if (currentWeapon != null) currentWeapon.MuzzleFlash.SetActive(muzzleFlashTime > Time.time);

        // Shoot
        if (!InAction && fireWaitTime < Time.time) {
            if (currentWeapon != null) { 
                if (currentWeapon.automatic && mouseInput[0] || mouseInputDown[0].value) {
                    if (currentWeapon.ammoClip > 0) OnWeaponAction();
                    else if (equipmentAction == EquipmentActionType.Noone && currentWeapon.ammoStash == 0) equipmentAction = EquipmentActionType.Switch;
                }
            }
            else if (currentEquipment as MeleeInfo != null) {
                if (mouseInputDown[0].value) OnMeleeAction();
            }
        }

        // Recoil
        float targetRecoilValue = recoilOn ? recoilAmplitude : 0f;
        recoilValue = Mathf.Lerp(recoilValue, targetRecoilValue, recoilSnappiness * Time.deltaTime);
        if (recoilValue > recoilAmplitude / 1.1f) recoilOn = false;

        // SetIcon
        if (currentWeapon != null) {
            ammoIcon.sprite = currentWeapon.ammoIcon;
        }
    }
    private void UpdateAim() { 
        isAiming = mouseInput[1] && currentEquipment != null && currentEquipment as WeaponInfo != null;
        crosshairBody.gameObject.SetActive(isAiming);

        if (isAiming) {
            manualAimAngle.x += lookInput.x * manualAimSensitivity * (1f + cameraFOV / 50f);
            manualAimAngle.y += lookInput.y * manualAimSensitivity * (1f + cameraFOV / 50f);
            float aimRetrieveRate = 15f - (1f + Mathf.Max(lookInput.x.Abs(), lookInput.y.Abs()).Clamp(0, 1f) * 14f);
            manualAimAngle = Vector2.Lerp(manualAimAngle, Vector2.zero, aimRetrieveRate * manualAimSensitivity / 10000f);

            Vector2 aimPerc = manualAimAngle / mainCamera.fieldOfView;
            crosshairBody.localPosition = new Vector3(
                aimPerc.x * UICanvas.GetComponent<RectTransform>().rect.width,
                aimPerc.y * UICanvas.GetComponent<RectTransform>().rect.height) / 2f;

            if (aimPerc.x.Abs() > aimDeadZone) manualAimAngle.x = Mathf.Lerp(manualAimAngle.x, 0, (aimPerc.x.Abs() - aimDeadZone) * manualAimSensitivity / 20f * (1f + cameraFOV / 20f));
            if (aimPerc.y.Abs() > aimDeadZone) manualAimAngle.y = Mathf.Lerp(manualAimAngle.y, 0, (aimPerc.y.Abs() - aimDeadZone) * manualAimSensitivity / 20f * (1f + cameraFOV / 20f));

            aimDirection = (crosshairBody.transform.position - cameraBody.position).normalized;
        }
        else {
            if (currentWeapon != null && currentWeapon.useAutoAim || currentWeapon == null) { 
                float bestAngle = maxAimAssistAngle;
                Vector3 bestDirection = cameraBody.forward;

                if (aimTargets != null && aimTargets.Count > 0) {
                    for (int i = aimTargets.Count - 1; i >= 0; i--) {
                        if (aimTargets[i] == null) {
                            aimTargets.RemoveAt(i);
                            continue;
                        }

                        Vector3 dir = (aimTargets[i].GetAimPoint() - cameraBody.position);
                        float dist = dir.magnitude;
                        if (dist > maxAimAssistDistance) continue;

                        float ang = Vector3.Angle(dir, cameraBody.forward);
                        if (ang > bestAngle) continue;

                        if (Physics.Raycast(cameraBody.position, dir, out RaycastHit hitInfo, (cameraBody.position - aimTargets[i].GetAimPoint()).magnitude)) {
                            if (WorldManager.Instance.settings.enemyMask == (WorldManager.Instance.settings.enemyMask | (1 << hitInfo.collider.gameObject.layer))) {
                                bestAngle = ang;
                                bestDirection = dir;
                            }
                        }
                    }
                }

                Vector3 targetAimDirection = bestDirection;

                float xAngle = targetAimDirection.AngleAxis(cameraBody.forward, cameraBody.right);
                targetAimDirection = Quaternion.AngleAxis(-xAngle / 3f, cameraBody.right) * targetAimDirection;

                aimDirection = targetAimDirection;
                manualAimAngle = Vector2.zero;
            }
            else {
                aimDirection = cameraBody.forward;
                manualAimAngle = Vector2.zero;    
            }
        }

        // Set new FoV
        float targetFoV = defaultFOV;
        if (currentWeapon != null && isAiming) targetFoV /= currentWeapon.zoomRate;
        savedFOV += (targetFoV - savedFOV).Sign() * Mathf.Min(Mathf.Abs(targetFoV - savedFOV), zoomSpeed * Time.deltaTime);
        mainCamera.fieldOfView = savedFOV;

        // Resize canvas on FoV change (Values are an estimate!)
        UICanvas.GetComponent<RectTransform>().localScale = Vector3.one * Mathf.Pow(mainCamera.fieldOfView * 0.0000066f, 1.075f);  
    }
    
    private void OnWeaponAction() {
        mouseInputDown[0].SetFalse();

        Vector3 direction = aimDirection;
        if (!isAiming) {
            Vector2 offset = Random.insideUnitCircle.normalized;
            Vector3 axis = cameraBody.TransformDirection(new Vector3(offset.x, offset.y));
            direction = Quaternion.AngleAxis(Random.Range(0, currentWeapon.bloom) * 60, axis) * direction;
        }

        if (currentWeapon.customEmitter == null) { 
            Vector3 hitPoint = cameraBody.position + aimDirection * 100f;
            if (Physics.Raycast(cameraBody.position, direction, out RaycastHit hitInfo, 100f, WorldManager.Instance.settings.collideMask)) {
                if (WorldManager.Instance.settings.staticCollideMask == (WorldManager.Instance.settings.staticCollideMask | (1 << hitInfo.collider.gameObject.layer))) { 
                    Vector3 bulletHolePosition = hitInfo.point + hitInfo.normal / 100f;
                    GameObject bulletHoleObj = Instantiate(bulletHolePrefab, bulletHolePosition, Quaternion.LookRotation(hitInfo.normal, Vector3.up));
                    Destroy(bulletHoleObj, 5f);
            
                    if (!currentWeapon.isSilenced) AISoldier.InvokeOnShootNearby(bulletHolePosition);
                }
                else if (WorldManager.Instance.settings.targetHitMask == (WorldManager.Instance.settings.targetHitMask | (1 << hitInfo.collider.gameObject.layer))) {
                    if (hitInfo.collider.transform.GetComponent<Hitbox>() != null) {
                        hitInfo.collider.transform.GetComponent<Hitbox>().OnHit(currentWeapon.damage);
                    }
                }

                hitPoint = hitInfo.point;

                GameObject bulletHit = Instantiate(bulletHitPrefab, hitPoint + hitInfo.normal / 10f, Quaternion.identity);
                Destroy(bulletHit.gameObject, 5f);
            }

            if (Random.Range(0, 3) == 0) { 
                GameObject bulletObj = Instantiate(bulletPrefab, currentWeapon.EndPoint, Quaternion.identity);
                bulletObj.GetComponent<BulletControl>().targetPosition = hitPoint;
                Destroy(bulletObj, 2f);
            }

            if (Random.Range(0, 2) == 0) {
                muzzleFlashTime = Time.time + 0.1f;
                currentWeapon.MuzzleFlash.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360)); 
            }
        }
        else {
            GameObject emitter = Instantiate(currentWeapon.customEmitter, currentWeapon.EndPoint, cameraBody.rotation);
            if (emitter.GetComponent<ExplodableGrenade>() != null) {
                emitter.GetComponent<ExplodableGrenade>().damage = currentWeapon.damage;
                emitter.GetComponent<ExplodableGrenade>().direction = direction;
            }
        }

        if (!currentWeapon.isSilenced) AISoldier.InvokeOnShootNearby(transform.position);

        fireWaitTime = Time.time + 1f / currentWeapon.fireRate;
        recoilValue = 0;
        recoilOn = true;
        currentWeapon.ammoClip--;
        currentEquipment.ingamePrefab.GetComponent<AudioSource>().pitch = Random.Range(0.95f, 1.05f); 
        currentEquipment.ingamePrefab.GetComponent<AudioSource>().PlayOneShot(currentEquipment.ingamePrefab.GetComponent<AudioSource>().clip);

        if (currentEquipment.equipmentAnimator != null) currentWeapon.equipmentAnimator.OnActionFire();
    }
    private void OnMeleeAction() {
        mouseInputDown[0].SetFalse();

        if (Physics.Raycast(cameraBody.position, cameraBody.forward, out RaycastHit hitInfo, 2f, WorldManager.Instance.settings.collideMask)) {
            if (WorldManager.Instance.settings.targetHitMask == (WorldManager.Instance.settings.targetHitMask | (1 << hitInfo.collider.gameObject.layer))) {
                if (hitInfo.collider.transform.GetComponent<Hitbox>() != null) {
                    hitInfo.collider.transform.GetComponent<Hitbox>().OnHit(((MeleeInfo)currentEquipment).damage);
                }
            }
            
            Vector3 hitPoint = hitInfo.point;
            GameObject bulletHit = Instantiate(bulletHitPrefab, hitPoint + hitInfo.normal / 10f, Quaternion.identity);
            Destroy(bulletHit.gameObject, 5f);
        }

        fireWaitTime = Time.time + 1f / ((MeleeInfo)currentEquipment).speed;
        currentEquipment.equipmentAnimator.OnActionFire();
        currentEquipment.ingamePrefab.GetComponent<AudioSource>().PlayOneShot(currentEquipment.ingamePrefab.GetComponent<AudioSource>().clip);
    }
    private void OnReload() {
        if (currentWeapon == null) return;

        GetComponent<AudioSource>().PlayOneShot(reloadSound);
        currentWeapon.Reload();
    }
    private void OnSwitch() {
        equipmentIndex++;
        if (equipmentIndex >= equipments.Count) equipmentIndex = 0;
    }
    #endregion

    #region Stats Control
    private float damageCooldown;
    public void OnDamage(Vector3 descent, int damage) {
        if (WorldManager.Instance.isDead || damageCooldown > Time.time) return;

        if (stats.armor == 0) stats.health -= damage;
        else stats.armor = Mathf.Max(0, stats.armor - damage);
        hudTimer = Time.time + UIActiveTime;

        rb.AddForce((transform.position - descent).Horizontal().normalized * hitForce, ForceMode.VelocityChange);
        staticHitTimer = Time.time + staticHitActiveTime;
        damageCooldown = Time.time + 0.5f;

        GetComponent<AudioSource>().Play();

        if (stats.health <= 0) OnDeath();
    }
    public void OnFakeDamage(Transform descent) {
        OnDamage(descent.position, 0);
    }
    public void OnDeath() {
        WorldManager.Instance.isDead = true;
    }
    #endregion

    #region Interact Control
    public void ObtainEquipment(EquipmentInfo eq, bool displayMessage = true) {
        foreach (EquipmentInfo curEq in equipments) {
            if (curEq.name == eq.name) {
                WeaponInfo curWp = curEq as WeaponInfo;
                if (curWp == null) return;

                curWp.ammoStash += Mathf.CeilToInt(curWp.clipSize / 2f);
                if (displayMessage) DisplayMessage($"Picked up some {eq.name} ammo");
                return;
            }
        }

        GameObject ingamePrefab = Instantiate(eq.prefab, weaponBody);
        
        eq.Initialize(ingamePrefab);
        equipments.Add(eq);
        if (displayMessage) DisplayMessage($"Picked up a {eq.name}");
    }
    public void ObtainArmor(bool displayMessage = true) {
        stats.armor = Mathf.Min(stats.armor + 30, 80);
        hudTimer = Time.time + UIActiveTime;
        if (displayMessage) DisplayMessage("Picked up some body armor");
    }
    public void ObtainAmmo(bool displayMessage = true) {
        foreach (EquipmentInfo curEq in equipments) {
            WeaponInfo curWp = curEq as WeaponInfo;
            if (curWp == null) continue;

            curWp.ammoStash += curWp.clipSize * 2;
        }

        if (displayMessage) DisplayMessage("Picked up some ammo");
    }
    #endregion

    #region UI Control
    private const float displayTextYOffset = 30f;
    private const float UIActiveTime = 2f;
    private const float staticHitActiveTime = 0.4f;
    private float hudTimer = 0;
    private float staticHitTimer = 0;
    private List<GameObject> displayTextElements;
    private string hintText;
    private float hintActiveTimer;

    private void UpdateUI() {
        // DisplayText
        for (int i = displayTextElements.Count - 1; i >= 0; i--) {
            if (displayTextElements[i] == null) {
                displayTextElements.RemoveAt(i);
                continue;
            }

            Vector3 targetPosition = new Vector3(0, i * displayTextYOffset, 0);
            RectTransform rectTrans = displayTextElements[i].GetComponent<RectTransform>();
            rectTrans.anchoredPosition = Vector3.Lerp(rectTrans.anchoredPosition, targetPosition, 10f * Time.deltaTime);

            UnityEngine.UI.Text textMesh = displayTextElements[i].GetComponent<UnityEngine.UI.Text>();
            textMesh.color = Color.Lerp(textMesh.color, Color.white, 5f * Time.deltaTime);
        }

        // HintText
        float hintTextLerpValue = Mathf.Pow((hintActiveTimer - Time.time) / (UIActiveTime / 5f), 0.2f);
        hintTextMesh.color = Color.Lerp(new Color(0, 0, 0, 0), Color.white, hintTextLerpValue);
        hintTextMesh.text = hintText;

        // Ammo
        ammoUIParent.SetActive(currentWeapon != null);
        if (currentWeapon != null) {
            ammoClipTextMesh.text = currentWeapon.ammoClip.ToString();
            ammoStashTextMesh.text = currentWeapon.ammoStash.ToString();
        }

        // HUD
        hudParent.SetActive(hudTimer > Time.time); 
        if (hudTimer > Time.time) {
            hpImage.fillAmount = stats.health / 80f;
            armorImage.fillAmount = stats.armor / 80f;
        }

        // HitStatic
        Color staticHitCol = staticHitImage.color;
        staticHitCol.a = Mathf.Lerp(0, 1f, Mathf.Pow(staticHitActiveTime / 2f - ((staticHitTimer - Time.time).Max(0) - staticHitActiveTime / 2f).Abs(), 2f));
        staticHitImage.color = staticHitCol;
    }
    public void DisplayMessage(string message, float activeTimeMultiplier) {
        GameObject newPrefab = Instantiate(displayTextPrefab, displayTextParent);
        newPrefab.GetComponent<UnityEngine.UI.Text>().text = message;
        newPrefab.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        displayTextElements.Add(newPrefab);

        Destroy(newPrefab, UIActiveTime * activeTimeMultiplier);
    }
    public void DisplayMessage(string message) {
        DisplayMessage(message, 1f);
    }
    public void DisplayHint(string text) {
        hintText = text;
        hintActiveTimer = Time.time + UIActiveTime / 5f;
    }
    #endregion

    private void OnExplode(Vector3 position, int damage, float radius) {
        if ((transform.position - position).magnitude < radius) OnDamage(position, Mathf.RoundToInt(damage));
    }

    public class Stats
    {
        public int health = 80;
        public int armor = 0;
    }
}
