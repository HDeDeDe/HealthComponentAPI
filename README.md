# HealthComponentAPI

![icon](https://github.com/HDeDeDe/HealthComponentAPI/blob/main/Resources/icon.png?raw=true)

is api for healthcomponent. designed to be like RecalculateStatsAPI.

### How it works
There are 2 Events you can subscribe to, `HDeMods.HealthComponentAPI.GetHealthStats` and `HDeMods.HealthComponentAPI.GetHealStats`.

`GetHealthStats` fires every fixed update on the server and provides the following values to change:
- TOTAL Regen: Regen value after all regen related calculations but before it's applied as healing.
- Barrier Decay Rate: The amount of time it takes for barrier to decay. This is exposed in CharacterBody.RecalculateStats but RecalculateStatsAPI doesn't provide any bindings for it.
- Shield Recharge Rate: The amount of time it takes for shields to recharge once you are out of danger.
- Adaptive Armor Decay Rate: The amount of time it takes for adaptive armor stacks to decay.
- Instated like this: `MyDelegate(HealthComponent sender, UpdateHealthEventArgs args)`


`GetHealStats` runs every time HealthComponent.Heal and HealthComponent.TakeDamageProcess are ran and provides the following values to change:
- Enable Eclipse Heal Reduction: Cuts healing in half as if you were on Eclipse 5 or higher. Does not count the run as Eclipse 5 or higher on its own. Does not apply if the character is not on the player team.
- Eclipse Heal Reduction Ignore Team: Remove the team restriction from the Eclipse 5 heal reduction.
- Damage Coyote Timer: The grace period in between damage frames. This is the only value HealthComponent.TakeDamageProcess reads.
- Crit Heal: Unused in the base game, the multiplier for critical heals.
- TOTAL Heal Amount: The final healing value after all healing calculations but before it's applied.
- Instated like this: `MyDelegate(HealthComponent sender, HealEventArgs args)`

That's it.

## Plans for the future
- Might add an actual OnHeal event that lets you modify the value before any other values apply to it, who knows.

## Feedback
Any and all feedback is appreciated, if you want to let me know anything please feel free to open an issue on the [GitHub Page](https://github.com/HDeDeDe/HealthComponentAPI) or @ me on the modding discord (hdedede).