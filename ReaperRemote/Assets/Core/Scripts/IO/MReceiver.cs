/* Copyright (c) 2020 ExT (V.Sigalkin) */

using UnityEngine;
using extOSC;

namespace Core.IO
{
	public class MReceiver : MonoBehaviour
	{
		#region Public Vars

		public string Address = "/play";

		[Header("OSC Settings")]
		public OSCReceiver Receiver;

		#endregion

		#region Unity Methods

		protected virtual void Start()
		{
			Receiver.Bind(Address, ReceivedMessage);
		}

		#endregion

		#region Private Methods

		private void ReceivedMessage(OSCMessage message)
		{
			Debug.Log("Message received");
			Debug.LogFormat("Received: {0}", message);
		}

		#endregion
	}
}