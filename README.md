[![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0)

# Revit_Toolkit
A module that enables exchange of information between BHoM and Revit:
- communication between BHoM and Revit via `RevitListener` and sockets
- `ToRevit` and `FromRevit` conversion
- a set of utility methods supporting conversion and processing of Revit elements

## Quick start ##
A great place to start is reading our Wiki [here](https://github.com/BHoM/documentation/wiki) including pages like the [Structure of the BHoM](https://github.com/BHoM/documentation/wiki/Structure-of-the-BHoM) and [Using the BHoM](https://github.com/BHoM/documentation/wiki/Using-the-BHoM).

Try the [installer](https://bhom.xyz/) and a selection of [sample scripts](https://github.com/BHoM/samples).

## Supported Revit Versions ##
Revit_Toolkit supports following Revit versions:
- Revit 2018
- Revit 2019
- Revit 2020

## Build Revit_Toolkit from Source ###
You will need the following to build Revit_Toolkit:
- Microsoft Visual Studio 2015 or higher
- [GitHub for Windows](https://windows.github.com/)
- Microsoft .NET Framework
    - .NET Framework 4.5.2 for Revit 2018
    - .NET Framework 4.7.2 for Revit 2019
    - .NET Framework 4.7.2 for Revit 2020
- [BHoM](https://github.com/BHoM/BHoM)
- [BHoM_Engine](https://github.com/BHoM/BHoM_Engine)
- [BHoM_Adapter](https://github.com/BHoM/BHoM_Adapter)
- [Socket_Toolkit](https://github.com/BHoM/Socket_Toolkit)

## Debugging ##
To switch between Revit_Toolkit for specific Revit version use Configuration Manager:
Debug -> Revit 2018 (default)

Debug2018 -> Revit 2018

Debug2019 -> Revit 2019

Debug2020 -> Revit 2020

Revit_Toolkit requires Revit 2018 or higher to be installed on the computer.

## Contribute ##
BHoM is an open-source project and would be nothing without its community. Take a look at our contributing guidelines and tips [here](https://github.com/BHoM/BHoM/blob/master/CONTRIBUTING.md).

## License ##
BHoM is free software licenced under GNU Lesser General Public Licence - [https://www.gnu.org/licenses/lgpl-3.0.html](https://www.gnu.org/licenses/lgpl-3.0.html)  
Each contributor holds copyright over their respective contributions.
The project versioning (Git) records all such contribution source information.
See [LICENSE](https://github.com/BHoM/BHoM/blob/master/LICENSE) and [COPYRIGHT_HEADER](https://github.com/BHoM/BHoM/blob/master/COPYRIGHT_HEADER.txt).
