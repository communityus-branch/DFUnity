﻿using UnityEngine;
using System.Collections;
using DaggerfallWorkshop.Utility;

namespace DaggerfallWorkshop.Demo
{
    /// <summary>
    /// Example enemy death.
    /// </summary>
    public class EnemyDeath : MonoBehaviour
    {
        DaggerfallMobileUnit mobile;

        void Start()
        {
            mobile = GetComponentInChildren<DaggerfallMobileUnit>();
        }

        public void Die()
        {
            if (!mobile)
                return;

            // Get corpse marker texture indices
            int archive, record;
            EnemyBasics.ReverseCorpseTexture(mobile.Summary.Enemy.CorpseTexture, out archive, out record);

            // Leave corpse marker
            DaggerfallUnity dfUnity;
            if (DaggerfallUnity.FindDaggerfallUnity(out dfUnity))
            {
                // Spawn marker
                GameObject go = GameObjectHelper.CreateDaggerfallBillboardGameObject(dfUnity, archive, record, transform.parent, true);
                go.transform.position = transform.position;

                // Align to ground. Be generous with distance as flying enemies might have a way to drop.
                // This could also be hanlded by adding a Rigidbody and collider then let gravity do the work.
                GameObjectHelper.AlignBillboardToGround(go, go.GetComponent<DaggerfallBillboard>().Summary.Size, 16f);
            }

            // Disable enemy gameobject and schedule for destruction
            gameObject.SetActive(false);
            GameObject.Destroy(gameObject);
        }
    }
}