using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SynthesisServer
{
    public sealed class Server
    {
        private Socket _udpSocket;

        public int Port { get; set; }

        private static readonly Lazy<Server> lazy = new Lazy<Server>(() => new Server());
        public static Server Instance { get { return lazy.Value; } }
        private Server() 
        {
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
        }

        public ECDomainParameters GenerateParameters()
        {
            X9ECParameters x9EC = NistNamedCurves.GetByName("P-521");
            return new ECDomainParameters(x9EC.Curve, x9EC.G, x9EC.N, x9EC.H, x9EC.GetSeed());
        }

        public AsymmetricCipherKeyPair GenerateKeyPair(ECDomainParameters domainParameters)
        {
            ECKeyPairGenerator g = (ECKeyPairGenerator)GeneratorUtilities.GetKeyPairGenerator("ECDH");
            g.Init(new ECKeyGenerationParameters(domainParameters, new SecureRandom()));
            return g.GenerateKeyPair();
        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {

        }

        private void RecieveCallback(IAsyncResult asyncResult)
        {

        }

        private void SendCallback(IAsyncResult asyncResult)
        {

        }


    }
}
