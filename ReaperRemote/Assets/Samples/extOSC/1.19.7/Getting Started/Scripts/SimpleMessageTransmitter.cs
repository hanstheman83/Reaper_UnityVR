/* Copyright (c) 2020 ExT (V.Sigalkin) */

using UnityEngine;

namespace extOSC.Examples
{
	public class SimpleMessageTransmitter : MonoBehaviour
	{
		#region Public Vars

		public string Address = "/example/1";

		[Header("OSC Settings")]
		public OSCTransmitter Transmitter;

		#endregion

		#region Unity Methods

		protected virtual void Start()
		{
			var message = new OSCMessage(Address);
			message.AddValue(OSCValue.String("Hello, world!"));

			Transmitter.Send(message);
		}

		#endregion

		public void TransmitMidi(bool state, int note){
			// string midiNoteMessage = "/vkb_midi/0/note/80";
			string midiNoteMessage = $"/vkb_midi/0/note/{note}";
			var message = new OSCMessage(midiNoteMessage);
			message.AddValue(OSCValue.Int(state ? 1 : 0));
			// message.AddValue(OSCValue.Int(1));
			// Transmitter.SendMessage(message);
			Transmitter.Send(message);
		}

	}
}