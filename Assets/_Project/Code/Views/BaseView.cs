using UnityEngine;

namespace Project.UI
{
    public abstract class BaseView : MonoBehaviour
    {
        protected void OnEnable() =>
            Subscribe();

        public virtual void Subscribe() { }

        protected void OnDisable() =>
            Unsubscribe();

        public virtual void Unsubscribe() { }
    }
}
