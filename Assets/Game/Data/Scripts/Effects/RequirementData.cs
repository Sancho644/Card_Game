using System;
using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.Effects;

namespace Game.Data.Effects
{
    [Serializable, DataLabel("Если ты хозяин или гость")]
    public sealed class BattleRoleRequirementData : RequirementData
    {
        public BattleRole Role = BattleRole.Host;

        public override IEffectRequirement Create() => new BattleRoleRequirement(Role);
    }

    [Serializable, DataLabel("Если карта поля открыта или закрыта")]
    public sealed class FieldStateRequirementData : RequirementData
    {
        public bool MustBeOpen;

        public override IEffectRequirement Create() => new FieldStateRequirement(MustBeOpen);
    }

    [Serializable, DataLabel("Если твоё существо этой стихии")]
    public sealed class CreatureElementRequirementData : RequirementData
    {
        public Element Element = Element.Fire;

        public override IEffectRequirement Create() => new CreatureElementRequirement(Element);
    }

    [Serializable, DataLabel("Если карта открыта и даёт твоему существу бонус")]
    public sealed class FieldGivesBonusRequirementData : RequirementData
    {
        public override IEffectRequirement Create() => new FieldGivesBonusRequirement();
    }

    [Serializable, DataLabel("Если твоё существо слабее")]
    public sealed class WeakerThanOpponentRequirementData : RequirementData
    {
        public override IEffectRequirement Create() => new WeakerThanOpponentRequirement();
    }
}
