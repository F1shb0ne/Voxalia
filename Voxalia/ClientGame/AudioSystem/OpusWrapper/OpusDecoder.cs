//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Voxalia.ClientGame.AudioSystem.OpusWrapper
{
    /// <summary>
    /// Opus audio decoder.
    /// </summary>
    public class OpusDecoder : IDisposable
    {
        /// <summary>
        /// Creates a new Opus decoder.
        /// </summary>
        /// <param name="outputSampleRate">Sample rate to decode at (Hz). This must be one of 8000, 12000, 16000, 24000, or 48000.</param>
        /// <param name="outputChannels">Number of channels to decode.</param>
        /// <returns>A new <c>OpusDecoder</c>.</returns>
        public static OpusDecoder Create(int outputSampleRate, int outputChannels)
        {
            if (outputSampleRate != 8000 &&
                outputSampleRate != 12000 &&
                outputSampleRate != 16000 &&
                outputSampleRate != 24000 &&
                outputSampleRate != 48000)
            {
                throw new ArgumentOutOfRangeException("inputSamplingRate");
            }
            if (outputChannels != 1 && outputChannels != 2)
            {
                throw new ArgumentOutOfRangeException("inputChannels");
            }

            IntPtr error;
            IntPtr decoder = OpusAPI.opus_decoder_create(outputSampleRate, outputChannels, out error);
            if ((Errors)error != Errors.OK)
            {
                throw new Exception("Exception occured while creating decoder");
            }
            return new OpusDecoder(decoder, outputSampleRate, outputChannels);
        }

        private IntPtr _decoder;

        private OpusDecoder(IntPtr decoder, int outputSamplingRate, int outputChannels)
        {
            _decoder = decoder;
            OutputSamplingRate = outputSamplingRate;
            OutputChannels = outputChannels;
            MaxDataBytes = 4000;
        }

        /// <summary>
        /// Produces PCM samples from Opus encoded data.
        /// </summary>
        /// <param name="inputOpusData">Opus encoded data to decode, null for dropped packet.</param>
        /// <param name="dataLength">Length of data to decode.</param>
        /// <returns>PCM audio samples.</returns>
        public byte[] Decode(byte[] inputOpusData, int dataLength)
        {
            int frameCount = FrameCount(MaxDataBytes);
            int length;
            IntPtr decodedPtr = Marshal.AllocHGlobal(MaxDataBytes);
            if (inputOpusData == null)
            {
                length = OpusAPI.opus_decode(_decoder, IntPtr.Zero, 0, decodedPtr, frameCount, (ForwardErrorCorrection) ? 1 : 0);
            }
            else
            {
                IntPtr inputPtr = Marshal.AllocHGlobal(inputOpusData.Length);
                Marshal.Copy(inputOpusData, 0, inputPtr, inputOpusData.Length);
                length = OpusAPI.opus_decode(_decoder, inputPtr, dataLength, decodedPtr, frameCount, 0);
                Marshal.FreeHGlobal(inputPtr);
            }
            if (length < 0)
            {
                throw new Exception("Decoding failed - " + ((Errors)length).ToString());
            }
            byte[] decoded = new byte[length * 2];
            Marshal.Copy(decodedPtr, decoded, 0, length * 2);
            Marshal.FreeHGlobal(decodedPtr);
            return decoded;
        }

        /// <summary>
        /// Determines the number of frames that can fit into a buffer of the given size.
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public int FrameCount(int bufferSize)
        {
            //  seems like bitrate should be required
            int bitrate = 16;
            int bytesPerSample = (bitrate / 8) * OutputChannels;
            return bufferSize / bytesPerSample;
        }

        /// <summary>
        /// Gets the output sampling rate of the decoder.
        /// </summary>
        public int OutputSamplingRate;

        /// <summary>
        /// Gets the number of channels of the decoder.
        /// </summary>
        public int OutputChannels;

        /// <summary>
        /// Gets or sets the size of memory allocated for decoding data.
        /// </summary>
        public int MaxDataBytes;

        /// <summary>
        /// Gets or sets whether forward error correction is enabled or not.
        /// </summary>
        public bool ForwardErrorCorrection;

        ~OpusDecoder()
        {
            Dispose();
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed)
                return;
            GC.SuppressFinalize(this);
            if (_decoder != IntPtr.Zero)
            {
                OpusAPI.opus_decoder_destroy(_decoder);
                _decoder = IntPtr.Zero;
            }
            disposed = true;
        }
    }
}

