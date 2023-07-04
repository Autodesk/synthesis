using System;
using System.Collections.Generic;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Mirabuf.Signal;
using SynthesisAPI.Utilities;

#nullable enable

namespace SynthesisAPI.Controller {

	public partial class ControllableState {

		private readonly ReaderWriterLockSlim _signalMapLock;

		private Dictionary<string, bool> _modifiedSignals;

		public ControllableState(Signals signals) {
			_signalMapLock = new ReaderWriterLockSlim();
			_modifiedSignals = new Dictionary<string, bool>();

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

			_signalMapLock.EnterReadLock();
			SignalMap.TryGetValue(signal_guid, out SignalData sig);
			if (sig != null) {
				result = new Value(sig.Value);
			}
			_signalMapLock.ExitReadLock();

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
			_signalMapLock.EnterWriteLock();
			s.Value = new Value(v);
			_modifiedSignals[signal_guid] = false;
			_signalMapLock.ExitWriteLock();
		}

		public List<SignalData> CompileChanges() {
			_signalMapLock.EnterReadLock();
			List<SignalData> updatedSignals = new List<SignalData>(_modifiedSignals.Count);
			_modifiedSignals.ForEach(x => updatedSignals.Add(new SignalData(SignalMap[x.Key])));
			_signalMapLock.ExitReadLock();
			return updatedSignals;
		}

	}

}

