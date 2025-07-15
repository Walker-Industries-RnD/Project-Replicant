
# Project Replicant
## Bringing Humanity to Artificial Intelligence

#### Project Replicant is an AI system made with creating high quality/interactive AI running completely locally. Built atop LLMUnity, this system is made to offer developers a high fidelity template for creating AI. [Try out the project on itch.io!](https://walkerdev.itch.io/project-replicant)



[![Clause1](https://github.com/Walker-Industries-RnD/Malicious-Affiliation-Ban/blob/main/WIBan.png?raw=true)](https://github.com/Walker-Industries-RnD/Malicious-Affiliation-Ban/blob/main/README.md)



#### Warning! This project is still a WIP, if you are not proficient with C# and editing code I highly reccomend waiting for me to finish the new UI/Comments/Codes! This should come before the end of July 2024 with a few features talked about below!


![Logo](https://github.com/Walker-Industries-RnD/Project-Replicant/blob/main/Project%20Replicant%20Head%20(8).png)


[![Support me on Patreon](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dwalkerdev%26type%3Dpledges&style=for-the-badge)](https://patreon.com/walkerdev)


[[Note: This Requires LLMUnity To Work]](https://github.com/undreamai/LLMUnity)

[[Note: This Requires UniversalSave To Work]](https://github.com/LifeandStyleMedia/UniversalSave)

For Universal Save, get UniversalSave_1_0_2_UVS.unitypackage and you can have UVS at the BG if you want, also be sure to go to UniversalSave.cs and switch the default to JSON with (public DataFormat dataFormat = DataFormat.JSON;)! I might switch back to nodes later if the files feel like they will be huge!

A few people were interested in the code even with this in mind, so this is a prerelease if anything!

[[Note: You can install MOP2 or delete all code related to it!]](https://github.com/QFSW/MasterObjectPooler2)



## Current Features 

- Easily saves and loads information within a ChatML focused format

- Summarizes information using a summarizer and AIlang (Engels) DB

- Easily save and load different AIs under the Arhua-Sys (Or Artificial Reality Humanoid Understanding and Assistance System) template

## Features on the way (Before the end of July 2024)

- Have information saved using the summarizer and Engels system as an ONNX

- Uses RVCPython to speak to user

- Understand user speech with WhisperX

- Visualizer with a 2D avatar (And a 3D for VR/AR)




## Under Construction!

### This project is still under development!

While you're capable of copy/pasting all scripts into a project and having them work (After replacing some functions I removed for UI navigation since it is getting a complete redo), many, MANY updates are coming to this project! This is more of a beta release as I slowly have the UI/UX refined! I also need to clean and comment a bit more so there are holes here and there! 

I originally planned on dropping everything here but some UI aspects *really* didn't like being exported to a package

## Important Functions To Use

Then, when we want to get our value again, we will take our EncryptedBoolGroup, find the value we want and split the string by //.

The first result is the bool value and dummy string, the second is the counter and the third is the dummy string length as an integer.

If the counter is not equal to the proper place in the boolean order, we know this was tampered with.

Now, we erase the dummy string thanks to us knowing the dummy string length, which leaves us with the bool value. If it is not equal to the true or false value presented by the EncryptedBoolGroup, we know someone copy/pasted another value.







## License

[Apache 2](https://www.apache.org/licenses/LICENSE-2.0)


