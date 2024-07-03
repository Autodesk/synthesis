from dataclasses import dataclass
import pickle
from src.general_imports import root_logger, gm, INTERNAL_ID, APP_NAME, DESCRIPTION, my_addin_path
import time
import pathlib
import logging
import urllib.parse
import urllib.request
import json
import os

CLIENT_ID = 'GCxaewcLjsYlK8ud7Ka9AKf9dPwMR3e4GlybyfhAK2zvl3tU'
auth_path = os.path.abspath(os.path.join(my_addin_path, "..", ".aps_auth"))

APS_AUTH = None
APS_USER_INFO = None

@dataclass
class APSAuth:
    access_token: str
    refresh_token: str
    expires_in: int
    expires_at: int
    token_type: str

@dataclass
class APSUserInfo:
    name: str
    given_name: str
    family_name: str
    preferred_username: str
    email: str
    email_verified: bool
    profile: str
    locale: str
    country_code: str
    about_me: str
    language: str
    company: str
    picture: str

def getAPSAuth() -> APSAuth | None:
    return APS_AUTH

def _res_json(res):
    return json.loads(res.read().decode(res.info().get_param('charset') or 'utf-8'))

def getCodeChallenge() -> str | None:
    endpoint = 'http://localhost:3003/api/aps/challenge/'
    res = urllib.request.urlopen(endpoint)
    data = _res_json(res)
    return data["challenge"]

def getAuth() -> APSAuth:
    global APS_AUTH
    if APS_AUTH is not None:
        return APS_AUTH
    try:
        with open(auth_path, 'rb') as f:
            p = pickle.load(f)
            APS_AUTH = APSAuth(
                access_token=p["access_token"],
                refresh_token=p["refresh_token"],
                expires_in=p["expires_in"],
                expires_at=int(p["expires_in"]*1000),
                token_type=p["token_type"]
            )
    except:
        raise Exception("Need to sign in!")
    curr_time = int(time.time() * 1000)
    if curr_time >= APS_AUTH.expires_at:
        refreshAuthToken()
    if APS_USER_INFO is None:
        loadUserInfo()
    return APS_AUTH

def convertAuthToken(code: str):
    global APS_AUTH
    authUrl = f'http://localhost:3003/api/aps/code/?code={code}&redirect_uri={urllib.parse.quote_plus("http://localhost:3003/api/aps/exporter/")}'
    res = urllib.request.urlopen(authUrl)
    data = _res_json(res)['response']
    APS_AUTH = APSAuth(
        access_token=data["access_token"],
        refresh_token=data["refresh_token"],
        expires_in=data["expires_in"],
        expires_at=int(data["expires_in"]*1000),
        token_type=data["token_type"]
    )
    with open(auth_path, 'wb') as f:
        pickle.dump(data, f)
        f.close()

    loadUserInfo()

def removeAuth():
    global APS_AUTH, APS_USER_INFO
    APS_AUTH = None
    APS_USER_INFO = None
    pathlib.Path.unlink(pathlib.Path(auth_path))

def refreshAuthToken():
    global APS_AUTH
    if APS_AUTH is None or APS_AUTH.refresh_token is None:
        raise Exception("No refresh token found.")
    body = urllib.parse.urlencode({
        'client_id': CLIENT_ID,
        'grant_type': 'refresh_token',
        'refresh_token': APS_AUTH.refresh_token,
        'scope': 'data:read'
    }).encode('utf-8')
    req = urllib.request.Request('https://developer.api.autodesk.com/authentication/v2/token', data=body)
    req.method = 'POST'
    req.add_header(key='Content-Type', val='application/x-www-form-urlencoded')
    try:
        res = urllib.request.urlopen(req)
        data = _res_json(res)
        APS_AUTH = APSAuth(
            access_token=data["access_token"],
            refresh_token=data["refresh_token"],
            expires_in=data["expires_in"],
            expires_at=int(data["expires_in"]*1000),
            token_type=data["token_type"]
        )
    except urllib.request.HTTPError as e:
        removeAuth()
        logging.getLogger(f"{INTERNAL_ID}").error(f"Refresh Error:\n{e.code} - {e.reason}")
        gm.ui.messageBox("Please sign in again.")

def loadUserInfo() -> APSUserInfo | None:
    global APS_AUTH
    if not APS_AUTH:
        return None
    global APS_USER_INFO
    req = urllib.request.Request("https://api.userprofile.autodesk.com/userinfo")
    req.add_header(key="Authorization", val=APS_AUTH.access_token)
    try:
        res = urllib.request.urlopen(req)
        data = _res_json(res)
        APS_USER_INFO = APSUserInfo(
            name=data["name"],
            given_name=data["given_name"],
            family_name=data["family_name"],
            preferred_username=data["preferred_username"],
            email=data["email"],
            email_verified=data["email_verified"],
            profile=data["profile"],
            locale=data["locale"],
            country_code=data["country_code"],
            about_me=data["about_me"],
            language=data["language"],
            company=data["company"],
            picture=data["picture"]
        )
        return APS_USER_INFO
    except urllib.request.HTTPError as e:
        removeAuth()
        logging.getLogger(f"{INTERNAL_ID}").error(f"User Info Error:\n{e.code} - {e.reason}")
        gm.ui.messageBox("Please sign in again.")

def getUserInfo() -> APSUserInfo | None:
    if APS_USER_INFO is not None:
        return APS_USER_INFO
    return loadUserInfo()
