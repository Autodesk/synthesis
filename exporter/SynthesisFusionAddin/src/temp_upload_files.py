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

"""
Creates a folder on an APS project
"""
def create_folder(auth: str, project: Project, folder: Folder) -> str | None:
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
def upload_mirabuf(project: Project, folder: Folder, file_path: str) -> Result[str | None, None]:
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


"""
Returns the hub id on success and API error on failure
If the hub does not exist, it returns None
"""
def find_user_hub(auth: str, hub_name: str) -> Result[str | None, str]:
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

"""
Returns the project id on a sucess and the API error on a failure given a hub id and a project name
If the project doesn't exist, it returns None
"""
def get_project_id(auth: str, hub_id: str, project_name: str) -> Result[str | None, str]:
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

"""
Gets the file id given a file name and a folder, returns none if the file doesn't exist
Can be used to check if a file exists
"""
def get_file_id(auth: str, project: Project, folder: Folder, file_name: str) -> Result[str | None, str]:
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
        print(f"Failed to complete upload: {completed_res.text}")
        return Err(f"Failed to complete upload: {completed_res.text}")
    return Ok(None)

"""
Initializes versioning for a file

Returns the lineage id of this file and it's href
"""
def create_first_file_version(auth: str, object_id: str, project_id: str, folder_id: str, file_name: str) -> Result[tuple[str, str], None]:
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
