//----------------------------------------------------------------------------
// This file contains code originating from the Microsoft DirectX SDK (see the
// CaptureSound sample for more information).
//
// Copyright (c) Microsoft Corp. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;

namespace UDPSoundSender
{
    // Represents a sound format for a specific capture device.
    internal class SoundFormat
    {
        // Capture device for which this sound format has been created and validated.
        Capture _captureDevice;
        // Constructed format for wave file.
        WaveFormat _currentFormat;

        // Number of channels (stereo sound would be equal to 2).
        short _channels = 0;
        // Number of bits used to represent each sample taken.
        short _bitsPerSample = 0;
        // Number of samples to take per second of recording time.
        int _samplesPerSecond = 0;

        // Simple constructor takes a Capture instance and assumes some default
        // values for channels, bits, samples.
        internal SoundFormat(Capture captureDevice)
            : this(captureDevice, SampleRate.Rate11KHz, SampleSize.Bits8, 1)
        { }

        // Full constructor takes Capture instance and specific values for
        // channels, bits, and samples.
        internal SoundFormat(Capture captureDevice, SampleRate rate, SampleSize size, short channels)
        {
            if (captureDevice == null)
            { 
                throw new ArgumentNullException("captureDevice"); 
            }

            this._captureDevice = captureDevice;
            
            try
            {
                // Test the supplied format characteristics.
                this._currentFormat = ConstructFormat((int)rate, (short)size, (short)channels);
            }
            catch (Exception ex)
            {
                string errMsg = 
                    string.Format("Sound format not supported: {0} samples/sec, {1} bits/sample, {2} channels.",
                        (int)rate, (short)size, (short)channels);
                throw new Exception(errMsg, ex);
            }

            this._channels = channels;
            this._bitsPerSample = (short)size;
            this._samplesPerSecond = (int)rate;
        }

        internal WaveFormat Format
        {
            get 
            { 
                return this._currentFormat; 
            }
        }

        internal short Channels
        {
            get 
            { 
                return this._channels; 
            }
        }

        internal short BitsPerSample
        {
            get 
            { 
                return this._bitsPerSample; 
            }
        }

        internal int SamplesPerSecond
        {
            get 
            { 
                return this._samplesPerSecond; 
            }
        }

        // Helper method to test a specific combination of samples, bits, and channels
        // against the capture device.
        private WaveFormat ConstructFormat(int samplesPerSecond, short bitsPerSample, short channels)
        {
            WaveFormat newFormat = new WaveFormat();

            newFormat.FormatTag = WaveFormatTag.Pcm;
            newFormat.SamplesPerSecond = samplesPerSecond;
            newFormat.BitsPerSample = bitsPerSample;
            newFormat.Channels = channels;
            newFormat.BlockAlign = (short)(channels * (bitsPerSample / 8));
            newFormat.AverageBytesPerSecond = newFormat.BlockAlign * samplesPerSecond;

            VerifyFormat(newFormat);

            return newFormat;
        }

        // Helper method to test a specific WaveFormat instance.
        private void VerifyFormat(WaveFormat newFormat)
        {
            if (this._captureDevice == null)
            { 
                throw new InvalidOperationException("Capture device is null."); 
            }

            CaptureBufferDescription capBuffDesc = new CaptureBufferDescription();
            capBuffDesc.BufferBytes = newFormat.AverageBytesPerSecond;
            capBuffDesc.Format = newFormat;

            CaptureBuffer capBuff = null;

            try
            {
                capBuff = new CaptureBuffer(capBuffDesc, this._captureDevice);
            }
            catch (Exception ex)
            {
                string errMsg =
                   string.Format("Sound format not supported: {0} samples/sec, {1} bits/sample, {2} channels.",
                       newFormat.SamplesPerSecond, newFormat.BitsPerSample, newFormat.Channels);
                throw new Exception(errMsg, ex);
            }

            if (capBuff != null)
            {
                capBuff.Dispose();
                capBuff = null;
            }
        }
    }
}
