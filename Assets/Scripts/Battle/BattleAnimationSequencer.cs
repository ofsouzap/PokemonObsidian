using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Battle;

namespace Battle
{
    public partial class BattleAnimationSequencer : MonoBehaviour
    {

        public Queue<Animation> animationQueue = new Queue<Animation>();

        private TextBoxController textBoxController;

        private void Start()
        {

            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);

        }

        public void EnqueueAnimation(Animation animation)
        {

            animationQueue.Enqueue(animation);

        }

        public static Animation CreateSingleTextAnimation(string message,
            bool requireUserContinue = false)
        {
            return new Animation()
            {
                type = Animation.Type.Text,
                messages = new string[] { message },
                requireUserContinue = requireUserContinue
            };
        }

        public void EnqueueSingleText(string message,
            bool requireUserContinue = false)
        {
            EnqueueAnimation(CreateSingleTextAnimation(message, requireUserContinue));
        }

        public IEnumerator PlayAll()
        {

            while (true)
            {

                if (animationQueue.Count == 0)
                    break;

                Animation anim = animationQueue.Dequeue();
                
                yield return StartCoroutine(RunAnimation(anim));

            }

        }

    }
}