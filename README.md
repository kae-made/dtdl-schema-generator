# DTDL Schema Generator Library  
[Generator library](./DTDLSchemaGeneration/Kae.XTUML.Tools.Generator.DTDL/) that generate from [BridgePoint domain model](https://github.com/xtuml/bridgepoint) to [DTDL schema](https://docs.microsoft.com/en-us/azure/digital-twins/concepts-models).

## How to use  
1. Create conceptual information model for your conceptual domain according to the style of [eXecutable and Translatable UML modeling](https://xtuml.org/).  
1. Generate DTDL shcema from the model. Please see [Sample Generator Application](./DTDLSchemaGeneration/ConsoleAppDTDLGenerator/).  

※ You can use BrdigePoint MicrowaveOven or [LaundromatInHotel](https://github.com/kae-made/artifacts-laundromat-in-hotel-tutorial/tree/main/model/LaundromatInHotel) as a sample model.
※ Generated DTDL schemas of LaundromatInHotel are published at https://github.com/kae-made/artifacts-laundromat-in-hotel-tutorial/tree/main/dtdl.

## Overview of Translation Rule  
- Generate an interface for each class
- For each element of each class

|xtUML|->|DTDL|
|-|-|-|
|classe|->|interface|
|attribute|->|property|
|event|->|telemetry|
|operation|->|command|

- Relationships are defined in the class-side interface where the reference attribute is defined.
- User can select extends or relationship style for super/sub relationship

