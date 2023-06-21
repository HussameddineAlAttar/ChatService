# ChatAppBackend API Documentation
## Profile API Documentation

This README file provides documentation for the Profile API endpoints. Below you will find information about the available endpoints, their request parameters, and response parameters.

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
