using NewFangServerPlugin.Utils;
using NLog;
using System;
using System.Timers;

namespace NewFangServerPlugin.Handler {

    public class CountDownTimer {
        public bool CountDownEnabled = true;

        private Action<int> _onTimerTick { get; set; }
        private Action _onCountDownEnd { get; set; }

        private int _timeRemaining;

        private Timer _countDownTimer;

        /// <summary>
        /// Creates a new countdown timer.
        /// </summary>
        /// <param name="time">Time remaining</param>
        /// <param name="interval">Interval in milliseconds</param>
        public CountDownTimer(int time, int interval, Action<int> onTimerTick = null, Action onCountDownEnd = null) {
            _onTimerTick = onTimerTick;
            _onCountDownEnd = onCountDownEnd;

            _timeRemaining = time;

            CountDown();

            _countDownTimer = new Timer(interval);
            _countDownTimer.Elapsed += (sender, e) => CountDown();
            _countDownTimer.Start();
        }

        private void CountDown() {
            if(!CountDownEnabled) return;
            if(_timeRemaining == 0) {
                _onCountDownEnd?.Invoke();
                _countDownTimer.Stop();
            } else {
                _onTimerTick?.Invoke(_timeRemaining);
                _timeRemaining--;
            }
        }
    }

    public class RestartTimer {
        public static CountDownTimer CountDownTimer { get; set; }

        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;

        /// <summary>
        /// Creates a new restart timer.
        /// 
        /// Recommended puting this class in a try-catch block to handle the TimerAlreadyActiveException.
        /// </summary>
        /// <param name="time">Restart time in seconds.</param>
        /// <exception cref="TimerAlreadyActiveException">Thrown when a restart timer is already active.</exception>
        public RestartTimer(int time) {
            if(CountDownTimer != null) {
                throw new TimerAlreadyActiveException(typeof(RestartTimer));
            }
            CountDownTimer = new CountDownTimer(time, 1000, RestartServerTimerTick, RestartServer);
        }

        private void RestartServerTimerTick(int time) {
            if(time >= 3600 && time % 3600 == 0) {
                ManagerUtils.ChatManagerServer?.SendMessageAsSelf($"Server will restart in {time / 3600} hours.");
                Log.Info($"Server will restart in {time / 3600} hours.");
            } else if(time <= 600 && time % 60 == 0) {
                ManagerUtils.ChatManagerServer?.SendMessageAsSelf($"Server will restart in {time / 60} minutes.");
                Log.Info($"Server will restart in {time / 60} minutes.");
            } else if(time <= 10) {
                ManagerUtils.ChatManagerServer?.SendMessageAsSelf($"Server will restart in {time} seconds.");
                Log.Info($"Server will restart in {time} seconds.");
            }
        }

        private void RestartServer() {
            Log.Info("Restarting server...");
            PluginInstance.Torch.Restart();
        }
    }

    public class TimerAlreadyActiveException : Exception {
        public TimerAlreadyActiveException(Type type)
            : base($"{type.Name} already active.") { }

        public TimerAlreadyActiveException(Type type, string message)
            : base($"{type.Name}: {message}") { }

        public TimerAlreadyActiveException(Type type, string message, Exception inner)
            : base($"{type.Name}: {message}", inner) { }
    }

}
