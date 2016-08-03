# TeamFlash

This is a fork of the Readify [TeamFlash](https://github.com/Readify/TeamFlash) utility which is a super simple tool for connecting TeamCity to a Delcom USB build light.

It's tested with the [Delcom 904003](http://www.delcomproducts.com/productdetails.asp?productnum=904003) and [Delcom 804003](http://www.delcomproducts.com/productdetails.asp?productnum=804003) (old generation).

## New Features
We have removed the interactive prompts and separate config file which was stored in the user profile folder, and instead we are using app settings.

The application can now be installed as a Windows Service. This is to avoid having to login and restart the app after a power failure.
