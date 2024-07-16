import json
import logging
import os
import pathlib
import pickle
import time
import urllib.parse
import urllib.request
from dataclasses import dataclass
from typing import Any
import requests

from ..general_imports import (
    INTERNAL_ID,
    gm,
    my_addin_path,
)

CLIENT_ID = "GCxaewcLjsYlK8ud7Ka9AKf9dPwMR3e4GlybyfhAK2zvl3tU"
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
    return json.loads(res.read().decode(res.info().get_param("charset") or "utf-8"))


def getCodeChallenge() -> str | None:
    endpoint = "http://localhost:80/api/aps/challenge/"
    res = urllib.request.urlopen(endpoint)
    data = _res_json(res)
    return data["challenge"]


def getAuth() -> APSAuth | None:
    global APS_AUTH
    if APS_AUTH is not None:
        return APS_AUTH
    try:
        curr_time = time.time()
        with open(auth_path, "rb") as f:
            p: APSAuth = pickle.load(f)
            logging.getLogger(f"{INTERNAL_ID}").info(msg=f"{json.dumps(p.__dict__)}")
            APS_AUTH = p
    except Exception as arg:
        gm.ui.messageBox(f"ERROR:\n{arg}", "Please Sign In")
        return None
    curr_time = int(time.time() * 1000)
    if curr_time >= APS_AUTH.expires_at:
        refreshAuthToken()
    if APS_USER_INFO is None:
         _ = loadUserInfo()
    return APS_AUTH


def convertAuthToken(code: str):
    global APS_AUTH
    authUrl = f'http://localhost:80/api/aps/code/?code={code}&redirect_uri={urllib.parse.quote_plus("http://localhost:80/api/aps/exporter/")}'
    res = urllib.request.urlopen(authUrl)
    data = _res_json(res)["response"]
    curr_time = time.time()
    APS_AUTH = APSAuth(
        access_token=data["access_token"],
        refresh_token=data["refresh_token"],
        expires_in=data["expires_in"],
        expires_at=int(curr_time + data["expires_in"] * 1000),
        token_type=data["token_type"],
    )
    with open(auth_path, "wb") as f:
        pickle.dump(APS_AUTH, f)
        f.close()

    _ = loadUserInfo()


def removeAuth():
    global APS_AUTH, APS_USER_INFO
    APS_AUTH = None
    APS_USER_INFO = None
    pathlib.Path.unlink(pathlib.Path(auth_path))


def refreshAuthToken():
    global APS_AUTH
    if APS_AUTH is None or APS_AUTH.refresh_token is None:
        raise Exception("No refresh token found.")
    body = urllib.parse.urlencode(
        {
            "client_id": CLIENT_ID,
            "grant_type": "refresh_token",
            "refresh_token": APS_AUTH.refresh_token,
            "scope": "data:create data:write data:search",
        }
    ).encode("utf-8")
    req = urllib.request.Request("https://developer.api.autodesk.com/authentication/v2/token", data=body)
    req.method = "POST"
    req.add_header(key="Content-Type", val="application/x-www-form-urlencoded")
    try:
        res = urllib.request.urlopen(req)
        data = _res_json(res)
        curr_time = time.time()
        APS_AUTH = APSAuth(
            access_token=data["access_token"],
            refresh_token=data["refresh_token"],
            expires_in=data["expires_in"],
            expires_at=int(curr_time + data["expires_in"] * 1000),
            token_type=data["token_type"],
        )
        with open(auth_path, "wb") as f:
            pickle.dump(APS_AUTH, f)
            f.close()
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
            picture=data["picture"],
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

def create_folder(auth: str, project_id: str, parent_folder_id: str, folder_display_name: str) -> str | None:
    """
    creates a folder on an APS project

    params:
    auth - auth token
    project - project blueprint; might be changed to just the project id
    folder - the blueprint for the new folder

    returns:
    success - the href of the new folder ; might be changed to the id in the future
    failure - none if the API request fails ; the failure text will be printed
    """
    headers = {
        "Authorization": f"Bearer {auth}",
        "Content-Type": "application/vnd.api+json"

    }
    data: dict[str, Any] = {
        "jsonapi": {
            "version": "1.0"
        },
        "data": {
            "type": "folders",
            "attributes": {
                "name": folder_display_name,
                "extension": {
                    "type": "folders:autodesk.core:Folder",
                    "version": "1.0"
                }
            },
            "relationships": {
                "parent": {
                    "data": {
                        "type": "folders",
                        "id": f"{parent_folder_id}"
                    }
                }
            }
        }
    }

    res = requests.post(f"https://developer.api.autodesk.com/data/v1/projects/{project_id}/folders", headers=headers, data=data)
    if not res.ok:
        gm.ui.messageBox("", f"Failed to create folder: {res.text}")
        return None
    json: dict[str, Any] = res.json()
    href: str = json["links"]["self"]["href"]
    return href

def file_path_to_file_name(file_path: str) -> str:
    return file_path.split("/").pop()

def upload_mirabuf(project_id: str, folder_id: str, file_path: str) -> str | None:
    """
    uploads mirabuf file to a specific folder in an APS project
    the folder and project must be created and valid
    if the file has already been created, it will use the APS versioning API to upload it as a new file version

    parameters:
    project - the project reference object, used for it's id ; may be changed to project_id in the future
    folder - the folder reference object, used for it's id ; may be changed to folder_id in the future
    file_path - the path to the file on your machine, to be uploaded to APS

    returns:
    success - if the file already exists, the new version id, otherwise, None
    failure - none ; the cause of the failure will be printed

    potential causes of failure:
    - invalid auth
    - incorrectly formatted requests
    - API update
    - API down

    notes:
    - this function is janky as hell, it should bubble errors up but I'm super lazy
    - check appropriate called function ~~if~~ when this function fails

    todo: Change so a folder is not needed, and the entire project is checked for files
    """

    # data:create
    global APS_AUTH
    if APS_AUTH is None:
        gm.ui.messageBox("You must login to upload designs to APS", "USER ERROR")
    auth = APS_AUTH.access_token
    # Get token from APS API later
    
    # Check if file is already on aps
    file_name = file_path_to_file_name(file_path)
    (lineage_id, file_id, file_version) = get_file_id(auth, project_id, folder_id, file_name)
    if file_id is not "":
        _ = update_file_version(auth, project_id, folder_id, lineage_id, file_id, file_path, file_version)
        return ""

    """
    Create APS Storage Location
    """
    object_id = create_storage_location(auth, project_id, folder_id, file_name)
    if object_id is None:
        gm.ui.messageBox("UPLOAD ERROR", "Object id is none; check create storage location")
        return None
    (prefix, object_key) = str(object_id).split("/", 1)
    bucket_key = prefix.split(":", 3)[3] # gets the last element smth like: wip.dm.prod

    """
    Create Signed URL For APS Upload
    """
    generate_signed_url_result = generate_signed_url(auth, bucket_key, object_key)
    if generate_signed_url_result is None:
        return None

    (upload_key, signed_url) = generate_signed_url_result
    if upload_file(signed_url, file_path) is None:
        return None

    """
    Finish Upload and Initialize First File Version
    """
    if complete_upload(auth, upload_key, object_key, bucket_key) is None:
        return None
    lineage_info = create_first_file_version(auth, str(object_id), project_id, str(folder_id), file_name)

    with open(f"lineage_{file_id}", "wb") as f:
        _ = pickle.dump(lineage_info, f)

    return ""

def get_hub_id(auth: str, hub_name: str) -> str | None:
    """
    gets a user's hub based on a hub name

    params:
    auth - authorization token
    hub_name - the name of the desired hub

    returns:
    success - the hub's id or none if the hub doesn't exist
    failure - the API text if there's an error
    """

    headers = {
        "Authorization": f"Bearer {auth}"
    }
    hub_list_res = requests.get("https://developer.api.autodesk.com/project/v1/hubs", headers=headers)
    if not hub_list_res.ok:
        gm.ui.messageBox("UPLOAD ERROR", f"Failed to retrieve hubs: {hub_list_res.text}")
        return None
    hub_list: list[dict[str, Any]] = hub_list_res.json()
    for hub in hub_list:
        if hub["attributes"]["name"] == hub_name:
            id: str = hub["id"]
            return id
    return ""

def get_project_id(auth: str, hub_id: str, project_name: str) -> str | None:
    """
    gets a project in a hub with a project name

    params:
    auth - authorization token
    hub_id - the id of the hub
    project_name - the name of the desired project

    returns:
    success - the project's id or none if the project doesn't exist
    failure - the API text if there's an error

    notes:
    - a hub_id can be derived from it's name with the get_hub_id function
    """

    headers = {
        "Authorization": f"Bearer {auth}"
    }
    project_list_res = requests.get(f"https://developer.api.autodesk.com/project/v1/hubs/{hub_id}/projects", headers=headers)
    if not project_list_res.ok:
        gm.ui.messageBox("UPLOAD ERROR", f"Failed to retrieve hubs: {project_list_res.text}")
        return None
    project_list: list[dict[str, Any]] = project_list_res.json()
    for project in project_list:
        if project["attributes"]["name"] == project_name:
            id: str = project["id"]
            return id
    return ""


def update_file_version(auth: str, project_id: str, folder_id: str, lineage_id: str, file_id: str, file_path: str, curr_file_version: str) -> str| None:
    """
    updates an existing file in an APS folder

    params:
    auth - authorization token
    project - the project reference object that the file is contain within
    folder - the folder reference object that the file is contained within
    file_id - the id of the file in APS
    file_name - the name of the file in APS ; ex. test.mira

    returns:
    success - the new version_id
    failure - none

    potential causes of failure:
    - invalid auth
    - file doesn't exist in that position / with that id / name ; fix: get_file_id() or smth
    - version one of the file hasn't been created ; fix: create_first_file_version()
    """

    file_name = file_path_to_file_name(file_path)

    object_id = create_storage_location(auth, project_id, folder_id, file_name)
    if object_id is None: 
        return None
    
    (prefix, object_key) = str(object_id).split("/", 1)
    bucket_key = prefix.split(":", 3)[3] # gets the last element smth like: wip.dm.prod
    (upload_key, signed_url) = generate_signed_url(auth, bucket_key, object_key)
    
    if upload_file(signed_url, file_path) is None:
        return None

    if complete_upload(auth, upload_key, object_key, bucket_key) is None:
        return None


    gm.ui.messageBox(f"file_name:{file_name}\nfile_id:{file_id}\ncurr_file_version:{curr_file_version}\nobject_id:{object_id}", "REUPLOAD ARGS")
    headers = {
        "Authorization": f"Bearer {auth}",
        "Content-Type": "application/vnd.api+json",
    }

    attributes = {
        "name": file_name,
        "extension": {
            "type": "versions:autodesk.core:File",
            "version": f"{curr_file_version}.0"
        }
    }

    relationships: dict[str, Any] = {
        "item": {
            "data": {
                "type": "items",
                "id": lineage_id,
            }
        },
        "storage": {
            "data": {
                "type": "objects",
                "id": object_id,
            }
        },
    }

    data = {
        "jsonapi": {
            "version": "1.0"
        },
        "data": {
            "type": "versions",
            "attributes": attributes,
            "relationships": relationships
        },
    }
    update_res = requests.post(f"https://developer.api.autodesk.com/data/v1/projects/{project_id}/versions", headers=headers, json=data)
    if not update_res.ok:
        gm.ui.messageBox(f"REUPLOAD ERROR:\n{update_res.text}", "Updating file to new version failed")
        return None
    gm.ui.messageBox("REUPLOAD SUCCESS", f"File {file_name} successfully updated to version {int(curr_file_version) + 1}")
    new_id: str = update_res.json()["data"]["id"]
    return new_id

def get_file_id(auth: str, project_id: str, folder_id: str, file_name: str) -> tuple[str, str, str] | None:
    """
    gets the file id given a file name

    params:
    auth - authorization token
    project - the project reference object that the file is contain within
    folder - the folder reference object that the file is contained within
    file_name - the name of the file in APS ; ex. test.mira

    returns:
    success - the id of the file and it's current version, or an empty tuple string if the file doesn't exist
    failure - none

    potential causes of failure:
    - incorrect auth

    notes:
    - checking if a file exists is an intended use-case
    """

    headers: dict[str, str] = {
        "Authorization": f"Bearer {auth}"
    }

    params = {
        "filter[attributes.name]": file_name
    }

    file_res = requests.get(f"https://developer.api.autodesk.com/data/v1/projects/{project_id}/folders/{folder_id}/search", headers=headers, params=params)
    if file_res.status_code is 404:
        return ("", "")
    elif not file_res.ok:
        gm.ui.messageBox(f"UPLOAD ERROR: {file_res.text}", "Failed to get file")
        return None
    file_json: list[dict[str, Any]] = file_res.json()
    id: str = str(file_json["data"][0]["id"])
    lineage: str = str(file_json["data"][0]["relationships"]["item"]["data"]["id"])
    version: str = str(file_json["data"][0]["attributes"]["versionNumber"])
    return (lineage, id, version)


def create_storage_location(auth: str, project_id: str, folder_id: str, file_name: str) -> str| None:
    """
    creates a storage location (a bucket)
    the bucket can be used to upload a file to
    every file must have a reserved storage location
    I believe at the moment, object, bucket, and storage location are all used semi-interchangeably by APS documentation

    params:
    auth - authorization token
    project - a project reference object used for project id ; may be changed to project_id later
    folder - a folder reference object used for project id ; may be changed to folder_id later
    file_name - the name of the file to be later stored in the bucket

    returns:
    success - the object_id of the bucket, which can be split into a bucket_key and upload_key
    failure - the API failure text

    notes:
    - fails if the project doesn't exist or auth is invalid
    - the folder must be inside the project, the storage location will be inside the folder
    """

    data = {
        "jsonapi": {
            "version": "1.0"
        },
        "data": {
            "type": "objects",
            "attributes": {
              "name": file_name
            },
            "relationships": {
                "target": {
                    "data": { "type": "folders", "id": f"{folder_id}" }
                }
            }
        }
    }
    headers = {
        "Authorization": f"Bearer {auth}",
        "Content-Type": "application/vnd.api+json",
    }
    storage_location_res = requests.post(f"https://developer.api.autodesk.com/data/v1/projects/{project_id}/storage", json=data, headers=headers)
    if not storage_location_res.ok:
        gm.ui.messageBox(f"UPLOAD ERROR: {storage_location_res.text}", f"Failed to create storage location")
        return None
    storage_location_json: dict[str, Any]  = storage_location_res.json()
    object_id: str = storage_location_json["data"]["id"]
    return object_id

def generate_signed_url(auth: str, bucket_key: str, object_key: str) -> tuple[str, str] | None:
    """
    generates a signed_url for a bucket, given a bucket_key and object_key

    params:
    auth - authorization token
    bucket_key - the key of the bucket that the file will be stored in
    object_key - the key of the object that the file will be stored in

    returns:
    success - the upload_key and the signed_url
    failure - the API error

    notes:
    - fails if auth, the bucket, or object keys are invalid
    - both params are returned by the create_storage_location function
    """

    headers = {
        "Authorization": f"Bearer {auth}",
    }
    signed_url_res = requests.get(f"https://developer.api.autodesk.com/oss/v2/buckets/{bucket_key}/objects/{object_key}/signeds3upload", headers=headers)
    if not signed_url_res.ok:
        gm.ui.messageBox(f"UPLOAD ERROR: {signed_url_res.text}","Failed to get signed url")
        return None
    signed_url_json: dict[str, str] = signed_url_res.json()
    return (signed_url_json["uploadKey"], signed_url_json["urls"][0])

def upload_file(signed_url: str, file_path: str) -> str |None:
    """
    uploads a file to APS given a signed_url a path to the file on your machine

    params:
    signed_url - the url to used to upload the file to a specific bucket ; returned by the generate_signed_url function
    file_path - the path of the file to be uploaded

    returns:
    success - none
    failure - the API error

    notes:
    - fails if the auth or the signed URL are invalid
    """

    with open(file_path, 'rb') as f:
        data = f.read()
    upload_response = requests.put(url=signed_url, data=data)
    if not upload_response.ok:
        gm.ui.messageBox("UPLOAD ERROR", f"Failed to upload to signed url: {upload_response.text}")
        return None
    return ""

def complete_upload(auth: str, upload_key: str, object_key: str, bucket_key: str) -> str |None:
    """
    completes and verifies the APS file upload given the upload_key

    params:
    auth - authorization token
    upload_key - the key to verify the upload, returned by generate_signed_url function
    bucket_key - the key of the bucket that the file was uploaded to, returned by the create_storage_location function

    returns:
    success - none
    failure - the API error
    """

    headers = {
        "Authorization": f"Bearer {auth}",
        "Content-Type": "application/json",
    }
    data = {
        "uploadKey": upload_key
    }

    completed_res = requests.post(f"https://developer.api.autodesk.com/oss/v2/buckets/{bucket_key}/objects/{object_key}/signeds3upload", json=data, headers=headers)
    if not completed_res.ok:
        gm.ui.messageBox(f"UPLOAD ERROR: {completed_res.text}\n{completed_res.status_code}", "Failed to complete upload")
        return None
    return ""

def create_first_file_version(auth: str, object_id: str, project_id: str, folder_id: str, file_name: str) -> tuple[str, str]| None:
    """
    initializes versioning for a file

    params:
    auth - authorization token
    project_id - the id of the project the file was uploaded to
    object_id - the id of the object the file was uploaded to
    folder_id - the id of the folder the file was uploaded to
    file_name - the name of the file

    returns:
    success - the lineage id of the versioning history of the file and the href to the new version
    failure - none

    potential causes of failure
    - incorrect auth
    - the named file's upload was never completed
    - invalid project, object, or folder id

    notes:
    - super complex request, probably not written correctly, likely a dev error
    """

    headers = {
        "Authorization": f"Bearer {auth}",
        "Content-Type": "application/vnd.api+json",
        "Accept": "application/vnd.api+json"

    }

    included_attributes = {
        "name": file_name,
        "extension": {
            "type": "versions:autodesk.core:File",
            "version": "1.0"
        }
    }

    attributes = {
        "displayName": file_name,
        "extension": {
            "type": "items:autodesk.core:File",
            "version": "1.0",
        }
    }

    relationships = {
        "tip": {
            "data": {
                "type": "versions",
                "id": "1"
            }
        },
        "parent": {
            "data": {
                "type": "folders",
                "id": folder_id
            }
        }
    }

    included = [
        {
            "type": "versions",
            "id": "1",
            "attributes": included_attributes,
            "relationships": {
                "storage": {
                    "data": {
                        "type": "objects",
                        "id": object_id
                    }
                }
            }
        },
    ]

    data = {
        "jsonapi": {
            "version": "1.0"
        },
        "data": {
            "type": "items",
            "attributes": attributes,
            "relationships": relationships
        },
        "included": included
    }

    first_version_res = requests.post(f"https://developer.api.autodesk.com/data/v1/projects/{project_id}/items", json=data, headers=headers)
    if not first_version_res.ok:
        gm.ui.messageBox(f"Failed to create first file version: {first_version_res.text}", "UPLOAD ERROR")
        return None
    first_version_json: dict[str, Any] = first_version_res.json()

    lineage_id: str = first_version_json["data"]["id"]
    href: str = first_version_json["links"]["self"]["href"]

    return (lineage_id, href)
