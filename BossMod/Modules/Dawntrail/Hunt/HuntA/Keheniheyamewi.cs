﻿namespace BossMod.Dawntrail.Hunt.RankA.Keheniheyamewi;

public enum OID : uint
{
    Boss = 0x43DC, // R8.500, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Scatterscourge1 = 39807, // Boss->self, 4.0s cast, range 10-40 donut
    BodyPress = 40063, // Boss->self, 4.0s cast, range 15 circle
    SlipperyScatterscourge = 38648, // Boss->self, 5.0s cast, range 20 width 10 rect
    WildCharge = 39559, // Boss->self, no cast, range 20 width 10 rect
    Scatterscourge2 = 38650, // Boss->self, 1.5s cast, range 10-40 donut
    PoisonGas = 38652, // Boss->self, 5.0s cast, range 60 circle
    BodyPress2 = 38651, // Boss->self, 4.0s cast, range 15 circle
    MalignantMucus = 38653, // Boss->self, 5.0s cast, single-target
    PoisonMucus = 38654, // Boss->location, 1.0s cast, range 6 circle
}

public enum SID : uint
{
    RightFace = 2164,
    LeftFace = 2163,
    ForwardMarch = 2161,
    AboutFace = 2162
}

class BodyPress(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BodyPress), new AOEShapeCircle(15));

class BodyPress2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BodyPress2), new AOEShapeCircle(15));

class Scatterscourge1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scatterscourge1), new AOEShapeDonut(10, 40));

class SlipperyScatterscourge : Components.GenericAOEs
{
    private Actor? _caster;
    private List<AOEInstance> _activeAOEs = new();
    private static readonly AOEShapeRect _shapeRect = new(20, 6); // Manual adjustments due to weirdness.
    private static readonly AOEShapeDonut _shapeDonut = new(8, 40); // Manual adjustments due to weirdness.
    private static readonly AOEShapeCircle _shapeCircle = new(8); // Manual adjustments due to weirdness.

    public SlipperyScatterscourge(BossModule module) : base(module) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster == null)
            yield break;

        foreach (var aoe in _activeAOEs)
        {
            yield return aoe;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.SlipperyScatterscourge)
            return;

        _caster = caster;
        _activeAOEs.Add(new AOEInstance(_shapeRect, _caster.Position, _caster.Rotation, WorldState.CurrentTime.AddSeconds(10), Colors.Danger, true));

        WPos rectEndPosition = GetRectEndPosition(_caster.Position, _caster.Rotation, _shapeRect.LengthFront);

        _activeAOEs.Add(new AOEInstance(_shapeDonut, rectEndPosition, default, WorldState.CurrentTime.AddSeconds(10), Colors.AOE, true));
        _activeAOEs.Add(new AOEInstance(_shapeCircle, rectEndPosition, default, WorldState.CurrentTime.AddSeconds(10), Colors.SafeFromAOE, false));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SlipperyScatterscourge)
        {
            _activeAOEs.RemoveAll(aoe => aoe.Shape == _shapeRect);
            var index = _activeAOEs.FindIndex(aoe => aoe.Shape == _shapeDonut);
            if (index != -1)
            {
                _activeAOEs[index] = new AOEInstance(_shapeDonut, _activeAOEs[index].Origin, _activeAOEs[index].Rotation, WorldState.CurrentTime.AddSeconds(10), Colors.Danger, true);
                var circleIndex = _activeAOEs.FindIndex(aoe => aoe.Shape == _shapeCircle);
                if (circleIndex != -1)
                {
                    _activeAOEs[circleIndex] = new AOEInstance(_shapeCircle, _activeAOEs[circleIndex].Origin, _activeAOEs[circleIndex].Rotation, WorldState.CurrentTime.AddSeconds(10), Colors.SafeFromAOE, false);
                }
            }
        }
        else if (spell.Action.ID == (uint)AID.Scatterscourge2)
        {
            _activeAOEs.RemoveAll(aoe => aoe.Shape == _shapeDonut || aoe.Shape == _shapeCircle);
            _caster = null;
        }
    }

    private WPos GetRectEndPosition(WPos origin, Angle rotation, float lengthFront)
    {
        WDir direction = rotation.ToDirection();
        float offsetX = direction.X * lengthFront;
        float offsetZ = direction.Z * lengthFront;
        return new WPos(origin.X + offsetX, origin.Z + offsetZ);
    }
}

class PoisonGas(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PoisonGas), new AOEShapeCircle(60));

class PoisonGasMarch : Components.StatusDrivenForcedMarch // TODO: AI still doesn't seem to always get the correct safe spot on this :(
{

    public PoisonGasMarch(BossModule module)
        : base(module, 13, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace) { }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        return Module.FindComponent<SlipperyScatterscourge>()?.ActiveAOEs(slot, actor).Any(a => (a.Color != Colors.SafeFromAOE) && a.Shape.Check(pos, a.Origin, a.Rotation)) ?? false;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.PoisonGas) ?? false)
            hints.Add("Forced March! Check debuff and aim towards the end of the jump!");
    }
}

class MalignantMucus(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.MalignantMucus));

class PoisonMucus(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PoisonMucus), 6);

class KeheniheyamewiStates : StateMachineBuilder
{
    public KeheniheyamewiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BodyPress>()
            .ActivateOnEnter<BodyPress2>()
            .ActivateOnEnter<Scatterscourge1>()
            .ActivateOnEnter<SlipperyScatterscourge>()
            .ActivateOnEnter<PoisonGas>()
            .ActivateOnEnter<PoisonGasMarch>()
            .ActivateOnEnter<MalignantMucus>()
            .ActivateOnEnter<PoisonMucus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13401)]
public class Keheniheyamewi(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
