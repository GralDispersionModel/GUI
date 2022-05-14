# Contributing to GRAL Graphical User Interface (GUI)
Thank you very much for developing the GRAL User Interface (GUI) further or for fixing bugs, so that the entire community can benefit from it!

Do not hesitate to contact the project maintainers at the beginning of your work. 

## Branch Configuration

```
-- main    : production and bug fixes
-- V2XXX   : release ready commits and bug fixes for the upcoming version
-- features/feature-xx: always branch from develop and delete after merging to develop
```

- *main*   branch is inteded for production release. Keep it simple and easy to rollback
- *V2XXX*  branch is for release preparation. Only for release ready commits.


## Recommended Process

If you're developing a **new feature**

1. Create a feature branch from `develop` branch
2. Branch name dependend on your new `feature`
3. When your code is ready for release, pull request to the `develop` branch
4. Delete the feature branch


If you're making a **bug fix**

1. Pull request to the `develop` branch
2. Add an issue tag in the commit message or pull request message

If you're making a **hot fix**, which has to be deployed immediately.
1. Pull request to `develop` **and** `main` branch

## I don't want to contribute, I just have a question!
Support is provided by the [Technical University of Graz, Austria](http://lampz.tugraz.at/~gral/). 

## Found a Bug?
If you find a bug in the source code, you can help us by submitting an issue to our GitHub Repository. Even better, you can submit a Pull Request with a fix or send us an E Mail.
Please test the bug fix by one ore more projects and document the changes.

## What should I know before I get started?
GRAL GUI is developed for the .NetFramework. You can use MonoDevelop for development across platforms or Visual Studio 2017 or above in Windows.

## Styleguides
We are trying to comply with the Microsoft design rules for new developments but the application has been grown historically.

### Git Commit Messages
* Use the present tense ("Add feature" not "Added feature")
* Use the imperative mood ("Change array a[] to..." not "Changes array a[] to...")
* Reference issues and pull requests liberally after the first line

