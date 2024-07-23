namespace BossMod.Endwalker.Alliance.A34Eulogia;

class LovesLight(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeRect _shape = new(80, 12.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (AOEs.Count > 0)
            yield return AOEs[0] with { Color = ArenaColor.Danger };
        if (AOEs.Count > 1)
            yield return AOEs[1];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FirstBlush1 or AID.FirstBlush2 or AID.FirstBlush3 or AID.FirstBlush4)
        {
            AOEs.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            AOEs.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FirstBlush1 or AID.FirstBlush2 or AID.FirstBlush3 or AID.FirstBlush4)
        {
            ++NumCasts;
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
        }
    }
}
