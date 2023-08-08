using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.UI.Dynamic;
using System.Threading.Tasks;
using SynthesisAPI.Utilities;
using Synthesis.Util;

public class NetworkTaskStatusModal : ModalDynamic {

	private const float MODAL_HEIGHT = 300;
	private const float MODAL_WIDTH = 300;

	public NetworkTaskStatusModal(Task<bool> task, AtomicReadOnly<NetworkTaskStatus> status) : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT))
	{
	}

	public override void Create()
	{
		throw new System.NotImplementedException();
	}

	public override void Delete()
	{
		throw new System.NotImplementedException();
	}

	public override void Update()
	{
		throw new System.NotImplementedException();
	}
}
