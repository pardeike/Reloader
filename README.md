## RimWorld Reloader 
A mod add-on for **RimWorld** that allows you to develop and patch code while the game is running

**Reloader** is a dll for developers. It was created with the frustration in mind that comes up if things don't work your way and you end up restarting RimWorld too many times.

So what do you need to do to get hot code reloading? The following steps are necessary to get started:

##### 1) Clone/download this repo

All you need is the file **0Reloader.dll**

###### Your mods directory

**Reloader** works its magic when you put the 0Reloader.dll into the same **Assemblies** folder as your own dll. You can either copy it in there and reference it from your project or you have it somewhere else and let the project copy it over to the Assemblies folder. It doesn't matter which way you choose.

Its job is to watch the Assemblies directory and search for changes in any of the dlls. When it detects that a dll has changed (time stamp AND dll version number must be different) it loads the dll in and patches the original methods with their new copies.

###### IDE Setup

To allow Reloader change your methods, you annotate the methods that you want to change with the Attribute *ReloadMethod*: 

```
[ReloadMethod]
public void SomeMethod()
```

To do this, you need to import a **reference to the Reloader.dll** into the patch project. Not for any functionality, but only for the attribute.

**Important:** Before you go and build project, you need one last step. .NET will not load a dll if it has the same version number as a earlier dll it has loaded. So unless you increment your version every time you deploy you won't see any changes. So I recommend strongly to automate this in your project by using an extension (For Visual Studio, i.e. "Build Version Increment Add-in") or by following the steps in this Gist: https://gist.github.com/pardeike/c36c4007b2855bc85950b59ade935bac for the free Visual Studio Community edition.

Once you rebuild your mod (I recommend building right inside the Mods directory), Reloader will pick up the changes and patch the designated method in the running mod and you will see the effect immediately. You can even edit methods without loosing their state.

Once you are satisfied with your changes just remove the Reloader.dll, the reference to it and the method attributes. You should **NOT** ship any final mod with Reloader still in it!

###### TODO

I tried to get real AppDomain load/unloading to work but failed because if you do so, all communication with the second AppDomain has to be serializable and that means that a lot of method parameters and return types would could not be supported. It's simply too much hassle. Instead, Reloader simply loads more and more dlls into memory and uses Detours to point the original method to method1, method2, etc until you run out of memory.

---

##### Feedback

If you feel that you want to contribute, please go ahead and send me pull requests, file bugs or send suggestions. I want this to be a help to everyone in the community.

---

##### License

Free. As in free beer. Copy, learn and be respectful.

If you like what I do, please consider becoming a supporter at:
https://www.patreon.com/bePatron?c=937205

---

##### Contact

Andreas Pardeike
Email: andreas@pardeike.net
Steam: pardeike
Twitter: @pardeike
