using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ReflectWindow : MonoBehaviourPlus, IBulletReceiver
{
    public new Collider collider;
    public Color bulletColor;
    public bool hit;

    Humanoid player;
    Humanoid ai;
    Coroutine crtDelay;
    readonly List<Bullet> bullets = new List<Bullet>();

    public ReflectWindow Initialise(Player player)
    {
        this.player = player;
        name = "Player Reflect Window";
        ConstraintSource constraintSource = new ConstraintSource();//why the heck is there no constructor for this
        constraintSource.sourceTransform = player.transform;
        constraintSource.weight = 1;
        ParentConstraint constraint = GetComponent<ParentConstraint>();
        constraint.SetSource(0, constraintSource);
        constraint.constraintActive = true;
        collider.enabled = false;
        enabled = false;
        return this;
    }

    public ReflectWindow Initialise(MeleeAI ai)
    {
        this.ai = ai;
        name = "Player Reflect Window";
        ConstraintSource constraintSource = new ConstraintSource();
        constraintSource.sourceTransform = ai.transform;
        constraintSource.weight = 1;
        ParentConstraint constraint = GetComponent<ParentConstraint>();
        constraint.SetSource(0, constraintSource);
        constraint.constraintActive = true;
        collider.enabled = false;
        enabled = false;
        return this;
    }

    public void Update()
    {
        Debug.Log($"hit: {hit}, enabled: {enabled}");
    }

    private void OnEnable()
    {
        collider.enabled = true;
    }

    void OnDisable()
    {
        hit = false;
        /*
        for (int i = 0; i < bullets.Count; i++)
        {
            bullets[i].direction = player.LookDirection;
            bullets[i].enabled = true;
        }
        bullets.Clear();
        */
        collider.enabled = false;
    }

    public void OnBulletHit(Collision collision, Bullet bullet)
    {
        // This can go two ways. A deflect in the direction of the player's crosshair, or straight back towards the shooter.

        //bullet.direction = player.LookDirection;
        //bullet.shooter = this;
        //bullet.color = bulletColor;

        // OR

        if (player || (ai && bullet.shooter is not AIController))
        {
            if (!hit)
            {
                hit = true;
                if (bullet.shooter)
                {
                    bullet.direction = (bullet.shooter.transform.position - transform.position).normalized;
                    bullet.shooter = this;
                    bullet.color = bulletColor;
                }
                else
                {
                    if (player) bullet.direction = player.LookDirection;
                    if (ai) bullet.direction = ai.LookDirection;
                    bullet.shooter = this;
                    bullet.color = bulletColor;
                }
            }
        }

        //hit = false;

        /*
        bullet.enabled = false;
        bullet.shooter = this;
        bullet.color = bulletColor;
        bullets.Add(bullet);
        */
    }

    public void PlayerReflect(float waitTime)
    {
        enabled = true;
        if (crtDelay == null)
        {
            crtDelay = StartCoroutine(Delay());
            IEnumerator Delay()
            {
                yield return new WaitForSeconds(waitTime);
                hit = false;
                enabled = false;
                crtDelay = null;
            }
        }
    }

    public void EnemyReflect(float waitTime)
    {
        collider.enabled = false;
        if (crtDelay == null)
        {
            crtDelay = StartCoroutine(Delay());
            IEnumerator Delay()
            {
                yield return new WaitForSeconds(waitTime);
                hit = false;
                collider.enabled = true;
                crtDelay = null;
            }
        }
    }
}
