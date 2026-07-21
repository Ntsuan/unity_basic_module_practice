using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PracticeSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/BasicCombatRoom.unity";

    [MenuItem("Tools/Practice/Build Basic Combat Room")]
    public static void BuildBasicCombatRoom()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog(
                "Stop Play Mode",
                "Exit Play Mode before rebuilding the practice scene.",
                "OK"
            );
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "BasicCombatRoom";

        Material floorMaterial = CreateMaterial("Practice Floor Dark Blue", new Color(0.12f, 0.18f, 0.24f));
        Material gridMaterial = CreateMaterial("Practice Grid Line", new Color(0.2f, 0.34f, 0.42f));
        Material wallMaterial = CreateMaterial("Practice Wall Green", new Color(0.1f, 0.55f, 0.46f));
        Material obstacleMaterial = CreateMaterial("Practice Obstacle Steel", new Color(0.32f, 0.36f, 0.42f));
        Material playerMaterial = CreateMaterial("Practice Player Yellow", new Color(1f, 0.74f, 0.18f));
        Material playerHurtMaterial = CreateMaterial("Practice Player Hurt Red", new Color(1f, 0.18f, 0.12f));
        Material playerDownMaterial = CreateMaterial("Practice Player Down Dark Red", new Color(0.35f, 0.03f, 0.03f));
        Material facingMaterial = CreateMaterial("Practice Facing Marker White", new Color(0.95f, 0.95f, 0.9f));
        Material projectileMaterial = CreateMaterial("Practice Projectile Cyan", new Color(0.0f, 1f, 1f));
        Material targetMaterial = CreateMaterial("Practice Target Magenta", new Color(0.95f, 0.18f, 0.85f));
        Material targetHitMaterial = CreateMaterial("Practice Target Hit White", new Color(1f, 0.95f, 0.95f));
        Material pickupMaterial = CreateMaterial("Practice Pickup Blue", new Color(0.25f, 0.65f, 1f));
        Material healthPickupMaterial = CreateMaterial("Practice Health Pickup Red", new Color(1f, 0.15f, 0.12f));
        Material ammoPickupMaterial = CreateMaterial("Practice Ammo Pickup Blue", new Color(0.05f, 0.55f, 1f));
        Material enemyMaterial = CreateMaterial("Practice Enemy Purple", new Color(0.62f, 0.16f, 1f));
        Material enemyHitMaterial = CreateMaterial("Practice Enemy Hit White", new Color(1f, 0.94f, 1f));
        Material enemySelectedMaterial = CreateMaterial("Practice Enemy Selected Yellow", new Color(1f, 0.95f, 0.18f));
        Material enemyVisionHighlightedMaterial = CreateMaterial("Practice Enemy Vision Highlight", new Color(0.15f, 1f, 0.82f));
        Material enemyAttackWarningMaterial = CreateMaterial("Practice Enemy Attack Warning", new Color(1f, 0.25f, 0.12f));
        Material attackRangeMaterial = CreateTransparentMaterial("Practice Enemy Attack Range", new Color(1f, 0.08f, 0.04f, 0.32f));
        Material enemyRespawnMaterial = CreateTransparentMaterial("Practice Enemy Respawn Warning", new Color(0.72f, 0.28f, 1f, 0.36f));
        GameObject healthPickupPrefab = CreatePickupPrefab(
            "HealthPickup",
            healthPickupMaterial,
            PickupItem.PickupKind.Health,
            "Medkit",
            5,
            new Vector3(0.75f, 0.35f, 0.75f)
        );
        GameObject ammoPickupPrefab = CreatePickupPrefab(
            "AmmoPickup",
            ammoPickupMaterial,
            PickupItem.PickupKind.Ammo,
            "Ammo",
            0,
            new Vector3(0.95f, 0.28f, 0.45f)
        );

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0f, -0.5f, 0f);
        floor.transform.localScale = new Vector3(60f, 1f, 60f);
        SetMaterial(floor, floorMaterial);

        CreateFloorGrid(gridMaterial);

        CreateWall("North Wall", new Vector3(0f, 1f, 29.5f), new Vector3(60f, 2f, 1f), wallMaterial);
        CreateWall("South Wall", new Vector3(0f, 1f, -29.5f), new Vector3(60f, 2f, 1f), wallMaterial);
        CreateWall("East Wall", new Vector3(29.5f, 1f, 0f), new Vector3(1f, 2f, 60f), wallMaterial);
        CreateWall("West Wall", new Vector3(-29.5f, 1f, 0f), new Vector3(1f, 2f, 60f), wallMaterial);

        CreateTargetDummy("Target Dummy A", new Vector3(8f, 0.75f, 8f), targetMaterial, targetHitMaterial, ammoPickupPrefab);
        CreateTargetDummy("Target Dummy B", new Vector3(-10f, 0.75f, 12f), targetMaterial, targetHitMaterial, ammoPickupPrefab);
        CreateTargetDummy("Target Dummy C", new Vector3(12f, 0.75f, -8f), targetMaterial, targetHitMaterial, ammoPickupPrefab);

        CreatePickupItem("Loot Pickup A", new Vector3(-5f, 0.35f, -6f), pickupMaterial);
        CreatePickupItem("Loot Pickup B", new Vector3(6f, 0.35f, -12f), pickupMaterial);
        CreatePickupItem("Loot Pickup C", new Vector3(-14f, 0.35f, 2f), pickupMaterial);

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);
        SetMaterial(player, playerMaterial);

        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.center = Vector3.zero;
        characterController.height = 2f;
        characterController.radius = 0.5f;

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerAim>();
        player.AddComponent<PlayerPickup>();
        player.AddComponent<PlayerVision>();

        PlayerHealth playerHealth = player.AddComponent<PlayerHealth>();
        SerializedObject serializedHealth = new SerializedObject(playerHealth);
        serializedHealth.FindProperty("maxHealth").intValue = 10;
        serializedHealth.FindProperty("normalMaterial").objectReferenceValue = playerMaterial;
        serializedHealth.FindProperty("hurtMaterial").objectReferenceValue = playerHurtMaterial;
        serializedHealth.FindProperty("downMaterial").objectReferenceValue = playerDownMaterial;
        serializedHealth.FindProperty("invulnerabilityDuration").floatValue = 0.45f;
        serializedHealth.FindProperty("knockbackDistance").floatValue = 1.1f;
        serializedHealth.FindProperty("knockbackDuration").floatValue = 0.12f;
        serializedHealth.FindProperty("screenFlashDuration").floatValue = 0.22f;
        serializedHealth.FindProperty("screenFlashColor").colorValue = new Color(1f, 0f, 0f, 0.28f);
        serializedHealth.ApplyModifiedPropertiesWithoutUndo();

        WorldHealthBar playerHealthBar = player.AddComponent<WorldHealthBar>();
        SerializedObject serializedPlayerHealthBar = new SerializedObject(playerHealthBar);
        serializedPlayerHealthBar.FindProperty("target").objectReferenceValue = player.transform;
        serializedPlayerHealthBar.FindProperty("worldHeightOffset").floatValue = 2.95f;
        serializedPlayerHealthBar.FindProperty("width").intValue = 42;
        serializedPlayerHealthBar.FindProperty("height").intValue = 4;
        serializedPlayerHealthBar.FindProperty("fillColor").colorValue = new Color(0.2f, 1f, 0.35f);
        serializedPlayerHealthBar.ApplyModifiedPropertiesWithoutUndo();

        GameObject facingMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        facingMarker.name = "Facing Marker";
        facingMarker.transform.SetParent(player.transform);
        facingMarker.transform.localPosition = new Vector3(0f, 0.05f, 0.75f);
        facingMarker.transform.localScale = new Vector3(0.14f, 0.08f, 0.75f);
        SetMaterial(facingMarker, facingMaterial);
        Object.DestroyImmediate(facingMarker.GetComponent<BoxCollider>());

        GameObject muzzlePoint = new GameObject("MuzzlePoint");
        muzzlePoint.transform.SetParent(player.transform);
        muzzlePoint.transform.localPosition = new Vector3(0f, 0.1f, 1.25f);
        muzzlePoint.transform.localRotation = Quaternion.identity;

        GameObject projectilePrefab = CreateProjectilePrefab(projectileMaterial);
        PlayerShooting playerShooting = player.AddComponent<PlayerShooting>();
        SerializedObject serializedShooting = new SerializedObject(playerShooting);
        serializedShooting.FindProperty("muzzlePoint").objectReferenceValue = muzzlePoint.transform;
        serializedShooting.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        serializedShooting.FindProperty("fireCooldown").floatValue = 0.25f;
        serializedShooting.FindProperty("magazineSize").intValue = 12;
        serializedShooting.FindProperty("startingReserveAmmo").intValue = 96;
        serializedShooting.FindProperty("reloadDuration").floatValue = 1.2f;
        serializedShooting.ApplyModifiedPropertiesWithoutUndo();

        CreateSimpleEnemy(
            "Simple Enemy",
            new Vector3(-15f, 1f, -12f),
            player.transform,
            enemyMaterial,
            enemyHitMaterial,
            enemySelectedMaterial,
            enemyVisionHighlightedMaterial,
            enemyAttackWarningMaterial,
            attackRangeMaterial,
            enemyRespawnMaterial,
            healthPickupPrefab
        );

        GameObject obstacleSpawnerObject = new GameObject("Random Obstacle Spawner");
        RandomObstacleSpawner obstacleSpawner = obstacleSpawnerObject.AddComponent<RandomObstacleSpawner>();
        SerializedObject serializedObstacleSpawner = new SerializedObject(obstacleSpawner);
        serializedObstacleSpawner.FindProperty("obstacleMaterial").objectReferenceValue = obstacleMaterial;
        serializedObstacleSpawner.FindProperty("obstacleCount").intValue = 8;
        serializedObstacleSpawner.FindProperty("roomHalfSize").vector2Value = new Vector2(23f, 23f);
        serializedObstacleSpawner.FindProperty("minimumDistance").floatValue = 5f;
        serializedObstacleSpawner.ApplyModifiedPropertiesWithoutUndo();

        GameObject runGateObject = new GameObject("Practice Run Gate");
        runGateObject.AddComponent<PracticeRunGate>();

        GameObject feedbackObject = new GameObject("Practice Feedback");
        feedbackObject.AddComponent<PracticeFeedback>();

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 20f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.farClipPlane = 200f;
        camera.backgroundColor = new Color(0.02f, 0.03f, 0.08f);
        Vector3 cameraOffset = new Vector3(-16f, 36f, -16f);
        cameraObject.transform.position = player.transform.position + cameraOffset;
        cameraObject.transform.rotation = Quaternion.LookRotation(-cameraOffset.normalized, Vector3.up);

        CameraFollow cameraFollow = cameraObject.AddComponent<CameraFollow>();
        SerializedObject serializedCameraFollow = new SerializedObject(cameraFollow);
        serializedCameraFollow.FindProperty("target").objectReferenceValue = player.transform;
        serializedCameraFollow.FindProperty("offset").vector3Value = cameraOffset;
        serializedCameraFollow.FindProperty("followSpeed").floatValue = 8f;
        serializedCameraFollow.FindProperty("orthographicSize").floatValue = 20f;
        serializedCameraFollow.ApplyModifiedPropertiesWithoutUndo();

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorSceneManager.OpenScene(ScenePath);
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));

        Debug.Log($"Built practice scene: {ScenePath}");
    }

    [MenuItem("Tools/Practice/Build Basic Combat Room", true)]
    public static bool CanBuildBasicCombatRoom()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        SetMaterial(wall, material);
    }

    private static void CreateTargetDummy(
        string name,
        Vector3 position,
        Material normalMaterial,
        Material hitMaterial,
        GameObject ammoDropPrefab
    )
    {
        GameObject target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        target.name = name;
        target.transform.position = position;
        target.transform.localScale = new Vector3(1f, 1.5f, 1f);
        SetMaterial(target, normalMaterial);

        TargetDummy targetDummy = target.AddComponent<TargetDummy>();
        SerializedObject serializedTarget = new SerializedObject(targetDummy);
        serializedTarget.FindProperty("maxHealth").intValue = 3;
        serializedTarget.FindProperty("respawnDelay").floatValue = 1.2f;
        serializedTarget.FindProperty("normalMaterial").objectReferenceValue = normalMaterial;
        serializedTarget.FindProperty("hitMaterial").objectReferenceValue = hitMaterial;
        serializedTarget.FindProperty("ammoDropPrefab").objectReferenceValue = ammoDropPrefab;
        serializedTarget.FindProperty("randomizeRespawnPosition").boolValue = true;
        serializedTarget.FindProperty("respawnRoomHalfSize").vector2Value = new Vector2(23f, 23f);
        serializedTarget.FindProperty("minimumRespawnDistance").floatValue = 4f;
        serializedTarget.FindProperty("respawnPlacementAttempts").intValue = 40;
        serializedTarget.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreatePickupItem(string name, Vector3 position, Material material)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pickup.name = name;
        pickup.transform.position = position;
        pickup.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        pickup.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        SetMaterial(pickup, material);
        SetTriggerCollider(pickup);

        PickupItem pickupItem = pickup.AddComponent<PickupItem>();
        SerializedObject serializedPickup = new SerializedObject(pickupItem);
        serializedPickup.FindProperty("kind").enumValueIndex = (int)PickupItem.PickupKind.Loot;
        serializedPickup.FindProperty("displayName").stringValue = "Loot";
        serializedPickup.FindProperty("amount").intValue = 1;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateSimpleEnemy(
        string name,
        Vector3 position,
        Transform target,
        Material normalMaterial,
        Material hitMaterial,
        Material selectedMaterial,
        Material visionHighlightedMaterial,
        Material attackWarningMaterial,
        Material attackRangeMaterial,
        Material respawnWarningMaterial,
        GameObject healthDropPrefab
    )
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = name;
        enemy.transform.position = position;
        enemy.transform.localScale = new Vector3(1f, 1.25f, 1f);
        SetMaterial(enemy, normalMaterial);
        Object.DestroyImmediate(enemy.GetComponent<CapsuleCollider>());

        CharacterController enemyController = enemy.AddComponent<CharacterController>();
        enemyController.center = Vector3.zero;
        enemyController.height = 2f;
        enemyController.radius = 0.5f;

        GameObject attackRangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        attackRangeIndicator.name = "Attack Range Indicator";
        attackRangeIndicator.transform.SetParent(enemy.transform);
        attackRangeIndicator.transform.localPosition = new Vector3(0f, -0.76f, 0f);
        attackRangeIndicator.transform.localScale = new Vector3(3.2f, 0.01f, 3.2f);
        SetMaterial(attackRangeIndicator, attackRangeMaterial);
        Object.DestroyImmediate(attackRangeIndicator.GetComponent<Collider>());
        attackRangeIndicator.SetActive(false);

        GameObject respawnIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        respawnIndicator.name = "Respawn Indicator";
        respawnIndicator.transform.SetParent(enemy.transform);
        respawnIndicator.transform.localPosition = new Vector3(0f, -0.77f, 0f);
        respawnIndicator.transform.localScale = new Vector3(2.2f, 0.01f, 2.2f);
        SetMaterial(respawnIndicator, respawnWarningMaterial);
        Object.DestroyImmediate(respawnIndicator.GetComponent<Collider>());
        respawnIndicator.SetActive(false);

        SimpleEnemy simpleEnemy = enemy.AddComponent<SimpleEnemy>();
        SerializedObject serializedEnemy = new SerializedObject(simpleEnemy);
        serializedEnemy.FindProperty("maxHealth").intValue = 3;
        serializedEnemy.FindProperty("moveSpeed").floatValue = 2.2f;
        serializedEnemy.FindProperty("attackRange").floatValue = 1.6f;
        serializedEnemy.FindProperty("attackCooldown").floatValue = 1f;
        serializedEnemy.FindProperty("attackWindupDuration").floatValue = 0.35f;
        serializedEnemy.FindProperty("respawnDelay").floatValue = 1.5f;
        serializedEnemy.FindProperty("attackDamage").intValue = 1;
        serializedEnemy.FindProperty("target").objectReferenceValue = target;
        serializedEnemy.FindProperty("normalMaterial").objectReferenceValue = normalMaterial;
        serializedEnemy.FindProperty("hitMaterial").objectReferenceValue = hitMaterial;
        serializedEnemy.FindProperty("selectedMaterial").objectReferenceValue = selectedMaterial;
        serializedEnemy.FindProperty("visionHighlightedMaterial").objectReferenceValue = visionHighlightedMaterial;
        serializedEnemy.FindProperty("attackWarningMaterial").objectReferenceValue = attackWarningMaterial;
        serializedEnemy.FindProperty("attackRangeIndicator").objectReferenceValue = attackRangeIndicator.transform;
        serializedEnemy.FindProperty("respawnIndicator").objectReferenceValue = respawnIndicator.transform;
        serializedEnemy.FindProperty("healthDropPrefab").objectReferenceValue = healthDropPrefab;
        serializedEnemy.FindProperty("obstacleProbeDistance").floatValue = 1.2f;
        serializedEnemy.FindProperty("obstacleProbeRadius").floatValue = 0.45f;
        serializedEnemy.FindProperty("detourAngle").floatValue = 55f;
        serializedEnemy.FindProperty("blockedRetryDelay").floatValue = 0.15f;
        serializedEnemy.FindProperty("deathShrinkDuration").floatValue = 0.22f;
        serializedEnemy.FindProperty("respawnWarningDuration").floatValue = 0.55f;
        serializedEnemy.FindProperty("respawnPopDuration").floatValue = 0.22f;
        serializedEnemy.FindProperty("respawnIndicatorWorldRadius").floatValue = 2.2f;
        serializedEnemy.ApplyModifiedPropertiesWithoutUndo();

        WorldHealthBar enemyHealthBar = enemy.AddComponent<WorldHealthBar>();
        SerializedObject serializedEnemyHealthBar = new SerializedObject(enemyHealthBar);
        serializedEnemyHealthBar.FindProperty("target").objectReferenceValue = enemy.transform;
        serializedEnemyHealthBar.FindProperty("worldHeightOffset").floatValue = 2.85f;
        serializedEnemyHealthBar.FindProperty("width").intValue = 36;
        serializedEnemyHealthBar.FindProperty("height").intValue = 4;
        serializedEnemyHealthBar.FindProperty("fillColor").colorValue = new Color(1f, 0.2f, 0.55f);
        serializedEnemyHealthBar.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateFloorGrid(Material material)
    {
        GameObject gridRoot = new GameObject("Floor Grid");
        const int halfSize = 30;
        const float lineThickness = 0.035f;
        const float lineHeight = 0.025f;

        for (int i = -halfSize; i <= halfSize; i += 2)
        {
            GameObject horizontalLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            horizontalLine.name = $"Grid Z {i}";
            horizontalLine.transform.SetParent(gridRoot.transform);
            horizontalLine.transform.position = new Vector3(0f, lineHeight, i);
            horizontalLine.transform.localScale = new Vector3(60f, lineThickness, lineThickness);
            SetMaterial(horizontalLine, material);

            GameObject verticalLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            verticalLine.name = $"Grid X {i}";
            verticalLine.transform.SetParent(gridRoot.transform);
            verticalLine.transform.position = new Vector3(i, lineHeight, 0f);
            verticalLine.transform.localScale = new Vector3(lineThickness, lineThickness, 60f);
            SetMaterial(verticalLine, material);
        }
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = name;
        material.color = color;
        return material;
    }

    private static Material CreateTransparentMaterial(string name, Color color)
    {
        Material material = CreateMaterial(name, color);
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }

    private static GameObject CreateProjectilePrefab(Material material)
    {
        const string prefabFolder = "Assets/Prefabs";
        const string prefabPath = "Assets/Prefabs/PracticeProjectile.prefab";

        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "PracticeProjectile";
        projectile.transform.localScale = Vector3.one * 0.6f;
        SetMaterial(projectile, material);
        Object.DestroyImmediate(projectile.GetComponent<SphereCollider>());
        projectile.AddComponent<Projectile>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(projectile, prefabPath);
        Object.DestroyImmediate(projectile);

        return prefab;
    }

    private static GameObject CreatePickupPrefab(
        string prefabName,
        Material material,
        PickupItem.PickupKind kind,
        string displayName,
        int amount,
        Vector3 scale
    )
    {
        const string prefabFolder = "Assets/Prefabs";

        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pickup.name = prefabName;
        pickup.transform.localScale = scale;
        pickup.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        SetMaterial(pickup, material);
        SetTriggerCollider(pickup);

        PickupItem pickupItem = pickup.AddComponent<PickupItem>();
        SerializedObject serializedPickup = new SerializedObject(pickupItem);
        serializedPickup.FindProperty("kind").enumValueIndex = (int)kind;
        serializedPickup.FindProperty("displayName").stringValue = displayName;
        serializedPickup.FindProperty("amount").intValue = amount;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(pickup, $"Assets/Prefabs/{prefabName}.prefab");
        Object.DestroyImmediate(pickup);

        return prefab;
    }

    private static void SetMaterial(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void SetTriggerCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }
}
