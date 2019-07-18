using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis.Utils
{
    public class Either<TLeft, TRight>
    {
        public enum State
        {
            Left,
            Right,
            Invalid
        }

        private TLeft left;
        private TRight right;
        private State state;

        public Either()
        {
            state = State.Invalid;
        }

        public Either(TLeft l)
        {
            state = State.Left;
            left = l;
        }

        public Either(TRight r)
        {
            state = State.Right;
            right = r;
        }

        public TLeft Left()
        {
            if (state == State.Left) 
                return left;
            throw new Exception("Attempt to get Left, while value is " + Enum.GetName(typeof(State), state));
        }

        public TRight Right()
        {
            if (state == State.Right)
                return right;
            throw new Exception("Attempt to get Right, while value is " + Enum.GetName(typeof(State), state));
        }

        public State GetState()
        {
            return state;
        }

        public static Either<R,U> Bimap<R, U>(Func<TLeft,R> f, Func<TRight,U> g, Either<TLeft, TRight> e)
        {
            switch(e.state)
            {
                case State.Left:
                    return new Either<R, U>(f(e.left));
                case State.Right:
                    return new Either<R, U>(g(e.right));
                default:
                    return new Either<R, U>();
            }
        }
    }
}
