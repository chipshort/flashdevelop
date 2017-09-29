using System;
using Enum = haxe.lang.Enum;

namespace HaXeContext.Helpers
{
    public class EnumSwitch
    {
        readonly Enum matched;

        public EnumSwitch Case<A>(Func<A, Enum> type, Action<A> action)
        {
            if (type.Method.Name != matched.getTag()) return this;

            action((A)matched.getParams()[0]);
            return this;
        }

        public EnumSwitch Case<A, B>(Func<A, B, Enum> type, Action<A, B> action)
        {
            if (type.Method.Name != matched.getTag()) return this;

            var pars = matched.getParams();
            action((A)pars[0], (B)pars[1]);
            return this;
        }

        public EnumSwitch Case<A, B, C>(Func<A, B, C, Enum> type, Action<A, B, C> action)
        {
            if (type.Method.Name != matched.getTag()) return this;

            var pars = matched.getParams();
            action((A)pars[0], (B)pars[1], (C)pars[2]);
            return this;
        }

        public EnumSwitch Case<A, B, C, D>(Func<A, B, C, D, Enum> type, Action<A, B, C, D> action)
        {
            if (type.Method.Name != matched.getTag()) return this;

            var pars = matched.getParams();
            action((A)pars[0], (B)pars[1], (C)pars[2], (D)pars[3]);
            return this;
        }

        public EnumSwitch Case<A, B, C, D, E>(Func<A, B, C, D, E, Enum> type, Action<A, B, C, D, E> action)
        {
            if (type.Method.Name != matched.getTag()) return this;

            var pars = matched.getParams();
            action((A)pars[0], (B)pars[1], (C)pars[2], (D)pars[3], (E)pars[4]);
            return this;
        }


        public EnumSwitch Case(Enum type, Action action)
        {
            if (type.getTag() != matched.getTag()) return this;

            action();
            return this;
        }

        internal EnumSwitch(Enum x)
        {
            this.matched = x;
        }
    }

    public static class SwitchHelper
    {
        public static EnumSwitch SwitchOn(this Enum on)
        {
            return new EnumSwitch(on);
        }
    }
}
