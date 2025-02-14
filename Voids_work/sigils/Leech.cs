﻿using System.Collections;
using APIPlugin;
using DiskCardGame;
using UnityEngine;
using Artwork = voidSigils.Voids_work.Resources.Resources;

namespace voidSigils
{
	public partial class Plugin
	{
		//Port from Cyn Sigil a day
		private NewAbility AddLeech()
		{
			// setup ability
			const string rulebookName = "Leech";
			const string rulebookDescription = "When [creature] deals damage, it will heal 1 Health for each damage dealt to a card.";
			const string LearnDialogue = "Vigor from blood!";
			// const string TextureFile = "Artwork/void_pathetic.png";

			AbilityInfo info = SigilUtils.CreateInfoWithDefaultSettings(rulebookName, rulebookDescription, LearnDialogue,  true, 3, Plugin.configLeech.Value);
			info.canStack = true;
			info.pixelIcon = SigilUtils.LoadSpriteFromResource(Artwork.ability_leech_a2);
			Texture2D tex = SigilUtils.LoadTextureFromResource(Artwork.ability_leech);

			var abIds = SigilUtils.GetAbilityId(info.rulebookName);
			
			NewAbility newAbility = new NewAbility(info, typeof(void_Leech), tex, abIds);

			// set ability to behaviour class
			void_Leech.ability = newAbility.ability;

			return newAbility;
		}
	}

	public class void_Leech : AbilityBehaviour
	{
		public override Ability Ability => ability;

		public static Ability ability;

		
		public override bool RespondsToDealDamage(int amount, PlayableCard target)
    {
      return amount > 0;
    }

    public override IEnumerator OnDealDamage(int amount, PlayableCard target)
    {
      yield return base.PreSuccessfulTriggerSequence();
      if (base.Card.Status.damageTaken > 0)
      {
        base.Card.HealDamage(Mathf.Clamp(amount, 1, base.Card.Status.damageTaken));
      }
      base.Card.OnStatsChanged();
      base.Card.Anim.StrongNegationEffect();
      yield return new WaitForSeconds(0.25f);
      yield return base.LearnAbility(0.25f);
      yield break;
    }
	}

}
