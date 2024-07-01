import requests

def revoke_token(token: str, client_id: str) -> None:

    # If the client is private, an authorizartion header will be needed, and the client_id property can be removed.
    headers: dict[str, str] = {
        "Content-Type": "application/x-www-form-urlencoded"
    }
    data: dict[str, str] = {
        "token": token,
        "token_type_hint": "access_token",
        "client_id": client_id # determine if client is public later
    }
    revoke_res = requests.post("https://developer.api.autodesk.com/authentication/v2/revoke", headers=headers, data=data)
    if not revoke_res.ok:
        print(f"Failed to revoke token: {token}\n\t{revoke_res.text}")
        return None
