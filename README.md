[![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0) [![Build status](https://ci.appveyor.com/api/projects/status/11a3ucotxcl9746k/branch/master?svg=true)](https://ci.appveyor.com/project/BHoMBot/robot-toolkit/branch/master) [![Build Status](https://dev.azure.com/BHoMBot/BHoM/_apis/build/status/Revit_Toolkit/Revit_Toolkit.CheckCore?branchName=master)](https://dev.azure.com/BHoMBot/BHoM/_build/latest?definitionId=99&branchName=master)

# Revit_Toolkit

A set of tools enabling exchange of information between BHoM and [Revit](https://www.autodesk.com/products/revit/overview):
- communication between BHoM and Revit via the Adapter, `RevitListener` plugin and sockets
- `ToRevit` and `FromRevit` conversion
- a set of utility methods supporting conversion and processing of Revit elements

### Known Versions of Software Supported
Autodesk Revit 2018  
Autodesk Revit 2019  
Autodesk Revit 2020  

### Documentation
For more information about functionality see [Revit_Toolkit Wiki](https://github.com/BHoM/Revit_Toolkit/wiki).

This toolkit is part of the Buildings and Habitats object Model. Find out more on our [wiki](https://github.com/BHoM/documentation/wiki) or at [https://bhom.xyz](https://bhom.xyz/)

## Quick Start 🚀 

Grab the [latest installer](https://bhom.xyz/) and a selection of [sample scripts](https://github.com/BHoM/samples).


## Getting Started for Developers 🤖 

If you want to build the BHoM and the Toolkits from source, it's hopefully easy! 😄 
Do take a look at our specific wiki pages here: [Getting Started for Developers](https://github.com/BHoM/documentation/wiki/Getting-started-for-developers)

You will need the following to build Revit_Toolkit:
- Microsoft Visual Studio 2015 or higher
- Microsoft .NET Framework
    - .NET Framework 4.5.2 for Revit 2018
    - .NET Framework 4.7.2 for Revit 2019
    - .NET Framework 4.7.2 for Revit 2020
- [BHoM](https://github.com/BHoM/BHoM)
- [BHoM_Engine](https://github.com/BHoM/BHoM_Engine)
- [BHoM_Adapter](https://github.com/BHoM/BHoM_Adapter)
- [Socket_Toolkit](https://github.com/BHoM/Socket_Toolkit)

Revit_Toolkit needs to be built separately for each version of Revit. To switch between version-specific Revit_Toolkit configurations use Configuration Manager:  
Debug2018 -> Revit 2018  
Debug2019 -> Revit 2019  
Debug2020 -> Revit 2020  


## Want to Contribute? ##

BHoM is an open-source project and would be nothing without its community. Take a look at our contributing guidelines and tips [here](https://github.com/BHoM/BHoM/blob/master/CONTRIBUTING.md).


## Licence ##

BHoM is free software licenced under GNU Lesser General Public Licence - [https://www.gnu.org/licenses/lgpl-3.0.html](https://www.gnu.org/licenses/lgpl-3.0.html)  
Each contributor holds copyright over their respective contributions.
The project versioning (Git) records all such contribution source information.
See [LICENSE](https://github.com/BHoM/BHoM/blob/master/LICENSE) and [COPYRIGHT_HEADER](https://github.com/BHoM/BHoM/blob/master/COPYRIGHT_HEADER.txt).
