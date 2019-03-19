# UPM Package Starter Kit

The purpose of this package template starter kit is to provide the data structure and development guidelines for new packages meant for the **Unity Package Manager (UPM)**.

This is the first of many steps towards an automated package publishing experience within Unity. This package template starter kit is merely a fraction of the creation, edition, validation, and publishing tools that we will end up with.

We hope you enjoy your experience. You can use **#devs-packman** on Slack to provide feedback or ask questions regarding your package development efforts.

## Are you ready to become a package?
The Package Manager is a work-in-progress for Unity and, in that sense, there are a few criteria that must be met for your package to be considered on the package list at this time:
- **Your code accesses public Unity C# APIs only.**  If you have a native code component, it will need to ship with an official editor release.  Internal API access might eventually be possible for Unity made packages, but not at this time.
- **Your code doesn't require security, obfuscation, or conditional access control.**  Anyone should be able to download your package and access the source code.
- **You have no urgent need to release your package.**  Our current target for new packages is aligned with 2018.1. Although, based on upcoming package requests and limited packman capacity, that release date is not assured for any package.
- **You are willing to bleed with us a little!** Packman is still in development, and therefore has a few rough edges that will require patience and workarounds.

## Package structure

```none
<root>
  ├── package.json
  ├── README.md
  ├── CHANGELOG.md
  ├── LICENSE.md
  ├── QAReport.md
  ├── Editor
  │   ├── Unity.[YourPackageName].Editor.asmdef
  │   └── EditorExample.cs
  ├── Runtime
  │   ├── Unity.[YourPackageName].asmdef
  │   └── RuntimeExample.cs
  ├── Tests
  │   ├── Editor
  │   │   ├── Unity.[YourPackageName].Editor.Tests.asmdef
  │   │   └── EditorExampleTest.cs
  │   └── Runtime
  │       ├── Unity.[YourPackageName].Tests.asmdef
  │       └── RuntimeExampleTest.cs
  ├── Samples
  │   └── SampleExample.cs
  └── Documentation
      ├── your-package-name.md
      └── Images
```

## Develop your package
Package development works best within the Unity Editor.  Here's how to set that up:

1. Fork the `package-template` repository

    Forking a repository is a simple two-step process.
    - On Gitlab, navigate to the [upm-packages](https://gitlab.internal.unity3d.com/upm-packages), and make sure you have a sub-group for your feature area.  If you don't have one, you can request a new sub-group in #devs-packman.
    - Head over to the [upm-packages/package-template](https://gitlab.internal.unity3d.com/upm-packages/upm-package-template) repository. Click on the **Fork** button at the top-left corner of the page, and follow the instructions to fork the starter kit template into your sub-group.

    That's it! You now have your own copy (fork) of the original `upm-packages/package-template` repository you can use to develop your package.

1. Start **Unity**, create a local empty project.

1. In a console (or terminal) application, go to the newly created project folder, then clone your newly forked repository into the packages directory.
    ```none
    cd <YourProjectPath>/Packages
    git clone https://gitlab.internal.unity3d.com/upm-packages/[sub-group]/[your-package-name].git com.unity.[sub-group].[your-package-name]
    ```
    __Note:__ Your directory name must be the name of your package (Example: `"com.unity.terrain-builder"`)
1. Fill in your package information

	Update the following required fields in file **package.json**:
	- `"name"`: Package name, it should follow this naming convention: `"com.unity.[sub-group].[your-package-name]"`
	(Example: `"com.unity.2d.animation"`, where `sub-group` should match the sub-group you selected in Gitlab)
	- `"displayName"`: Package user friendly display name. (Example: `"Terrain Builder SDK"`). <br>__Note:__ Use a display name that will help users understand what your package is intended for.
	- `"version"`: Package version `"X.Y.Z"`, your project **must** adhere to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).
		Follow this guideline:
		- To introduce a new feature or bug fix, increment the minor version (X.**Y**.Z)
		- To introduce a breaking API change, increment the major version (**X**.Y.Z)
		- The patch version (X.Y.**Z**), is reserved for sustainable engineering use only.
	- `"unity"`: Unity Version your package is compatible with. (Example: `"2018.1"`)
	- `"description"`: This description appears in the Package Manager window when the user selects this package from the list. For best results, use this text to summarize what the package does and how it can benefit the user.<br>__Note:__ Special formatting characters are supported, including line breaks (`\n`) and unicode characters such as bullets (`\u25AA`).<br>For more information, see the [Writing Package docs](https://confluence.hq.unity3d.com/display/DOCS/Writing+Package+docs) page on Confluence.

	Update the following recommended fields in file **package.json**:
	- `"dependencies"`: List of packages this package depends on.  All dependencies will also be downloaded and loaded in a project with your package.  Here's an example:
        ```
        dependencies: {
          "com.unity.ads": "1.0.0"
          "com.unity.analytics": "2.0.0"
        }
        ```
	- `"keywords"`: List of words that will be indexed by the package manager search engine to facilitate discovery.
	- `"category"`: List of Unity defined categories used for browsing and filtering (**In Development**)

1. Restart Unity. For more information on embedded packages see [here](https://confluence.hq.unity3d.com/display/PAK/How+to+embed+a+package+in+your+project).

1. If on 2018.1 - Enable package support in the editor (*Internal Feature*).  From the **Project** window's right hang menu, enable `DEVELOPER`->`Show Packages in Project Window` (*only available in developer builds*).  You should now see your package in the Project Window, along with all other available packages for your project.

1. Update **README.md**

    The README.md file should contain all pertinent information for developers using your package, such as:
	* Prerequistes
	* External tools or development libraries
	* Required installed Software
	* Command line examples to build, test, and run your package.

1. Rename and update **your-package-name.md** documentation file.

    Use this template to create preliminary, high-level documentation. This document is meant to introduce users to the features and sample files included in your package.

1. Rename and update assembly definition files.

	If your package contains Editor code, rename and modify [Editor/Unity.YourPackageName.Editor.asmdef](Editor/Unity.YourPackageName.Editor.asmdef). Otherwise, delete the Editor directory.
	* Name **must** match your package name, suffixed by `.Editor` (i.e `Unity.[YourPackageName].Editor`)
	* Assembly **must** reference `Unity.[YourPackageName]` (if you have any Runtime)
	* Platforms **must** include `"Editor"`


	If your package contains code that needs to be included in Unity runtime builds, rename and modify [Runtime/Unity.YourPackageName.asmdef](Runtime/Unity.YourPackageName.asmdef). Otherwise, delete the Runtime directory.
	* Name **must** match your package name (i.e `Unity.[YourPackageName]`)

	If your package has Editor code, you **must** have Editor Tests. In that case, rename and modify [Tests/Editor/Unity.YourPackageName.Editor.Tests.asmdef](Tests/Editor/Unity.YourPackageName.Editor.Tests.asmdef).
	* Name **must** match your package name, suffixed by `.Editor.Tests` (i.e `Unity.[YourPackageName].Editor.Tests`)
	* Assembly **must** reference `Unity.[YourPackageName].Editor` and `Unity.[YourPackageName]` (if you have any Runtime)
	* Platforms **must** include `"Editor"`
	* Optional Unity references **must** include `"TestAssemblies"` to allow your Editor Tests to show up in the Test Runner/run on Katana when your package is listed in project manifest `testables`

	If your package has Runtime code, you **must** have Playmode Tests. In that case, rename and modify [Tests/Runtime/Unity.YourPackageName.Tests.asmdef](Tests/Runtime/Unity.YourPackageName.Tests.asmdef).
	* Name **must** match your package name, suffixed by `.Tests` (i.e `Unity.[YourPackageName].Tests`)
	* Assembly **must** reference `Unity.[YourPackageName]`
	* Optional Unity references **must** include `"TestAssemblies"` to allow your Playmode Tests to show up in the Test Runner/run on Katana when your package is listed in project manifest `testables`

>
>  The reason for choosing such name schema is to ensure that the name of the assembly built based on *assembly definition file* (_a.k.a .asmdef_) will follow the .Net [Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/index)

1. Document your package.

	**Document your public APIs**
	* All public APIs need to be documented with XmlDoc.  If you don't need an API to be accessed by clients, mark it as internal instead.
	* API documentation is generated from [XmlDoc tags](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/xml-documentation-comments) included with all public APIs found in the package. See [Editor/EditorExample.cs](Editor/EditorExample.cs) for an example.

	**Document your features**
    * All packages that expose UI in the editor or runtime features should use the documentation template in [Documentation/your-package-name.md](Documentation/your-package-name.md).

	**Documentation flow**
	* Documentation needs to be ready when a publish request is sent to Release Management, as they will ask the documentation team to review it.
	* The package will remain in `preview` mode until the final documentation is completed.  Users will have access to the developer-generated documentation only in preview packages.
	* When the documentation is completed, the documentation team will update the package git repo with the updates and they will publish it on the web.
	* The package's development team will then need to submit a new package version with updated docs.
	* The starting page in the user manual that links to package documentation is [Here](http://docs.hq.unity3d.com/2018.1/Documentation/Manual/PackagesList.html).

1. Design Guidelines

 * You should follow these design guideline when creating your package
   * [Package design guidelines](https://confluence.hq.unity3d.com/display/UX/Packages)
   * [Unity design checklist](https://unitytech.github.io/unityeditor-hig/topics/checklist.html)

 * The namespace for code in the asmdef **must** match the asmdef name, except the initial `Unity`, which should be replaced with `UnityEngine` or `UnityEditor`:
   * Runtime codeWe should only use the `Unity` namespace for code that has no dependency on anything in `UnityEngine` or `UnityEditor` and instead uses `ECS` and other `Unity`-namespace systems.

2. Add tests to your package.

	**Editor tests**
	* Write all your Editor Tests in `Tests/Editor`
	* If your tests require access to internal methods, add an `AssemblyInfo.cs` file to your `Editor` code and use `[assembly: InternalsVisibleTo("Unity.[YourPackageName].Editor.Tests")]`

	**Playmode Tests**
	* Write all your Playmode Tests in `Tests/Runtime`.
	* If your tests require access to internal methods, add an `AssemblyInfo.cs` file to your `Runtime` code and use `[assembly: InternalsVisibleTo("Unity.[YourPackageName].RuntimeTests")]`

3. Update **CHANGELOG.md**

	Every new feature or bug fix should have a trace in this file. For more details on the chosen changelog format, see [Keep a Changelog](http://keepachangelog.com/en/1.0.0/).

## Create a Pre-Release Package
Pre-Release Packages are a great way of getting your features in front of Unity Developers in order to get early feedback on functionality and UI designs.  Pre-Release packages need to go through the publishing to production flow, as would any other package, but with diminished requirements.  Here are the supported Pre-Release tags (to be used in package.json,`version` field), along with the requirements for each one:

**Preview**  -  ex: `"version" : "1.2.0-preview"`
  * Expected Package structure respected
  * Package loads in Unity Editor without errors
  * License file present
  * Test coverage is good - Optional but preferred
  * Public APIs documented, minimal feature docs exists- Optional but preferred

## Register your package

If you think you are working on a feature that is a good package candidate, please take a minute to fill-in this form: https://docs.google.com/forms/d/e/1FAIpQLSedxgDcIyf1oPyhWegp5FBvMm63MGAopeJhHDT5bU_BkFPNIQ/viewform?usp=sf_link.

Working with the board of dev directors and with product management, we will schedule the entry of the candidates in the ecosystem, based on technical challenges and on our feature roadmap.
Don’t hesitate to reach out and join us on **#devs-packman** on Slack.

## Share your package

If you want to share your project with other developers, the steps are similar to what's presented above. On the other developer's machine:

1. Start **Unity**, create a local empty project.

1. Launch console (or terminal) application, go to the newly created project folder, then clone your repository in the packages directory

    ```none
    cd <YourProjectPath>/Packages
    git clone https://gitlab.internal.unity3d.com/upm-packages/[sub-group]/[your-package-name].git com.unity.[sub-group].[your-package-name]
    ```
    __Note:__ Your directory name must be the name of your package (Example: `"com.unity.terrain-builder"`)
    
## Make sure your package meets all legal requirements    

If you have not already done so for your package, COMPLETE AND SUBMIT [THIS FORM](https://docs.google.com/forms/d/e/1FAIpQLSe3H6PARLPIkWVjdB_zMvuIuIVtrqNiGlEt1yshkMCmCMirvA/viewform).

## Preparing your package for Staging

Before publishing your package to production, you must send your package on the Package Manager's **staging** repository.  The staging repository is monitored by QA and release management, and is where package validation will take place before it is accepted in production.

1. Publishing your changes to the package manager's **staging** repository happens from Gitlab.  To do so, simply setup your project's Continuous integration, which will be triggered by "Tags" on your branches.
    * Join the **#devs-packman** channel on Slack, and request a staging **USERNAME** and **API_KEY**.
    * In Gitlab, under the **Settings-> CI/CD -> Secret Variables** section, setup the following 2 project variables:
        * API_KEY = [your API KEY]
        * USER_NAME = [your USER NAME@unity]
    * You're almost done!  To publish a version of your package, make sure all your changes are checked into Gitlab, then create a new tag to reflect the version you are publishing (ex. "v1.2.2"), **the tag will trigger a publish to Staging**. You can view progress you the publish request by switch over to the "CI / CD" part of your project.

2. Test your package locally

    Now that your package is published on the package manager's **staging** repository, you can test your package in the editor by creating a new project, and editing the project's `manifest.json` file to point to your staging package, as such:
      ```
      dependencies: {
        "com.unity.[sub-group].[your-package-name]": "0.1.0"
      },
      "registry": "https://staging-packages.unity.com"
      ```

## Get your package published to Production

Packages are promoted to the **production** repository from **staging**, described above. Certain criteria must be met before submitting a request to promote a package to production.
[The list of criteria can be found here](https://docs.google.com/document/d/1TSnlSKJ6_h0C-CYO2LvV0fyGxJvH6OxC2-heyN8o-Gw/edit#heading=h.xxfb5jk2jda2)

Once you feel comfortable that your package meets the list of Release Management Criteria, [Submit your package publishing request to Release Management](https://docs.google.com/forms/d/e/1FAIpQLSdSIRO6s6_gM-BxXbDtdzIej-Hhk-3n68xSyC2sM8tp7413mw/viewform).

**Release management will validate your package content, and check that the editor/playmode tests are passed before promoting the package to production.  You will receive a confirmation email once the package is in production.**

**You're not done!**
At this point, your package is available on the cloud, 2 more steps are required to make your package discoverable in the editor:

1. Contact the Package Manager team in #devs-packman to ask them to add your package to the list of discoverable package for the Unity Editor.  All you need to provide is the package name (com.unity.[sub-group].[your-package-name])
1. If your package is meant to ship with a release of the editor (Unity Recommended Packages), follow these steps:
	* Modify the editor manifest ``[root]\External\PackageManager\Editor\Manifest.json`` to include your package in the list of dependencies.
    * Submit one or more Test Project(s) in Ono, so that your new package can be tested in all ABVs moving forward.  The following steps will create a test project that will run in ABVs, load your package into the project, and run all the tests found in your package.  The better your test coverage, the more confident you'll be that your package works with trunk.
    	* Create a branch in Ono, based on the latest branch this package must be compatible with (trunk, or release branch)
    	* If your package contains **Editor Tests**:
    		* In ``[root]\Tests\Editor.Tests``, create a new EditorTest Project (for new packages use **YourPackageName**) or use an existing project (for new versions of existing package).
    		* A [skeleton of EditorTest Project can be found here](https://oc.unity3d.com/index.php/s/Cldvuy6NpxqYy8y).
    		* Modify the project’s manifest.json file to include the production version of the package (name@version).
    		* Your project's manifest.json file should contain the following line: ``"testables" : [ "com.unity.[sub-group].[your-package-name]" ]``
    	* If your package contains **PlaymodeTests**:
    		* In ``[root]\Tests\PlaymodeTests``, create a new PlaymodeTest Project (for new packages use **YourPackageName**) or use an existing project (for new versions of existing package).
    		* Modify the project’s manifest.json file to include the staging version of the package (name@version).
    		* Your project's manifest.json file should contain the following line: ``"testables" : [ "com.unity.[sub-group].[your-package-name]" ]``.
    		* Commit your branch changes to Ono, and run all Windows & Mac Editor/PlayMode tests (not full ABV) in Katana.
    * Once the tests are green on Katana, create your PR, add both `Latest Release Manager` and  `Trunk Merge Queue` as reviewers.
