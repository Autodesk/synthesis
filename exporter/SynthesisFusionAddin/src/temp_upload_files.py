import requests
from dataclasses import dataclass
from typing import Any 
from result import Ok, Err, Result

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


def create_folder(auth: str, project: Project, folder: Folder) -> Result[str, None]:
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
        print(f"Failed to create folder: {res.text}")
        return Err(None)
    json: dict[str, Any] = res.json()
    href: str = json["links"]["self"]["href"]
    return Ok(href)
    

def file_path_to_file_name(file_path: str) -> str:
    return file_path.split("/").pop()

def upload_mirabuf(project: Project, folder: Folder, file_path: str) -> Result[str | None, None]:
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

    _auth = "" # Get token from APS API later
    file_id_result = get_file_id(_auth, project, folder, file_path_to_file_name(file_path))
    if file_id_result.is_err():
        return Err(None)
    file_id: str | None = file_id_result.ok();
    if not file_id == None:
        print("Mirabuf file already exists!")
        update_file_result: Result[str, None] = update_file_version(_auth, project, folder, str(file_id), file_path_to_file_name(file_path), "1") # Grab real file version number
        if update_file_result.is_err():
            return Err(None)
        new_version_id = str(update_file_result.ok())
        return Ok(new_version_id)
    object_id = create_storage_location(_auth, project)
    if object_id.is_err():
        return Err(None)
    object_id = object_id.ok()
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

    file_name = file_path_to_file_name(file_path)

    if object_id == None:
        print("Object id is none; check create storage location")
        return Err(None)

    (_lineage_id, _lineage_href) = create_first_file_version(_auth, str(object_id), project.id, str(folder.id), file_name)
    
    return Ok(None)


def get_hub_id(auth: str, hub_name: str) -> Result[str | None, str]:
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
        print(f"Failed to retrieve hubs: {hub_list_res.text}")
        return Err(f"Failed to retrieve hubs: {hub_list_res.text}")
    hub_list: list[dict[str, Any]] = hub_list_res.json()
    for hub in hub_list:
        if hub["attributes"]["name"] == hub_name:
            id: str = hub["id"]
            return Ok(id)
    return Ok(None)

def get_project_id(auth: str, hub_id: str, project_name: str) -> Result[str | None, str]:
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
        print(f"Failed to retrieve hubs: {project_list_res.text}")
        return Err(f"Failed to retrieve hubs: {project_list_res.text}")
    project_list: list[dict[str, Any]] = project_list_res.json()
    for project in project_list:
        if project["attributes"]["name"] == project_name:
            id: str = project["id"]
            return Ok(id)
    return Ok(None)


"""
Updates a an existing file and returns the id of the new version
"""
def update_file_version(auth: str, project: Project, folder: Folder, file_id: str, file_name: str, curr_file_version: str) -> Result[str, None]:
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


    object_id_res = create_storage_location(auth, project)
    if object_id_res.is_err():
        return Err(None)
    object_id = object_id_res.ok()


    headers = {
        "Authorization:": f"Bearer {auth}",
        "Content-Type": "application/vnd.api+json",
        "Accept": "application/vnd.api+json"

    }

    attributes = {
        "name": file_name,
        "extension": {
            "type": "versions:autodesk.core:File",
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
    update_res = requests.post(f"https://developer.api.autodesk.com/data/v1/projects/{project.id}/versions", headers=headers, data=data)
    if not update_res.ok:
        print(f"updating file to new version failed: {update_res.text}")
        return Err(None)
    print(f"File {file_name} successfully updated to version {int(curr_file_version) + 1}")
    new_id: str = update_res.json()["data"]["id"]
    return Ok(new_id)

def get_file_id(auth: str, project: Project, folder: Folder, file_name: str) -> Result[str | None, str]:
    """
    gets the file id given a file name
    
    params:
    auth - authorization token
    project - the project reference object that the file is contain within
    folder - the folder reference object that the file is contained within
    file_name - the name of the file in APS ; ex. test.mira

    returns:
    success - the id of the file, or none if the file doesn't exist
    failure - none

    potential causes of failure:
    - incorrect auth

    notes:
    - checking if a file exists is an intended use-case
    """

    file_list_res = requests.get(f"https://developer.api.autodesk.com/data/v1/projects/{project.id}/folders/{folder.id}/contents")
    if not file_list_res.ok:
        print("Failed to get file list")
        return Err("Failed to get file list")
    file_list_json: list[dict[str, Any]] = file_list_res.json()
    for file in file_list_json:
        name: str = file["attributes"]["name"]
        if name == file_name:
            id: str = file["id"]
            print(f"Found file {name} with id: {id}")
            return Ok(id)
    return Ok(None)

def create_storage_location(auth: str, project: Project, folder: Folder, file_name: str) -> Result[str, str]:
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

def generate_signed_url(auth: str, bucket_key: str, object_key: str) -> Result[tuple[str, str], str]:
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
        "Authorization:": f"Bearer {auth}",
    }
    signed_url_res = requests.get(f"https://developer.api.autodesk.com/oss/v2/buckets/{bucket_key}/objects/{object_key}/signeds3upload", headers=headers)
    if not signed_url_res.ok:
        print("Failed to get signed url")
        return Err(f"Failed to get signed url: {signed_url_res.text}")
    signed_url_json: dict[str, str] = signed_url_res.json()
    return Ok((signed_url_json["uploadKey"], signed_url_json["urls"][0]))

def upload_file(signed_url: str, file_path: str) -> Result[None, str]:
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
        print(f"Failed to upload to signed url: {upload_response.text}")
        return Err(f"Failed to upload to signed url: {upload_response.text}")
    return Ok(None)

def complete_upload(auth: str, upload_key: str, bucket_key: str) -> Result[None, str]:
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
        "Authorization:": f"Bearer {auth}",
        "Content-Type": "application/vnd.api+json",
        "Accept": "application/vnd.api+json"
    }
    data = {
        "uploadKey": upload_key
    }

    completed_res = requests.post(f"https://developer.api.autodesk.com/oss/v2/buckets/{bucket_key}/objects/{upload_key}/signeds3upload", data=data, headers=headers)
    if not completed_res.ok:
        print(f"Failed to complete upload: {completed_res.text}")
        return Err(f"Failed to complete upload: {completed_res.text}")
    return Ok(None)

def create_first_file_version(auth: str, project_id: str, object_id: str, folder_id: str, file_name: str) -> Result[tuple[str, str], None]:
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
        "Authorization:": f"Bearer {auth}",
        "Content-Type": "application/vnd.api+json",
        "Accept": "application/vnd.api+json"

    }

    attributes = {
        "name": file_name,
        "extension": {
            "type": "items:autodesk.core:File",
            "version": "1.0" 
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
            "attributes": attributes,
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
            "relationships": relationships
        },
        "included": included
    }

    first_version_res = requests.post(f"https://developer.api.autodesk.com/data/v1/projects/{project_id}L/items", data=data, headers=headers)
    if not first_version_res.ok:
        print(f"Failed to create first file version: {first_version_res.text}")
        return Err(None)
    first_version_json: dict[str, Any] = first_version_res.json()

    lineage_id: str = first_version_json["data"]["id"]
    href: str = first_version_json["links"]["self"]["href"]
    
    return Ok((lineage_id, href))
