using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class BuildGuest1HotelHallMap
{
    private const string MenuPath = "Tools/Hotel Hunger/Build Guest 1 Hotel Hall Map";
    private const string UndoName = "Build Guest 1 Hotel Hall Map";
    private const float PixelsPerUnit = 18f;

    private const string MainMenuScenePath = "Assets/Scenes/MainMenu_XW.unity";
    private const string LobbyScenePath = "Assets/Scenes/HotelLobby_XW.unity";
    private const string HotelHall1ScenePath = "Assets/Scenes/HotelHall1_XW.unity";
    private const string HotelHall2ScenePath = "Assets/Scenes/HotelHall2_XW.unity";
    private const string CartHallScenePath = "Assets/Scenes/Guest1CartHallway_XW.unity";
    private const string WaitingHallScenePath = "Assets/Scenes/Guest1WaitingHallway_XW.unity";
    private const string DiningRoomScenePath = "Assets/Scenes/DiningRoom_XW.unity";
    private const string BasementRoomScenePath = "Assets/Scenes/BasementRoom_XW.unity";
    private const string Guest3DiningRoomScenePath = "Assets/Scenes/Guest3DiningRoom_XW.unity";
    private const string ZiaTask1ScenePath = "Assets/Scenes/Pov1_Guest3_Guide_Task1_WetFloorSign.unity";
    private const string ZiaTask2ScenePath = "Assets/Scenes/Pov1_Guest3_Guide_Task2_PottedPlant.unity";
    private const string ZiaTask3ScenePath = "Assets/Scenes/Pov1_Guest3_Guide_Task3_LuggageCart.unity";
    private const string ZiaFinalChaseScenePath = "Assets/Scenes/Guest3_Final_Chasing.unity";
    private const string ZiaTask1SceneName = "Pov1_Guest3_Guide_Task1_WetFloorSign";
    private const string SkippedZiaBgmPrefabGuid = "72f5896f6fc84982a711fa29b7885d8a";
    private const string ZiaGuest3SpritePath = "Assets/Characters/Guest_3_char.png";
    private const string ZiaFloorSpritePath = "Assets/Sprites/Floor.png";
    private const string ZiaTask1BackgroundPath = "Assets/Pixel Art/3rd_Hotel_hall/Final_no_sign.png";
    private const string ZiaTask1WetFloorSignSpritePath = "Assets/Pixel Art/3rd_Hotel_hall/Slipper_sign.png";
    private const string ZiaTask2PottedPlantSpritePath = "Assets/Pixel Art/1st_Hotel_hall/Plants.png";
    private const string ZiaTask3BackgroundPath = "Assets/Pixel Art/No_Cart_4_hotel_hall.png";
    private const string ZiaFinalChaseBackgroundPath = "Assets/Pixel Art/Elevator Room/Final.png";
    private const string HorrorChaseScenePath = "Assets/Scenes/Guest3HorrorChase_XW.unity";
    private const string GoodEndingScenePath = "Assets/Scenes/GoodEnding_XW.unity";
    private const string BadEndingScenePath = "Assets/Scenes/BadEnding_XW.unity";
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

    private const string ConciergeSpritePath = "Assets/Pixel Art/Characters/Concierge_char.png";
    private const string Guest1SpritePath = "Assets/Pixel Art/Characters/Guest_1_char.png";
    private const string Guest3SpritePath = "Assets/Pixel Art/Characters/Guest_3_char.png";
    private const string HotelHall1BackgroundPath = "Assets/Pixel Art/1st_Hotel_hall/Final.png";
    private const string HotelHall2BackgroundPath = "Assets/Pixel Art/2nd_Hotel_hall/Final.png";
    private const string WaitingHallBackgroundPath = "Assets/Pixel Art/3rd_Hotel_hall/Final.png";
    private const string CartHallBackgroundPath = "Assets/Pixel Art/4th_Hotel_hall/Final.png";
    private const string BasementBackgroundPath = "Assets/Pixel Art/Basement_Room/Final.png";
    private const string BasementElevatorPath = "Assets/Pixel Art/Basement_Room/Elevator.png";
    private const string DiningRoomBackgroundPath = "Assets/Pixel Art/Dining_Room/Final.png";
    private const string SuitcaseSpritePath = "Assets/Pixel Art/Suitcase/Generated/Suitcase_Red_Cropped.png";
    private const string SourceSuitcaseSpritePath = "Assets/Pixel Art/Suitcase/Suitcase_Red.png";
    private const string UpdatedBlueCartSpritePath = "Assets/Pixel Art/Updated 4th_Hotel_Hall/Blue_Trolley_Cart.png";
    private const string UpdatedRedCartSpritePath = "Assets/Pixel Art/Updated 4th_Hotel_Hall/Red_Trolley_Cart.png";
    private const string UpdatedFourthHallFloorPath = "Assets/Pixel Art/Updated 4th_Hotel_Hall/Floor.png";
    private const string UpdatedFourthHallOutOfBoundsPath = "Assets/Pixel Art/Updated 4th_Hotel_Hall/Out_of_bounds.png";

    private static readonly string[] RequiredSpritePaths =
    {
        ConciergeSpritePath,
        Guest1SpritePath,
        Guest3SpritePath,
        ZiaGuest3SpritePath,
        ZiaFloorSpritePath,
        ZiaTask1BackgroundPath,
        ZiaTask1WetFloorSignSpritePath,
        ZiaTask2PottedPlantSpritePath,
        ZiaTask3BackgroundPath,
        ZiaFinalChaseBackgroundPath,
        HotelHall1BackgroundPath,
        HotelHall2BackgroundPath,
        WaitingHallBackgroundPath,
        CartHallBackgroundPath,
        BasementBackgroundPath,
        BasementElevatorPath,
        DiningRoomBackgroundPath,
        SuitcaseSpritePath,
        SourceSuitcaseSpritePath,
        UpdatedBlueCartSpritePath,
        UpdatedRedCartSpritePath,
        UpdatedFourthHallFloorPath,
        UpdatedFourthHallOutOfBoundsPath
    };

    private static readonly string[] ImportedZiaScenePaths =
    {
        ZiaTask1ScenePath,
        ZiaTask2ScenePath,
        ZiaTask3ScenePath,
        ZiaFinalChaseScenePath
    };

    private static readonly Dictionary<string, string[]> RequiredZiaSpritePathsByScene = new Dictionary<string, string[]>
    {
        {
            ZiaTask1ScenePath,
            new[]
            {
                ZiaGuest3SpritePath,
                ZiaFloorSpritePath,
                ZiaTask1BackgroundPath,
                ZiaTask1WetFloorSignSpritePath
            }
        },
        {
            ZiaTask2ScenePath,
            new[]
            {
                ZiaGuest3SpritePath,
                ZiaFloorSpritePath,
                HotelHall2BackgroundPath,
                ZiaTask2PottedPlantSpritePath
            }
        },
        {
            ZiaTask3ScenePath,
            new[]
            {
                ZiaGuest3SpritePath,
                ZiaFloorSpritePath,
                ZiaTask3BackgroundPath,
                UpdatedRedCartSpritePath,
                UpdatedBlueCartSpritePath
            }
        },
        {
            ZiaFinalChaseScenePath,
            new[]
            {
                ZiaGuest3SpritePath,
                ZiaFloorSpritePath,
                ZiaFinalChaseBackgroundPath
            }
        }
    };

    private static readonly HashSet<string> ExactImportedZiaSpritePaths = new HashSet<string>
    {
        ZiaGuest3SpritePath,
        ZiaFloorSpritePath,
        ZiaTask1BackgroundPath,
        ZiaTask1WetFloorSignSpritePath,
        ZiaTask2PottedPlantSpritePath,
        ZiaTask3BackgroundPath,
        ZiaFinalChaseBackgroundPath
    };

    private static readonly string[] PreferredBuildSceneOrder =
    {
        MainMenuScenePath,
        LobbyScenePath,
        HotelHall1ScenePath,
        HotelHall2ScenePath,
        CartHallScenePath,
        WaitingHallScenePath,
        DiningRoomScenePath,
        BasementRoomScenePath,
        Guest3DiningRoomScenePath,
        ZiaTask1ScenePath,
        ZiaTask2ScenePath,
        ZiaTask3ScenePath,
        ZiaFinalChaseScenePath,
        HorrorChaseScenePath,
        GoodEndingScenePath,
        BadEndingScenePath,
        SampleScenePath
    };

    [MenuItem(MenuPath)]
    private static void Build()
    {
        AssetDatabase.Refresh();

        if (!VerifyRequiredSpritesExist())
        {
            return;
        }

        if (AnyOpenSceneIsDirty())
        {
            EditorUtility.DisplayDialog(
                "Build Guest 1 Hotel Hall Map",
                "One or more currently open scenes have unsaved changes. Save or discard them manually, then run this utility again.",
                "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Build Guest 1 Hotel Hall Map",
                "This utility will create or update and save these scenes/settings:\n\n" +
                "HotelHall1_XW\n" +
                "HotelHall2_XW\n" +
                "HotelLobby_XW route wiring\n" +
                "Guest1CartHallway_XW as 4th Hotel Hall\n" +
                "Guest1WaitingHallway_XW as 3rd Hotel Hall\n" +
                "BasementRoom_XW\n" +
                "DiningRoom_XW\n" +
                "Guest3DiningRoom_XW chase art wiring\n" +
                "Zia Guest 3 guide scene visual repair\n" +
                "Lobby Guest 1 suitcase visual\n" +
                "Editor Build Settings\n\n" +
                "It will not create HotelHall3_XW.",
                "Build and Save",
                "Cancel"))
        {
            Debug.Log("Build Guest 1 Hotel Hall Map cancelled before any scene changes.");
            return;
        }

        ConfigureSpriteImporters();
        AssetDatabase.Refresh();

        string originalScenePath = SceneManager.GetActiveScene().path;
        string ziaValidationSummary = string.Empty;

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            BuildHotelHall1();
            BuildHotelHall2();
            ConfigureLobbyRoute();
            ConfigureCartHallwayAsFourthHall();
            ConfigureWaitingHallwayAsThirdHall();
            BuildNormalDiningRoom();
            BuildBasementRoom();
            ConfigureGuest3DiningRoomChaseArt();
            ziaValidationSummary = CleanupImportedZiaScenes();
            UpdateBuildSettings();
        }
        finally
        {
            Undo.CollapseUndoOperations(undoGroup);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!string.IsNullOrWhiteSpace(originalScenePath) &&
            AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScenePath) != null)
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        string completionMessage = "Guest 1 hotel hall route scenes and wiring were built.";
        if (!string.IsNullOrWhiteSpace(ziaValidationSummary))
        {
            completionMessage += "\n\n" + ziaValidationSummary;
        }

        EditorUtility.DisplayDialog(
            "Build Guest 1 Hotel Hall Map",
            completionMessage,
            "OK");
    }

    private static void BuildHotelHall1()
    {
        Scene scene = OpenOrCreateScene(HotelHall1ScenePath);
        EnsureMainCamera(scene);
        EnsureHallBackground(scene, "PixelArtVisuals_HotelHall1", HotelHall1BackgroundPath);

        Vector3 spawnPosition = PixelToWorld(64f, 90f);
        EnsurePlayer(scene, spawnPosition);
        EnsureSpawnPoint(scene, "Spawn_FromHotelLobby", "FromHotelLobby", spawnPosition);
        EnsureSpawnPoint(scene, "Spawn_FromHotelHall2", "FromHotelHall2", PixelToWorld(256f, 90f));

        ConfigureTransitionZone(
            scene,
            "LeftBarDoorToLobby",
            PixelToWorld(20f, 90f),
            new Vector2(28f / PixelsPerUnit, 58f / PixelsPerUnit),
            "Enter Bar Door",
            "HotelLobby_XW",
            "FromHotelHall1");

        ConfigureAutoTransitionZone(
            scene,
            "RightExitToHotelHall2",
            PixelToWorld(304f, 90f),
            new Vector2(18f / PixelsPerUnit, 64f / PixelsPerUnit),
            "HotelHall2_XW",
            "FromHotelHall1");

        RebuildHall1Collision(scene);
        SaveScene(scene, HotelHall1ScenePath);
    }

    private static void BuildHotelHall2()
    {
        Scene scene = OpenOrCreateScene(HotelHall2ScenePath);
        EnsureMainCamera(scene);
        EnsureHallBackground(scene, "PixelArtVisuals_HotelHall2", HotelHall2BackgroundPath);

        Vector3 spawnPosition = PixelToWorld(64f, 90f);
        EnsurePlayer(scene, spawnPosition);
        EnsureSpawnPoint(scene, "Spawn_FromHotelHall1", "FromHotelHall1", spawnPosition);
        EnsureSpawnPoint(scene, "Spawn_FromFourthHall", "FromFourthHall", PixelToWorld(256f, 90f));
        DestroyObjectsByName(scene, "Spawn_FromThirdHall");
        EnsureSpawnPoint(scene, "Spawn_FromGuest1WaitingHallway", "FromGuest1WaitingHallway", PixelToWorld(160f, 104f));

        ConfigureAutoTransitionZone(
            scene,
            "LeftExitToHotelHall1",
            PixelToWorld(16f, 90f),
            new Vector2(18f / PixelsPerUnit, 64f / PixelsPerUnit),
            "HotelHall1_XW",
            "FromHotelHall2");

        ConfigureAutoTransitionZone(
            scene,
            "RightExitToFourthHall",
            PixelToWorld(304f, 90f),
            new Vector2(18f / PixelsPerUnit, 64f / PixelsPerUnit),
            "Guest1CartHallway_XW",
            "FromHotelHall2");

        ConfigureAutoTransitionZone(
            scene,
            "DownExitToThirdHall",
            PixelToWorld(160f, 138f),
            new Vector2(88f / PixelsPerUnit, 28f / PixelsPerUnit),
            "Guest1WaitingHallway_XW",
            "FromHotelHall2");

        RebuildHall2Collision(scene);
        SaveScene(scene, HotelHall2ScenePath);
    }

    private static void ConfigureLobbyRoute()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Single);

        ConfigurePlayerVisual(FindByName(scene, "Player"));
        ConfigureGuest1Visual(FindByName(scene, "Guest1"));

        GameObject storyController = FindByName(scene, "Guest1LobbyStoryController");
        Guest1LobbyFlow lobbyFlow = storyController != null ? storyController.GetComponent<Guest1LobbyFlow>() : null;
        UnityAction pickUpLuggageAction = lobbyFlow != null ? new UnityAction(lobbyFlow.PickUpGuest1Luggage) : null;
        InteractionTarget suitcaseInteraction = ConfigureLobbyGuest1SuitcaseInteraction(
            scene,
            pickUpLuggageAction,
            out GameObject suitcaseObject);

        GameObject elevatorDoor = FindByName(scene, "ElevatorDoor");
        SetWorldPosition(elevatorDoor, PixelToWorld(288f, 98f));

        GameObject transitionObject = FindByName(scene, "ElevatorInteractionZone");
        SetLocalPosition(transitionObject, new Vector3(0f, -0.45f, 0f));
        SceneTransitionTrigger lobbyTransition = ConfigureSceneTransition(
            transitionObject,
            "Enter Bar Door",
            "HotelHall1_XW",
            "FromHotelLobby",
            true);

        EnsureSpawnPoint(scene, "Spawn_FromHotelHall1", "FromHotelHall1", PixelToWorld(268f, 104f));

        ConfigureLobbyFlowReferences(lobbyFlow, suitcaseObject, suitcaseInteraction, lobbyTransition);
        SaveScene(scene, LobbyScenePath);
    }

    private static void ConfigureCartHallwayAsFourthHall()
    {
        Scene scene = EditorSceneManager.OpenScene(CartHallScenePath, OpenSceneMode.Single);

        EnsureHallBackground(scene, "PixelArtVisuals_CartHallway", CartHallBackgroundPath);
        ConfigurePlayerVisual(FindByName(scene, "Player"));

        HideRenderers(scene, "LobbyFloor", "TopWall", "BottomWall", "LeftWall", "RightWall");
        HideRenderers(scene, "ElevatorVisual", "PixelArtElevatorSign");

        GameObject storyController = EnsureRootObject(scene, "Guest1CartStoryController");
        Guest1CartFlow cartFlow = EnsureComponent<Guest1CartFlow>(storyController);
        Guest1PostCartCutscene postCartCutscene = EnsureComponent<Guest1PostCartCutscene>(storyController);
        UnityAction inspectArrivalCartAction = cartFlow != null ? new UnityAction(cartFlow.InspectArrivalCart) : null;
        UnityAction chooseDepartureCartAction = cartFlow != null ? new UnityAction(cartFlow.ChooseDepartureCart) : null;
        UnityAction inspectElevatorAction = cartFlow != null ? new UnityAction(cartFlow.InspectElevator) : null;

        ConfigureCartChoiceObject(
            scene,
            "ArrivalCart",
            "ArrivalCartInteractionZone",
            PixelToWorld(226f, 76f),
            new Vector2(50f / PixelsPerUnit, 30f / PixelsPerUnit),
            "Inspect Arrival Cart",
            inspectArrivalCartAction);

        DisableCartObject(
            scene,
            "RoomServiceCart",
            "RoomServiceCartInteractionZone");

        ConfigureCartChoiceObject(
            scene,
            "DepartureCart",
            "DepartureCartInteractionZone",
            PixelToWorld(276f, 112f),
            new Vector2(58f / PixelsPerUnit, 30f / PixelsPerUnit),
            "Choose Departure Cart",
            chooseDepartureCartAction);

        Vector3 hotelHall2Spawn = PixelToWorld(64f, 90f);
        EnsureSpawnPoint(scene, "Spawn_FromHotelHall2", "FromHotelHall2", hotelHall2Spawn);
        EnsureSpawnPoint(scene, "Spawn_FromBasementElevator", "FromBasementElevator", PixelToWorld(160f, 70f));
        SetWorldPosition(FindByName(scene, "Player"), hotelHall2Spawn);

        ConfigureAutoTransitionZone(
            scene,
            "LeftExitToHotelHall2",
            PixelToWorld(16f, 90f),
            new Vector2(18f / PixelsPerUnit, 64f / PixelsPerUnit),
            "HotelHall2_XW",
            "FromFourthHall");

        GameObject elevatorDoor = FindByName(scene, "ElevatorDoor");
        SetWorldPosition(elevatorDoor, PixelToWorld(160f, 42f));

        GameObject elevatorInteraction = FindChildByName(elevatorDoor, "ElevatorInteractionZone");
        SetLocalPosition(elevatorInteraction, Vector3.zero);
        ConfigureBoxTrigger(elevatorInteraction, new Vector2(42f / PixelsPerUnit, 32f / PixelsPerUnit));
        InteractionTarget elevatorInspection = ConfigureInteractionTarget(
            elevatorInteraction,
            "Call elevator",
            inspectElevatorAction,
            true);

        GameObject elevatorTransitionObject = EnsureDirectChild(elevatorDoor, "ElevatorToThirdHallTransitionZone");
        SetLocalPosition(elevatorTransitionObject, Vector3.zero);
        ConfigureBoxTrigger(elevatorTransitionObject, new Vector2(42f / PixelsPerUnit, 32f / PixelsPerUnit));
        SceneTransitionTrigger elevatorTransition = ConfigureSceneTransition(
            elevatorTransitionObject,
            "Enter Elevator",
            "BasementRoom_XW",
            "FromHall4Elevator",
            false);

        GameObject elevatorTarget = EnsureRootObject(scene, "PostCartElevatorWaypoint");
        SetWorldPosition(elevatorTarget, PixelToWorld(160f, 42f));

        GameObject player = FindByName(scene, "Player");
        postCartCutscene.Configure(
            player,
            player != null ? player.GetComponent<SidePlayerController>() : null,
            player != null ? player.GetComponent<PlayerInteractor>() : null,
            player != null ? player.GetComponent<Rigidbody2D>() : null,
            player != null ? player.transform : null,
            elevatorTarget.transform,
            "BasementRoom_XW");

        ConfigureCartFlowReferences(cartFlow, elevatorInspection, elevatorTransition, postCartCutscene);

        RebuildFourthHallCollision(scene);
        SaveScene(scene, CartHallScenePath);
    }

    private static void ConfigureWaitingHallwayAsThirdHall()
    {
        Scene scene = EditorSceneManager.OpenScene(WaitingHallScenePath, OpenSceneMode.Single);
        EnsureHallBackground(scene, "PixelArtVisuals_WaitingHallway", WaitingHallBackgroundPath);
        ConfigurePlayerVisual(FindByName(scene, "Player"));

        HideRenderers(scene, "LobbyFloor", "TopWall", "BottomWall", "LeftWall", "RightWall");
        HideRenderers(scene, "PrivateElevatorVisual", "PrivateElevatorSign", "WaitingRoomDoorVisual");
        SetTextMeshText(FindByName(scene, "WaitingRoomSign"), "DINING ROOM");
        SetTextMeshStyle(FindByName(scene, "WaitingRoomSign"), new Color(0.45f, 0f, 0f, 1f), 42, TextAnchor.MiddleCenter);

        Vector3 hotelHall2Spawn = PixelToWorld(160f, 88f);
        Vector3 cartHallSpawn = PixelToWorld(256f, 90f);
        EnsureSpawnPoint(scene, "Spawn_FromHotelHall2", "FromHotelHall2", hotelHall2Spawn);
        EnsureSpawnPoint(scene, "Spawn_FromCartHallway", "FromCartHallway", cartHallSpawn);
        EnsureSpawnPoint(scene, "Spawn_FromDiningRoom", "FromDiningRoom", PixelToWorld(28f, 90f));
        SetWorldPosition(FindByName(scene, "Player"), cartHallSpawn);

        ConfigureAutoTransitionZone(
            scene,
            "UpExitToHotelHall2",
            PixelToWorld(160f, 42f),
            new Vector2(88f / PixelsPerUnit, 28f / PixelsPerUnit),
            "HotelHall2_XW",
            "FromGuest1WaitingHallway");

        DisableSceneObject(FindByName(scene, "PrivateElevator"));
        DisableInteractionComponents(FindByName(scene, "PrivateElevatorInteractionZone"));
        DisableCollidersInChildren(FindByName(scene, "PrivateElevator"));

        Guest1WaitingFlow waitingFlow = FindByName(scene, "Guest1WaitingStoryController") != null ?
            FindByName(scene, "Guest1WaitingStoryController").GetComponent<Guest1WaitingFlow>() :
            null;

        GameObject luggage = FindByName(scene, "Guest1Luggage");
        DisableHall3Luggage(luggage);
        DisableHall3Luggage(FindByName(scene, "Guest1LuggageInteractionZone"));

        GameObject diningRoomDoor = FindByName(scene, "WaitingRoomDoor");
        SetWorldPosition(diningRoomDoor, PixelToWorld(12f, 90f));
        GameObject diningRoomTransitionObject = FindChildByName(diningRoomDoor, "WaitingRoomInteractionZone");
        if (diningRoomTransitionObject == null)
        {
            diningRoomTransitionObject = FindByName(scene, "WaitingRoomInteractionZone");
        }

        SetLocalPosition(diningRoomTransitionObject, Vector3.zero);
        ConfigureBoxTrigger(diningRoomTransitionObject, new Vector2(28f / PixelsPerUnit, 58f / PixelsPerUnit));
        SceneTransitionTrigger diningRoomTransition = ConfigureSceneTransition(
            diningRoomTransitionObject,
            "Enter Dining Room",
            "DiningRoom_XW",
            "FromHotelHall3",
            true);

        ConfigureWaitingFlowReferences(waitingFlow, null, null, diningRoomTransition);
        RebuildThirdHallCollision(scene);
        SaveScene(scene, WaitingHallScenePath);
    }

    private static void BuildNormalDiningRoom()
    {
        Scene scene = OpenOrCreateScene(DiningRoomScenePath);
        EnsureMainCamera(scene);
        EnsureHallBackground(scene, "PixelArtVisuals_DiningRoom", DiningRoomBackgroundPath);

        Vector3 spawnPosition = PixelToWorld(286f, 92f);
        EnsurePlayer(scene, spawnPosition);
        EnsureSpawnPoint(scene, "Spawn_FromHotelHall3", "FromHotelHall3", spawnPosition);

        ConfigureTransitionZone(
            scene,
            "RightDoorToHotelHall3",
            PixelToWorld(304f, 92f),
            new Vector2(18f / PixelsPerUnit, 58f / PixelsPerUnit),
            "Return to 3rd Hall",
            "Guest1WaitingHallway_XW",
            "FromDiningRoom");

        RebuildDiningRoomCollision(scene, "PixelArtCollisionLayout_DiningRoom");
        SaveScene(scene, DiningRoomScenePath);
    }

    private static void BuildBasementRoom()
    {
        Scene scene = OpenOrCreateScene(BasementRoomScenePath);
        EnsureMainCamera(scene);
        EnsureHallBackground(scene, "PixelArtVisuals_BasementRoom", BasementBackgroundPath);

        Vector3 spawnPosition = PixelToWorld(160f, 64f);
        EnsurePlayer(scene, spawnPosition);
        EnsureSpawnPoint(scene, "Spawn_FromHall4Elevator", "FromHall4Elevator", spawnPosition);

        GameObject sign = EnsureRootObject(scene, "WaitingRoomDoorSign");
        SetWorldPosition(sign, PixelToWorld(36f, 78f));
        TextMesh signText = EnsureComponent<TextMesh>(sign);
        Undo.RecordObject(signText, UndoName);
        signText.text = "WAITING ROOM";
        signText.anchor = TextAnchor.MiddleCenter;
        signText.alignment = TextAlignment.Center;
        signText.fontSize = 36;
        signText.characterSize = 0.08f;
        signText.color = new Color(0.45f, 0f, 0f, 1f);
        EditorUtility.SetDirty(signText);

        GameObject waitingRoomTarget = EnsureRootObject(scene, "BasementWaitingRoomWaypoint");
        SetWorldPosition(waitingRoomTarget, PixelToWorld(28f, 92f));

        GameObject player = FindByName(scene, "Player");
        GameObject controllerObject = EnsureRootObject(scene, "BasementRoomCutsceneController");
        BasementRoomCutscene basementCutscene = EnsureComponent<BasementRoomCutscene>(controllerObject);
        basementCutscene.Configure(
            player,
            player != null ? player.GetComponent<SidePlayerController>() : null,
            player != null ? player.GetComponent<PlayerInteractor>() : null,
            player != null ? player.GetComponent<Rigidbody2D>() : null,
            player != null ? player.GetComponentsInChildren<SpriteRenderer>(true) : new SpriteRenderer[0],
            player != null ? player.transform : null,
            waitingRoomTarget.transform,
            "Guest3DiningRoom_XW");
        EditorUtility.SetDirty(basementCutscene);

        RebuildBasementCollision(scene);
        SaveScene(scene, BasementRoomScenePath);
    }

    private static void ConfigureGuest3DiningRoomChaseArt()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Guest3DiningRoomScenePath) == null)
        {
            Debug.LogWarning("Build Guest 1 Hotel Hall Map: Guest3DiningRoom_XW does not exist, so chase art wiring was skipped.");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(Guest3DiningRoomScenePath, OpenSceneMode.Single);
        EnsureHallBackground(scene, "PixelArtVisuals_Guest3DiningRoom", DiningRoomBackgroundPath);

        HideRenderersInChildren(FindByName(scene, "DiningTableSettings"));
        HideRenderersInChildren(FindByName(scene, "DiningChairs"));
        HideRenderersInChildren(FindByName(scene, "LongDiningTable"));
        HideRenderers(scene, "LobbyFloor", "TopWall", "BottomWall", "LeftWall", "RightWall");

        DiningRoomChaseCutscene cutscene = FindComponentInScene<DiningRoomChaseCutscene>(scene);
        if (cutscene != null)
        {
            SerializedObject serializedObject = new SerializedObject(cutscene);
            Transform guestActor = serializedObject.FindProperty("guest2Actor").objectReferenceValue as Transform;
            Transform staffActor = serializedObject.FindProperty("staffActor").objectReferenceValue as Transform;
            serializedObject.FindProperty("loadNextSceneOnComplete").boolValue = true;
            serializedObject.FindProperty("nextSceneName").stringValue = ZiaTask1SceneName;
            serializedObject.FindProperty("nextSpawnId").stringValue = string.Empty;

            if (guestActor != null)
            {
                HideRenderersInChildren(guestActor.gameObject);
                ConfigureCharacterVisual(guestActor.gameObject, "PixelArtGuest3Visual", Guest3SpritePath, 1f, 12);
            }

            if (staffActor != null)
            {
                HideRenderersInChildren(staffActor.gameObject);
                ConfigureCharacterVisual(staffActor.gameObject, "PixelArtConciergeVisual", ConciergeSpritePath, 1f, 13);

                GameObject exclamation = EnsureDirectChild(staffActor.gameObject, "ChaseExclamationMark");
                ConfigureExclamationMark(exclamation);
                serializedObject.FindProperty("staffExclamationObject").objectReferenceValue = exclamation;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(cutscene);
        }

        RebuildDiningRoomCollision(scene, "PixelArtCollisionLayout_Guest3DiningRoom");
        SaveScene(scene, Guest3DiningRoomScenePath);
    }

    private static string CleanupImportedZiaScenes()
    {
        StringBuilder summary = new StringBuilder();

        for (int i = 0; i < ImportedZiaScenePaths.Length; i++)
        {
            string scenePath = ImportedZiaScenePaths[i];
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                summary.AppendLine($"Missing imported Zia scene: {scenePath}");
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            List<string> issues = new List<string>();
            List<string> warnings = new List<string>();
            ZiaSceneValidationStats stats = new ZiaSceneValidationStats();
            bool changed = EnsureImportedZiaCamera(scene, warnings);

            ValidateImportedZiaScene(scene, scenePath, issues, warnings, stats);

            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            if (issues.Count == 0)
            {
                stats.OptionalBgmRemovedCount = RemoveOptionalZiaBgmObjects(scene);
                changed |= stats.OptionalBgmRemovedCount > 0;
                if (changed)
                {
                    SaveScene(scene, scenePath);
                }

                summary.AppendLine(FormatZiaValidationSummary(sceneName, stats, true, changed));
            }
            else
            {
                summary.AppendLine(FormatZiaValidationSummary(sceneName, stats, false, false));
                for (int issueIndex = 0; issueIndex < issues.Count; issueIndex++)
                {
                    summary.AppendLine($"- {issues[issueIndex]}");
                }
            }

            for (int warningIndex = 0; warningIndex < warnings.Count; warningIndex++)
            {
                summary.AppendLine($"- Warning: {warnings[warningIndex]}");
            }
        }

        string result = summary.ToString().TrimEnd();
        if (!string.IsNullOrWhiteSpace(result))
        {
            Debug.Log("Build Guest 1 Hotel Hall Map: Zia guide scene validation\n" + result);
            return "Zia guide scene validation:\n" + result;
        }

        return string.Empty;
    }

    private sealed class ZiaSceneValidationStats
    {
        public bool CameraValid;
        public int VisibleSpriteRendererCount;
        public int CameraOverlappingSpriteRendererCount;
        public int MissingScriptCount;
        public int OptionalBgmRemovedCount;
    }

    private static string FormatZiaValidationSummary(
        string sceneName,
        ZiaSceneValidationStats stats,
        bool passed,
        bool saved)
    {
        string result = passed ? "validation passed" : "validation failed, not saved";
        if (saved)
        {
            result += ", saved cleanup";
        }

        return $"{sceneName}: {result}; camera valid: {stats.CameraValid}; " +
            $"visible SpriteRenderers: {stats.VisibleSpriteRendererCount}; " +
            $"camera-overlapping SpriteRenderers: {stats.CameraOverlappingSpriteRendererCount}; " +
            $"missing required scripts: {stats.MissingScriptCount}; " +
            $"optional BGM objects removed: {stats.OptionalBgmRemovedCount}";
    }

    private static bool EnsureImportedZiaCamera(Scene scene, List<string> warnings)
    {
        Camera camera = FindPreferredCamera(scene);
        if (camera == null)
        {
            return false;
        }

        bool changed = false;
        if (!camera.gameObject.activeSelf)
        {
            SetGameObjectAndParentsActive(camera.gameObject);
            warnings.Add($"{scene.path}: enabled inactive camera '{GetGameObjectPath(camera.gameObject)}'.");
            changed = true;
        }

        if (!camera.enabled)
        {
            Undo.RecordObject(camera, UndoName);
            camera.enabled = true;
            EditorUtility.SetDirty(camera);
            warnings.Add($"{scene.path}: enabled disabled camera '{GetGameObjectPath(camera.gameObject)}'.");
            changed = true;
        }

        if (camera.cullingMask == 0)
        {
            Undo.RecordObject(camera, UndoName);
            camera.cullingMask = -1;
            EditorUtility.SetDirty(camera);
            warnings.Add($"{scene.path}: reset zero camera culling mask on '{GetGameObjectPath(camera.gameObject)}'.");
            changed = true;
        }

        return changed;
    }

    private static void ValidateImportedZiaScene(
        Scene scene,
        string scenePath,
        List<string> issues,
        List<string> warnings,
        ZiaSceneValidationStats stats)
    {
        Camera camera = FindEnabledCamera(scene);
        stats.CameraValid = camera != null && camera.cullingMask != 0;
        if (camera == null)
        {
            issues.Add($"{scenePath}: no active enabled Camera found.");
        }
        else if (camera.cullingMask == 0)
        {
            issues.Add($"{scenePath}: camera '{GetGameObjectPath(camera.gameObject)}' has an empty culling mask.");
        }

        CountMissingScripts(scene, issues, stats);
        ValidateVisibleSpriteRenderers(scene, scenePath, camera, issues, warnings, stats);
        ValidateRequiredZiaSprites(scene, scenePath, camera, issues);
    }

    private static void ValidateVisibleSpriteRenderers(
        Scene scene,
        string scenePath,
        Camera camera,
        List<string> issues,
        List<string> warnings,
        ZiaSceneValidationStats stats)
    {
        int visibleSpriteRenderers = 0;
        int cameraOverlappingSpriteRenderers = 0;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            SpriteRenderer[] renderers = roots[i].GetComponentsInChildren<SpriteRenderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                SpriteRenderer renderer = renderers[rendererIndex];
                if (renderer == null || !renderer.gameObject.activeInHierarchy || !renderer.enabled)
                {
                    continue;
                }

                if (renderer.sprite == null)
                {
                    string message = $"{scenePath}: active SpriteRenderer '{GetGameObjectPath(renderer.gameObject)}' has no sprite.";
                    if (IsKnownOptionalZiaNullSprite(scenePath, renderer))
                    {
                        warnings.Add(message + " This is a stale duplicate from the latest Zia scene and was left unchanged.");
                    }
                    else
                    {
                        issues.Add(message);
                    }

                    continue;
                }

                visibleSpriteRenderers++;
                if (camera != null && RendererOverlapsCamera(renderer, camera))
                {
                    cameraOverlappingSpriteRenderers++;
                }
            }
        }

        stats.VisibleSpriteRendererCount = visibleSpriteRenderers;
        stats.CameraOverlappingSpriteRendererCount = cameraOverlappingSpriteRenderers;

        if (visibleSpriteRenderers == 0)
        {
            issues.Add($"{scenePath}: no active visible SpriteRenderer with a non-null sprite was found.");
        }

        if (camera != null && cameraOverlappingSpriteRenderers == 0)
        {
            issues.Add($"{scenePath}: no active SpriteRenderer overlaps camera '{GetGameObjectPath(camera.gameObject)}'.");
        }
    }

    private static void ValidateRequiredZiaSprites(
        Scene scene,
        string scenePath,
        Camera camera,
        List<string> issues)
    {
        if (!RequiredZiaSpritePathsByScene.TryGetValue(scenePath, out string[] requiredSpritePaths))
        {
            return;
        }

        for (int i = 0; i < requiredSpritePaths.Length; i++)
        {
            string spritePath = requiredSpritePaths[i];
            Sprite sprite = LoadSprite(spritePath);
            if (sprite == null)
            {
                issues.Add($"{scenePath}: required Zia sprite asset is missing or not imported as a Sprite: {spritePath}");
                continue;
            }

            if (!SceneHasVisibleSprite(scene, sprite, camera))
            {
                issues.Add($"{scenePath}: required Zia sprite is not visible in the scene/camera: {spritePath}");
            }
        }
    }

    private static bool SceneHasVisibleSprite(Scene scene, Sprite sprite, Camera camera)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            SpriteRenderer[] renderers = roots[i].GetComponentsInChildren<SpriteRenderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                SpriteRenderer renderer = renderers[rendererIndex];
                if (renderer == null ||
                    !renderer.gameObject.activeInHierarchy ||
                    !renderer.enabled ||
                    renderer.sprite != sprite)
                {
                    continue;
                }

                if (camera == null || RendererOverlapsCamera(renderer, camera))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool RendererOverlapsCamera(Renderer renderer, Camera camera)
    {
        if (renderer == null || camera == null)
        {
            return false;
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    private static bool IsKnownOptionalZiaNullSprite(string scenePath, SpriteRenderer renderer)
    {
        return scenePath == ZiaTask2ScenePath && renderer != null && renderer.gameObject.name == "Final_0";
    }

    private static void CountMissingScripts(Scene scene, List<string> issues, ZiaSceneValidationStats stats)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
            {
                GameObject gameObject = transforms[transformIndex].gameObject;
                int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (missingCount > 0)
                {
                    if (IsOptionalZiaBgmObjectOrChild(gameObject))
                    {
                        continue;
                    }

                    stats.MissingScriptCount += missingCount;
                    issues.Add($"{scene.path}: '{GetGameObjectPath(gameObject)}' has {missingCount} missing script component(s).");
                }
            }
        }
    }

    private static Camera FindEnabledCamera(Scene scene)
    {
        Camera camera = FindPreferredCamera(scene);
        if (camera != null && camera.gameObject.activeInHierarchy && camera.enabled)
        {
            return camera;
        }

        Camera[] cameras = FindSceneCameras(scene);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].gameObject.activeInHierarchy && cameras[i].enabled)
            {
                return cameras[i];
            }
        }

        return null;
    }

    private static Camera FindPreferredCamera(Scene scene)
    {
        GameObject mainCameraObject = FindByName(scene, "Main Camera");
        if (mainCameraObject != null && mainCameraObject.TryGetComponent(out Camera mainCamera))
        {
            return mainCamera;
        }

        Camera[] cameras = FindSceneCameras(scene);
        return cameras.Length > 0 ? cameras[0] : null;
    }

    private static Camera[] FindSceneCameras(Scene scene)
    {
        List<Camera> cameras = new List<Camera>();
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            cameras.AddRange(roots[i].GetComponentsInChildren<Camera>(true));
        }

        return cameras.ToArray();
    }

    private static string GetGameObjectPath(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return "<null>";
        }

        Stack<string> pathParts = new Stack<string>();
        Transform current = gameObject.transform;
        while (current != null)
        {
            pathParts.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", pathParts.ToArray());
    }

    private static bool AssignSpriteIfObjectExists(
        Scene scene,
        string objectName,
        string spritePath,
        int sortingOrder,
        Color color)
    {
        GameObject gameObject = FindByName(scene, objectName);
        Sprite sprite = LoadSprite(spritePath);
        if (gameObject == null || sprite == null)
        {
            return false;
        }

        SetGameObjectAndParentsActive(gameObject);

        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        bool changed =
            renderer == null ||
            !renderer.enabled ||
            renderer.sprite != sprite ||
            renderer.sortingOrder != sortingOrder ||
            renderer.color != color;

        AssignSprite(gameObject, sprite, sortingOrder, color);
        return changed;
    }

    private static int RemoveOptionalZiaBgmObjects(Scene scene)
    {
        HashSet<GameObject> objectsToRemove = new HashSet<GameObject>();
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                GameObject candidate = transforms[j].gameObject;
                if (IsOptionalZiaBgmObject(candidate))
                {
                    objectsToRemove.Add(candidate);
                }
            }
        }

        if (objectsToRemove.Count == 0)
        {
            return 0;
        }

        List<GameObject> filteredObjects = new List<GameObject>(objectsToRemove);
        filteredObjects.RemoveAll(candidate => HasRemovableAncestor(candidate, objectsToRemove));

        for (int i = 0; i < filteredObjects.Count; i++)
        {
            Debug.Log(
                $"Build Guest 1 Hotel Hall Map: removing optional Zia BGM object '{filteredObjects[i].name}' " +
                $"from scene '{scene.path}' because skipped prefab guid {SkippedZiaBgmPrefabGuid} is intentionally not imported.");
            Undo.DestroyObjectImmediate(filteredObjects[i]);
        }

        return filteredObjects.Count;
    }

    private static bool IsOptionalZiaBgmObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return false;
        }

        string lowerName = gameObject.name.ToLowerInvariant();
        bool hasBgmName =
            lowerName.Contains("bgm_pov1") ||
            lowerName.Contains("bgm_player") ||
            lowerName.Contains("persistentbgm") ||
            lowerName.Contains("pov 1 background");

        if (hasBgmName)
        {
            return true;
        }

        PrefabInstanceStatus prefabStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
        return prefabStatus == PrefabInstanceStatus.MissingAsset && lowerName.Contains("bgm");
    }

    private static bool IsOptionalZiaBgmObjectOrChild(GameObject gameObject)
    {
        Transform current = gameObject != null ? gameObject.transform : null;
        while (current != null)
        {
            if (IsOptionalZiaBgmObject(current.gameObject))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool HasRemovableAncestor(GameObject candidate, HashSet<GameObject> objectsToRemove)
    {
        if (candidate == null)
        {
            return false;
        }

        Transform parent = candidate.transform.parent;
        while (parent != null)
        {
            if (objectsToRemove.Contains(parent.gameObject))
            {
                return true;
            }

            parent = parent.parent;
        }

        return false;
    }

    private static void ConfigureCartChoiceObject(
        Scene scene,
        string cartName,
        string interactionZoneName,
        Vector3 worldPosition,
        Vector2 triggerSize,
        string promptText,
        UnityAction interactionAction)
    {
        GameObject cart = FindByName(scene, cartName);
        SetWorldPosition(cart, worldPosition);
        HideChildRenderers(cart, "CartVisual", "PixelArtCart");

        GameObject interactionZone = FindChildByName(cart, interactionZoneName);
        if (interactionZone == null)
        {
            interactionZone = FindByName(scene, interactionZoneName);
        }

        SetLocalPosition(interactionZone, Vector3.zero);
        ConfigureBoxTrigger(interactionZone, triggerSize);

        ConfigureInteractionTarget(interactionZone, promptText, interactionAction, true);
    }

    private static InteractionTarget ConfigureInteractionTarget(
        GameObject interactionZone,
        string promptText,
        UnityAction interactionAction,
        bool enabled)
    {
        if (interactionZone == null)
        {
            return null;
        }

        RemoveComponentIfPresent<SceneTransitionTrigger>(interactionZone);
        RemoveComponentIfPresent<HallAutoSceneTransition>(interactionZone);

        InteractionTarget target = EnsureComponent<InteractionTarget>(interactionZone);
        Undo.RecordObject(target, UndoName);
        target.enabled = enabled;
        target.SetPromptText(promptText);
        ClearPersistentListeners(target.OnInteract);
        if (interactionAction != null)
        {
            UnityEventTools.AddPersistentListener(target.OnInteract, interactionAction);
        }

        EditorUtility.SetDirty(target);
        return target;
    }

    private static void DisableCartObject(Scene scene, string cartName, string interactionZoneName)
    {
        GameObject cart = FindByName(scene, cartName);
        HideRenderersInChildren(cart);

        GameObject interactionZone = FindChildByName(cart, interactionZoneName);
        if (interactionZone == null)
        {
            interactionZone = FindByName(scene, interactionZoneName);
        }

        DisableInteractionComponents(interactionZone);
        DisableCollidersInChildren(cart);

        if (cart != null)
        {
            SetWorldPosition(cart, PixelToWorld(300f, 160f));
        }
    }

    private static void DisableHall3Luggage(GameObject luggageObject)
    {
        if (luggageObject == null)
        {
            return;
        }

        HideRenderersInChildren(luggageObject);
        DisableInteractionComponents(luggageObject);
        DisableCollidersInChildren(luggageObject);
        SetWorldPosition(luggageObject, PixelToWorld(300f, 160f));
        DisableSceneObject(luggageObject);
    }

    private static void EnsureMainCamera(Scene scene)
    {
        GameObject cameraObject = EnsureRootObject(scene, "Main Camera");
        SafeSetTag(cameraObject, "MainCamera");
        SetWorldPosition(cameraObject, new Vector3(0f, 0f, -10f));
        SetLocalScale(cameraObject, Vector3.one);

        Camera camera = EnsureComponent<Camera>(cameraObject);
        Undo.RecordObject(camera, UndoName);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.19215687f, 0.3019608f, 0.4745098f, 0f);
        EditorUtility.SetDirty(camera);

        EnsureComponent<AudioListener>(cameraObject);
    }

    private static void EnsureHallBackground(Scene scene, string visualRootName, string spritePath)
    {
        GameObject visualRoot = EnsureSingleRoot(scene, visualRootName);
        SetWorldPosition(visualRoot, Vector3.zero);
        SetLocalScale(visualRoot, Vector3.one);

        GameObject background = EnsureDirectChild(visualRoot, "PixelArtBackground");
        RemoveRuntimeComponents(background);
        SetLocalPosition(background, Vector3.zero);
        SetLocalScale(background, Vector3.one);
        AssignSprite(background, LoadSprite(spritePath), -20, Color.white);
    }

    private static void EnsurePlayer(Scene scene, Vector3 worldPosition)
    {
        GameObject player = EnsureRootObject(scene, "Player");
        SafeSetTag(player, "Player");
        SetWorldPosition(player, worldPosition);
        SetLocalScale(player, new Vector3(0.6f, 1.2f, 1f));

        Rigidbody2D body = EnsureComponent<Rigidbody2D>(player);
        Undo.RecordObject(body, UndoName);
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        EditorUtility.SetDirty(body);

        BoxCollider2D collider = EnsureComponent<BoxCollider2D>(player);
        Undo.RecordObject(collider, UndoName);
        collider.isTrigger = false;
        collider.offset = Vector2.zero;
        collider.size = Vector2.one;
        EditorUtility.SetDirty(collider);

        EnsureComponent<SidePlayerController>(player);
        EnsureComponent<PlayerInteractor>(player);
        ConfigurePlayerVisual(player);
    }

    private static void ConfigurePlayerVisual(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        HideOwnRenderer(player);
        ConfigureCharacterVisual(player, "PixelArtConciergeVisual", ConciergeSpritePath, 1f, 12);
    }

    private static void ConfigureGuest1Visual(GameObject guest1)
    {
        if (guest1 == null)
        {
            return;
        }

        HideOwnRenderer(guest1);
        HideOwnRenderer(FindChildByName(guest1, "Guest1Visual"));
        HideOwnRenderer(FindChildByName(guest1, "PixelArtSprite"));
        ConfigureCharacterVisual(guest1, "PixelArtGuest1Visual", Guest1SpritePath, 1f, 12);
    }

    private static InteractionTarget ConfigureLobbyGuest1SuitcaseInteraction(
        Scene scene,
        UnityAction pickUpLuggageAction,
        out GameObject suitcase)
    {
        DestroyObjectsByName(scene, "PixelArtGuest1SuitcaseVisual");
        suitcase = EnsureSingleRoot(scene, "Guest1LobbySuitcase");
        GameObject guest1 = FindByName(scene, "Guest1");
        Vector3 suitcasePosition = guest1 != null ?
            guest1.transform.position + new Vector3(0.65f, -0.35f, 0f) :
            PixelToWorld(132f, 112f);

        SetGameObjectAndParentsActive(suitcase);
        SetWorldPosition(suitcase, suitcasePosition);
        SetLocalScale(suitcase, Vector3.one * 0.75f);
        RemoveRuntimeComponents(suitcase);
        AssignSprite(suitcase, LoadSprite(SuitcaseSpritePath), 13, Color.white);
        ConfigureBoxTrigger(suitcase, new Vector2(1.3f, 1.1f));
        return ConfigureInteractionTarget(
            suitcase,
            "Pick up Guest 1's luggage",
            pickUpLuggageAction,
            true);
    }

    private static void ConfigureCharacterVisual(
        GameObject parent,
        string childName,
        string spritePath,
        float uniformWorldScale,
        int sortingOrder)
    {
        if (parent == null)
        {
            return;
        }

        GameObject visual = EnsureDirectChild(parent, childName);
        RemoveRuntimeComponents(visual);
        SetLocalPosition(visual, Vector3.zero);
        SetCompensatedWorldScale(visual.transform, uniformWorldScale);
        AssignSprite(visual, LoadSprite(spritePath), sortingOrder, Color.white);
    }

    private static void ConfigureAutoTransitionZone(
        Scene scene,
        string objectName,
        Vector3 worldPosition,
        Vector2 triggerSize,
        string targetSceneName,
        string targetSpawnPointId)
    {
        GameObject transitionObject = EnsureRootObject(scene, objectName);
        SetWorldPosition(transitionObject, worldPosition);
        SetLocalScale(transitionObject, Vector3.one);
        ConfigureBoxTrigger(transitionObject, triggerSize);

        RemoveComponentIfPresent<InteractionTarget>(transitionObject);
        RemoveComponentIfPresent<SceneTransitionTrigger>(transitionObject);

        HallAutoSceneTransition autoTransition = EnsureComponent<HallAutoSceneTransition>(transitionObject);
        Undo.RecordObject(autoTransition, UndoName);
        autoTransition.enabled = true;
        autoTransition.Configure(targetSceneName, targetSpawnPointId);
        EditorUtility.SetDirty(autoTransition);
    }

    private static void ConfigureTransitionZone(
        Scene scene,
        string objectName,
        Vector3 worldPosition,
        Vector2 triggerSize,
        string promptText,
        string targetSceneName,
        string targetSpawnPointId)
    {
        GameObject transitionObject = EnsureRootObject(scene, objectName);
        SetWorldPosition(transitionObject, worldPosition);
        SetLocalScale(transitionObject, Vector3.one);
        ConfigureBoxTrigger(transitionObject, triggerSize);

        ConfigureSceneTransition(transitionObject, promptText, targetSceneName, targetSpawnPointId, true);
    }

    private static SceneTransitionTrigger ConfigureSceneTransition(
        GameObject transitionObject,
        string promptText,
        string targetSceneName,
        string targetSpawnPointId,
        bool enabled)
    {
        if (transitionObject == null)
        {
            return null;
        }

        RemoveComponentIfPresent<InteractionTarget>(transitionObject);
        RemoveComponentIfPresent<HallAutoSceneTransition>(transitionObject);

        SceneTransitionTrigger transition = EnsureComponent<SceneTransitionTrigger>(transitionObject);
        Undo.RecordObject(transition, UndoName);
        transition.enabled = enabled;
        transition.SetPromptText(promptText);
        transition.Configure(targetSceneName, targetSpawnPointId);
        EditorUtility.SetDirty(transition);
        return transition;
    }

    private static void ConfigureBoxTrigger(GameObject gameObject, Vector2 size)
    {
        if (gameObject == null)
        {
            return;
        }

        BoxCollider2D collider = EnsureComponent<BoxCollider2D>(gameObject);
        Undo.RecordObject(collider, UndoName);
        collider.enabled = true;
        collider.isTrigger = true;
        collider.offset = Vector2.zero;
        collider.size = size;
        EditorUtility.SetDirty(collider);
    }

    private static void EnsureSpawnPoint(Scene scene, string objectName, string spawnId, Vector3 worldPosition)
    {
        GameObject spawnObject = EnsureRootObject(scene, objectName);
        SetWorldPosition(spawnObject, worldPosition);
        SetLocalScale(spawnObject, Vector3.one);

        SpawnPoint spawnPoint = EnsureComponent<SpawnPoint>(spawnObject);
        Undo.RecordObject(spawnPoint, UndoName);
        spawnPoint.SetSpawnPointId(spawnId);
        EditorUtility.SetDirty(spawnPoint);
    }

    private static void RebuildHall1Collision(Scene scene)
    {
        GameObject root = EnsureSingleRoot(scene, "PixelArtCollisionLayout_HotelHall1");
        ClearChildren(root);

        AddSolidBox(root, "TopWallAndPlants", 160f, 26f, 320f, 52f);
        AddSolidBox(root, "BottomWallAndPlants", 160f, 150f, 320f, 60f);
        AddSolidBox(root, "LeftBarDoorBlocker", 4f, 90f, 8f, 100f);
    }

    private static void RebuildHall2Collision(Scene scene)
    {
        GameObject root = EnsureSingleRoot(scene, "PixelArtCollisionLayout_HotelHall2");
        ClearChildren(root);

        AddSolidBox(root, "TopWallAndPlants", 160f, 26f, 320f, 52f);
        AddSolidBox(root, "BottomLeftOutOfBounds", 58f, 150f, 116f, 60f);
        AddSolidBox(root, "BottomRightOutOfBounds", 260f, 150f, 120f, 60f);
        AddSolidBox(root, "DownRoadEndSafetyBlocker", 160f, 174f, 88f, 12f);
    }

    private static void RebuildFourthHallCollision(Scene scene)
    {
        GameObject root = EnsureSingleRoot(scene, "PixelArtCollisionLayout_HotelHall4");
        ClearChildren(root);

        AddSolidBox(root, "TopLeftOutOfBounds", 56f, 12f, 112f, 24f);
        AddSolidBox(root, "TopRightOutOfBounds", 256f, 12f, 128f, 24f);
        AddSolidBox(root, "LowerLeftOutOfBounds", 57f, 166f, 114f, 28f);
        AddSolidBox(root, "RopeQueue", 182f, 156f, 50f, 24f);
    }

    private static void RebuildThirdHallCollision(Scene scene)
    {
        GameObject root = EnsureSingleRoot(scene, "PixelArtCollisionLayout_HotelHall3");
        ClearChildren(root);

        AddSolidBox(root, "TopLeftOutOfBounds", 56f, 12f, 112f, 24f);
        AddSolidBox(root, "TopRightOutOfBounds", 256f, 12f, 128f, 24f);
        AddSolidBox(root, "UpRoadEndSafetyBlocker", 160f, 6f, 88f, 12f);
        AddSolidBox(root, "BottomOutOfBounds", 160f, 166f, 320f, 28f);
        AddSolidBox(root, "LeftDiningRoomDoorBlocker", 4f, 90f, 8f, 100f);
    }

    private static void RebuildDiningRoomCollision(Scene scene, string rootName)
    {
        GameObject root = EnsureSingleRoot(scene, rootName);
        ClearChildren(root);

        AddSolidBox(root, "CentralDiningTable", 146f, 88f, 208f, 50f);
    }

    private static void RebuildBasementCollision(Scene scene)
    {
        GameObject root = EnsureSingleRoot(scene, "PixelArtCollisionLayout_BasementRoom");
        ClearChildren(root);

        AddSolidBox(root, "TopOutOfBounds", 160f, 14f, 320f, 28f);
        AddSolidBox(root, "BottomOutOfBounds", 160f, 166f, 320f, 28f);
        AddSolidBox(root, "RightOutOfBounds", 310f, 90f, 20f, 180f);
    }

    private static void ConfigureExclamationMark(GameObject exclamation)
    {
        if (exclamation == null)
        {
            return;
        }

        RemoveRuntimeComponents(exclamation);
        SetLocalPosition(exclamation, new Vector3(0f, 1.05f, 0f));
        SetLocalScale(exclamation, Vector3.one);

        TextMesh textMesh = EnsureComponent<TextMesh>(exclamation);
        Undo.RecordObject(textMesh, UndoName);
        textMesh.text = "!";
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 96;
        textMesh.characterSize = 0.1f;
        textMesh.color = new Color(0.85f, 0f, 0f, 1f);
        EditorUtility.SetDirty(textMesh);

        MeshRenderer renderer = exclamation.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Undo.RecordObject(renderer, UndoName);
            renderer.sortingOrder = 40;
            EditorUtility.SetDirty(renderer);
        }

        Undo.RecordObject(exclamation, UndoName);
        exclamation.SetActive(false);
        EditorUtility.SetDirty(exclamation);
    }

    private static void AddSolidBox(
        GameObject parent,
        string name,
        float centerPixelX,
        float centerPixelY,
        float widthPixels,
        float heightPixels)
    {
        GameObject colliderObject = EnsureDirectChild(parent, name);
        RemoveRuntimeComponents(colliderObject);
        SetWorldPosition(colliderObject, PixelToWorld(centerPixelX, centerPixelY));
        SetLocalScale(colliderObject, Vector3.one);

        BoxCollider2D collider = EnsureComponent<BoxCollider2D>(colliderObject);
        Undo.RecordObject(collider, UndoName);
        collider.enabled = true;
        collider.isTrigger = false;
        collider.offset = Vector2.zero;
        collider.size = new Vector2(widthPixels / PixelsPerUnit, heightPixels / PixelsPerUnit);
        EditorUtility.SetDirty(collider);
    }

    private static void UpdateBuildSettings()
    {
        EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
        Dictionary<string, EditorBuildSettingsScene> existingByPath = new Dictionary<string, EditorBuildSettingsScene>();
        for (int i = 0; i < currentScenes.Length; i++)
        {
            if (!existingByPath.ContainsKey(currentScenes[i].path))
            {
                existingByPath.Add(currentScenes[i].path, currentScenes[i]);
            }
        }

        HashSet<string> preferredPaths = new HashSet<string>(PreferredBuildSceneOrder);
        List<EditorBuildSettingsScene> orderedScenes = new List<EditorBuildSettingsScene>();

        for (int i = 0; i < PreferredBuildSceneOrder.Length; i++)
        {
            string path = PreferredBuildSceneOrder[i];
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                continue;
            }

            if (existingByPath.TryGetValue(path, out EditorBuildSettingsScene existingScene))
            {
                existingScene.enabled = true;
                orderedScenes.Add(existingScene);
            }
            else
            {
                orderedScenes.Add(new EditorBuildSettingsScene(path, true));
            }
        }

        for (int i = 0; i < currentScenes.Length; i++)
        {
            if (preferredPaths.Contains(currentScenes[i].path))
            {
                continue;
            }

            if (IsObsoleteZiaBuildScene(currentScenes[i].path))
            {
                continue;
            }

            orderedScenes.Add(currentScenes[i]);
        }

        EditorBuildSettings.scenes = orderedScenes.ToArray();
    }

    private static bool IsObsoleteZiaBuildScene(string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            return false;
        }

        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
        return sceneName.StartsWith("Pov1_Guest3_Guide_", System.StringComparison.Ordinal) ||
            (sceneName.StartsWith("Guest3_Final", System.StringComparison.Ordinal) &&
             scenePath != ZiaFinalChaseScenePath);
    }

    private static Scene OpenOrCreateScene(string scenePath)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
        {
            return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
        return scene;
    }

    private static GameObject EnsureRootObject(Scene scene, string objectName)
    {
        GameObject gameObject = FindByName(scene, objectName);
        if (gameObject == null)
        {
            gameObject = new GameObject(objectName);
            Undo.RegisterCreatedObjectUndo(gameObject, UndoName);
            SceneManager.MoveGameObjectToScene(gameObject, scene);
        }

        if (gameObject.transform.parent != null)
        {
            Undo.SetTransformParent(gameObject.transform, null, UndoName);
        }

        return gameObject;
    }

    private static GameObject EnsureSingleRoot(Scene scene, string rootName)
    {
        List<GameObject> roots = FindAllByName(scene, rootName);
        GameObject root = roots.Count > 0 ? roots[0] : null;

        if (root == null)
        {
            root = new GameObject(rootName);
            Undo.RegisterCreatedObjectUndo(root, UndoName);
            SceneManager.MoveGameObjectToScene(root, scene);
        }

        if (root.transform.parent != null)
        {
            Undo.SetTransformParent(root.transform, null, UndoName);
        }

        for (int i = 1; i < roots.Count; i++)
        {
            Undo.DestroyObjectImmediate(roots[i]);
        }

        SetWorldPosition(root, Vector3.zero);
        SetLocalScale(root, Vector3.one);
        return root;
    }

    private static GameObject EnsureDirectChild(GameObject parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        GameObject child = null;
        List<GameObject> duplicates = new List<GameObject>();

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform candidate = parent.transform.GetChild(i);
            if (candidate.name != childName)
            {
                continue;
            }

            if (child == null)
            {
                child = candidate.gameObject;
            }
            else
            {
                duplicates.Add(candidate.gameObject);
            }
        }

        if (child == null)
        {
            child = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(child, UndoName);
            Undo.SetTransformParent(child.transform, parent.transform, UndoName);
        }

        for (int i = 0; i < duplicates.Count; i++)
        {
            Undo.DestroyObjectImmediate(duplicates[i]);
        }

        return child;
    }

    private static T EnsureComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = Undo.AddComponent<T>(gameObject);
        }

        return component;
    }

    private static void AssignSprite(GameObject gameObject, Sprite sprite, int sortingOrder, Color color)
    {
        if (gameObject == null || sprite == null)
        {
            return;
        }

        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(gameObject);
        }

        Undo.RecordObject(renderer, UndoName);
        renderer.enabled = true;
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.sortingOrder = sortingOrder;
        EditorUtility.SetDirty(renderer);
    }

    private static void HideRenderers(Scene scene, params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            List<GameObject> objects = FindAllByName(scene, names[i]);
            for (int j = 0; j < objects.Count; j++)
            {
                HideOwnRenderer(objects[j]);
            }
        }
    }

    private static void HideChildRenderers(GameObject parent, params string[] childNames)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = 0; i < childNames.Length; i++)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < children.Length; j++)
            {
                if (children[j].name == childNames[i])
                {
                    HideOwnRenderer(children[j].gameObject);
                }
            }
        }
    }

    private static void HideRenderersInChildren(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        SpriteRenderer[] renderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Undo.RecordObject(renderers[i], UndoName);
            renderers[i].enabled = false;
            EditorUtility.SetDirty(renderers[i]);
        }
    }

    private static void HideOwnRenderer(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            return;
        }

        Undo.RecordObject(renderer, UndoName);
        renderer.enabled = false;
        EditorUtility.SetDirty(renderer);
    }

    private static void DisableInteractionComponents(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        InteractionTarget interactionTarget = gameObject.GetComponent<InteractionTarget>();
        if (interactionTarget != null)
        {
            Undo.RecordObject(interactionTarget, UndoName);
            interactionTarget.enabled = false;
            EditorUtility.SetDirty(interactionTarget);
        }

        SceneTransitionTrigger sceneTransition = gameObject.GetComponent<SceneTransitionTrigger>();
        if (sceneTransition != null)
        {
            Undo.RecordObject(sceneTransition, UndoName);
            sceneTransition.enabled = false;
            EditorUtility.SetDirty(sceneTransition);
        }

        HallAutoSceneTransition autoTransition = gameObject.GetComponent<HallAutoSceneTransition>();
        if (autoTransition != null)
        {
            Undo.RecordObject(autoTransition, UndoName);
            autoTransition.enabled = false;
            EditorUtility.SetDirty(autoTransition);
        }
    }

    private static void DisableCollidersInChildren(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Undo.RecordObject(colliders[i], UndoName);
            colliders[i].enabled = false;
            EditorUtility.SetDirty(colliders[i]);
        }
    }

    private static void DisableSceneObject(GameObject gameObject)
    {
        if (gameObject == null || !gameObject.activeSelf)
        {
            return;
        }

        Undo.RecordObject(gameObject, UndoName);
        gameObject.SetActive(false);
        EditorUtility.SetDirty(gameObject);
    }

    private static void SetGameObjectAndParentsActive(GameObject gameObject)
    {
        Transform current = gameObject != null ? gameObject.transform : null;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                Undo.RecordObject(current.gameObject, UndoName);
                current.gameObject.SetActive(true);
                EditorUtility.SetDirty(current.gameObject);
            }

            current = current.parent;
        }
    }

    private static void SetTextMeshText(GameObject gameObject, string text)
    {
        if (gameObject == null)
        {
            return;
        }

        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        if (textMesh == null)
        {
            return;
        }

        Undo.RecordObject(textMesh, UndoName);
        textMesh.text = text;
        EditorUtility.SetDirty(textMesh);
    }

    private static void SetTextMeshStyle(GameObject gameObject, Color color, int fontSize, TextAnchor anchor)
    {
        if (gameObject == null)
        {
            return;
        }

        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        if (textMesh == null)
        {
            return;
        }

        Undo.RecordObject(textMesh, UndoName);
        textMesh.color = color;
        textMesh.fontSize = fontSize;
        textMesh.anchor = anchor;
        textMesh.alignment = TextAlignment.Center;
        EditorUtility.SetDirty(textMesh);
    }

    private static void ConfigureLobbyFlowReferences(
        Guest1LobbyFlow lobbyFlow,
        GameObject suitcaseVisual,
        InteractionTarget suitcaseInteraction,
        SceneTransitionTrigger lobbyTransition)
    {
        if (lobbyFlow == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(lobbyFlow);
        serializedObject.FindProperty("suitcaseVisual").objectReferenceValue = suitcaseVisual;
        serializedObject.FindProperty("suitcaseInteraction").objectReferenceValue = suitcaseInteraction;
        serializedObject.FindProperty("elevatorTransition").objectReferenceValue = lobbyTransition;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(lobbyFlow);
    }

    private static void ConfigureCartFlowReferences(
        Guest1CartFlow cartFlow,
        InteractionTarget elevatorInspectionInteraction,
        SceneTransitionTrigger elevatorTransition,
        Guest1PostCartCutscene postCartCutscene)
    {
        if (cartFlow == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(cartFlow);
        serializedObject.FindProperty("elevatorInspectionInteraction").objectReferenceValue = elevatorInspectionInteraction;
        serializedObject.FindProperty("elevatorTransition").objectReferenceValue = elevatorTransition;
        serializedObject.FindProperty("postCartCutscene").objectReferenceValue = postCartCutscene;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(cartFlow);
    }

    private static void ConfigureWaitingFlowReferences(
        Guest1WaitingFlow waitingFlow,
        GameObject luggageVisual,
        InteractionTarget luggageInteraction,
        SceneTransitionTrigger diningRoomTransition)
    {
        if (waitingFlow == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(waitingFlow);
        serializedObject.FindProperty("luggageVisual").objectReferenceValue = luggageVisual;
        serializedObject.FindProperty("luggageInteraction").objectReferenceValue = luggageInteraction;
        serializedObject.FindProperty("waitingRoomTransition").objectReferenceValue = diningRoomTransition;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(waitingFlow);
    }

    private static void ClearPersistentListeners(UnityEvent unityEvent)
    {
        if (unityEvent == null)
        {
            return;
        }

        while (unityEvent.GetPersistentEventCount() > 0)
        {
            UnityEventTools.RemovePersistentListener(unityEvent, 0);
        }
    }

    private static void RemoveRuntimeComponents(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            Undo.DestroyObjectImmediate(rigidbody);
        }

        Collider2D[] colliders = gameObject.GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Undo.DestroyObjectImmediate(colliders[i]);
        }
    }

    private static void RemoveComponentIfPresent<T>(GameObject gameObject) where T : Component
    {
        if (gameObject == null)
        {
            return;
        }

        T component = gameObject.GetComponent<T>();
        if (component != null)
        {
            Undo.DestroyObjectImmediate(component);
        }
    }

    private static void ClearChildren(GameObject parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(parent.transform.GetChild(i).gameObject);
        }
    }

    private static void DestroyObjectsByName(Scene scene, string objectName)
    {
        List<GameObject> matches = FindAllByName(scene, objectName);
        for (int i = 0; i < matches.Count; i++)
        {
            Undo.DestroyObjectImmediate(matches[i]);
        }
    }

    private static GameObject FindByName(Scene scene, string objectName)
    {
        List<GameObject> matches = FindAllByName(scene, objectName);
        return matches.Count > 0 ? matches[0] : null;
    }

    private static T FindComponentInScene<T>(Scene scene) where T : Component
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            T component = roots[i].GetComponentInChildren<T>(true);
            if (component != null)
            {
                return component;
            }
        }

        return null;
    }

    private static List<GameObject> FindAllByName(Scene scene, string objectName)
    {
        List<GameObject> matches = new List<GameObject>();
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                if (transforms[j].name == objectName)
                {
                    matches.Add(transforms[j].gameObject);
                }
            }
        }

        return matches;
    }

    private static GameObject FindChildByName(GameObject parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        Transform[] transforms = parent.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == childName)
            {
                return transforms[i].gameObject;
            }
        }

        return null;
    }

    private static void SetWorldPosition(GameObject gameObject, Vector3 position)
    {
        if (gameObject == null)
        {
            return;
        }

        Undo.RecordObject(gameObject.transform, UndoName);
        gameObject.transform.position = position;
        EditorUtility.SetDirty(gameObject.transform);
    }

    private static void SetLocalPosition(GameObject gameObject, Vector3 position)
    {
        if (gameObject == null)
        {
            return;
        }

        Undo.RecordObject(gameObject.transform, UndoName);
        gameObject.transform.localPosition = position;
        EditorUtility.SetDirty(gameObject.transform);
    }

    private static void SetLocalScale(GameObject gameObject, Vector3 scale)
    {
        if (gameObject == null)
        {
            return;
        }

        Undo.RecordObject(gameObject.transform, UndoName);
        gameObject.transform.localScale = scale;
        EditorUtility.SetDirty(gameObject.transform);
    }

    private static void SetCompensatedWorldScale(Transform transform, float uniformWorldScale)
    {
        if (transform == null)
        {
            return;
        }

        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        Vector3 compensatedScale = new Vector3(
            SafeScale(uniformWorldScale, parentScale.x),
            SafeScale(uniformWorldScale, parentScale.y),
            1f);

        Undo.RecordObject(transform, UndoName);
        transform.localScale = compensatedScale;
        EditorUtility.SetDirty(transform);
    }

    private static float SafeScale(float desiredWorldScale, float parentAxisScale)
    {
        if (Mathf.Approximately(parentAxisScale, 0f))
        {
            return desiredWorldScale;
        }

        return desiredWorldScale / parentAxisScale;
    }

    private static void SafeSetTag(GameObject gameObject, string tag)
    {
        if (gameObject == null)
        {
            return;
        }

        try
        {
            Undo.RecordObject(gameObject, UndoName);
            gameObject.tag = tag;
            EditorUtility.SetDirty(gameObject);
        }
        catch (UnityException)
        {
            Debug.LogWarning($"Build Guest 1 Hotel Hall Map: tag '{tag}' is not defined.");
        }
    }

    private static Sprite LoadSprite(string assetPath)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Vector3 PixelToWorld(float pixelX, float pixelY)
    {
        return new Vector3((pixelX - 160f) / PixelsPerUnit, (90f - pixelY) / PixelsPerUnit, 0f);
    }

    private static void SaveScene(Scene scene, string path)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
    }

    private static bool AnyOpenSceneIsDirty()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).isDirty)
            {
                return true;
            }
        }

        return false;
    }

    private static bool VerifyRequiredSpritesExist()
    {
        for (int i = 0; i < RequiredSpritePaths.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(RequiredSpritePaths[i]) != null)
            {
                continue;
            }

            EditorUtility.DisplayDialog(
                "Build Guest 1 Hotel Hall Map",
                $"Missing required sprite asset:\n{RequiredSpritePaths[i]}",
                "OK");
            return false;
        }

        return true;
    }

    private static void ConfigureSpriteImporters()
    {
        for (int i = 0; i < RequiredSpritePaths.Length; i++)
        {
            if (ExactImportedZiaSpritePaths.Contains(RequiredSpritePaths[i]))
            {
                continue;
            }

            TextureImporter importer = AssetImporter.GetAtPath(RequiredSpritePaths[i]) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
}
