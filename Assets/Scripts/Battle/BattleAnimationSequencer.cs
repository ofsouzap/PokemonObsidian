using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;

namespace Battle
{
    public partial class BattleAnimationSequencer : MonoBehaviour
    {

        public bool queueEmptied => animationQueue.Count == 0;
        public Queue<Animation> animationQueue;
        
        public void EnqueueAnimation(Animation animation)
        {

            animationQueue.Enqueue(animation);

        }

        public IEnumerator PlayAll()
        {

            while (true)
            {

                if (animationQueue.Count == 0)
                    break;

                Animation anim = animationQueue.Dequeue();

                StartCoroutine(anim.Play());

                yield return new WaitUntil(() => anim.completed);

            }

        }

    }
}