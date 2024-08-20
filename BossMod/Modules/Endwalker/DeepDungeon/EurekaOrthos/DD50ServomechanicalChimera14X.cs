namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD50ServomechanicalChimera14X;

public enum OID : uint
{
    Boss = 0x3D9C, // R6.0
    Cacophony = 0x3D9D, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    Attack = 6499, // Boss->player, no cast, single-target
    SongsOfIceAndThunder = 31851, // Boss->self, 5.0s cast, range 9 circle
    SongsOfThunderAndIce = 31852, // Boss->self, 5.0s cast, range 8-40 donut
    TheRamsVoice1 = 31854, // Boss->self, no cast, range 9 circle
    TheRamsVoice2 = 32807, // Boss->self, no cast, range 9 circle
    TheDragonsVoice1 = 31853, // Boss->self, no cast, range 8-40 donut
    TheDragonsVoice2 = 32806, // Boss->self, no cast, range 8-40 donut
    RightbreathedCold = 31863, // Boss->self, 5.0s cast, range 40 180-degree cone
    LeftbreathedThunder = 31861, // Boss->self, 5.0s cast, range 40 180-degree cone
    ColdThunder = 31855, // Boss->player, 5.0s cast, width 8 rect charge
    ThunderousCold = 31856, // Boss->player, 5.0s cast, width 8 rect charge
    Cacophony = 31864, // Boss->self, 3.0s cast, single-target
    ChaoticChorus = 31865, // Cacophony->self, no cast, range 6 circle
}

class RamsDragonVoice(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(8, 40);
    private static readonly AOEShapeCircle circle = new(9);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly HashSet<AID> castEnd = [AID.TheRamsVoice1, AID.TheRamsVoice2, AID.TheDragonsVoice1,
    AID.TheDragonsVoice2, AID.SongsOfIceAndThunder, AID.SongsOfThunderAndIce];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        if (_aoes.Count > 1)
            yield return _aoes[1] with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SongsOfIceAndThunder:
                AddAOEs(circle, donut, spell);
                break;
            case AID.SongsOfThunderAndIce:
                AddAOEs(donut, circle, spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ColdThunder:
                AddAOEs(circle, donut, spell);
                break;
            case AID.ThunderousCold:
                AddAOEs(donut, circle, spell);
                break;
        }
    }

    public override void Update()
    {
        if (_aoes.Count > 0)
            for (var i = 0; i < _aoes.Count; i++)
                _aoes[i] = new(_aoes[i].Shape, Module.PrimaryActor.Position, default, _aoes[i].Activation);
    }

    private void AddAOEs(AOEShape first, AOEShape second, ActorCastInfo spell)
    {
        var position = Module.PrimaryActor.Position;
        _aoes.Add(new(first, position, default, Module.CastFinishAt(spell)));
        _aoes.Add(new(second, position, default, Module.CastFinishAt(spell, 3)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && castEnd.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }
}

class ThunderousCold(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.ThunderousCold), 4);
class ColdThunder(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.ColdThunder), 4);
class RightbreathedCold(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightbreathedCold), new AOEShapeCone(40, 90.Degrees()));
class LeftbreathedThunder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftbreathedThunder), new AOEShapeCone(40, 90.Degrees()));
class Tether(BossModule module) : Components.StretchTetherDuo(module, 15, 5.1f);

class DD50ServomechanicalChimera14XStates : StateMachineBuilder
{
    public DD50ServomechanicalChimera14XStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RamsDragonVoice>()
            .ActivateOnEnter<ThunderousCold>()
            .ActivateOnEnter<ColdThunder>()
            .ActivateOnEnter<RightbreathedCold>()
            .ActivateOnEnter<LeftbreathedThunder>()
            .ActivateOnEnter<Tether>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 901, NameID = 12265)]
public class DD50ServomechanicalChimera14X(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(19.5f));