/* Copyright (c) 2020 ExT (V.Sigalkin) */

using UnityEngine;
using extOSC;

namespace Core.IO
{
public class MTransmitter : MonoBehaviour
{
	#region Public Vars

	public string Address = "/example/1";

	[Header("OSC Settings")]
	public OSCTransmitter Transmitter;

	#endregion

	#region Unity Methods

	protected virtual void Start()
	{

	}

	#endregion

	#region Public Methods
	public void TransmitMidiNote(int channel, int note, bool isOne){
		string midiNoteMessage = $"/vkb_midi/{channel}/note/{note}";
		var message = new OSCMessage(midiNoteMessage);
		message.AddValue(OSCValue.Int(isOne ? 126 : 0));
		Transmitter.Send(message);
	}
	public void TransmitMidiNote(int channel, int note, int velocity){
		string midiNoteMessage = $"/vkb_midi/{channel}/note/{note}";
		var message = new OSCMessage(midiNoteMessage);
		message.AddValue(OSCValue.Int(velocity));
		Transmitter.Send(message);
	}
	public void TransmitMidiCC(int channel, int cc, int velocity){
		string midiCCMessage = $"/vkb_midi/{channel}/cc/{cc}";
		var message = new OSCMessage(midiCCMessage);
		message.AddValue(OSCValue.Int(velocity));
		Transmitter.Send(message);
	}
	public void TransmitMidiCC(int channel, int cc, bool isOne){
		string midiCCMessage = $"/vkb_midi/{channel}/cc/{cc}";
		var message = new OSCMessage(midiCCMessage);
		message.AddValue(OSCValue.Int(isOne ? 126 : 0));
		Transmitter.Send(message);
	}


	#endregion Public Methods
}

}