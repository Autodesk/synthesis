using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.UI.Dynamic;
using Synthesis.Util;
using SynthesisAPI.Utilities;
using UnityEngine;

public class NetworkWaitModal : ModalDynamic {

    private const float MODAL_WIDTH = 300;
    private const float MODAL_HEIGHT = 300;

    private Task<bool> _task;
    private AtomicReadOnly<NetworkTaskStatus> _status;

    public NetworkWaitModal((Task<bool> task, AtomicReadOnly<NetworkTaskStatus> status) data)
        : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
        _task = data.task;
        _status = data.status;
    }
    
    public override void Create() { }
    public override void Update() { }
    public override void Delete() { }
}
