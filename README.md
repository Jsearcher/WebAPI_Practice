# WebAPI_Practice
This is a C# RESTful API for confirming the state and get required baggage information loaded from A3 station.
## Functions
To receive the source loaded baggage information and provide any required data for BRS handhelds, API functions are designed and listed as follows. Details will be written down in the API document _{fafdfadf}_
* **GetAllLoadingBag**
> **_[GET]_** method for getting all loaded baggage information received from A3 station.
>> Service Path: {SRU}/all?queryDate=yyyy-MM-dd
* **GetLoadingBagByConstraint**
> **_[GET]_** method for getting any requiredloaded baggage, containers or trucks information received from A3 station. Users should specify what kind of query contraints for required data they want, and what list type of those data they need.
>> Service Path: {SRU}/{QueryType}/{id}/{ListType}/{level}?queryDate=yyyy-MM-dd
* **SetUnloadingBagState**
> **_[PUT]_** method for BRS handhelds reporting the state that represents a loaded baggage was scanned and confirmaed to be unloaded from a container.
>> Service Path: {SRU}/unloading with a json body _bagList_
* **SetNewLoadingBagList**
> **_[POST]_** method for the server in A3 station providing source data of loaded baggage, and for BRS handhelds reporting an informal (abnormal) baggage data that needs to be added.
>> Service Path: {SRU}/new-loading/{ProcType} with a json body _bagList_
# License Released under © CTCI Advanced Systems Inc.
