//----------------------------------------------------------------------------
// This file contains code originating from the Microsoft DirectX SDK (see the
// CaptureSound sample for more information).
//
// Copyright (c) Microsoft Corp. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;
using System.Threading;
using System.Net.Sockets;

namespace UDPSoundSender
{
    /// <summary>
    /// Represents a sound recorder capable of saving captured sound information to disk.
    /// </summary>
    public class SoundRecorder : IDisposable
    {
        // Number of notifications that DirectSound will provide us while filling up a sound buffer.
        const int NumberRecordNotifications = 16;
        const string MicrophoneSearch = "microphone";

        // Device type that will be used.
        SoundDeviceType _desiredDeviceType = SoundDeviceType.Default;
        // Available capture devices to search through and select from.
        CaptureDevicesCollection _devices;
        // Index into CaptureDevicesCollection where _desiredDeviceType found.
        int _selectedDevice = 0;

        // Constructed sound format for the desired device.
        SoundFormat _recorderFormat = null;

        // DirectSound Capture object specific to the desired device.
        Capture _applicationDevice = null;
        // Buffer used by DirectSound to record sound data.
        CaptureBuffer _applicationBuffer = null;
        
        int _sampleCount = 0;
        int _captureBufferSize = 0;

        string _fileName = string.Empty;
        FileStream _waveFile = null;
        BinaryWriter _writer = null;
        
        bool _capturing = false;
        bool _recording = false;

        UdpClient udpClient;

        // Tied to WaitThread method, responsible for saving buffered samples when signaled.
        Thread _notifyThread = null;
        // Event that is singaled by DirectSound to indicate data available.
        AutoResetEvent _notificationEvent = null;

        public AutoResetEvent NotificationEvent
        {
            get { return _notificationEvent; }
            set { _notificationEvent = value; }
        }
        // Allows DirectSound to know what capture buffer and what positions within that buffer
        // are set to have notifications.
        Notify _applicationNotify = null;
        int _notifySize = 0;
        int _nextCaptureOffset = 0;
        // Will contain offsets that represent positions within the capture buffer for
        // notifications.
        BufferPositionNotify[] _positionNotify = new BufferPositionNotify[NumberRecordNotifications + 1];


        /// <summary>
        /// Create a new sound recorder.
        /// </summary>
        /// <param name="type">Sound capture device.</param>
        /// <param name="rate">Desired sample rate.</param>
        /// <param name="size">Desired sample size.</param>
        /// <param name="channels">Desired channels to use.</param>
        public SoundRecorder(SoundDeviceType type, SampleRate rate, SampleSize size, short channels)
        {
            this._desiredDeviceType = type;
            this._devices = new CaptureDevicesCollection();

            if (this._devices == null || this._devices.Count < 1)
            {
                throw new InvalidOperationException("No sound capture devices detected.");
            }

            this.Find(type);

            InitDirectSound();

            this._recorderFormat = new SoundFormat(this._applicationDevice, rate, size, channels);
        }

        /// <summary>
        /// Start recording.
        /// </summary>
        /// <param name="filename">Filename to record sound to.</param>
        public void Start(string filename)
        {
            // Create a capture buffer, and tell the capture 
            // buffer to start recording   
            CreateCaptureBuffer();

            InitNotifications();
            this._applicationBuffer.Start(true);
        }

        /// <summary>
        /// Stop recording.
        /// </summary>
        public void Stop()
        {
            // Stop the buffer, and read any data that was not 
            // caught by a notification
            this._applicationBuffer.Stop();
            /*
            RecordCapturedData();
            
            this._writer.Seek(4, SeekOrigin.Begin); // Seek to the length descriptor of the RIFF file.
            this._writer.Write((int)(this._sampleCount + 36));	// Write the file length, minus first 8 bytes of RIFF description.
            this._writer.Seek(40, SeekOrigin.Begin); // Seek to the data length descriptor of the RIFF file.
            this._writer.Write(this._sampleCount); // Write the length of the sample data in bytes.

            this._writer.Close();	// Close the file now.
            this._writer = null;	// Set the writer to null.
            this._waveFile = null; // Set the FileStream to null.

            this._fileName = string.Empty;
            */
        }

        public bool Capturing(){
            return _capturing;
        }


        /// <summary>
        /// Get the GUID for the specified sound device type.
        /// </summary>
        /// <param name="type">Sound capture device type to select.</param>
        /// <returns>GUID of selected sound device type.</returns>
        private Guid Select(SoundDeviceType type)
        {
            Find(type);

            return this._devices[this._selectedDevice].DriverGuid;
        }
        
        private void Find(SoundDeviceType type)
        {
            switch (type)
            {
                case SoundDeviceType.Default:
                    this._selectedDevice = 0;
                    break;
                case SoundDeviceType.Microphone:
                    this._selectedDevice = FindMicrophone();
                    break;
            }
        }

        private int FindMicrophone()
        {
            int microphone = 0;

            for (int i = 0; i < this._devices.Count; i++)
            {
                if (this._devices[i].Description.ToLower().Contains(MicrophoneSearch))
                {
                    microphone = i;
                    break;
                }
            }

            return microphone;
        }

        private void InitDirectSound()
        {
            this._captureBufferSize = 0;
            this._notifySize = 0;

            // Create DirectSound.Capture using the preferred capture device.
            this._applicationDevice =
                new Capture(Select(this._desiredDeviceType));
        }

        private void CreateSoundFile()
        {
            //-----------------------------------------------------------------------------
            // Name: OnCreateSoundFile()
            // Desc: Called when the user requests to save to a sound file
            //-----------------------------------------------------------------------------
            if (this._recording)
            {
                // Stop the capture and read any data that 
                // was not caught by a notification
                this.Stop();
                this._recording = false;
            }
            /*
            try
            {
                CreateRIFF();
            }
            catch
            {
                throw;
            }
             * */
        }

        private void CreateRIFF()
        {
            /**************************************************************************
			 
                Here is where the file will be created. A
                wave file is a RIFF file, which has chunks
                of data that describe what the file contains.
                A wave RIFF file is put together like this:
			 
                The 12 byte RIFF chunk is constructed like this:
                Bytes 0 - 3 :	'R' 'I' 'F' 'F'
                Bytes 4 - 7 :	Length of file, minus the first 8 bytes of the RIFF description.
                                (4 bytes for "WAVE" + 24 bytes for format chunk length +
                                8 bytes for data chunk description + actual sample data size.)
                Bytes 8 - 11:	'W' 'A' 'V' 'E'
			
                The 24 byte FORMAT chunk is constructed like this:
                Bytes 0 - 3 :	'f' 'm' 't' ' '
                Bytes 4 - 7 :	The format chunk length. This is always 16.
                Bytes 8 - 9 :	File padding. Always 1.
                Bytes 10- 11:	Number of channels. Either 1 for mono,  or 2 for stereo.
                Bytes 12- 15:	Sample rate.
                Bytes 16- 19:	Number of bytes per second.
                Bytes 20- 21:	Bytes per sample. 1 for 8 bit mono, 2 for 8 bit stereo or
                                16 bit mono, 4 for 16 bit stereo.
                Bytes 22- 23:	Number of bits per sample.
			
                The DATA chunk is constructed like this:
                Bytes 0 - 3 :	'd' 'a' 't' 'a'
                Bytes 4 - 7 :	Length of data, in bytes.
                Bytes 8 -...:	Actual sample data.
			
            ***************************************************************************/

            // Open up the wave file for writing.
            this._waveFile = new FileStream(this._fileName, FileMode.Create);
            this._writer = new BinaryWriter(this._waveFile);

            // Set up file with RIFF chunk info.
            char[] ChunkRiff = { 'R', 'I', 'F', 'F' };
            char[] ChunkType = { 'W', 'A', 'V', 'E' };
            char[] ChunkFmt = { 'f', 'm', 't', ' ' };
            char[] ChunkData = { 'd', 'a', 't', 'a' };

            short shPad = 1; // File padding
            int nFormatChunkLength = 0x10; // Format chunk length.
            int nLength = 0; // File length, minus first 8 bytes of RIFF description. This will be filled in later.
            short shBytesPerSample = 0; // Bytes per sample.
            
            // Figure out how many bytes there will be per sample.
            if (8 == this._recorderFormat.Format.BitsPerSample && 1 == this._recorderFormat.Format.Channels)
                shBytesPerSample = 1;
            else if ((8 == this._recorderFormat.Format.BitsPerSample && 2 == this._recorderFormat.Format.Channels) || (16 == this._recorderFormat.Format.BitsPerSample && 1 == this._recorderFormat.Format.Channels))
                shBytesPerSample = 2;
            else if (16 == this._recorderFormat.Format.BitsPerSample && 2 == this._recorderFormat.Format.Channels)
                shBytesPerSample = 4;

            // Fill in the riff info for the wave file.
            this._writer.Write(ChunkRiff);
            this._writer.Write(nLength);
            this._writer.Write(ChunkType);

            // Fill in the format info for the wave file.
            this._writer.Write(ChunkFmt);
            this._writer.Write(nFormatChunkLength);
            this._writer.Write(shPad);
            this._writer.Write(this._recorderFormat.Format.Channels);
            this._writer.Write(this._recorderFormat.Format.SamplesPerSecond);
            this._writer.Write(this._recorderFormat.Format.AverageBytesPerSecond);
            this._writer.Write(shBytesPerSample);
            this._writer.Write(this._recorderFormat.Format.BitsPerSample);

            // Now fill in the data chunk.
            this._writer.Write(ChunkData);
            this._writer.Write((int)0);	// The sample length will be written in later.
        }

        private void CreateCaptureBuffer()
        {
            CaptureBufferDescription dscheckboxd = new CaptureBufferDescription();

            if (null != this._applicationNotify)
            {
                this._applicationNotify.Dispose();
                this._applicationNotify = null;
            }
            if (null != this._applicationBuffer)
            {
                this._applicationBuffer.Dispose();
                this._applicationBuffer = null;
            }

            if (0 == this._recorderFormat.Format.Channels)
                return;

            // Set the notification size
            this._notifySize = (1024 > this._recorderFormat.Format.AverageBytesPerSecond / 8) ? 1024 : (this._recorderFormat.Format.AverageBytesPerSecond / 8);
            this._notifySize -= _notifySize % this._recorderFormat.Format.BlockAlign;

            // Set the buffer sizes
            this._captureBufferSize = this._notifySize * NumberRecordNotifications;

            // Create the capture buffer
            dscheckboxd.BufferBytes = this._captureBufferSize;
            //this._recorderFormat.Format.FormatTag = WaveFormatTag.Pcm;
            dscheckboxd.Format = this._recorderFormat.Format; // Set the format during creatation

            this._applicationBuffer = new CaptureBuffer(dscheckboxd, this._applicationDevice);
            this._nextCaptureOffset = 0;

            //InitNotifications();
        }

        private void InitNotifications()
        {
            if (null == this._applicationBuffer)
            { throw new NullReferenceException(); }

            // Create a thread to monitor the notify events
            if (null == this._notifyThread)
            {
                // Create a notification event
                this._notificationEvent = new AutoResetEvent(false);

                this._notifyThread = Thread.CurrentThread;
                this._capturing = true;
            }

            // Setup the notification positions
            for (int i = 0; i < NumberRecordNotifications; i++)
            {
                this._positionNotify[i].Offset = (this._notifySize * i) + this._notifySize - 1;
                this._positionNotify[i].EventNotifyHandle = this._notificationEvent.Handle;
            }

            // Setup DirectSound Notify object to operate on our capture buffer.
            this._applicationNotify = new Notify(this._applicationBuffer);

            // Tell DirectSound when to notify the app. The notification will come in the form 
            // of signaled events that are handled in the notify thread.
            this._applicationNotify.SetNotificationPositions(this._positionNotify, NumberRecordNotifications);
        }

        public void SendCapturedData(UdpClient client)
        {
            //-----------------------------------------------------------------------------
            // Name: RecordCapturedData()
            // Desc: Copies data from the capture buffer to the output buffer 
            // invoked on a separate thread, notified by DX.
            //-----------------------------------------------------------------------------
            byte[] CaptureData = null;
            int ReadPos=0;
            int CapturePos;
            int LockSize;
            
            this._applicationBuffer.GetCurrentPosition(out CapturePos, out ReadPos);

            LockSize = ReadPos - this._nextCaptureOffset;
            if (LockSize < 0)
                LockSize += this._captureBufferSize;

            // Block align lock size so that we are always write on a boundary
            LockSize -= (LockSize % this._notifySize);

            if (0 == LockSize)
                return;

            // Read the capture buffer.
            CaptureData = (byte[])this._applicationBuffer.Read(this._nextCaptureOffset, typeof(byte), LockFlag.None, LockSize);

            
            client.Send(CaptureData, CaptureData.Length);
            //            Console.WriteLine(ToHexString(CaptureData));
            // Write the data into the wav file
            /*this._writer.Write(CaptureData, 0, CaptureData.Length);
    //-->        */
            // Update the number of samples, in bytes, of the file so far.
            this._sampleCount += CaptureData.Length;

            // Move the capture offset along
            this._nextCaptureOffset += CaptureData.Length;
            this._nextCaptureOffset %= this._captureBufferSize; // Circular buffer
        }


        #region IDisposable Members

        public void Dispose()
        {
            ShutDown();
        }

        private void ShutDown()
        {
            if (null != this._notificationEvent)
            {
                this._capturing = false;
                this._notificationEvent.Set();
            }
            if (null != this._applicationBuffer)
            {
                if (this._applicationBuffer.Capturing)
                {
                    this.Stop();
                }
            }
        }

        #endregion
    }

   
}
