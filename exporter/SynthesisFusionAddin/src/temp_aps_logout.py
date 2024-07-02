import requests
from result import Result, Ok, Err
import base64

def revoke_token_public(token: str, client_id: str) -> Result[None, None]:
    """
    Revoke's a public client's access or refresh token
    
    Params:
        token - The token to be revoked
        client_id - The id of the client who owns the token

    Returns:
        Success - None
        Failure - None, error message will be printed
    """
    
    # If the client is private, an authorizartion header will be needed, and the client_id property can be removed.
    headers: dict[str, str] = {
        "Content-Type": "application/x-www-form-urlencoded"
    }
    data: dict[str, str] = {
        "token": token,
        "token_type_hint": "access_token",
        "client_id": client_id
    }
    revoke_res = requests.post("https://developer.api.autodesk.com/authentication/v2/revoke", headers=headers, data=data)
    if not revoke_res.ok:
        print(f"Failed to revoke token: {token}\n\t{revoke_res.text}")
        return Err(None)
    return Ok(None)

def revoke_token_private(token: str, client_id: str, client_secret: str) -> Result[None, None]:
    """
    Revoke's a private client's access or refresh token
    
    Params:
        token - The token to be revoked
        client_id - The id of the client who owns the token
        client_secret - The client's secret for authorization 

    Returns:
        Success - None
        Failure - None, error message will be printed
    """
    
    # If the client is private, an authorizartion header will be needed, and the client_id property can be removed.
    auth: bytes = base64.b64encode(f"{client_id}:{client_secret}".encode("UTF_8"))
    headers: dict[str, str] = {
        "Content-Type": "application/x-www-form-urlencoded",
        "Authorization": f"{auth}"
    }
    data: dict[str, str] = {
        "token": token,
        "token_type_hint": "access_token",
        "client_id": client_id
    }
    revoke_res = requests.post("https://developer.api.autodesk.com/authentication/v2/revoke", headers=headers, data=data)
    if not revoke_res.ok:
        print(f"Failed to revoke token: {token}\n\t{revoke_res.text}")
        return Err(None)
    return Ok(None)

