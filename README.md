# TeamFlash

This is a fork of the Readify [TeamFlash](https://github.com/Readify/TeamFlash) utility which is a super simple tool for connecting TeamCity to a Delcom USB build light.

It's tested with the [Delcom 904003](http://www.delcomproducts.com/productdetails.asp?productnum=904003) and [Delcom 804003](http://www.delcomproducts.com/productdetails.asp?productnum=804003) (old generation).

## New Features
We have removed the interactive prompts and separate config file which was stored in the user profile folder, and instead we are using app settings.

The application can now be installed as a Windows Service. This is to avoid having to login and restart the app after a power failure.

## Requirements
Any (recent) Windows machine running .NET Framework 4.5.2+

A suitable Delcom product (see above).

## Instructions
Edit **TeamFlash.exe.config** and insert your TeamCity settings including _serverUrl_, _username_ and _password_, and add your project inclusions and exclusions.

Run **TeamFlash.exe** to test. See **TeamFlash.log** for details. If you require verbose logging set _verbose_ to true and restart.

To install as a service run:

```
TeamFlash install
TeamFlash start
```

To uninstall the service run:

```
TeamFlash stop
TeamFlash uninstall
```
