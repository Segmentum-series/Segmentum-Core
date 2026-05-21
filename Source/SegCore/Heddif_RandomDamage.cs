using Verse;

#nullable disable
namespace seg
{
public class HediffCompProperties_RandomDamage : HediffCompProperties
{
  public int intervalTicks;
  public FloatRange amount;
  public DamageDef type;

  public HediffCompProperties_RandomDamage() => this.compClass = typeof (HediffComp_RandomDamage);
}

public class HediffComp_RandomDamage : HediffComp
{
  private int ticks;

  public HediffCompProperties_RandomDamage Props => this.props as HediffCompProperties_RandomDamage;

  public override void CompPostTick(ref float severityAdjustment)
  {
    base.CompPostTick(ref severityAdjustment);
    ++this.ticks;
    if (this.ticks < this.Props.intervalTicks)
      return;
    this.ticks = 0;
    this.Pawn.TakeDamage(new DamageInfo(this.Props.type, this.Props.amount.RandomInRange, 1f));
  }
}
}


