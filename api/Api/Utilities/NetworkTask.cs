using System.Threading.Tasks;

#nullable enable

namespace SynthesisAPI.Utilities {
    public class NetworkTask<T> {
        private Task<T> _task;
        private AtomicReadOnly<NetworkTaskStatus> _status;
        public NetworkTaskStatus Status => _status.Value;

        public bool IsCompleted => _task.IsCompleted;

        public NetworkTask(Task<T> task, AtomicReadOnly<NetworkTaskStatus> status) {
            _task = task;
            _status = status;
        }

        public static NetworkTask<T> FromResult(T val, string? msg = null)
            => new NetworkTask<T>(Task.FromResult(val), new AtomicReadOnly<NetworkTaskStatus>(
                new NetworkTaskStatus { Progress = 1f, Message = msg ?? string.Empty }));
    }

    public struct NetworkTaskStatus {
        public float Progress;
        public string Message;

		public override string ToString() {
            return $"[{System.Math.Round(Progress * 100f, 1)}%] {Message}";
		}
	}
}