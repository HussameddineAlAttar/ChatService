# ChatAppBackend API Documentation
<details>
<summary><h2>Profile API Documentation</h2></summary>
  
## GetProfile

Retrieves the profile information for a given username.

**Endpoint:** `/api/profile/{username}`

**Method:** GET

**Parameters:**
- `{username}` (path parameter): The username of the profile to retrieve.

**Response:**
- Status: 200 OK
- Body:
  - `Username` (string): The username of the profile.
  - `Email` (string): The email address of the profile.
  - `Password` (string): The password of the profile.
  - `FirstName` (string): The first name of the profile.
  - `LastName` (string): The last name of the profile.

**Failed Request Responses:**
- Status: 404 Not Found
  - Body: Profile with username {username} not found.

## AddProfile

Creates a new profile.

**Endpoint:** `/api/profile`

**Method:** POST

**Parameters:**
- Body: The profile object containing the following fields:
  - `Username` (string, required): The username for the new profile.
  - `Email` (string, required): The email address for the new profile.
  - `Password` (string, required): The password for the new profile.
  - `FirstName` (string, required): The first name for the new profile.
  - `LastName` (string, required): The last name for the new profile.

**Response:**
- Status: 201 Created
- Headers:
  - `Location`: The URL of the newly created profile resource.
- Body:
  - `Username` (string): The username of the created profile.
  - `Email` (string): The email address of the created profile.
  - `Password` (string): The password of the created profile.
  - `FirstName` (string): The first name of the created profile.
  - `LastName` (string): The last name of the created profile.
 
**Failed Request Responses:**
- Status: 409 Conflict
  - Body: Cannot create profile.\n{error message}
</details>

<details>
<summary><h2>Image API Documentation</h2></summary>

## UploadImage

Uploads an image for a given username.

**Endpoint:** `/api/images/{username}`

**Method:** POST

**Parameters:**
- `{username}` (path parameter): The username of the user associated with the image.
- `File` (form field, required): The image file to upload.

**Response:**
- Status: 201 Created
- Headers:
  - `Location`: The URL of the newly created image.
- Body:
  - `imageId` (string): The unique identifier of the uploaded image.


## DownloadImage

Downloads the image associated with a given username.

**Endpoint:** `/api/images/{username}`

**Method:** GET

**Parameters:**
- `{username}` (path parameter): The username of the user associated with the image.

**Response:**
- Status: 200 OK
- Body: The binary content of the image file.
- Content-Type: image/png

**Failed Request Responses:**
- Status: 404 Not Found
  - Body: Image for user {username} not found.

</details>
