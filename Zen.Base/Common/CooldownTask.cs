using System;
using System.Threading.Tasks;

namespace Zen.Base.Common
{
    public class CooldownTask
    {
        private readonly Action _action;
        private bool _dirty;
        private Task _task;

        public CooldownTask(Action action)
        {
            _action = action;
            Poke();
        }

        public CooldownTask(Action<object> action) { }
        public TimeSpan CooldownTimeSpan { get; set; } = TimeSpan.FromSeconds(5);

        private void TryAction()
        {
            if (_dirty)
            {
                StartCooldownTask();
                return;
            }

            _task = null;

            _action();

        }

        public void Poke()
        {
            if (_task == null)
            {
                StartCooldownTask();
                return;
            }

            if (_dirty) return;

            _dirty = true;
        }

        private void StartCooldownTask()
        {
            _task = Task.Delay(CooldownTimeSpan).ContinueWith(t => TryAction());
            _dirty = false;
        }
    }
}