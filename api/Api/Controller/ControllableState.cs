using System;
using System.Collections.Generic;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Mirabuf.Signal;
using SynthesisAPI.Utilities;

#nullable enable

namespace SynthesisAPI.Controller {

	public partial class ControllableState {

		private ReaderWriterLockSlim? _signalMapLock;
		public ReaderWriterLockSlim SignalMapLock {
			get => _signalMapLock ?? (_signalMapLock = new ReaderWriterLockSlim());
		}

		private Dictionary<string, bool>? _modifiedSignals;
		private Dictionary<string, bool> ModifiedSignals {
			get => _modifiedSignals ?? (_modifiedSignals = new Dictionary<string, bool>());
		}

		public ControllableState(Signals signals) {
			signals.SignalMap.ForEach(x => {
				SignalMap.Add(
					x.Key,
					new SignalData {
						SignalGuid = x.Key,
						Name = x.Value.Info.Name,
						Value = Value.ForNull()
					}
				);
			});
		}

		public Value? GetValue(string signal_guid) {
			// TODO: Limit read to tho classified as output and write to inputs
			Value? result = null;

			SignalMapLock.EnterReadLock();
			SignalMap.TryGetValue(signal_guid, out SignalData sig);
			if (sig != null) {
				result = new Value(sig.Value);
			}
			SignalMapLock.ExitReadLock();

			return result;
		}

		public void SetValue(string signal_guid, Value v, bool trackChange = true) {
			SignalData s;
			if (!SignalMap.ContainsKey(signal_guid)) {
				s = new SignalData() { Io = UpdateIOType.Input, SignalGuid = signal_guid };
				SignalMap.Add(signal_guid, s);
			} else {
				s = SignalMap[signal_guid];
			}
			SignalMapLock.EnterWriteLock();
			s.Value = new Value(v);
			ModifiedSignals[signal_guid] = false;
			SignalMapLock.ExitWriteLock();
		}

		public List<SignalData> CompileChanges() {
			SignalMapLock.EnterWriteLock();
			List<SignalData> updatedSignals = new List<SignalData>(ModifiedSignals.Count);
			ModifiedSignals.ForEach(x => updatedSignals.Add(new SignalData(SignalMap[x.Key])));
			ModifiedSignals.Clear();
			SignalMapLock.ExitWriteLock();
			return updatedSignals;
		}

	}

}

