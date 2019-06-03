# WoADialer

A dialer based on UWP and .NET for the WoA on Lumia Project. 
It's composed of two parts: The first one is a UWP app, and the other part is a consoleapp.

The UWP app provides a graphical interface. Its objectives are:

- Allow dialing of phone numbers, through a graphical interface
- Provide an in-call UI because the OS lacks one.

The Console App serves as an helper for the UWP app, to access restricted win32 capabilities:

- It allows the UWP app to close a call by (at the moment) reloading the PhoneSvc service. This effectively closes the call locally and remotely after a few seconds. This was needed since there's no public API for UWP apps to close a call, unlike the ones to open a call or check the call status.

- It keeps listening in the background to a call to be accepted. Everytime this happens, the UWP app is opened and the in-call UI is showed.

## Requirements
The UWP requires at least Windows 10 version 1809, build 17763, and ARM64 architecture, unless you compile the app for yourself since I only can provide ARM64 packages with VS2019.

## Building
You'll need a reference to the Windows.ApplicationModel.Calls.CallsPhoneContract contract for the helper.
You'll also need to restore the NuGet packages.

This is the setup I used:

- VS 2019
- For the UWP app, the min version is 1809, while the target version is 1903.

## This readme
I must have forgot things. Please forgive me and point me to the right directions, and I'll fix it.

## This application
I surely have forgot things. I surely have implemented things really badly. Things will be broken for sure. I'm a noob, so open those issues and I'll try to fix them. If you have suggestions on how to improve certain things, feel free to send me a message, do a PR, whatever you want! It's 100% accepted! I'm a student so I have little time to work on side projects, and I still have a lot to learn, so forgive me, please!
