﻿using HarmonyLib;
using APIPlugin;
using DiskCardGame;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Artwork = voidSigils.Voids_work.Resources.Resources;

namespace voidSigils
{
	public partial class Plugin
	{
		//Inspired by Nevernamed, coded differently.
		private NewAbility AddSubmergedAmbush()
		{
			// setup ability
			const string rulebookName = "Submerged Ambush";
			const string rulebookDescription = "[creature] will deal 1 damage to cards that attacked over it while it was face-down.";
			const string LearnDialogue = "It strikes from the water.";
			// const string TextureFile = "Artwork/void_pathetic.png";

			AbilityInfo info = SigilUtils.CreateInfoWithDefaultSettings(rulebookName, rulebookDescription, LearnDialogue,  true, 4);
			info.canStack = false;
			info.pixelIcon = SigilUtils.LoadSpriteFromResource(Artwork.void_SubmergedAmbush_a2);

			Texture2D tex = SigilUtils.LoadTextureFromResource(Artwork.void_SubmergedAmbush);

			var abIds = SigilUtils.GetAbilityId(info.rulebookName);
			
			NewAbility newAbility = new NewAbility(info, typeof(void_SubmergedAmbush), tex, abIds);

			// set ability to behaviour class
			void_SubmergedAmbush.ability = newAbility.ability;

			return newAbility;
		}
	}

	public class void_SubmergedAmbush : AbilityBehaviour
	{
		public override Ability Ability => ability;

		public static Ability ability;

		public static IEnumerator TakeDamage(int damage, PlayableCard defender, PlayableCard attacker)
		{
			yield return new WaitForSeconds(1.0f);
			bool flag = defender.HasShield();
			if (flag)
			{
				defender.Status.lostShield = true;
				defender.Anim.StrongNegationEffect();
				bool flag2 = defender.Info.name == "MudTurtle";
				if (flag2)
				{
					defender.SwitchToAlternatePortrait();
				}
				defender.UpdateFaceUpOnBoardEffects();
			}
			else
			{
				defender.Status.damageTaken += damage;
				defender.UpdateStatsText();
				bool flag3 = defender.Health > 0;
				if (flag3)
				{
					defender.Anim.PlayHitAnimation();
				}
				bool flag4 = defender.TriggerHandler.RespondsToTrigger(Trigger.TakeDamage, new object[]
				{
					attacker
				});
				if (flag4)
				{
					yield return defender.TriggerHandler.OnTrigger(Trigger.TakeDamage, new object[]
					{
						attacker
					});
				}
				bool flag5 = defender.Health <= 0;
				if (flag5)
				{
					yield return defender.Die(false, attacker, true);
				}
				bool flag6 = attacker != null;
				if (flag6)
				{
					bool flag7 = attacker.TriggerHandler.RespondsToTrigger(Trigger.DealDamage, new object[]
					{
						damage,
						defender
					});
					if (flag7)
					{
						yield return attacker.TriggerHandler.OnTrigger(Trigger.DealDamage, new object[]
						{
							damage,
							defender
						});
					}
					yield return Singleton<GlobalTriggerHandler>.Instance.TriggerCardsOnBoard(Trigger.OtherCardDealtDamage, false, new object[]
					{
						attacker,
						attacker.Attack,
						defender
					});
				}
			}
			yield break;
		}

	}

	[HarmonyPatch(typeof(CombatPhaseManager), "SlotAttackSlot", MethodType.Normal)]
	public class CombatPhaseManager_SlotAttackSlot_SubmergedAmbushAttack
	{
		[HarmonyPostfix]
		public static IEnumerator Postfix(IEnumerator enumerator, CardSlot attackingSlot, CardSlot opposingSlot, float waitAfter = 0f)
		{
			if (attackingSlot.Card != null && opposingSlot.Card != null && opposingSlot.Card.FaceDown && opposingSlot.Card.HasAbility(void_SubmergedAmbush.ability) && !attackingSlot.Card.AttackIsBlocked(opposingSlot))
			{
				yield return enumerator;
				yield return new WaitForSeconds(0.55f);
				yield return attackingSlot.Card.TakeDamage(1, opposingSlot.Card);

			} else
            {
				yield return enumerator;

			}
			yield break;
		}
	}
}