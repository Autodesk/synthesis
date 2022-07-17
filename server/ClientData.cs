using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SynthesisServer
{
    public class ClientData
    {
        // The public and private key for the server that corresponds with a client
        // Maybe try implementing ECDH in the future

        public IPEndPoint ClientEndpoint { get; set; }
        public string Name { get; set; }
        public string ClientID { get; set; } // May change to actually guid object probably not though
        public byte[] SymmetricKey { get; private set; }
        public long LastHeartbeat { get; private set; }
        public bool IsReady { get; set; }

        private string _currentLobbyName = null;

        private AsymmetricCipherKeyPair _keyPair;

        public ClientData(string importedPublicKey, DHParameters parameters)
        {

            _keyPair = GenerateKeys(parameters);

            DHPublicKeyParameters importedPublicKeyParameters = new DHPublicKeyParameters(new BigInteger(importedPublicKey), parameters);
            IBasicAgreement internalKeyAgreement = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgreement.Init(_keyPair.Private);
            BigInteger sharedKey = internalKeyAgreement.CalculateAgreement(importedPublicKeyParameters);
            byte[] sharedKeyBytes = sharedKey.ToByteArray();

            IDigest digest = new Sha256Digest();
            SymmetricKey = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(sharedKeyBytes, 0, sharedKeyBytes.Length);
            digest.DoFinal(SymmetricKey, 0);

            LastHeartbeat = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        private AsymmetricCipherKeyPair GenerateKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        public String GetPublicKey() { return ((DHPublicKeyParameters)_keyPair.Public).Y.ToString(); }

        public void UpdateHeartbeat() { LastHeartbeat = System.DateTimeOffset.Now.ToUnixTimeMilliseconds(); }


        // Allows the server to not perform a bunch of iterations just to do nothing 
        public void SetCurrentLobby(string? lobbyName, int? index, Dictionary<string, Lobby> lobbies, ReaderWriterLockSlim lobbiesLock)
        {
            if (_currentLobbyName != lobbyName)
            {
                if (lobbyName == null)
                {
                    lobbiesLock.EnterWriteLock();
                    lobbies[_currentLobbyName].TryRemoveClient(this);
                    lobbiesLock.ExitWriteLock();
                } 
                else if (lobbies.ContainsKey(lobbyName))
                {
                    lobbiesLock.EnterWriteLock();
                    lobbies[lobbyName].TrySetClient(this, index);
                    lobbiesLock.ExitWriteLock();
                }
                else
                {
                    lobbiesLock.EnterWriteLock();
                    lobbies.Add(lobbyName, new Lobby(this));
                    lobbiesLock.ExitWriteLock();
                }
                _currentLobbyName = lobbyName;
            }
        }
    }
}
