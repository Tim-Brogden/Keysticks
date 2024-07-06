# Keysticks

- Control your PC using a gamepad or joystick
- Copyright 2024 Tim Brogden
- https://keysticks.net

## License
This program and the accompanying materials are made available under the terms of the Eclipse Public License v1.0 which accompanies this distribution, and is available at https://www.eclipse.org/legal/epl-v10.html
          
Keysticks uses the OpenAdaptxt API by KeyPoint Technologies (UK) Ltd to provide word suggestions as you type text using your controller. The OpenAdaptxt API is made available under the terms of the Eclipse Public License v1.0 (https://www.eclipse.org/legal/epl-v10.html).

Keysticks uses icons from the Silk icon set 1.3 by Mark James. The icon set is licensed under the Creative Commons Attribution 2.5 License (https://creativecommons.org/licenses/by/2.5/). For further information, see http://www.famfamfam.com/lab/icons/silk/.

Keysticks uses the SlimDX API by SlimDX Group to read input from gamepad  and joystick devices. SlimDX is made available under the terms of the MIT License. If the Keysticks installer needs to install SlimDX, you are asked to confirm your acceptance of the license terms for it.

## Prerequisites
If you are a software developer who would like to build the Keysticks application, you will need to install the following prerequisites:

- SlimDX SDK .NET 4.0 x86 (provided in the ThirdParty folder). This is required to build the KeysticksApp project.
- NSIS (the Nullsoft Scriptable Install System, available from https://nsis.sourceforge.io/Main_Page). This is required to package the MSI file built by the KeysticksSetup project into an installer exe of the form KeysticksSetup-x.x.x.x.exe.

If you just want to install and use Keysticks, you don't need these.

## Projects in the solution
The Keysticks.sln solution, which can be built using Microsoft Visual Studio Community 2019, contains the following projects:

### KeysticksApp
C# .NET WPF application - the main Keysticks program.

### KeysticksSetup
Setup project - builds the MSI installer file. 

In order for this project to load in Microsoft Visual Studio 2019, you need to install the "Microsoft Visual Studio Installer Projects" extension.

Before building this project, you need to add the SlimDX Runtime to Visual Studio's bootstrapper packages so that it can be selected in the Prerequisites dialog. To do this, copy the folder SlimDXJan2012_VS10_Net40_x86 from the ThirdParty folder to your bootstrapper packages folder (which might be "C:\Program Files (x86)\Microsoft SDKs\ClickOnce Bootstrapper\Packages" or similar - see https://docs.microsoft.com/en-us/visualstudio/deployment/creating-bootstrapper-packages?view=vs-2019 for more information). Note that the registry check in Product.xml within that folder 
has been changed to ensure that the installer correctly detects whether the SlimDX runtime is already installed (changes marked 'Keysticks.net').

After building the KeysticksSetup project, you can manually combine the project outputs into
a single installer exe of the form KeysticksSetup-x.x.x.x.exe by editing and running the CreateKeysticksSetup.bat script, which itself calls the KeysticksSetup.nsi NSIS script.

### Keysticks.InstallUtils
Performs a small number of installation tasks when the user runs the Keysticks installer.

### WordPredictor
COM Component (C++) - a wrapper that allows Keysticks to interface with the OpenAdaptxt API.
After building this project for the first time, run the script WordPredictor/RegisterComponent.bat as Administrator to register the COM component. If you wish to deregister the component at any time, run WordPredictor/DeregisterComponent.bat as Administrator.

### WordPredictionDemo
C# .NET WPF application - a simple demonstration of how to use the WordPredictor component in a .NET application. This application is not required by Keysticks.

## Other folders
The source distribution also contains the following folders:

### Data
Local data folder that is used by the application when run from its build location 
(i.e. during development).

### Help
Microsoft HTML Help Workshop project that builds the CHM help file for Keysticks.

### ThirdParty
Third party components that are used by Keysticks.