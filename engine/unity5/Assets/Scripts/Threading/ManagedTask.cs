using Synthesis.Utils;
using System;

namespace Synthesis
{
    public abstract class ManagedTask : ManagedTaskBase
    {
        bool paused;
        bool alive;

        protected Channel<IMessage> commandChannel;
        protected Channel<IMessage> stateChannel;

    public ManagedTask(Channel<IMessage> c, Channel<IMessage> s)
        {
            commandChannel = c;
            stateChannel = s;
        }

        protected void Pause()
        {
            paused = true;
        }

        protected void Resume()
        {
            paused = false;
        }

        public bool IsPaused()
        {
            return paused;
        }

        public bool IsAlive()
        {
            return alive;
        }

        public override void OnMessage()
        {
            var currentCommand = commandChannel.TryGet();
            if (currentCommand.IsValid())
            {
                switch (currentCommand.Get().GetName())
                {
                    case StandardMessage.Resume:
                        OnResume();
                        break;
                    case StandardMessage.Start:
                        OnStart();
                        break;
                    case StandardMessage.Pause:
                        OnPause();
                        break;
                    case StandardMessage.Stop:
                        OnStop();
                        break;
                    case StandardMessage.Exit:
                        OnExit();
                        break;
                    default:
                        throw new Exception("Unrecognized thread command \"" + currentCommand.Get().GetName() + "\"");
                }
            }
        }

        public override void OnExit()
        {
            alive = false;
        }

        public override void OnPause()
        {
            Pause();
        }

        public override void OnResume()
        {
            Resume();
        }

        public override void OnStart()
        {
            alive = true;
            Resume();
        }

        public override void OnStop()
        {
            Pause();
        }

        public override void OnCycle()
        {
        }

    }
}
