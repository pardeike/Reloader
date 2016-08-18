## RimWorld Reloader 
A mod for **RimWorld** that allows you to develop and patch code while the game is running

**Reloader** is a mod for developers. It was created with the frustration in mind that comes up if things don't work your way and you end up restarting RimWorld too many times.

So what do you need to do to get hot code reloading? The following steps are necessary to get started:

##### 1) Clone/Download this repo

You will get three directories that contain

- **Reloader**, the mod
- **TestMod**, a very simple mod, ready to be patched
- **Patch**, a patch for TestMod

###### Mods Directory

**Reloader** is a complete mod, ready to be place into RimWorld. It has no side effects when installed and as long as you don't deploy patch files, it will not do anything or affect RimWorld in any way.

Its job is to watch the Mods directory, searching for changes in any of the sub-directories named "Assemblies.reloading". So a typical mod structure might look like:

```  
Mods
+- Core
+- Reloader
   +- About
   +- Assemblies
      +- Reloader.dll
+- FooBarMod
   +- About
   +- Assemblies
      +- FooBar.dll
+- TestMod
   +- About
   +- Assemblies
      +- TestMod.dll
   +- Assemblies.reloading    <- create this directory
      +- Patch1.dll           <- deploy here
```
For now, it looks like patches will patch only the corresponding mod but that is not true. Since you define inside the patch which class you want to patch you can possibly patch anything in RimWorld in realtime. The directories are mostly grouped by mod because it is conenient to do so.

###### IDE Setup

You can basically keep your work style. All you need to do is to open two projects or have two projects in a solution. The first project is the main one and it will deploy into the Assemblies directory as usual. You don't have to change anything in your own mod project at all!

The second project will be the patch project. The idea here is that you, in your main project, develop and deploy, then start RimWorld and test. Once you want to change something without restarting RimWorld, you copy the class (or part of it) from your main project to the patch project. There, you annotate the methods that you want to change with the Attribute: 

```
[ReloadMethod]
public void SomeMethod()
```

To do this, you need to import a **reference to the Reloader.dll** into the patch project. Make sure it is set to not copy while deploying.

You can only annotate methods and you specify the full classname of the method you want to change as the first parameter. The second parameter is the method name. Currently, the attribute can take a third parameter Type[] to further specify the method but this is not supported in Reloader yet.

**Important:** Before you go and deploy the patch project, you need one last step. .NET will not load a dll if it has the same version number as a earlier dll it has loaded. So unless you increment your version every time you deploy you won't see any changes. So I recommend strongly to automate this in the patch project by using an extension (For VS2016, I like "Build Version Increment Add-in").

Now you deploy this project into the Assemblies.reloading directory, Reloader will pick it up and patch the designated method in the running mod and you will see the effect immediately. If you copy a whole class you can even patch methods within it without loosing state.

Once you are satisfied with your changes or you have hit some change that requires substantial changes in the code, you copy over the code changes to your main project and take it from there as usual.

###### TODO

Patching methods has a few pitfalls. For now, you cannot patch constructors and if you have a class A with a method you patch and a class B that instantiates class A you will not see the patch happen. What you need to do is to copy class B into your patch too and patch the method (without code change) that instantiates class A. This is because in your patch, class A is actually class A' (so A is not A sort of) and thus class B in the original code will instantiate the old class A. It's not a big deal if you copy all your code from your original mod to the patch and just annotate those things to get you what you want.

I tried to get real AppDomain load/unloading to work but failed because if you do so, all communication with the second AppDomain has to be serializable and that means that a lot of method parameters and return types would could not be supported. It's simply too much hassle. Instead, Reloader simply loads more and more dlls into memory and uses Detours to point the original method to method1, method2, etc until you run out of memory.

A second way to designate methods would be to even use annotations in your main code. By having an annotation with a unique id you would simply use that in your patch to designate the method to patch. Easier, but with the disadvantage of having to manipulate the main project and also the hassle to come up with unique ids for every method you want to replace. Not really useful but nevertheless a thought.

---

##### Feedback

If you feel that you want to contribute, please go ahead and send me pull requests, file bugs or send suggestions. I want this to be a help to everyone in the community.

---

##### License

Free. As in free beer. Copy, learn and be respectful.

---

##### Contact

Andreas Pardeike  
Email: andreas@pardeike.net  
Steam: pardeike  
Twitter: @pardeike  
Cell: +46722120680