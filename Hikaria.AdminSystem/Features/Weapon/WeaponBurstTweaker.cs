using GameData;
using Gear;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Loader;
using UnityEngine;

namespace Hikaria.AdminSystem.Features.Weapon;

[EnableFeatureByDefault]
public class WeaponBurstTweaker : Feature
{
    public override string Name => "枪械连发调整";

    public override GroupBase Group => ModuleGroup.GetOrCreateSubGroup("Weapon");

    public override void Init()
    {
        LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<BWA_Burst_T>();
    }

    [ArchivePatch(typeof(BulletWeapon), nameof(BulletWeapon.SetupArchetype))]
    private static class BulletWeapon__SetupArchetype__Patch
    {
        private static bool Prefix(BulletWeapon __instance)
        {
            if (__instance.ArchetypeData == null)
                return true;
            if (__instance.ArchetypeData.FireMode != eWeaponFireMode.Burst)
                return true;
            __instance.m_archeType = new BWA_Burst_T(__instance.ArchetypeData);
            __instance.m_archeType.Setup(__instance);
            if (__instance.Owner != null)
                __instance.m_archeType.SetOwner(__instance.Owner);
            return false;
        }
    }

    private class BWA_Burst_T : BWA_Burst
    {
        public BWA_Burst_T(IntPtr ptr) : base(ptr)
        {
        }

        public BWA_Burst_T(ArchetypeDataBlock data) : base(LoaderWrapper.ClassInjector.DerivedConstructorPointer<BWA_Burst_T>())
        {
            LoaderWrapper.ClassInjector.DerivedConstructorBody(this);

            m_archetypeData = data;
            m_recoilData = GameDataBlockBase<RecoilDataBlock>.GetBlock(data.RecoilDataID);
        }

        public override void Update()
        {
            if (m_owner == null)
                return;

            bool flag = !m_owner.Locomotion.IsRunning && !m_owner.Locomotion.IsInAir;
            m_fireHeld = flag && (m_weapon.FireButton || (InputMapper.HasGamepad && InputMapper.GetAxisKeyMouseGamepad(InputAction.FreeflightSpeedDouble, m_owner.InputFilter) > 0f));
            if (flag && m_weapon.FireButtonPressed)
            {
                m_firePressed = true;
            }
            else if (m_fireHeld && m_firePressed)
            {
                m_firePressed = false;
            }
            if (m_weapon.IsEnabled || (m_owner.FPItemHolder.ItemDownTrigger && m_archetypeData.FireMode == eWeaponFireMode.Burst && !BurstIsDone()))
            {
                m_clip = m_weapon.GetCurrentClip();
                bool flag2 = (((HasChargeup && m_inChargeup) || !m_triggerNeedsPress) ? m_fireHeld : m_firePressed);
                if (!m_inChargeup && !m_firing && flag2 && Clock.Time > m_nextBurstTimer)
                {
                    if (m_clip > 0f)
                    {
                        if (HasChargeup)
                        {
                            m_chargeupTimer = Clock.Time + ChargeupDelay();
                            m_inChargeup = true;
                            m_readyToFire = false;
                            m_weapon.TriggerAudioChargeup();
                            m_weapon.FPItemHolder.DontRelax();
                            GuiManager.CrosshairLayer.SetChargeUpVisibleAndProgress(true, 0f);
                        }
                        else
                        {
                            m_chargeupTimer = 0f;
                            m_inChargeup = false;
                            GuiManager.CrosshairLayer.SetChargeUpVisibleAndProgress(false, 0f);
                            m_readyToFire = true;
                        }
                    }
                    else if (m_firePressed)
                    {
                        if (!m_clickTriggered || !CellSettingsManager.SettingsData.Gameplay.AutoReload.Value || !m_weapon.m_inventory.CanReloadCurrent())
                        {
                            m_weapon.TriggerAudio(m_weapon.AudioData.eventClick);
                            m_nextShotTimer = Clock.Time + ShotDelay();
                            m_clickTriggered = true;
                        }
                        else if (m_clickTriggered && CellSettingsManager.SettingsData.Gameplay.AutoReload.Value && m_weapon.m_inventory.CanReloadCurrent())
                        {
                            m_weapon.m_inventory.TriggerReload();
                            m_clickTriggered = false;
                        }
                        if (m_clip <= 0f && !m_weapon.m_inventory.CanReloadCurrent())
                        {
                            m_weapon.m_wasOutOfAmmo = true;
                        }
                    }
                    if (m_firePressed)
                    {
                        m_firePressed = false;
                    }
                }
                if (m_inChargeup)
                {
                    if (!m_fireHeld)
                    {
                        int num = Mathf.FloorToInt(Mathf.Clamp01(1f - (m_chargeupTimer - Clock.Time) / ChargeupDelay()) * (float)m_burstMax);
                        if (num == 0 || num == m_burstMax)
                        {
                            OnStopChargeup();
                            m_nextShotTimer = Clock.Time + ShotDelay();
                            return;
                        }
                        m_inChargeup = false;
                        m_readyToFire = true;
                        GuiManager.CrosshairLayer.SetChargeUpVisibleAndProgress(false, 0f);
                        OnStartFiring(num);
                        goto IL_03C6;
                    }
                    else
                    {
                        float num2 = 1f - (m_chargeupTimer - Clock.Time) / ChargeupDelay();
                        GuiManager.CrosshairLayer.SetChargeUpVisibleAndProgress(true, num2);
                        if (Clock.Time >= m_chargeupTimer)
                        {
                            m_inChargeup = false;
                            m_readyToFire = true;
                            GuiManager.CrosshairLayer.SetChargeUpVisibleAndProgress(false, 0f);
                        }
                    }
                }
                if (m_readyToFire && !m_firing)
                {
                    OnStartFiring();
                }
            IL_03C6:
                if (PreFireCheck())
                {
                    if (m_readyToFire && m_firing && Clock.Time > m_nextShotTimer)
                    {
                        if (m_clip > 0f)
                        {
                            OnFireShot();
                            m_nextShotTimer = Clock.Time + ShotDelay();
                        }
                        else
                        {
                            OnStopFiring();
                            OnFireShotEmptyClip();
                            m_nextShotTimer = Clock.Time + ShotDelay();
                        }
                        PostFireCheck();
                        return;
                    }
                }
                else if (m_firing)
                {
                    OnStopFiring();
                    return;
                }
            }
            else
            {
                if (m_inChargeup)
                {
                    OnStopChargeup();
                }
                if (m_firing)
                {
                    OnStopFiring();
                }
            }
        }

        private void OnStartFiring(int count)
        {
            m_firing = true;
            m_weapon.OnStartFiring();
            int currentClip = m_weapon.GetCurrentClip();
            m_burstCurrentCount = Mathf.Min(m_burstMax, Mathf.Min(count, currentClip));
            if (currentClip >= m_burstMax && !m_weapon.AudioData.TriggerBurstAudioForEachShot)
            {
                m_weapon.TriggerBurstFireAudio();
                m_weapon.FPItemHolder.DontRelax();
            }
        }
    }
}
