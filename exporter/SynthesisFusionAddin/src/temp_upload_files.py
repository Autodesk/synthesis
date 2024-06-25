import requests
from dataclasses import dataclass
from typing import Any 

# TODO: Port over full data model

@dataclass
class Folder:
    display_name: str | None
    parent_id: str | None

@dataclass
class Data:
    id: str
    type: str

@dataclass
class Project:
    id: str
    name: str
    folder: Folder

"""
Creates a folder on an APS project
"""
def create_folder(project: Project, folder: Folder) -> str | None:
    _auth = "" # Get token from APS API later
    headers = {
        "Authorization": f"Bearer {_auth}",
        "Content-Type": "application/vnd.api+json"

    }
    data = {
        "jsonapi": {
            "version": "1.0"
        },
        "data": {
            "type": "folders",
            "attributes": {
                "name": folder.display_name,
                "extension": {
                    "type": "folders:autodesk.core:Folder",
                    "version": "1.0"
                }
            },
            "relationships": {
                "parent": {
                    "data": {
                        "type": "folders",
                        "id": f"urn:adsk.wipprod:dm.folder:{folder.parent_id}" 
                    }
                }
            }
        }
    }

    res = requests.post(f"https://developer.api.autodesk.com/data/v1/projects/{project.id}/folders", headers=headers, data=data)
    if not res.ok:
        print("Failed to create folder")
        return None
    json: dict[str, Any] = res.json()
    href: str = json["links"]["self"]["href"]
    return href
    

"""
Uploads mirabuf file to APS project
"""
def upload_mirabuf(project: Project, folder_href: str, file_path: str) -> None:
    _auth = "" # Get token from APS API later
    object_id = create_storage_location(_auth, project, folder_href)
    (prefix, object_key) = str(object_id).split("/", 1)
    bucket_key = prefix.split(":", 3)[3] # gets the last element smth like: wip.dm.prod
    (upload_key, signed_url) = str(generate_signed_url(_auth, bucket_key, object_key))
    upload_file(signed_url, file_path)
    complete_upload(_auth, upload_key, bucket_key)


"""
Creates a storage location (bucket)

Returns the object_id, which can be split into a bucket_key and upload_key
"""
def create_storage_location(auth: str, project: Project, folder_href: str) -> str | None:
    data = {
        "jsonapi": {
            "version": "1.0"
        },
        "data": {
            "type": "objects",
            "attributes": {
              "name": "myfile.jpg"
            },
            "relationships": {
              "target": {
                "data": { "type": "folders", "id": "urn:adsk.wipprod:fs.folder:co.mgS-lb-BThaTdHnhiN_mbA" }
                }
            }
        }
    }
    headers = {
        "Authorization:": f"Bearer {auth}",
        "Content-Type": "application/vnd.api+json",
        "Accept": "application/vnd.api+json"
    }
    storage_location_res = requests.post(f"https://developer.api.autodesk.com/data/v1/projects/{project.id}/storage", data=data, headers=headers)
    if not storage_location_res.ok:
        print("Failed to create storage location")
        return None
    storage_location_json: dict[str, Any]  = storage_location_res.json()
    object_id: str = storage_location_json["data"]["id"]
    return object_id

"""
Generates a signed_url given a bucket_key and object_key
Both params are returned by create_storage_location

Returns the upload_key and the signed_url
"""
def generate_signed_url(auth: str, bucket_key: str, object_key: str) -> tuple[str, str] | None:
    headers = {
        "Authorization:": f"Bearer {auth}",
    }
    signed_url_res = requests.get(f"https://developer.api.autodesk.com/oss/v2/buckets/{bucket_key}/objects/{object_key}/signeds3upload", headers=headers)
    if not signed_url_res.ok:
        print("Failed to get signed url")
        return None
    signed_url_json: dict[str, str] = signed_url_res.json()
    return (signed_url_json["uploadKey"], signed_url_json["urls"][0])

"""
Uploads a file to APS given a signed_url produced by generate_signed_url and a file_path on your machine
"""
def upload_file(signed_url: str, file_path: str) -> None:
    with open(file_path, 'rb') as f:
        data = f.read()
    upload_response = requests.put(url=signed_url, data=data)
    if not upload_response.ok:
        print("Failed to upload to signed url")

"""
Completes and verifies the signed file upload with the upload_key, produced by generate_signed_url, and a bucket_key
"""
def complete_upload(auth: str, upload_key: str, bucket_key: str) -> None:
    headers = {
        "Authorization:": f"Bearer {auth}",
        "Content-Type": "application/vnd.api+json",
        "Accept": "application/vnd.api+json"
    }
    data = {
        "uploadKey": upload_key
    }

    completed_res = requests.post(f"https://developer.api.autodesk.com/oss/v2/buckets/{bucket_key}/objects/{upload_key}/signeds3upload", data=data, headers=headers)
    if not completed_res.ok:
        print("Failed to complete upload")
