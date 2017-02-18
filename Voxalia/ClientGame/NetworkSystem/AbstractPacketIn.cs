//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016-2017 FreneticXYZ under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.NetworkSystem
{
    public abstract class AbstractPacketIn
    {
        public Client TheClient;

        public bool ChunkN = false;

        /// <summary>
        /// Parse the given byte array and execute the results.
        /// </summary>
        /// <param name="data">The byte array received from a client.</param>
        /// <returns>False if the array is invalid, true if it parses successfully.</returns>
        public abstract bool ParseBytesAndExecute(byte[] data);
    }
}
