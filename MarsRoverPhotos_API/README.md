# Mars Rover API

Uses the NASA API described [here](https://api.nasa.gov) to call the Mars Rover Photos API with given days (via dates.txt) as input, returning corresponding photo images as output. The application downloads and stores each image locally in a folder associated with the requested date within the project root WatchGuard_NASA/StoredMarsRoverPhotos. Errors are stored with datetime stamp at WatchGuard_NASA/Errors deliniated in a file per date run (not date input)

## Requested Acceptance Criteria

- when complete, please send a link to your own GitHub repository in an email replying to the email from which you received this exercise (or to watchguard.eng.account.owner@motorolasolutions.com)
- you are encouraged to think of this as an incremental exercise (e.g., reply back when you have implemented the basic acceptance criteria, then again if you make further improvements or implement one or more of the bonuses)
- use the list of dates below, stored in text file "dates.txt", to pull the images that were captured on that date, by reading dates one-by-one from the file:
  - 02/27/17
  - June 2, 2018
  - Jul-13-2016
  - April 31, 2018
- language should be C#/.NET Core on the backend, and (if applicable) your choice of JavaScript framework on the frontend
- the project should run and build locally, after you submit it (each time you submit it)
- include relevant documentation (e.g., .MD file) in the repository

## Bonuses

- Bonus: unit tests, static analysis, performance tests, or any other things you feel are important to meet Acceptance Criteria for Definition of Done
- Double Bonus: have the application display the image in a web browser
- Triple Bonus: have the application run in a Docker container

## TODO

-asynchronous processing of photo downloads to reduce total runtime
-testing and analytics
-possibe Dockerization 