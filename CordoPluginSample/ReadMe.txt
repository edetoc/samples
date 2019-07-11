Followed instructions here:
https://msopentech.com/blog/2014/11/24/using-windows-runtime-component-with-cordova-project/

important note: 
in order to force VS to add MyRuntimeComponent.winmd to the Windows plateform during Build,
I had to tweak the windows.json file (under plugins folder) to specify that cordova-plugin-ericplugin is installed.

windows.json:

  "installed_plugins": {
    "cordova-plugin-whitelist": {
      "PACKAGE_NAME": "CordoPluginSample"
    },
    "cordova-plugin-ericplugin": {
      "PACKAGE_NAME": "CordoPluginSample"
    }

