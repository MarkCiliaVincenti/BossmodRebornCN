﻿namespace BossMod.Dawntrail.Raid.M4NWickedThunder;

class WickedJolt(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.WickedJolt), new AOEShapeRect(60, 2.5f), endsOnCastEvent: true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class WickedBolt(BossModule module) : Components.StackWithIcon(module, (uint)IconID.WickedBolt, ActionID.MakeSpell(AID.WickedBolt), 5, 5, 8, 8, 5);
class SoaringSoulpress(BossModule module) : Components.StackWithIcon(module, (uint)IconID.SoaringSoulpress, ActionID.MakeSpell(AID.SoaringSoulpress), 6, 5.4f, 8, 8);
class WrathOfZeus(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WrathOfZeus));
class Burst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Burst), new AOEShapeRect(40, 8));
class BewitchingFlight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BewitchingFlight), new AOEShapeRect(40, 2.5f));
class Thunderslam(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Thunderslam), 5);
class Thunderstorm(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Thunderstorm), 6);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 991, NameID = 13057)]
public class M4NWickedThunder(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.DefaultCenter, ArenaChanges.DefaultBounds);
