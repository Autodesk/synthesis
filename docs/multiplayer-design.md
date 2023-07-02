# Synthesis Multiplayer

## Design

|To Server| From Server|
|---|---|
|[Register](#register)|[Response](#response)|
|[Heartbeat](#hearbeat)||
|[Upload Data](#upload-data)|[Upload Receipt](#upload-receipt)|
|[Data Catalogue](#data-catalogue)|[Send Catalogue](#send-catalogue)|
|[Download Data](#download-data)|[Data Response](#data-response)|
|[Get Lobby Info](#get-lobby-info)|[Lobby Info](#lobby-info)|
|[Host Command](#host-command)|[Command Status](#command-status)|

---

### Register
(<-> Response)
- [Client Info](#client-info)

### Hearbeat
(-> Server)
- [Client Info](#client-info)?

### Upload Data
(<-> Upload Receipt)
- [Data Meta](#data-meta)
- byte[]

### Data Catalogue
(<-> Send Catalogue)
- ulong GUID

### Download Data
(<-> Data Response)
- ulong GUID

### Get Lobby Info
(<-> Lobby Info)
- ...

### Host Command
(<-> Command Status)
- [Command](#command)

### Select Robot
(<-> Select Robot Status)
- ulong GUID

---

### Response
(<-> Register)
- [Client Info](#client-info) (Updated)

### Upload Receipt
(<-> Upload Data)
- [Data Meta](#data-meta) (Updated)

### Send Catalogue
(<-> Data Catalogue)
- repeated MetaData

### Data Response
(<-> Download Data)
- MetaData
- byte[]

### Lobby Info
(<-> Get Lobby Info)
- repeated [Client Info](#client-info)

### Command Status
(<-> Host Command)
- int status code

### Select Robot Status
(<-> Select Robot)
- int status code

---

### Client Info
- ulong GUID
- string Name

### Data Meta
- string Name
- string Description
- PNG thumbnail?
- ulong GUID
- ulong Owner

### Command
- TBD

### Data
- One of
  - Robot
  - byte[]