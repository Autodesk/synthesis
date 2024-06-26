import requests
from dataclasses import dataclass
from typing import Any 
from result import Ok, Err, Result, is_ok, is_err

# TODO: Port over full data model

@dataclass
class Folder:
    display_name: str | None
    parent_id: str | None
    id: str | None

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
    

def file_path_to_file_name(file_path: str) -> str:
    return file_path.split("/").pop()


"""
Uploads mirabuf file to APS project

TODO: Change so a folder is not needed, and the entire project is checked for files
"""
def upload_mirabuf(project: Project, folder: Folder, file_path: str) -> Result[None, None]:
    _auth = "" # Get token from APS API later
    file_id_result = get_file_id(_auth, project, folder, file_path_to_file_name(file_path))
    if file_id_result.is_err():
        return Err(None)
    file_id: str | None = file_id_result.ok();
    if not file_id == None:
        print("Mirabuf file already exists!")
        _ = update_file_version(project, folder, str(file_id))
        return Ok(None)
    object_id = create_storage_location(_auth, project)
    (prefix, object_key) = str(object_id).split("/", 1)
    bucket_key = prefix.split(":", 3)[3] # gets the last element smth like: wip.dm.prod

    generate_signed_url_result = generate_signed_url(_auth, bucket_key, object_key)
    if generate_signed_url_result.is_err():
        return Err(None)

    (upload_key, signed_url) = str(generate_signed_url_result.ok())
    if upload_file(signed_url, file_path).is_err():
        return Err(None)

    if complete_upload(_auth, upload_key, bucket_key).is_err():
        return Err(None)
    
    return Ok(None)


def update_file_version(project: Project, folder: Folder, file_id: str, file_name: str, curr_file_version: str) -> Result[None, None]:
    object_id_res = create_storage_location(project, folder)
    if object_id_res.is_err():
        return Err(None)
    object_id = object_id_res.ok()

    

    attributes = {
        "name": file_name,
        "extension": {
            "type": "file",
            "version": curr_file_version
        }
    }

    refs = {
        "data": {
            "type": "versions",
            "id": "", #version URN
            "meta": {
                "refType": "xrefs",
                "direction": "to",
                "extension": {
                    "type": "xrefs:autodesk.core:Xref",
                    "version": "1.1.0"
                }
            }
        }
    }

    relationships = {
        "item": {
            "data": {
                "type": "items",
                "id": ""#wtf some id
            }
        },
        "storage": {
            "data": {
                "type": "objects",
                "id": object_id
            }
        },
        "refs": refs
    }

    data = {
        "jsonapi": {
            "version": "1.0"
        },
        "data": {
            "type": "versions",
            "attributes": attributes,
            "relationships": relationships
        }
    }
    update_res = requests.post(f"https://developer.api.autodesk.com/data/v1/projects/{project.id}/versions", data=data)
    if not update_res.ok:
        print(f"updating file to new version failed: {update_res.text}")
        return Err(None)
    update_json = 


"""
These two functions are the same fundamentally, I couldn't decide which one I wanted
"""

"""
Gets the file id given a file name and a folder, returns none if t
"""
def get_file_id(auth: str, project: Project, folder: Folder, file_name: str) -> Result[str | None, str]:
    file_list_res = requests.get(f"https://developer.api.autodesk.com/data/v1/projects/{project.id}/folders/{folder.id}/contents")
    if not file_list_res.ok:
        print("Failed to get file list")
        # Error
        return Err("Failed to get file list")
    file_list_json: list[dict[str, Any]] = file_list_res.json()
    for file in file_list_json:
        name: str = file["attributes"]["name"]
        if name == file_name:
            id: str = file["id"]
            print(f"Found file {name} with id: {id}")
            return Ok(id)
    # File does not exist in system
    return Ok(None)

"""
Checks if a file exist in a project
This could be done if a file id was known, without having to iterate, but I'm assuming only the file name is known
"""
def check_file_exists(auto: str, project: Project, folder: Folder, file_name: str) -> Result[bool, str]:
    file_list_res = requests.get(f"https://developer.api.autodesk.com/data/v1/projects/{project.id}/folders/{folder.id}/contents")
    if not file_list_res.ok:
        print("Failed to get file list")
        # Error
        return Err("Failed to get file list")
    file_list_json: list[dict[str, Any]] = file_list_res.json()
    for file in file_list_json:
        name: str = file["attributes"]["name"]
        if name == file_name:
            id: str = file["id"]
            print(f"Found file {name} with id: {id}")
            return Ok(True)
    # File does not exist in system
    return Ok(False)

"""
End twin functions
"""


"""
Creates a storage location (bucket)

Returns the object_id, which can be split into a bucket_key and upload_key
On failure, returns the API error
"""
def create_storage_location(auth: str, project: Project) -> Result[str, str]:
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
        print(f"Failed to create storage location")
        return Err(f"Failed to create storage location: {storage_location_res.text}")
    storage_location_json: dict[str, Any]  = storage_location_res.json()
    object_id: str = storage_location_json["data"]["id"]
    return Ok(object_id)

"""
Generates a signed_url given a bucket_key and object_key
Both params are returned by create_storage_location

Returns the upload_key and the signed_url
"""
def generate_signed_url(auth: str, bucket_key: str, object_key: str) -> Result[tuple[str, str], str]:
    headers = {
        "Authorization:": f"Bearer {auth}",
    }
    signed_url_res = requests.get(f"https://developer.api.autodesk.com/oss/v2/buckets/{bucket_key}/objects/{object_key}/signeds3upload", headers=headers)
    if not signed_url_res.ok:
        print("Failed to get signed url")
        return Err(f"Failed to get signed url: {signed_url_res.text}")
    signed_url_json: dict[str, str] = signed_url_res.json()
    return Ok((signed_url_json["uploadKey"], signed_url_json["urls"][0]))

"""
Uploads a file to APS given a signed_url produced by generate_signed_url and a file_path on your machine
"""
def upload_file(signed_url: str, file_path: str) -> Result[None, str]:
    with open(file_path, 'rb') as f:
        data = f.read()
    upload_response = requests.put(url=signed_url, data=data)
    if not upload_response.ok:
        print(f"Failed to upload to signed url: {upload_response.text}")
        return Err(f"Failed to upload to signed url: {upload_response.text}")
    return Ok(None)

"""
Completes and verifies the signed file upload with the upload_key, produced by generate_signed_url, and a bucket_key
"""
def complete_upload(auth: str, upload_key: str, bucket_key: str) -> Result[None, str]:
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
        print(f"Failed to complete upload: {completed_res.text}")
        return Err(f"Failed to complete upload: {completed_res.text}")
    return Ok(None)

