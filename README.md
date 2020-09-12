# ingvilt-dal
The data access layer aspect of the Ingvilt application. The main application can be found [on the Microsoft app store](https://www.microsoft.com/en-ca/p/ingvilt/9p7jw80w0jjt#activetab=pivot:overviewtab)

### The name Ingvilt
The name for this application was primarily inspired by old norse names from the Viking age.

### Purpose
This repository serves multiple purposes:
 - It is easy to keep track of issues with the Ingvilt application here
 - It is easy for developers who use Ingvilt to use the code in this repository to quickly build up their video collections
 - Anyone can make contributions that improve the application as a whole
 - If you want new features to be added to the Ingvilt application, you can contribute the back-end code that is needed to speed up the feature's end-to-end integration

### Local setup
 - Install Visual Studio 2019 with the `Universal Windows Platform` feature enabled (the integration testing application runs with UWP)
 - git clone https://github.com/amalik2/ingvilt-dal.git
 - Open up IngviltDataAccessLayer.sln in Visual Studio
 - To run tests, open up the the "Test Explorer", and run tests by clicking the play button inside the tests explorer modal
 - To run arbitrary code using the DAL as a library, select the "DemoApp" project, and go to the `Program.cs` file. Inside of the `main` method, write your code.

### Folder structure
This repo is broken down into three parts:
 - `IngviltDataAccessLayer`: The DAL code that is used inside of the main Ingvilt application.
    - `Constants`: generic constants. Primarily, these are used for initializing columns in tables in the database
    - `Core`: Classes that are usable across the DAL and the main Ingvilt application, but do not necessarily fall under models
    - `Dto`: DTOs that are used for representing entities in the database as objects
    - `Models`: Classes and interfaces that are more detailed than DTOs
    - `Repositories`: A layer of objects that directly communicates with the SQLite database
    - `Services`: A layer that provides abstraction to the repository layer. Logic that requires accessing multiple repositories should be placed here
 - `DemoApp`: A console application that can be used to write arbitrary code. Changes in this file should not be included in your PRs, it is meant to be a playground where you can do whatever you want with the DAL code
 - `IntegrationTests`: A UWP application that runs integration tests against the DAL

### Contribution Guidelines
The contribution guidelines can be found [here](CONTRIBUTING.md). Please read them before contributing to this repository.

### License
The license for the code in this repository can be found [here](LICENSE).


