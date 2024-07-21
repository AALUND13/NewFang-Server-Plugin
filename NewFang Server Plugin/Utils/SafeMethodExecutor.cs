using System;

namespace NewFangServerPlugin.Utils {
    public static class SafeMethodExecutor {
        public static (TResult, Exception) ExecuteSafe<TResult>(Func<TResult> action) {
            try {
                return (action(), null);
            } catch(Exception e) {
                return (default, e);
            }
        }

        public static (TResult, Exception) ExecuteSafe<T1, TResult>(Func<T1, TResult> action, T1 arg1) {
            try {
                return (action(arg1), null);
            } catch(Exception e) {
                return (default, e);
            }
        }

        public static (TResult, Exception) ExecuteSafe<T1, T2, TResult>(Func<T1, T2, TResult> action, T1 arg1, T2 arg2) {
            try {
                return (action(arg1, arg2), null);
            } catch(Exception e) {
                return (default, e);
            }
        }

        public static (TResult, Exception) ExecuteSafe<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> action, T1 arg1, T2 arg2, T3 arg3) {
            try {
                return (action(arg1, arg2, arg3), null);
            } catch(Exception e) {
                return (default, e);
            }
        }

        public static (TResult, Exception) ExecuteSafe<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            try {
                return (action(arg1, arg2, arg3, arg4), null);
            } catch(Exception e) {
                return (default, e);
            }
        }

        public static (TResult, Exception) ExecuteSafe<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            try {
                return (action(arg1, arg2, arg3, arg4, arg5), null);
            } catch(Exception e) {
                return (default, e);
            }
        }
    }
}
