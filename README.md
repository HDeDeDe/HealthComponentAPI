# HealthComponentAPI

![icon](https://github.com/HDeDeDe/HealthComponentAPI/blob/main/Resources/icon.png?raw=true)

is api for healthcomponent. designed to be like RecalculateStatsAPI.

### How it works
There are 3 Events you can subscribe to, `HDeMods.HealthComponentAPI.GetHealthStats`, `HDeMods.HealthComponentAPI.GetHealStats` and `HDeMods.HealthComponentAPI.GetTakeDamageStats`.

`GetHealthStats` fires every fixed update on the server and provides the following values to change:
- Final Regen: Regen value after all regen related calculations but before it's applied as healing.
- Barrier Decay Rate: The amount of time it takes for barrier to decay. This is exposed in CharacterBody.RecalculateStats but RecalculateStatsAPI doesn't provide any bindings for it.
- Shield Recharge Rate: The amount of time it takes for shields to recharge once you are out of danger.
- Adaptive Armor Decay Rate: The amount of time it takes for adaptive armor stacks to decay.
- Instated like this:
- `MyDelegate(HealthComponent sender, UpdateHealthEventArgs args)`


`GetHealStats` runs every time HealthComponent.Heal and HealthComponent.TakeDamageProcess are ran and provides the following values to change:
- Enable Eclipse Heal Reduction: Cuts healing in half as if you were on Eclipse 5 or higher. Does not count the run as Eclipse 5 or higher on its own. Applies regardless of team.
- Damage Coyote Timer: The grace period in between damage frames. This value is read by HealthComponent.TakeDamageProcess.
- Crit Heal: Unused in the base game, the multiplier for critical heals.
- Final Heal Amount: The final healing value after all healing calculations but before it's applied.
- Instated like this: 
- `MyDelegate(HealthComponent sender, HealEventArgs args)`

`GetTakeDamageStats` Is not intended to be a replacement for `IOnIncomingDamageServerReceiver`, as its scope is much greater than IOnIncomingDamageServerReceiver. Due to this, `DamageInfo` is readonly. That being said, `GetTakeDamageStats` runs every time HealthComponent.TakeDamageProcess is ran and provides the following values to change:
- Reject Damage: Forces damage to be rejected. This should only be used if `IOnIncomingDamageServerReceiver` is not practical for your use case.
- Reject Force: Prevent damage forces from being applied. Note that this sets `DamageInfo.canRejectForce` to false.
- Damage Force: The push force attacks apply. The multiplier is provided as a float while the flat addition is provided as a Vector3
- Adaptive Armor Build Rate: The rate at which adaptive armor is built up.
- Adaptive Armor Max: The maximum amount of Adaptive Armor that can be built.
- Final Damage Amount: The final damage number after all item calculations (including things like warped echo), but before it is applied.
- Instated like this:
- `MyDelegate(HealthComponent sender, DamageInfo damageInfo, TakeDamageArgs args)`

That's it.

## Plans for the future
- Might add an actual OnHeal event that lets you modify the value before any other values apply to it, who knows.

## Feedback
Any and all feedback is appreciated, if you want to let me know anything please feel free to open an issue on the [GitHub Page](https://github.com/HDeDeDe/HealthComponentAPI) or @ me on the modding discord (hdedede).