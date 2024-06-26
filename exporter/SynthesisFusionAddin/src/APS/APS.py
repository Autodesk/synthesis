from dataclasses import dataclass
import pickle
from src.general_imports import root_logger, gm, INTERNAL_ID, APP_NAME, DESCRIPTION, my_addin_path
import time
import webbrowser
import logging
import urllib.parse
import urllib.request
import json
from src.UI.OsHelper import getOSPath
import os
import adsk.core, traceback

CLIENT_ID = 'GCxaewcLjsYlK8ud7Ka9AKf9dPwMR3e4GlybyfhAK2zvl3tU'
auth_path = os.path.abspath(os.path.join(my_addin_path, "..", ".aps_auth"))

APS_AUTH = None
APS_USER_INFO = None

@dataclass
class APSAuth:
    access_token: str
    refresh_token: str
    expires_in: int
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

def getCodeChallenge() -> str:
    endpoint = 'http://localhost:3003/api/aps/challenge/'
    res = urllib.request.urlopen(endpoint)
    data = _res_json(res)
    logging.getLogger(f"{INTERNAL_ID}").info(f"CHALLENGE: {data}")
    return data["challenge"]

def getAuth() -> APSAuth:
    global APS_AUTH
    if APS_AUTH is not None:
        return APS_AUTH
    try:
        with open(auth_path, 'rb') as f:
            p = pickle.load(f)
            logging.getLogger(f"{INTERNAL_ID}").info(f"LOADING PICKLED FILE: {p}")
            APS_AUTH = APSAuth(
                access_token=p["access_token"],
                refresh_token=p["refresh_token"],
                expires_in=p["expires_in"],
                token_type=p["token_type"]
            )
    except:
        raise Exception("Need to sign in!")
    if APS_USER_INFO is None:
        loadUserInfo()
    return APS_AUTH

def convertAuthToken(code: str):
    global APS_AUTH
    authUrl = f'http://localhost:3003/api/aps/code/?code={code}&redirect_uri={urllib.parse.quote_plus("http://localhost:3003/api/aps/exporter/")}'
    res = urllib.request.urlopen(authUrl)
    data = _res_json(res)['response']
    logging.getLogger(f"{INTERNAL_ID}").info(f"AUTH TOKEN: {data}")
    APS_AUTH = APSAuth(
        access_token=data["access_token"],
        refresh_token=data["refresh_token"],
        expires_in=data["expires_in"],
        token_type=data["token_type"]
    )
    with open(auth_path, 'wb') as f:
        pickle.dump(data, f)
        f.close()

    loadUserInfo()

def loadUserInfo() -> APSUserInfo | None:
    global APS_AUTH
    if not APS_AUTH:
        return None
    global APS_USER_INFO
    req = urllib.request.Request("https://api.userprofile.autodesk.com/userinfo")
    req.add_header(key="Authorization", val=APS_AUTH.access_token)
    res = urllib.request.urlopen(req)
    data = _res_json(res)
    logging.getLogger(f"{INTERNAL_ID}").info(f"USER INFO: {data}")
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

def getUserInfo() -> APSUserInfo | None:
    if APS_USER_INFO is not None:
        return APS_USER_INFO
    return loadUserInfo()
