namespace SynthesisServer.Client {
    public interface IClientHandler {
        void HandleStatusMessage(StatusMessage statusMessage);
        void HandleServerInfoResponse(ServerInfoResponse serverInfoResponse);
        void HandleCreateLobbyResponse(CreateLobbyResponse createLobbyResponse);
        void HandleDeleteLobbyResponse(DeleteLobbyResponse deleteLobbyResponse);
        void HandleJoinLobbyResponse(JoinLobbyResponse joinLobbyResponse);
        void HandleLeaveLobbyResponse(LeaveLobbyResponse leaveLobbyResponse);
        void HandleStartLobbyResponse(StartLobbyResponse startLobbyResponse);
        void HandleSwapResponse(SwapResponse swapResponse);
        void HandleChangeNameResponse(ChangeNameResponse changeNameResponse);
        void HandleConnectionDataClient(ConnectionDataClient connectionDataClient);
        void HandleConnectionDataHost(ConnectionDataHost connectionDataHost);
    }
}