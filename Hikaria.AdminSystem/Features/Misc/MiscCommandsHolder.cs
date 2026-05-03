using Agents;
using AIGraph;
using Enemies;
using GameData;
using Globals;
using Hikaria.AdminSystem.Extensions;
using Hikaria.AdminSystem.Suggestions.Suggestors.Attributes;
using Hikaria.AdminSystem.Utilities;
using Hikaria.AdminSystem.Utility;
using Hikaria.Core;
using Hikaria.QC;
using LevelGeneration;
using Player;
using SNetwork;
using System.Collections;
using System.Text;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using UnityEngine;

namespace Hikaria.AdminSystem.Features.Misc;

[HideInModSettings]
[EnableFeatureByDefault]
[DisallowInGameToggle]
public class MiscCommandsHolder : Feature
{
    public override string Name => "杂项指令";

    public override string Description => "杂项指令";

    public override TheArchive.Core.FeaturesAPI.Groups.GroupBase Group => ModuleGroup.GetOrCreateSubGroup("Misc");

    [Command("LeaveSessionHub")]
    private static void LeaveSessionHub()
    {
        PlayfabMatchmakingManager.Current.CancelMatchmake();
        SNet.SessionHub.LeaveHub(false);
        GuiManager.MainMenuLayer.PageRundownNew.TryShowMatchmakeButton();
    }

    [Command("WhoIsMaster")]
    private static string WhoIsMaster => SNet.HasMaster ? $"{SNet.Master.NickName} [{SNet.Master.Lookup}]" : string.Empty;

    //[Command("IsMaster")]
    //private static bool IsMaster => SNet.IsMaster;

    [Command("WantToSay")]
    private static void WantToSay(int playerID, uint eventID, uint inDialogID = 0, uint startDialogID = 0, uint subtitleId = 0)
    {
        PlayerVoiceManager.WantToSayInternal(playerID - 1, eventID, inDialogID, startDialogID, subtitleId);
    }

    [Command("EnemyDetection")]
    private static bool DisableEnemyPlayerDetection
    {
        get
        {
            return Global.EnemyPlayerDetectionEnabled;
        }
        set
        {
            Global.EnemyPlayerDetectionEnabled = value;
            ConsoleLogs.LogToConsole($"已{(Global.EnemyPlayerDetectionEnabled ? "启用" : "禁用")} 禁用敌人检测");
        }
    }

    [Command("LightningStrike", "释放闪电")]
    private static void LightningStrike(int r, int g, int b)
    {
        Color color = new(r, g, b);
        Vector3 dir = AdminUtils.LocalPlayerAgent.FPSCamera.CameraRayDir;
        EnvironmentStateManager.AttemptLightningStrike(dir, color);
    }

    [Command("FireGlue", "喷射结沫")]
    private static void FireGlue(int glueExpand = 20)
    {
        ProjectileManager.WantToFireGlue(AdminUtils.LocalPlayerAgent, AdminUtils.LocalPlayerAgent.FPSCamera.Position + AdminUtils.LocalPlayerAgent.FPSCamera.CameraRayDir * 0.2f, AdminUtils.LocalPlayerAgent.FPSCamera.CameraRayDir * 35f, glueExpand, true);
    }

    [Command("PortableFogTurbine", "切换便携驱雾器")]
    private static void TogglePortableFogRepeller()
    {
        FogRepeller_Sphere fogRepeller_Sphere = AdminUtils.LocalPlayerAgent.gameObject.GetComponent<FogRepeller_Sphere>();
        if (fogRepeller_Sphere == null)
        {
            fogRepeller_Sphere = AdminUtils.LocalPlayerAgent.gameObject.AddComponent<FogRepeller_Sphere>();
            fogRepeller_Sphere.InfiniteDuration = true;
            fogRepeller_Sphere.Range = 100f;
        }
        if (!fogRepeller_Sphere.m_repellerEnabled)
        {
            fogRepeller_Sphere.StartRepelling();
        }
        else
        {
            fogRepeller_Sphere.StopRepelling();
        }
        ConsoleLogs.LogToConsole($"已{(false ? "启用" : "禁用")}便携驱雾器");
    }

    [Command("FireTargeting", "发射追踪粒子")]
    private static void FireTargeting(ProjectileType type, [PlayerAgentSuggestorTag] PlayerAgent playerAgent, int count = 1)
    {
        while (count > 0)
        {
            ProjectileManager.WantToFireTargeting(type, playerAgent, AdminUtils.LocalPlayerAgent.EyePosition + AdminUtils.LocalPlayerAgent.Forward * 0.25f, AdminUtils.LocalPlayerAgent.Forward, count, 100);
            count--;
        }
    }

    [Command("FuckMaster", "强制夺取房主")]
    private static void FuckMaster()
    {
        SNet_Player master = SNet.Master;
        SNet.MasterManagement.TryStartOnBadConnectionWithMaster();
        pMigrationReport pMigrationReport = new()
        {
            type = MigrationReportType.MigartionIsDone,
            hasNewMaster = true
        };
        pMigrationReport.NewMaster.SetPlayer(SNet.LocalPlayer);
        SNet.MasterManagement.m_migrationReportPacket.Send(pMigrationReport, SNet_ChannelType.SessionOrderCritical);
        SNet.MasterManagement.OnMigrationReport(pMigrationReport);
        SNet.SessionHub.KickPlayer(master, SNet_PlayerEventReason.Kick_ByVote);
        SNet.MasterManagement.EndOnBadConnectionWithMaster();
        if (CurrentGameState == (int)eGameStateName.ExpeditionFail || CurrentGameState == (int)eGameStateName.ExpeditionSuccess)
            return;
        if (SNet.MasterManagement.TryFindBestCaptureBuffer(out var bestBufferSummary))
            SNet.Sync.StartRecallWithAllSyncedPlayers(bestBufferSummary.bufferType, false);
        else
            SNet.Sync.StartRecallWithAllSyncedPlayers(eBufferType.RestartLevel, false);

        ConsoleLogs.LogToConsole("已强行夺取房主权限");
    }

    [Command("RevealMap", "地图全显")]
    private static void RevealMap()
    {
        GameObject coneObj = new();
        coneObj.transform.localScale = new Vector3(128f, 32f, 128f);
        foreach (AIG_CourseNode node in AIG_CourseNode.s_allNodes)
        {
            try
            {
                LG_AreaAIGraphSource source = node.m_area.GraphSource;
                coneObj.transform.position = source.transform.position;
                MapDetails.AddVisiblityCone(coneObj.transform, MapDetails.VisibilityLayer.LocalPlayer);
            }
            catch
            {
            }
        }

        foreach (var zone in Builder.CurrentFloor.allZones)
        {
            foreach (var area in zone.m_areas)
            {
                var comps = area.GetComponentsInChildren<LG_MapLookatRevealerBase>();
                foreach (var comp in comps)
                {
                    MapDataManager.WantToSetGUIObjVisible(comp.MapGUIObjID, comp.CurrentStatus);
                }
            }
        }

        ConsoleLogs.LogToConsole("已设置地图全显");
    }

    [Command("OpenResourceContainers")]
    private static void OpenResourceContainers([LG_ZoneSuggestorTag] LG_Zone zone = null)
    {
        if (CurrentGameState != (int)eGameStateName.InLevel)
        {
            ConsoleLogs.LogToConsole("不在游戏中", LogLevel.Error);
            return;
        }
        var zones = zone == null ? Builder.CurrentFloor.allZones.ToSystemList() : new List<LG_Zone>() { zone };
        foreach (var zone1 in zones)
        {
            foreach (var node in zone1.m_courseNodes)
            {
                foreach (var containerStorage in node.MetaData.StorageContainers)
                {
                    var container = containerStorage.m_core.Cast<LG_WeakResourceContainer>();
                    if (container.IsLocked())
                    {
                        pWeakLockInteraction unlock = new()
                        {
                            open = true,
                            type = eWeakLockInteractionType.Melt
                        };
                        container.WeakLockComponent.AttemptInteract(unlock);
                    }
                    container.TriggerOpen(true);
                }
            }
        }

        ConsoleLogs.LogToConsole("已打开所有资源箱");
    }

    [Command("OperateOrUnlock", "操作或解锁")]
    private static void OperateOrUnlock()
    {
        PlayerAgent player = AdminUtils.LocalPlayerAgent;
        if (player != null && player.FPSCamera.CameraRayObject != null)
        {
            var doorCore = player.FPSCamera.CameraRayObject.GetComponentInParent<iLG_Door_Core>()
                ?? player.FPSCamera.CameraRayObject.GetComponentInChildren<iLG_Door_Core>();
            if (doorCore != null)
            {
                var door = doorCore.TryCast<LG_WeakDoor>();
                if (door != null)
                {
                    if (door.WeakLocks != null && door.WeakLocks.Count != 0)
                    {
                        pWeakLockInteraction unlock = new()
                        {
                            open = true,
                            type = eWeakLockInteractionType.Melt
                        };
                        foreach (var weaklock in door.WeakLocks)
                        {
                            weaklock.AttemptInteract(unlock);
                        }
                    }

                }
                doorCore.AttemptOpenCloseInteraction(true);
            }
            else
            {
                var containerCore = player.FPSCamera.CameraRayObject.GetComponentInParent<iLG_ResourceContainer_Core>()
                    ?? player.FPSCamera.CameraRayObject.GetComponentInChildren<iLG_ResourceContainer_Core>();
                if (containerCore != null)
                {
                    var container = containerCore.Cast<LG_WeakResourceContainer>();
                    if (container != null)
                    {
                        if (container.IsLocked())
                        {
                            pWeakLockInteraction unlock = new()
                            {
                                open = true,
                                type = eWeakLockInteractionType.Melt
                            };
                            container.WeakLockComponent.AttemptInteract(unlock);
                        }
                        container.TriggerOpen(true);
                    }
                }
            }
            return;
        }
        ConsoleLogs.LogToConsole("目标物体为非法物体或空", LogLevel.Error);
    }

    [Command("ForceMigration", "强制迁移主机")]
    private static void ForceMigration()
    {
        if (SNet.IsMaster)
        {
            SNet.MasterManagement.ForceMigration();
            return;
        }
        ConsoleLogs.LogToConsole("只有房主才可以强制更换主机", LogLevel.Error);
    }


    [Command("StoreCheckPoint", "保存重生点")]
    private static void StoreCheckPoint()
    {
        if (!SNet.IsMaster)
        {
            ConsoleLogs.LogToConsole("只有房主可以保存重生点", LogLevel.Error);
            return;
        }
        CheckpointManager.StoreCheckpoint(AdminUtils.LocalPlayerAgent.EyePosition);
        SNet.Capture.CaptureGameState(eBufferType.Checkpoint);
        ConsoleLogs.LogToConsole("重生点已保存");
    }

    private enum EnemyChoiceType
    {
        Awake = 0,
        Reachable = 1,
        All = 2,
    }

    private static pES_HitreactData deathHitreactData = new()
    {
        ReactionType = ES_HitreactType.ToDeath,
        DeathDelay = 1,
        ReactionDirection = ImpactDirection.Back,
        RotationDir = Vector3.up
    };

    private static pES_FlyerHitreactData deathFlyerHitreactData = new()
    {
        HitType = ES_HitreactType.ToDeath,
        DeathDelay = 1,
        HitNormal = Vector3.up
    };

    private static void KillEnemy(EnemyAgent enemy)
    {
        if (enemy.EnemyBehaviorData.IsFlyer)
        {
            var hitreactFlyer = enemy.Locomotion.Hitreact.TryCast<ES_HitreactFlyer>();
            if (hitreactFlyer != null)
            {
                hitreactFlyer.m_hitreactPacket.Send(deathFlyerHitreactData, SNet_ChannelType.GameReceiveCritical, SNet.Master);
                return;
            }
            var hitreact = enemy.Locomotion.Hitreact.Cast<ES_Hitreact>();
            hitreact.m_hitreactPacket.Send(deathHitreactData, SNet_ChannelType.GameReceiveCritical, SNet.Master);
        }
        else
        {
            enemy.Locomotion.Hitreact.Cast<ES_Hitreact>().m_hitreactPacket.Send(deathHitreactData, SNet_ChannelType.GameReceiveCritical, SNet.Master);
        }
    }

    [Command("KillEnemies", "杀死敌人")]
    private static void KillEnemies(EnemyChoiceType choice = EnemyChoiceType.Awake)
    {
        string msg = string.Empty;
        switch (choice)
        {
            default:
            case EnemyChoiceType.Awake:
                foreach (var enemy in UnityEngine.Object.FindObjectsOfType<EnemyAgent>())
                {
                    if (enemy.AI.Mode == AgentMode.Agressive)
                    {
                        if (SNet.IsMaster && enemy.Damage.IsImortal && !enemy.IsScout)
                            enemy.Damage.IsImortal = false;
                        KillEnemy(enemy);
                    }
                }
                msg = "惊醒";
                break;
            case EnemyChoiceType.Reachable:
                foreach (var enemy in AIG_CourseGraph.GetReachableEnemiesInNodes(AdminUtils.LocalPlayerAgent.CourseNode, 100))
                {
                    if (SNet.IsMaster && enemy.Damage.IsImortal && !enemy.IsScout)
                        enemy.Damage.IsImortal = false;
                    KillEnemy(enemy);
                }
                msg = "可到达";
                break;
            case EnemyChoiceType.All:
                foreach (var enemy in UnityEngine.Object.FindObjectsOfType<EnemyAgent>())
                {
                    if (SNet.IsMaster && enemy.Damage.IsImortal && !enemy.IsScout)
                        enemy.Damage.IsImortal = false;
                    KillEnemy(enemy);
                }
                msg = "所有";
                break;
        }
        ConsoleLogs.LogToConsole($"<color=orange>已处死{msg}的敌人</color>");
    }

    [Command("KillPlayer", "处死玩家")]
    private static void KillPlayer([PlayerAgentSuggestorTag] PlayerAgent agent)
    {
        if (agent.IsLocallyOwned)
        {
            agent.Locomotion.ChangeState(PlayerLocomotion.PLOC_State.Downed);
        }
        agent.Damage.m_setDeadPacket.Send(new()
        {
            allowRevive = true
        }, SNet_ChannelType.GameReceiveCritical);
        ConsoleLogs.LogToConsole($"已处死玩家 {agent.GetColoredName()}");
    }

    [Command("TagEnemy", "标记敌人")]
    private static void TagEnemies(EnemyChoiceType choice = EnemyChoiceType.Awake)
    {
        string msg = string.Empty;
        switch (choice)
        {
            case EnemyChoiceType.Awake:
                foreach (var enemy in UnityEngine.Object.FindObjectsOfType<EnemyAgent>())
                {
                    if (enemy.AI.Mode == AgentMode.Agressive)
                    {
                        ToolSyncManager.WantToTagEnemy(enemy);
                    }
                }
                msg = "惊醒";
                break;
            case EnemyChoiceType.Reachable:
                foreach (var enemy in AIG_CourseGraph.GetReachableEnemiesInNodes(AdminUtils.LocalPlayerAgent.CourseNode, 100))
                {
                    ToolSyncManager.WantToTagEnemy(enemy);
                }
                msg = "可到达";
                break;
            case EnemyChoiceType.All:
                foreach (var enemy in UnityEngine.Object.FindObjectsOfType<EnemyAgent>())
                {
                    ToolSyncManager.WantToTagEnemy(enemy);
                }
                msg = "所有";
                break;
        }
        ConsoleLogs.LogToConsole($"<color=orange>已标记{msg}的怪物</color>");
    }

    [Command("ListEnemyData")]
    private static void ListEnemyData()
    {
        var sb = new StringBuilder(500);
        sb.AppendLine("----------------------------------------------------------------");
        foreach (var block in EnemyDataBlock.GetAllBlocks().OrderBy(block => block.persistentID))
        {
            sb.AppendLine($"{block.name} [{block.persistentID}]");
        }
        sb.AppendLine("----------------------------------------------------------------");
        ConsoleLogs.LogToConsole(sb.ToString());
    }

    [Command("ListEnemiesInZone")]
    private static void ListEnemiesInZone([LG_ZoneSuggestorTag] LG_Zone zone)
    {
        if (GameStateManager.CurrentStateName != eGameStateName.InLevel)
        {
            ConsoleLogs.LogToConsole("不在游戏中", LogLevel.Error);
            return;
        }
        var enemiesInZone = new Dictionary<LG_Area, Dictionary<string, int>>();
        foreach (LG_Area area in zone.m_areas)
        {
            if (!enemiesInZone.TryGetValue(area, out var value))
            {
                value = new Dictionary<string, int>();
                enemiesInZone.Add(area, value);
            }
            foreach (EnemyAgent enemy in area.m_courseNode.m_enemiesInNode)
            {
                string EnemyName = enemy.EnemyData.name;
                if (!value.TryGetValue(EnemyName, out int count))
                {
                    value.Add(EnemyName, 1);
                }
                else
                {
                    value[EnemyName] = ++count;
                }
            }
        }

        if (enemiesInZone.Count == 0)
        {
            ConsoleLogs.LogToConsole($"ZONE_{zone.Alias}中没有敌人");
            return;
        }
        var sb = new StringBuilder(500);
        enemiesInZone = enemiesInZone.OrderBy(x => x.Key.m_navInfo.UID).ToDictionary(x => x.Key, x => x.Value.OrderBy(y => y.Key).ToDictionary(y => y.Key, y => y.Value));
        var total = new Dictionary<string, int>();
        sb.AppendLine("-------------------------------------------------------------------------");
        sb.AppendLine($"                           ZONE_{zone.Alias} 敌人统计");
        foreach (LG_Area area in enemiesInZone.Keys)
        {
            if (enemiesInZone[area].Count == 0)
            {
                continue;
            }
            sb.AppendLine("-------------------------------------------------------------------------");
            sb.AppendLine($"{area.m_navInfo.GetFormattedText(LG_NavInfoFormat.Full_And_Number_With_Underscore)}:");
            foreach (string enemyName in enemiesInZone[area].Keys)
            {
                if (!total.ContainsKey(enemyName))
                {
                    total.Add(enemyName, enemiesInZone[area][enemyName]);
                }
                else
                {
                    total[enemyName] += enemiesInZone[area][enemyName];
                }
                sb.AppendLine($"           敌人:{enemyName.PadRight(35)}数量:{enemiesInZone[area][enemyName]}");
            }
        }

        sb.AppendLine("-------------------------------------------------------------------------");
        sb.AppendLine("总计:");
        if (total.Count == 0)
        {
            sb.AppendLine("           没有敌人");
        }
        else
        {
            foreach (string enemyName in total.Keys)
            {
                sb.AppendLine($"           敌人:{enemyName.PadRight(35)}数量:{total[enemyName]}");
            }
        }
        sb.AppendLine("-------------------------------------------------------------------------");
        ConsoleLogs.LogToConsole(sb.ToString());
    }

    [Command("EnemySetTarget", "设置敌人目标")]
    private static void SetEnemyTarget([PlayerAgentSuggestorTag] PlayerAgent playerAgent, EnemyChoiceType choice = EnemyChoiceType.Awake)
    {
        EnemySync.pEnemyStateData data = new();

        switch (choice)
        {
            case EnemyChoiceType.Reachable:
                foreach (var enemy in AIG_CourseGraph.GetReachableEnemiesInNodes(AdminUtils.LocalPlayerAgent.CourseNode, 100))
                {
                    enemy.Damage.BulletDamage(0f, playerAgent, enemy.Position, enemy.Forward, enemy.Forward);
                    data = enemy.Sync.m_enemyStateData;
                    data.target.Set(playerAgent);
                    data.agentMode = AgentMode.Agressive;
                    if (SNet.IsMaster)
                    {
                        enemy.Sync.IncomingState(data);
                    }
                    else
                    {
                        enemy.Sync.m_aiStatePacket.Send(data, SNet_ChannelType.GameReceiveCritical);
                    }
                }
                break;
            case EnemyChoiceType.Awake:
                foreach (var enemy in UnityEngine.Object.FindObjectsOfType<EnemyAgent>())
                {
                    if (enemy.AI.Mode == AgentMode.Agressive)
                    {
                        enemy.Damage.BulletDamage(0f, playerAgent, enemy.Position, enemy.Forward, enemy.Forward);
                        data = enemy.Sync.m_enemyStateData;
                        data.target.Set(playerAgent);
                        data.agentMode = AgentMode.Agressive;
                        if (SNet.IsMaster)
                        {
                            enemy.Sync.IncomingState(data);
                        }
                        else
                        {
                            enemy.Sync.m_aiStatePacket.Send(data, SNet_ChannelType.GameReceiveCritical);
                        }
                    }
                }
                break;
            case EnemyChoiceType.All:
                foreach (var enemy in UnityEngine.Object.FindObjectsOfType<EnemyAgent>())
                {
                    enemy.Damage.BulletDamage(0f, playerAgent, enemy.Position, enemy.Forward, enemy.Forward);
                    data = enemy.Sync.m_enemyStateData;
                    data.target.Set(playerAgent);
                    data.agentMode = AgentMode.Agressive;
                    if (SNet.IsMaster)
                    {
                        enemy.Sync.IncomingState(data);
                    }
                    else
                    {
                        enemy.Sync.m_aiStatePacket.Send(data, SNet_ChannelType.GameReceiveCritical);
                    }
                }
                break;
        }

    }

    [Command("EnemySetState", "设置敌人状态")]
    private static void SetEnemyState(EB_States state, EnemyChoiceType choice = EnemyChoiceType.Awake)
    {
        AgentMode agentMode;
        switch (state)
        {
            default:
            case EB_States.Hibernating:
                agentMode = AgentMode.Hibernate;
                break;
            case EB_States.InCombat:
                agentMode = AgentMode.Agressive;
                break;
            case EB_States.Dead:
                agentMode = AgentMode.Off;
                break;
        }
        EnemySync.pEnemyStateData data;
        switch (choice)
        {
            case EnemyChoiceType.Reachable:
                foreach (var enemy in AIG_CourseGraph.GetReachableEnemiesInNodes(AdminUtils.LocalPlayerAgent.CourseNode, 100))
                {
                    data = enemy.Sync.m_enemyStateData;
                    data.behaviourState = state;
                    data.agentMode = agentMode;
                    if (enemy.IsScout)
                    {
                        data.agentMode = AgentMode.Scout;
                    }
                    if (SNet.IsMaster)
                    {
                        enemy.Sync.IncomingState(data);
                    }
                    else
                    {
                        enemy.Sync.m_aiStatePacket.Send(data, SNet_ChannelType.GameReceiveCritical);
                    }
                }
                break;
            case EnemyChoiceType.Awake:
                foreach (var enemy in UnityEngine.Object.FindObjectsOfType<EnemyAgent>())
                {
                    if (enemy.AI.Mode == AgentMode.Agressive)
                    {
                        data = enemy.Sync.m_enemyStateData;
                        data.behaviourState = state;
                        data.agentMode = agentMode;
                        if (enemy.IsScout)
                        {
                            data.agentMode = AgentMode.Scout;
                        }
                        if (SNet.IsMaster)
                        {
                            enemy.Sync.IncomingState(data);
                        }
                        else
                        {
                            enemy.Sync.m_aiStatePacket.Send(data, SNet_ChannelType.GameReceiveCritical);
                        }
                    }
                }
                break;
            case EnemyChoiceType.All:
                foreach (var enemy in UnityEngine.Object.FindObjectsOfType<EnemyAgent>())
                {
                    data = enemy.Sync.m_enemyStateData;
                    data.behaviourState = state;
                    data.agentMode = agentMode;
                    if (enemy.IsScout)
                    {
                        data.agentMode = AgentMode.Scout;
                    }
                    if (SNet.IsMaster)
                    {
                        enemy.Sync.IncomingState(data);
                    }
                    else
                    {
                        enemy.Sync.m_aiStatePacket.Send(data, SNet_ChannelType.GameReceiveCritical);
                    }
                }
                break;
        }
    }

    [Command("Revive", "复活玩家")]
    private static void RevivePlayer([PlayerAgentSuggestorTag] PlayerAgent playerAgent)
    {
        AgentReplicatedActions.PlayerReviveAction(playerAgent, AdminUtils.LocalPlayerAgent, playerAgent.Position);
        ConsoleLogs.LogToConsole($"已复活玩家 {playerAgent.GetColoredNameWithoutRichTextTags()}");
    }

    [Command("ReviveAll", "复活所有玩家")]
    private static void ReviveAllPlayer()
    {
        foreach (var player in PlayerManager.PlayerAgentsInLevel)
        {
            AgentReplicatedActions.PlayerReviveAction(player, AdminUtils.LocalPlayerAgent, player.Position);
            ConsoleLogs.LogToConsole($"已复活玩家 {player.GetColoredNameWithoutRichTextTags()}");
        }
    }

    [Command("DropItem", "丢弃物品")]
    private static void DropItem()
    {
        InventorySlot wieldedSlot = AdminUtils.LocalPlayerAgent.Inventory.WieldedSlot;
        if ((wieldedSlot == InventorySlot.ResourcePack || wieldedSlot == InventorySlot.Consumable || wieldedSlot == InventorySlot.InLevelCarry) && PlayerBackpackManager.TryGetItemInLevelFromItemData(AdminUtils.LocalPlayerAgent.Inventory.WieldedItem.Get_pItemData(), out var item))
        {
            ItemInLevel itemInLevel = item.Cast<ItemInLevel>();
            if (AIG_CourseNode.TryGetCourseNode(AdminUtils.LocalPlayerAgent.DimensionIndex, AdminUtils.LocalPlayerAgent.Position, 0f, out AIG_CourseNode aig_CourseNode))
            {
                AmmoType ammoType = wieldedSlot == InventorySlot.ResourcePack ? AmmoType.ResourcePackRel : AmmoType.CurrentConsumable;
                float ammoInPack = PlayerBackpackManager.GetBackpack(SNet.LocalPlayer).AmmoStorage.GetAmmoInPack(ammoType);
                pItemData_Custom custom = itemInLevel.pItemData.custom;
                custom.ammo = ammoInPack;
                itemInLevel.GetSyncComponent().AttemptPickupInteraction(ePickupItemInteractionType.Place, SNet.LocalPlayer, custom, AdminUtils.LocalPlayerAgent.FPSCamera.CameraRayPos, AdminUtils.LocalPlayerAgent.Rotation, aig_CourseNode, true, true);
                ConsoleLogs.LogToConsole($"已将 {item.ArchetypeName} 丢弃");
                return;
            }
        }
        ConsoleLogs.LogToConsole("丢弃失败", LogLevel.Error);
    }

    [Command("DropItem", "丢弃物品")]
    private static void DropItem([PlayerAgentSuggestorTag] PlayerAgent playerAgent, InventorySlot itemSlot)
    {
        if (itemSlot != InventorySlot.ResourcePack && itemSlot != InventorySlot.Consumable && itemSlot != InventorySlot.InLevelCarry)
        {
            ConsoleLogs.LogToConsole($"非法物品栏位 {itemSlot}", LogLevel.Error);
            return;
        }
        if (!PlayerBackpackManager.TryGetBackpack(playerAgent.Owner, out var backpack) || !backpack.TryGetBackpackItem(itemSlot, out var backpackItem) || backpackItem.Instance == null)
        {
            ConsoleLogs.LogToConsole($"无法获取物品", LogLevel.Error);
            return;
        }
        if (PlayerBackpackManager.TryGetItemInLevelFromItemData(backpackItem.Instance.Get_pItemData(), out var item))
        {
            ItemInLevel itemInLevel = item.Cast<ItemInLevel>();
            var localPlayer = AdminUtils.LocalPlayerAgent;
            if (AIG_CourseNode.TryGetCourseNode(localPlayer.DimensionIndex, localPlayer.Position, 0f, out var node))
            {
                float ammoInPack = backpack.AmmoStorage.GetAmmoInPack(PlayerAmmoStorage.GetAmmoTypeFromSlot(itemSlot));
                pItemData_Custom custom = itemInLevel.pItemData.custom;
                custom.ammo = ammoInPack;
                itemInLevel.GetSyncComponent().AttemptPickupInteraction(ePickupItemInteractionType.Place, playerAgent.Owner, custom, localPlayer.FPSCamera.CameraRayPos, localPlayer.Rotation, node, true, true);
                ConsoleLogs.LogToConsole($"已将 {item.ArchetypeName} 丢弃");
                return;
            }
        }
        ConsoleLogs.LogToConsole("丢弃失败", LogLevel.Error);
    }

    [Command("PlayerGiveBirth", "玩家下崽子")]
    private static void PlayerGiveBirth([PlayerAgentSuggestorTag] PlayerAgent playerAgent, [EnemyDataBlockSuggestorTag] EnemyDataBlock block, int count = 1)
    {
        UnityMainThreadDispatcher.Enqueue(PlayerGiveBirthCoroutine(playerAgent, block.persistentID, count));
    }

    private static IEnumerator PlayerGiveBirthCoroutine(PlayerAgent playerAgent, uint id, int count)
    {
        var yielder = new WaitForSecondsRealtime(0.2f);
        if (playerAgent == null || playerAgent.IsBeingDespawned || playerAgent.Owner.IsBot)
        {
            yield break;
        }
        uint dropSound = count > 15 ? 2806903738U : 3495940948U;
        uint startSound = count > 15 ? 3461742098 : 1544195408U;
        ConsoleLogs.LogToConsole($"玩家 {playerAgent.GetColoredNameWithoutRichTextTags()} 开始生 {TranslateHelper.EnemyName(id)}, 数量: {count} 只");
        PlayerVoiceManager.WantToSayInternal(playerAgent.PlayerSlotIndex, startSound, 0U, 0U, 0U);
        yield return new WaitForSecondsRealtime(2.5f);
        while (count > 0)
        {
            if (!playerAgent.Owner.IsInLobby || CurrentGameState != (int)eGameStateName.InLevel)
            {
                yield break;
            }
            if (AIG_CourseNode.TryGetCourseNode(playerAgent.DimensionIndex, playerAgent.Position, 1f, out AIG_CourseNode node))
            {
                PlayerVoiceManager.WantToSayInternal(playerAgent.PlayerSlotIndex, dropSound, 0U, 0U, 0U);
                EnemyAgent.SpawnEnemy(id, playerAgent.Position, node, AgentMode.Agressive);
                count--;
            }
            yield return yielder;
        }
    }

    [Command("PlayerControl", "设置玩家控制")]
    private static void SetPlayerControl([PlayerAgentSuggestorTag] PlayerAgent playerAgent, bool enable)
    {
        playerAgent.RequestToggleControlsEnabled(enable);
        ConsoleLogs.LogToConsole($"已{(enable ? "允许" : "禁止")} {playerAgent.GetColoredNameWithoutRichTextTags()} 活动");
    }


    [Command("KickPlayer", "踢出玩家")]
    private static void KickPlayer([SNet_PlayerSuggestorTag] SNet_Player player)
    {
        if (!player.IsInSessionHub || player.IsBot)
        {
            ConsoleLogs.LogToConsole("输入有误", LogLevel.Error);
            return;
        }

        if (player.IsLocal)
        {
            SNet.SessionHub.LeaveHub();
            return;
        }
        if (SNet.IsMaster)
        {
            TheArchive.Features.Security.PlayerLobbyManagement.KickPlayer(player);
        }
        else
        {
            var sessionData = player.Session;
            sessionData.m_playerSlotIndex = byte.MaxValue;
            SNet.Sync.m_playerSessionPacket.Send(sessionData, SNet_ChannelType.SessionOrderCritical, SNet.Master);
        }
        ConsoleLogs.LogToConsole($"已踢出玩家 {player.GetColoredNameWithoutRichTextTags()}");
    }

    [Command("BanPlayer", "封禁玩家")]
    private static void BanPlayer([SNet_PlayerSuggestorTag] SNet_Player player)
    {
        if (player.IsBot)
        {
            ConsoleLogs.LogToConsole("输入有误", LogLevel.Error);
            return;
        }
        TheArchive.Features.Security.PlayerLobbyManagement.BanPlayer(player);
        ConsoleLogs.LogToConsole($"已封禁玩家 {player.GetColoredNameWithoutRichTextTags()}");
    }

    [Command("GiveHealth", "给予玩家生命值")]
    private static void GiveHealth([PlayerAgentSuggestorTag] PlayerAgent playerAgent = null, [CommandParameterDescription("数量")] float amount = 100f)
    {
        if (playerAgent == null)
        {
            foreach (var p in PlayerManager.PlayerAgentsInLevel)
            {
                p.GiveHealth(AdminUtils.LocalPlayerAgent, 1f);
                ConsoleLogs.LogToConsole($"{p.GetColoredNameWithoutRichTextTags()} 已补充生命值");
            }
            return;
        }
        amount = Math.Max(-100f, Math.Min(amount, 100f));
        playerAgent.GiveHealth(AdminUtils.LocalPlayerAgent, amount / 100f);
        ConsoleLogs.LogToConsole($"{playerAgent.GetColoredNameWithoutRichTextTags()} 生命值 {(amount >= 0f ? "增加" : "减少")} {Math.Abs(amount)}%");
    }

    [Command("GiveAmmo", "给予玩家武器弹药")]
    private static void GiveAmmo([PlayerAgentSuggestorTag] PlayerAgent playerAgent = null, [CommandParameterDescription("数量")] float amount = 100f)
    {
        if (playerAgent == null)
        {
            foreach (var p in PlayerManager.PlayerAgentsInLevel)
            {
                p.GiveAmmoRel(AdminUtils.LocalPlayerAgent, 1f, 1f, 0f);
                ConsoleLogs.LogToConsole($"{p.GetColoredNameWithoutRichTextTags()} 已补充武器弹药");
            }
            return;
        }
        amount = Math.Max(-100f, Math.Min(amount, 100f));
        playerAgent.GiveAmmoRel(AdminUtils.LocalPlayerAgent, amount / 100f, amount / 100f, 0f);
        ConsoleLogs.LogToConsole($"{playerAgent.GetColoredNameWithoutRichTextTags()} 武器弹药 {(amount >= 0f ? "增加" : "减少")} {Math.Abs(amount)}%");
    }

    [Command("GiveDisinfection", "给予玩家消毒")]
    private static void GiveDisinfection([PlayerAgentSuggestorTag] PlayerAgent playerAgent = null, [CommandParameterDescription("数量")] float amount = 100f)
    {
        if (playerAgent == null)
        {
            foreach (var p in PlayerManager.PlayerAgentsInLevel)
            {
                if (p.Damage.Infection > 0f)
                    p.GiveDisinfection(AdminUtils.LocalPlayerAgent, 1f);

                ConsoleLogs.LogToConsole($"{p.GetColoredNameWithoutRichTextTags()} 已完全消毒");
            }
            return;
        }
        amount = Math.Max(-100f, Math.Min(amount, 100f));
        playerAgent.GiveDisinfection(AdminUtils.LocalPlayerAgent, amount / 100f);
        ConsoleLogs.LogToConsole($"{playerAgent.GetColoredNameWithoutRichTextTags()} 感染值 {(amount < 0f ? "增加" : "减少")} {Math.Abs(amount)}%");
    }

    [Command("GiveTool", "给予玩家工具弹药")]
    private static void GiveTool([PlayerAgentSuggestorTag] PlayerAgent playerAgent = null, [CommandParameterDescription("数量")] float amount = 100f)
    {
        if (playerAgent == null)
        {
            foreach (var p in PlayerManager.PlayerAgentsInLevel)
            {
                p.GiveAmmoRel(AdminUtils.LocalPlayerAgent, 0f, 0f, 1f);

                foreach (var sg in GameObject.FindObjectsOfType<SentryGunInstance>())
                {
                    if (sg.Owner?.GlobalID == p.GlobalID)
                    {
                        sg.GiveAmmoRel(AdminUtils.LocalPlayerAgent, 1f, 1f, 1f);
                    }
                }
                ConsoleLogs.LogToConsole($"{p.GetColoredNameWithoutRichTextTags()} 已补充工具弹药");
            }
            return;
        }
        amount = Math.Max(-100f, Math.Min(amount, 100f));
        playerAgent.GiveAmmoRel(AdminUtils.LocalPlayerAgent, 0f, 0f, amount / 100f);

        var sentryGuns = GameObject.FindObjectsOfType<SentryGunInstance>();
        foreach (var sg in sentryGuns)
        {
            if (sg.Owner?.GlobalID == playerAgent.GlobalID)
            {
                sg.GiveAmmoRel(AdminUtils.LocalPlayerAgent, 0f, 0f, amount / 100f);
            }
        }
        ConsoleLogs.LogToConsole($"{playerAgent.GetColoredNameWithoutRichTextTags()} 工具弹药 {(amount >= 0f ? "增加" : "减少")} {Math.Abs(amount)}%");
    }

    [Command("GiveResources", "给予玩家资源")]
    private static void GiveResources([PlayerAgentSuggestorTag] PlayerAgent playerAgent = null)
    {
        if (playerAgent == null)
        {
            foreach (var p in PlayerManager.PlayerAgentsInLevel)
            {
                p.GiveAmmoRel(AdminUtils.LocalPlayerAgent, 1f, 1f, 1f);
                p.GiveHealth(AdminUtils.LocalPlayerAgent, 1f);
                if (p.Damage.Infection > 0f)
                    p.GiveDisinfection(AdminUtils.LocalPlayerAgent, 1f);

                foreach (var sg in GameObject.FindObjectsOfType<SentryGunInstance>())
                {
                    if (sg.Owner?.GlobalID == p.GlobalID)
                    {
                        sg.GiveAmmoRel(AdminUtils.LocalPlayerAgent, 1f, 1f, 1f);
                    }
                }
                ConsoleLogs.LogToConsole($"{p.GetColoredNameWithoutRichTextTags()} 已补充资源");
            }
            return;
        }
        playerAgent.GiveAmmoRel(AdminUtils.LocalPlayerAgent, 1f, 1f, 1f);
        playerAgent.GiveHealth(AdminUtils.LocalPlayerAgent, 1f);
        if (playerAgent.Damage.Infection > 0f)
            playerAgent.GiveDisinfection(AdminUtils.LocalPlayerAgent, 1f);

        var sentryGuns = GameObject.FindObjectsOfType<SentryGunInstance>();
        foreach (var sg in sentryGuns)
        {
            if (sg.Owner?.GlobalID == playerAgent.GlobalID)
            {
                sg.GiveAmmoRel(AdminUtils.LocalPlayerAgent, 1f, 1f, 1f);
            }
        }
        ConsoleLogs.LogToConsole($"{playerAgent.GetColoredNameWithoutRichTextTags()} 已补充资源");
    }

    [Command("LightsSynced", "设置同步灯光")]
    private static void SetLightsEnabledSync(bool enable)
    {
        if (CurrentGameState != (int)eGameStateName.InLevel)
        {
            return;
        }
        EnvironmentStateManager.AttemptSetExpeditionLightMode(enable);
        ConsoleLogs.LogToConsole($"已{(enable ? "启用" : "禁用")} 同步灯光");
    }

    [Command("StopAlarms", "停止所有警报")]
    private static void StopAllAlarms()
    {
        if (!SNet.IsMaster)
        {
            ConsoleLogs.LogToConsole("只有房主才可以关闭所有警报", LogLevel.Error);
            return;
        }
        WardenObjectiveManager.StopAlarms();
        ConsoleLogs.LogToConsole("已关闭所有警报");
    }

    [Command("StopEnemyWaves", "停止当前所有刷怪")]
    private static void StopAllEnemyWaves()
    {
        if (!SNet.IsMaster)
        {
            ConsoleLogs.LogToConsole("只有房主可以停止所有刷怪进程", LogLevel.Error);
            return;
        }
        WardenObjectiveManager.StopAllWardenObjectiveEnemyWaves();
        ConsoleLogs.LogToConsole("已停止所有进行中的刷怪进程");
    }


    [Command("FogTransition", "改变雾气")]
    private static void StartFogTransition([FogSettingsDataBlockSuggestorTag] FogSettingsDataBlock fogDataBlock, eDimensionIndex dimensionIndex, int duration = 1)
    {
        EnvironmentStateManager.AttemptStartFogTransition(fogDataBlock.persistentID, duration, dimensionIndex);
        ConsoleLogs.LogToConsole($"开始变更雾气, 象限: {dimensionIndex}, ID: {fogDataBlock.persistentID}, 过渡时长 {duration} 秒");
    }

    [Command("FinishWardenObjectiveChain", "完成任务")]
    private static void ForceFinishWardenObjectiveChain(LG_LayerType layer)
    {
        if (CurrentGameState != (int)eGameStateName.InLevel)
        {
            ConsoleLogs.LogToConsole("不在游戏中", LogLevel.Error);
            return;
        }

        string name = string.Empty;
        if (layer == LG_LayerType.MainLayer)
        {
            name = "主要";
        }
        else if (layer == LG_LayerType.SecondaryLayer)
        {
            name = "次要";
        }
        else if (layer == LG_LayerType.ThirdLayer)
        {
            name = "附加";
        }
        if (!WardenObjectiveManager.TryGetLastWardenObjectiveDataForLayer(layer, out _))
        {
            ConsoleLogs.LogToConsole($"不存在{name}任务目标", LogLevel.Error);
            return;
        }
        WardenObjectiveManager.ForceCompleteObjectiveAll(layer);
        ConsoleLogs.LogToConsole($"已完成{name}任务目标"); ;
    }

    [Command("FinishAllWardenObjectiveChain")]
    private static void ForceFinishAllWardenObjectiveChain()
    {
        if (CurrentGameState != (int)eGameStateName.InLevel)
        {
            ConsoleLogs.LogToConsole("不在游戏中", LogLevel.Error);
            return;
        }

        foreach (LG_LayerType layer in Enum.GetValues(typeof(LG_LayerType)))
        {
            string name = string.Empty;
            if (layer == LG_LayerType.MainLayer)
            {
                name = "主要";
            }
            else if (layer == LG_LayerType.SecondaryLayer)
            {
                name = "次要";
            }
            else if (layer == LG_LayerType.ThirdLayer)
            {
                name = "附加";
            }
            if (WardenObjectiveManager.TryGetLastWardenObjectiveDataForLayer(layer, out _))
            {
                WardenObjectiveManager.ForceCompleteObjectiveAll(layer);
                ConsoleLogs.LogToConsole($"已完成{name}任务目标");
            }
        }
    }

    [Command("PauseGame", "切换游戏暂停")]
    private static bool PauseGame
    {
        get
        {
            return GameEventAPI.IsGamePaused;
        }
        set
        {
            if (CurrentGameState != (int)eGameStateName.InLevel)
            {
                ConsoleLogs.LogToConsole("不在游戏中", LogLevel.Error);
                return;
            }
            if (!SNet.IsMaster)
            {
                ConsoleLogs.LogToConsole("主机才能暂停游戏", LogLevel.Error);
                return;
            }
            GameEventAPI.IsGamePaused = !GameEventAPI.IsGamePaused;
            ConsoleLogs.LogToConsole($"已{(GameEventAPI.IsGamePaused ? "暂停" : "继续")}游戏");
        }
    }

    [Command("FastElevator")]
    private static bool FastElevator { get => Global.FastElevator; set => Global.FastElevator = value; }

    [Command("ReactorCodes")]
    private static void ShowReactorCodes([LG_WardenObjective_ReactorSuggestorTag] LG_WardenObjective_Reactor reactor, bool all = false)
    {
        var codes = reactor.GetOverrideCodes();
        StringBuilder sb = new(200);
        sb.AppendLine($"<color=orange>REACTOR_{reactor.m_serialNumber} 验证秘钥：</color>");
        for (int i = 0; i < (all ? codes.Count : reactor.m_waveCountMax); i++)
        {
            sb.AppendLine($"{i + 1}. {codes[i]}");
        }
        ConsoleLogs.LogToConsole(sb.ToString());
    }

    [Command("ReactorInteraction")]
    private static void ReactorInteraction([LG_WardenObjective_ReactorSuggestorTag] LG_WardenObjective_Reactor reactor, eReactorInteraction interaction, float progression = 0f)
    {
        reactor.AttemptInteract(interaction, progression);
        ConsoleLogs.LogToConsole($"REACTOR_{reactor.m_serialNumber}: {interaction}");
    }
}