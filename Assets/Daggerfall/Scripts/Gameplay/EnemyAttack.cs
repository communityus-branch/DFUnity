﻿// Project:         Daggerfall Unity -- A game built with Daggerfall Tools For Unity
// Description:     This is a modified version of a script provided by Daggerfall Tools for Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Project Page:    https://github.com/EBFEh/DFUnity -- https://code.google.com/p/daggerfall-unity/

using UnityEngine;
using System.Collections;
using DaggerfallWorkshop;

namespace Daggerfall.Gameplay
{
    /// <summary>
    /// Example enemy attack.
    /// </summary>
    [RequireComponent(typeof(EnemySenses))]
    public class EnemyAttack : MonoBehaviour
    {
        public float MeleeAttackSpeed = 1.0f;       // Number of seconds between melee attacks
        public float MeleeDistance = 2.5f;          // Maximum distance for melee attack

        EnemySenses senses;
        EnemySounds sounds;
        DaggerfallMobileUnit mobile;
        float meleeTimer = 0;
        bool isAttacking;

        void Start()
        {
            senses = GetComponent<EnemySenses>();
            sounds = GetComponent<EnemySounds>();
            mobile = GetComponentInChildren<DaggerfallMobileUnit>();
        }

        void Update()
        {
            // Handle state in progress
            if (mobile.IsPlayingOneShot())
            {
                // Are we attacking?
                if (mobile.IsAttacking())
                    isAttacking = true;

                return;
            }

            // If an attack was in progress it is now complete and we can apply damage
            if (isAttacking)
            {
                MeleeDamage();
                isAttacking = false;
            }

            // Countdown to next attack
            meleeTimer -= Time.deltaTime;
            if (meleeTimer < 0)
            {
                MeleeAnimation();
                meleeTimer = MeleeAttackSpeed;
            }
        }

        #region Private Methods

        private void MeleeAnimation()
        {
            // Are we in range and facing player? Then start attack.
            if (senses.DistanceToPlayer < MeleeDistance && senses.PlayerInSight)
            {
                // Set melee animation state
                mobile.ChangeEnemyState(MobileStates.PrimaryAttack);

                // Play melee sound
                if (sounds)
                {
                    sounds.PlayAttackSound();
                }
            }
        }

        private void MeleeDamage()
        {
            // Are we still in range and facing player? Then apply melee damage.
            if (senses.DistanceToPlayer < MeleeDistance && senses.PlayerInSight)
            {
                senses.Player.SendMessage("RemoveHealth");
            }
        }

        #endregion
    }
}