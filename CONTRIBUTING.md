# Contribution Guidelines
Contributing to this project is a way that you can help improve the Ingvilt application. Please note that by contributing to this project, you acknowledge that your contributions are permitted to be used in the Ingvilt UWP application (and on any other platforms that Ingvilt may be released on in the future).

## Issues
Please create issues for bugs that you have indentified in either the application front-end, or in the back-end code in this repository. Include a detailed bug report including how to reproduce the bug, and what version of Windows 10 you are using. It may also be helpful to upload a JSON file for a library export that can be used to quickly create the data required to reproduce the bug. Feature or enhancement requests should also be issues, and they should have a detailed summary of the feature or enhancement.

## Labels
Appropriate labels should be assigned to issues and PRs. Issues related to the front-end UWP code should have the *UWP* label applied to them. Issues and PRs related to the back-end code should have the *DAL* label applied to them. Other labels such as *optimization*, *bug*, *enhancement*, etc. should be applied as appropriate.

## Pull Requests
Feel free to write code and submit a PR for any open issue in the repository. There is currently not any mandatory coding style that must be followed, but please try to match the style of the existing code as much as possible. In your pull requests, link to the relevant issues that you worked on.

## Testing
There are integration tests that verify that almost all of the methods in the repository layer work correctly. When new features are added, please ensure that tests are also added to ensure that they work as intended.

## Releases
If your pull request to this repository is merged, please note that it may take a while before the changes are released as part of the Ingvilt front-end application. There is no guaranteed timeline for the release process.
