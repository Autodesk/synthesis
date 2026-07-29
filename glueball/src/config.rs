use argh::FromArgs;

const DEFAULT_PORT: u32 = 2610;

#[derive(FromArgs)]
#[argh(description = "A Server for Facilitating Multiplayer Synthesis")]
pub struct Config {
    #[argh(
        option,
        short = 'p',
        description = "on which port to run the server",
        default = "DEFAULT_PORT"
    )]
    pub port: u32,

    #[argh(
        switch,
        short = 'h',
        description = "whether to run the application without or tui or with one"
    )]
    pub headless: bool,

    #[argh(
        switch,
        short = 's',
        description = "whether to run the server through the WebSocketSecure protocol or not. self-signed PEM certficates will be automatically generated"
    )]
    pub secure: bool,

    #[argh(
        option,
        short = 'r',
        description = "initially populate the server with a room that will persist even when no users occupy it. value must be a six digit string consisting only of valid base-10 digits and uppercase characters"
    )]
    pub permanent_room: Option<String>,
}
