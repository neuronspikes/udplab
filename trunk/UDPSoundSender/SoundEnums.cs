using System;
using System.Collections.Generic;
using System.Text;

namespace UDPSoundSender
{
    /// <summary>
    /// Represents sound device type choices.
    /// </summary>
    public enum SoundDeviceType
    {
        /// <summary>
        /// Default device.
        /// </summary>
        Default,

        /// <summary>
        /// Microphone device (searches for "microphone" text in device descriptions).
        /// </summary>
        Microphone
    }

    /// <summary>
    /// How many thousands of samples will be taken per second (kHz).
    /// </summary>
    public enum SampleRate
    {
        Rate8KHz = 8000,
        Rate11KHz = 11025,
        Rate22KHz = 22050,
        Rate44KHz = 44100,
        Rate48KHz = 48000
    }

    /// <summary>
    /// The size in bits of each sound sample.
    /// </summary>
    public enum SampleSize
    {
        Bits8 = 8,
        Bits16 = 16
    }
}
