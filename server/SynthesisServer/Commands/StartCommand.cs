using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;


namespace SynthesisServer
{
    [Verb("start", HelpText = "Start the Synthesis Server")]
    class StartCommand
    {
        #nullable enable

        [Option('c', "config", Required = false, HelpText = "Specify a path to an appsettings.json file. Defaults to the same directory as the binary.")]
        public string? ConfigPath { get; set; } = null;

        [Option('p', "port", Required = false, HelpText = "Specify the UDP port you want the server to listen on.")]
        public int? Port { get; set; } = null;

        [Option('l', "lobby-timeout", Required = false, HelpText = "Set the maximum amount of time in seconds that a client can have a lobby up. (-1 for infinite)")]
        public int? LobbyTimeout { get; set; } = null;

        [Option('m', "max-lobbies", Required = false, HelpText = "Set the maximum number of lobbies that can be created on the server. (-1 for infinite)")]
        public int? MaxHosts { get; set; } = null;

        [Option('i', "heartbeat-interval", Required = false, HelpText = "Set the time interval in seconds that a client must send a heartbeat signal by. (-1 for infinite; Not recommended)")]
        public int? HearbeatInterval { get; set; } = null;

        #nullable disable

        [Option('n', "new-config", Required = false, HelpText = "Will generate a new config file based on arguments and current configuration if true. (Defaults to false)")]
        public bool NewConfig { get; set; } = false;

        [Option('f', "force", Required = false, HelpText = "Will run even if another instance is running. Defaults to false.")]
        public bool Force { get; set; } = false;

        public Command CommandType { get; } = Command.START;
    }
}
