using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;

public class NetworkFileTransfer : NetworkBehaviour
{
    static NetworkFileTransfer Self;

    const int Channel = 0;
    const int ChunkSize = 2048;

    enum Commands
    {
        FileName,
        FileChunk
    };


    public class Command_FileName : MessageBase
    {
        public string FileName;
        public long FileSize;
    };


    public class Command_FileChunk : MessageBase
    {
        public long Sequence;
        public byte[] Chunk;
    };

    private List<string> FileQueue;
    private bool SendInProgress = false;


    public override void OnStartClient()
    {
        Self = this;

        NetworkManager.singleton.client.RegisterHandler((int)Commands.FileName, Static_OnFileName);
        NetworkManager.singleton.client.RegisterHandler((int)Commands.FileChunk, Static_OnFileChunk);
    }


    void QueueFile( string FileName )
    {
        if (SendInProgress)
        {
            FileQueue.Add(FileName);
        }
        else
        {
            CmdSendFile(FileName);
        }
    }


    void CheckNextFile( )
    {
        if ( FileQueue.Count == 0 )
        {
            return;
        }

        string TopFileName = FileQueue[0];
        FileQueue.RemoveAt(0);

        CmdSendFile(TopFileName);
    }


    private void Update()
    {
        if ( SendInProgress)
        {
            SendChunk();
        }
    }


    private BinaryReader FileReader = null;
    private long ReaderSequence = 0;

    [Command]
    void CmdSendFile( string FileName )
    {
        if (File.Exists(FileName))
        {
            var FileNameMessage = new Command_FileName();
            FileNameMessage.FileName = FileName;
            FileNameMessage.FileSize = new System.IO.FileInfo(FileName).Length;
            base.connectionToClient.Send((int)Commands.FileName, FileNameMessage);

            FileReader = new BinaryReader(File.Open(FileName, FileMode.Open));
            SendInProgress = true;
            ReaderSequence = 0;
            SendChunk();
        }
        else
        {
            CheckNextFile();
        }
    }

    private void SendChunk()
    {
        var ChunkMessage = new Command_FileChunk();
        ChunkMessage.Chunk = FileReader.ReadBytes(ChunkSize);
        if (ChunkMessage.Chunk.Length > 0)
        {
            ReaderSequence++;
            ChunkMessage.Sequence = ReaderSequence;
            base.connectionToClient.Send((int)Commands.FileChunk, ChunkMessage);
        }
        else
        {
            SendInProgress = false;
            FileReader.Close();
            CheckNextFile();
        }
    }

    private string WriterFileName = "";
    private BinaryWriter FileWriter = null;
    private long WriterSequence = 0;
    private long FileSizeRemaining = 0;

    void OnFileName( Command_FileName FileName )
    {
        FileWriter = new BinaryWriter(File.Open(FileName.FileName, FileMode.Create));
        WriterFileName = FileName.FileName;
        FileSizeRemaining = FileName.FileSize;
        WriterSequence = 1;
    }


    void OnFileChunk( Command_FileChunk FileChunk)
    {
        if ( FileChunk.Sequence != WriterSequence )
        {   // Bad!
            Debug.LogErrorFormat("File {0} expected sequence {1} got sequence {2}", WriterFileName, WriterSequence, FileChunk.Sequence );
            return;
        }

        WriterSequence++;
        FileWriter.Write(FileChunk.Chunk);
        FileSizeRemaining -= FileChunk.Chunk.Length;
        if ( FileSizeRemaining <= 0 )
        {
            FileWriter.Close();
            FileWriter = null;
        }
    }


    static void Static_OnFileName(NetworkMessage netMsg)
    {
        Self.OnFileName(netMsg.ReadMessage<Command_FileName>());
    }


    static void Static_OnFileChunk(NetworkMessage netMsg)
    {
        Self.OnFileChunk(netMsg.ReadMessage<Command_FileChunk>());
    }
}
